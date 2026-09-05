
namespace realworld_net.Dtos;

public record UserResponseInnerDto(string Email, string Token, string Username, string? Bio, string? Image);

public record UserResponseDto(UserResponseInnerDto User);

public record ProfileResponseInnerDto(string Username, string? Bio, string? Image, bool Following);

public record ProfileResponseDto(ProfileResponseInnerDto Profile);

public record ArticleSingleInnerDto(string Slug, string Title, string Description, string Body, List<string> TagList, DateTime CreatedAt, DateTime UpdatedAt, bool Favorited, int FavoritesCount, ProfileResponseInnerDto Author);

public record ArticleSingleResponseDto(ArticleSingleInnerDto Article);

public record ArticleMultipleInnerDto(string Slug, string Title, string Description, List<string> TagList, DateTime CreatedAt, DateTime UpdatedAt, bool Favorited, int FavoritesCount, ProfileResponseInnerDto Author);

public record ArticleMultipleDto(List<ArticleMultipleInnerDto> Articles, int ArticlesCount);
