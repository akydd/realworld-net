using realworld_net.Dtos;
using realworld_net.Models;

namespace realworld_net.Services;

public interface IArticleService
{
    Task<Article> CreateArticleAsync(int userId, CreateArticleDto articleDto);
    Task<Article?> GetArticleBySlugAsync(string slug, int? userId);
    Task<Article> UpdateArticleAsync(int userId, string slug, UpdateArticleDto articleDto);
}
