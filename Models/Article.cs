namespace realworld_net.Models;

public record Article(string Slug, string Title, string Description, string Body, List<string> TagList, DateTime CreatedAt, DateTime UpdatedAt, bool Favorited, int FavoritesCount, Profile Author);
