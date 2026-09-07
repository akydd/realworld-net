using realworld_net.Dtos;
using realworld_net.Models;

namespace realworld_net.Services;

public interface IArticleService
{
    Task<Article> CreateArticleAsync(int userId, CreateArticleDto articleDto);
    Task<Article?> GetArticleBySlugAsync(string slug, int? userId);
    Task<Article> UpdateArticleAsync(int userId, string slug, UpdateArticleDto articleDto);
    Task<Article> FavoriteArticleAsync(int userId, string slug);
    Task<Article> UnfavoriteArticleAsync(int userId, string slug);

    /// <summary>
    /// Removes an article. Only the article author can do this.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">The caller is not the article author.</exception>
    /// <param name="userId"></param>
    /// <param name="slug"></param>
    /// <returns></returns>
    Task DeleteArticleAsync(int userId, string slug);
    Task<ArticleFeed> ListArticles(ArticleFilter filter, int? userId);
}
