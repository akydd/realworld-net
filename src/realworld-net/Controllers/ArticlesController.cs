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

    /// <summary>
    /// Create a new article.
    /// </summary>
    /// <param name="articleDto">Article details.</param>
    /// <returns>The newly created article.</returns>
    /// <response code="201">When an article is created.</response>
    /// <response code="401">When the caller is not authenticated.</response>
    /// <response code="422">When the request body is invalid.</response>
    [Authorize]
    [HttpPost(Name = "CreateArticle")]
    [ProducesResponseType(typeof(ArticleSingleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto articleDto)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var createdArticle = await _articleService.CreateArticleAsync(userId, articleDto);

        var responseDto = toDto(createdArticle);
        return Created("", responseDto);
    }

    /// <summary>
    /// Get an article by slug.
    /// </summary>
    /// <param name="slug">Unique article slug..</param>
    /// <returns>The full article.</returns>
    /// <response code="200">When an article is found.</response>
    /// <response code="404">When no article is found.</response>
    [HttpGet("{slug}", Name = "GetArticle")]
    [ProducesResponseType(typeof(ArticleSingleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArticle(string slug)
    {
        var article = await _articleService.GetArticleBySlugAsync(slug, null);
        var responseDto = toDto(article);
        return Ok(responseDto);
    }

    /// <summary>
    /// Update an article.
    /// </summary>
    /// <param name="slug">Unique slug of the article to update.</param>
    /// <param name="articleDto">Updated article fields.</param>
    /// <returns>The full updated article.</returns>
    /// <response code="200">When the article is updated.</response>
    /// <response code="401">When the caller is not authenticated.</response>
    /// <response code="403">When the caller is not the author of the article.</response>
    /// <response code="404">When no article is not found.</response>
    /// <response code="422">When the request body is invalid.</response>
    [Authorize]
    [HttpPut("{slug}", Name = "UpdateArticle")]
    [ProducesResponseType(typeof(ArticleSingleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateArticle(string slug, [FromBody] UpdateArticleDto articleDto)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var updatedArticle = await _articleService.UpdateArticleAsync(userId, slug, articleDto);

        var responseDto = toDto(updatedArticle);
        return Ok(responseDto);
    }

    /// <summary>
    /// Favorite an article.
    /// </summary>
    /// <remarks>If the article is already a favorite, this is a no-op.</remarks>
    /// <param name="slug">Unique article slug.</param>
    /// <returns>The full favorited article.</returns>
    /// <response code="200">When the article is favorited.</response>
    /// <response code="401">When the caller is not authenticated.</response>
    /// <response code="404">When the article is not found..</response>
    [Authorize]
    [HttpPost("{slug}/favorite", Name = "Favorite Article")]
    [ProducesResponseType(typeof(ArticleSingleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FavoriteArticle(string slug)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var updatedArticle = await _articleService.FavoriteArticleAsync(userId, slug);

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

    /// <summary>
    /// Unfavorite an article.
    /// </summary>
    /// <remarks>If the article is already not a favorite, this is a no-op.</remarks>
    /// <param name="slug">Unique article slug.</param>
    /// <returns>Full unfavorited article.</returns>
    /// <response code="200">When the article was un-set as a favorite.</response>
    /// <response code="401">When the caller is not authenticated.</response>
    /// <response code="404">When the article is not found.</response>
    [Authorize]
    [HttpDelete("{slug}/favorite", Name = "Unfavorite Article")]
    [ProducesResponseType(typeof(ArticleSingleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfavoriteArticle(string slug)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        var updatedArticle = await _articleService.UnfavoriteArticleAsync(userId, slug);
        var responseDto = toDto(updatedArticle);
        return Ok(responseDto);
    }

    /// <summary>
    /// Delete an article.
    /// </summary>
    /// <param name="slug">The unique slug of the article.</param>
    /// <returns>Nothing.</returns>
    /// <response code="200">The article was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to delete the article.</response>
    /// <response code="404">The article is not found.</response>
    [Authorize]
    [HttpDelete("{slug}", Name = "Delete Article")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteArticle(string slug)
    {
        var userId = int.Parse(User.FindFirstValue("id")!, CultureInfo.InvariantCulture);
        await _articleService.DeleteArticleAsync(userId, slug);
        return Ok();
    }

    /// <summary>
    /// List articles, filtered.
    /// </summary>
    /// <param name="filter">Filter parameters</param>
    /// <returns>Filtered list of articles, ordered by most recent.</returns>
    /// <response code="200"></response>
    [HttpGet(Name = "List Articles")]
    [ProducesResponseType(typeof(ArticleMultipleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListArticles([FromQuery] ArticleFilter filter)
    {
        int? userId = int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
        var articles = await _articleService.ListArticles(filter, userId);

        var responseDto = new ArticleMultipleDto(articles.Articles.Select(a => new ArticleMultipleInnerDto(
            a.Slug,
            a.Title,
            a.Description,
            new List<string>(),
            a.CreatedAt,
            a.UpdatedAt,
            a.Favorited,
            a.FavoritesCount,
            new ProfileResponseInnerDto(
                a.Author.Username,
                a.Author.Bio,
                a.Author.Image,
                a.Author.Following
            )
        )).ToList(), articles.ArticlesCount);

        return Ok(responseDto);
    }
}
