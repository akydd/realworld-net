namespace realworld_net.Dtos;

public record UserResponseInnerDto(string Email, string Token, string Username, string? Bio, string? Image);

public record UserResponseDto(UserResponseInnerDto User);

public record ProfileResponseInnerDto(string Username, string? Bio, string? Image, bool Following);

public record ProfileResponseDto(ProfileResponseInnerDto Profile);
