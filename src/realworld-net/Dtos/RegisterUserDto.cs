using System.ComponentModel.DataAnnotations;

namespace realworld_net.Dtos;

public record RegisterUserInnerDto([Required] string Username, [Required] string Email, [Required] string Password);

public record RegisterUserDto([Required] RegisterUserInnerDto User);
