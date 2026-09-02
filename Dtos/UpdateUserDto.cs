using System.ComponentModel.DataAnnotations;

namespace realworld_net.Dtos;

public record UpdateUserInnerDto(string? Email, string? Username, string? Password, string? Bio, string? Image);

public record UpdateUserDto([Required] UpdateUserInnerDto User);
