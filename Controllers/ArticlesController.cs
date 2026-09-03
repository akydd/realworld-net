using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realworld_net.Dtos;
using realworld_net.Services;

namespace realworld_net.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [Authorize]
    [HttpPost(Name = "CreateArticle")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto articleDto)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var createdArticle = await _articleService.CreateArticleAsync(userId, articleDto);

        var responseDto = new ArticleSingleResponseDto(
            new ArticleSingleInnerDto(
                createdArticle.Slug,
                createdArticle.Title,
                createdArticle.Description,
                createdArticle.Body,
                new List<string>(), // Assuming you have a way to get tags
                createdArticle.CreatedAt,
                createdArticle.UpdatedAt,
                false, // Assuming you have a way to determine if favorited
                0, // Assuming you have a way to get favorites count
                new ProfileResponseInnerDto(
                    createdArticle.Author.Username,
                    createdArticle.Author.Bio,
                    createdArticle.Author.Image,
                    createdArticle.Author.Following
                )
            )
        );

        return Ok(responseDto);
    }

    [HttpGet("{slug}", Name = "GetArticle")]
    public async Task<IActionResult> GetArticle(string slug)
    {
        var article = await _articleService.GetArticleBySlugAsync(slug, null);
        if (article == null)
        {
            return NotFound();
        }

        var responseDto = new ArticleSingleResponseDto(
            new ArticleSingleInnerDto(
                article.Slug,
                article.Title,
                article.Description,
                article.Body,
                new List<string>(), // Assuming you have a way to get tags
                article.CreatedAt,
                article.UpdatedAt,
                false, // Assuming you have a way to determine if favorited
                0, // Assuming you have a way to get favorites count
                new ProfileResponseInnerDto(
                    article.Author.Username,
                    article.Author.Bio,
                    article.Author.Image,
                    article.Author.Following
                )
            )
        );

        return Ok(responseDto);
    }
}
