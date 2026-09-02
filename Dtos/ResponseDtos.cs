namespace realworld_net.Dtos;

public record UserResponseInnerDto(string Email, string Token, string Username, string Bio, string Image);

public record UserResponseDto(UserResponseInnerDto User);