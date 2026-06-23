using api.Features.Users.Register;
using api.Middleware;
using api.Models;
using api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//=== Serilog ===
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

//=== ConfigurationBinder ===
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

//=== Services ===
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// === Build ===
var app = builder.Build();

// === MongoDB Unique Index on Email ===
try
{
    using var scope = app.Services.CreateScope();
    var settings = scope.ServiceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

    var client = new MongoClient(settings.ConnectionString);
    var database = client.GetDatabase(settings.DatabaseName);
    var collection = database.GetCollection<User>(settings.UserCollection);

    var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
    var indexOptions = new CreateIndexOptions { Unique = true };
    var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);

    await collection.Indexes.CreateOneAsync(indexModel);
    Log.Information("✅ Unique index on Email field ensured");
}
catch (Exception ex)
{
    Log.Warning(ex, "Failed to create unique index on Email (it may already exist)");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();