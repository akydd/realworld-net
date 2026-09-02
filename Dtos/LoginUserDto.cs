using System.ComponentModel.DataAnnotations;

namespace realworld_net.Dtos;

public record LoginUserInnerDto([Required] string Email, [Required] string Password);

public record LoginUserDto([Required] LoginUserInnerDto User);
