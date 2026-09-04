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

        var responseDto = toDto(createdArticle);
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

        var responseDto = toDto(article);
        return Ok(responseDto);
    }

    [Authorize]
    [HttpPut("{slug}", Name = "UpdateArticle")]
    public async Task<IActionResult> UpdateArticle(string slug, [FromBody] UpdateArticleDto articleDto)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var updatedArticle = await _articleService.UpdateArticleAsync(userId, slug, articleDto);
        if (updatedArticle == null)
        {
            return NotFound();
        }

        var responseDto = toDto(updatedArticle);
        return Ok(responseDto);
    }

    [Authorize]
    [HttpPost("{slug}/favorite", Name = "Favorite Article")]
    public async Task<IActionResult> FavoriteArticle(string slug)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var updatedArticle = await _articleService.FavoriteArticleAsync(userId, slug);
        if (updatedArticle == null)
        {
            return NotFound();
        }

        var responseDto = toDto(updatedArticle);
        return Ok(responseDto);
    }

    private static ArticleSingleResponseDto toDto(Models.Article updatedArticle)
    {
        var responseDto = new ArticleSingleResponseDto(
                    new ArticleSingleInnerDto(
                        updatedArticle.Slug,
                        updatedArticle.Title,
                        updatedArticle.Description,
                        updatedArticle.Body,
                        new List<string>(), // Assuming you have a way to get tags
                        updatedArticle.CreatedAt,
                        updatedArticle.UpdatedAt,
                        updatedArticle.Favorited,
                        updatedArticle.FavoritesCount,
                        new ProfileResponseInnerDto(
                            updatedArticle.Author.Username,
                            updatedArticle.Author.Bio,
                            updatedArticle.Author.Image,
                            updatedArticle.Author.Following
                        )

                    ));
        return responseDto;
    }

    [Authorize]
    [HttpDelete("{slug}/favorite", Name = "Unfavorite Article")]
    public async Task<IActionResult> UnfavoriteArticle(string slug)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var updatedArticle = await _articleService.UnfavoriteArticleAsync(userId, slug);
        if (updatedArticle == null)
        {
            return NotFound();
        }
        var responseDto = toDto(updatedArticle);
        return Ok(responseDto);

    }

    [Authorize]
    [HttpDelete("{slug}", Name = "Delete Article")]
    public async Task<IActionResult> DeleteArticle(string slug)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        await _articleService.DeleteArticleAsync(userId, slug);
        return Ok();
    }
}
