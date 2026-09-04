namespace realworld_net.Models;

public record ArticleFeedItem(string Slug, string Title, string Description, List<string> TagList, DateTime CreatedAt, DateTime UpdatedAt, bool Favorited, int FavoritesCount, Profile Author);
public record ArticleFeed(List<ArticleFeedItem> Articles, int ArticlesCount);
