namespace api.Features.Users.Register;

public record RegisterDto
{
    public string FirstName { get; set; } = default!;
    public string? LastName { get; set; } 
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
