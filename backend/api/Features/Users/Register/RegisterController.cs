using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace api.Features.Users.Register;

[ApiController]
[Route("api/user")]
public class RegisterController : ControllerBase
{
    private readonly IUserService _userService;

    public RegisterController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterDto dto)
    {
        var user = new User
        {
            Name = $"{dto.FirstName} {dto.LastName}".Trim(),
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12)
        };

        try
        {
            await _userService.CreateAsync(user);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return Conflict(new { message = "Email already taken" });
        }
        await _userService.CreateAsync(user);

        var response = new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            createdAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(CreateAccount), new { id = user.Id }, response);
    }
}
