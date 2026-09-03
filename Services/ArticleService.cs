using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using realworld_net.Dtos;
using realworld_net.Models;
using DbArticle = realworld_net.Entities.Article;

namespace realworld_net.Services;

public class ArticleService : IArticleService
{
    private readonly AppDbContext _context;

    public ArticleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Article> CreateArticleAsync(int userId, CreateArticleDto articleDto)
    {
        var innerDto = articleDto.Article;

        var slug = GenerateSlug(innerDto.Title);

        var article = new DbArticle
        {
            Slug = slug,
            Title = innerDto.Title,
            Description = innerDto.Description,
            Body = innerDto.Body,
            AuthorId = userId
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        return await _context.Articles
            .Where(a => a.Id == article.Id)
            .Select(a => new Article
            {
                Slug = a.Slug,
                Title = a.Title,
                Description = a.Description,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Author = new Profile
                {
                    Username = a.Author.Username,
                    Bio = a.Author.Bio,
                    Image = a.Author.Image,
                    Following = false
                }
            })
            .FirstAsync();
    }

    public async Task<Article?> GetArticleBySlugAsync(string slug, int? userId)
    {
        return await _context.Articles
            .Where(a => a.Slug == slug)
            .Select(a => new Article
            {
                Slug = a.Slug,
                Title = a.Title,
                Description = a.Description,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Author = new Profile
                {
                    Username = a.Author.Username,
                    Bio = a.Author.Bio,
                    Image = a.Author.Image,
                    Following = userId != null && _context.Follows.Any(f => f.FollowerId == userId && f.FolloweeId == a.AuthorId)
                }
            })
             .FirstOrDefaultAsync();
    }

    public async Task<Article> UpdateArticleAsync(int userId, string slug, UpdateArticleDto articleDto)
    {
        var innerDto = articleDto.Article;

        var articleToUpdate = await _context.Articles
            .Where(a => a.Slug == slug && a.AuthorId == userId)
            .FirstOrDefaultAsync() ?? throw new UnauthorizedAccessException("You are not authorized to update this article.");

        if (innerDto.Title != null)
        {
            articleToUpdate.Title = innerDto.Title;
            articleToUpdate.Slug = GenerateSlug(innerDto.Title);
        }

        if (innerDto.Description != null)
        {
            articleToUpdate.Description = innerDto.Description;
        }

        if (innerDto.Body != null)
        {
            articleToUpdate.Body = innerDto.Body;
        }

        await _context.SaveChangesAsync();

        return await GetArticleBySlugAsync(articleToUpdate.Slug, userId) ?? throw new Exception("Article not found after update.");
    }

    private string GenerateSlug(string title)
    {
        return title.ToLower().Replace(' ', '-');
    }
}
