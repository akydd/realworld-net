using Microsoft.EntityFrameworkCore;
using realworld_net.Dtos;
using realworld_net.Services;

namespace realworld_net.Tests.Services;

[Collection("Database")]
public class ArticleServiceTests : IAsyncLifetime
{
    private readonly DbFixture _dbFixture;

    public ArticleServiceTests(DbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task CreateArticleAsync_ShouldCreateNewArticle()
    {
        var user = await SeedUserAsync();
        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        var articleDto = new CreateArticleDto(new CreateArticleInnerDto("test title", "test description", "test body", new List<string>()));
        var savedArticle = await service.CreateArticleAsync(user.Id, articleDto);

        await using var assertContext = _dbFixture.CreateContext();
        var readArticle = await assertContext.Articles.SingleAsync(a => a.Slug == savedArticle.Slug);
        Assert.Equal(user.Id, readArticle.AuthorId);
    }

    [Fact]
    public async Task DeleteArticleAsync_ShouldDeleteArticleForAuthor()
    {
        var user = await SeedUserAsync();
        var article = await SeedArticleAsync(user.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);
        await service.DeleteArticleAsync(user.Id, article.Slug);

        await using var assertContext = _dbFixture.CreateContext();
        var articleExists = await assertContext.Articles.AnyAsync(a => a.Slug == article.Slug);
        Assert.False(articleExists);
    }

    [Fact]
    public async Task DeleteArticleAsync_ShouldFailsForNonAuthor()
    {
        var user = await SeedUserAsync("Jo");
        var author = await SeedUserAsync("Mo");
        var article = await SeedArticleAsync(author.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await service.DeleteArticleAsync(user.Id, article.Slug));

        await using var assertContext = _dbFixture.CreateContext();
        var articleExists = await assertContext.Articles.AnyAsync(a => a.Slug == article.Slug);
        Assert.True(articleExists);
    }

    [Fact]
    public async Task FavoriteArticleAsync_SucceedsWhenArticleExists()
    {
        var user = await SeedUserAsync();
        var article = await SeedArticleAsync(user.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        var fav = await service.FavoriteArticleAsync(user.Id, article.Slug);

        await using var assertContext = _dbFixture.CreateContext();
        var updatedArticle = await assertContext.Articles.SingleAsync(a => a.Slug == article.Slug);
        Assert.Equal(1, updatedArticle.FavoritesCount);

        var favRecordExists = await assertContext.Favorites.AnyAsync(f => f.ArticleId == article.Id && f.UserId == user.Id);
        Assert.True(favRecordExists);
    }

    [Fact]
    public async Task FavoriteArticleAsync_Idempotent()
    {
        var user = await SeedUserAsync();
        var article = await SeedArticleAsync(user.Id);


        await using (var context1 = _dbFixture.CreateContext())
        {
            await new ArticleService(context1).FavoriteArticleAsync(user.Id, article.Slug);
        }

        await using (var context2 = _dbFixture.CreateContext())
        {
            var fav = await new ArticleService(context2).FavoriteArticleAsync(user.Id, article.Slug);
            Assert.Equal(1, fav.FavoritesCount);
        }

        await using var assertContext = _dbFixture.CreateContext();
        var updatedArticle = await assertContext.Articles.SingleAsync(a => a.Slug == article.Slug);
        Assert.Equal(1, updatedArticle.FavoritesCount);

        var favRecordCount = await assertContext.Favorites.CountAsync(f => f.ArticleId == article.Id && f.UserId == user.Id);
        Assert.Equal(1, favRecordCount);
    }

    [Fact]
    public async Task GetArticleBySlugAsync_SucceedsWhenArticleExists_NoAuth()
    {
        var user = await SeedUserAsync();
        var article = await SeedArticleAsync(user.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        var foundArticle = await service.GetArticleBySlugAsync(article.Slug, null);
        Assert.NotNull(foundArticle);
        Assert.Equal(article.Slug, foundArticle.Slug);
        Assert.Equal(article.Title, foundArticle.Title);
        Assert.Equal(article.Description, foundArticle.Description);
        Assert.Equal(article.Body, foundArticle.Body);
        Assert.Equal(user.Username, foundArticle.Author.Username);
        Assert.False(foundArticle.Author.Following);   // no auth → not following
        Assert.False(foundArticle.Favorited);          // no auth → not favorited
        Assert.Equal(0, foundArticle.FavoritesCount);
    }

    [Fact]
    public async Task GetArticleBySlugAsync_SucceedsWhenArticleExists_Auth()
    {
        var author = await SeedUserAsync("Joe");
        var user = await SeedUserAsync("Mo");
        var article = await SeedArticleAsync(author.Id);

        // Make the user follow the author, and also fav this article.
        await SeedFavorite(user.Id, article.Id);
        await SeedFollows(user.Id, author.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        var foundArticle = await service.GetArticleBySlugAsync(article.Slug, user.Id);
        Assert.NotNull(foundArticle);
        Assert.Equal(article.Slug, foundArticle.Slug);
        Assert.True(foundArticle.Author.Following);
        Assert.True(foundArticle.Favorited);
        Assert.Equal(1, foundArticle.FavoritesCount);
    }

    [Fact]
    public async Task ListArticles_NoAuth_NoFilter()
    {
        var author1 = await SeedUserAsync("Joe");
        var article1 = await SeedArticleAsync(author1.Id);

        var author2 = await SeedUserAsync("Mo");
        var article2 = await SeedArticleAsync(author2.Id, "new-article");

        // Backdate article2.
        await using var context = _dbFixture.CreateContext();
        await context.Articles
            .Where(a => a.Id == article2.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.CreatedAt, DateTime.UtcNow.AddDays(-1)));

        var service = new ArticleService(context);
        var filter = new ArticleFilter();
        var articles = await service.ListArticles(filter, null);

        Assert.Equal(2, articles.ArticlesCount);
        Assert.Equal(article1.Slug, articles.Articles[0].Slug);
        Assert.Equal(article2.Slug, articles.Articles[1].Slug);
    }

    [Fact]
    public async Task ListArticles_AuthorFilter_NoAuth()
    {
        var author1 = await SeedUserAsync("Joe");
        var article1 = await SeedArticleAsync(author1.Id);

        var author2 = await SeedUserAsync("Mo");
        var article2 = await SeedArticleAsync(author2.Id, "new-article");

        // Backdate article2.
        await using var context = _dbFixture.CreateContext();
        await context.Articles
            .Where(a => a.Id == article2.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.CreatedAt, DateTime.UtcNow.AddDays(-1)));

        var service = new ArticleService(context);
        var filter = new ArticleFilter
        {
            Author = "Joe"
        };
        var articles = await service.ListArticles(filter, null);

        Assert.Equal(1, articles.ArticlesCount);
        Assert.Equal(article1.Slug, articles.Articles[0].Slug);
    }

    [Fact]
    public async Task UnfavoriteArticle_WorksWhenArticleExists()
    {
        var user = await SeedUserAsync();
        var article = await SeedArticleAsync(user.Id);
        await SeedFavorite(user.Id, article.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        var updatedArticle = await service.UnfavoriteArticleAsync(user.Id, article.Slug);
        Assert.NotNull(updatedArticle);
        Assert.False(updatedArticle.Favorited);
        Assert.Equal(0, updatedArticle.FavoritesCount);
    }

    [Fact]
    public async Task UnfavoriteArticle_Idempotent()
    {
        var user = await SeedUserAsync();
        var article = await SeedArticleAsync(user.Id);
        await SeedFavorite(user.Id, article.Id);

        await using (var context1 = _dbFixture.CreateContext())
        {
            var afterFirst = await new ArticleService(context1).UnfavoriteArticleAsync(user.Id, article.Slug);
        }

        await using (var context2 = _dbFixture.CreateContext())
        {
            var afterSecond = await new ArticleService(context2).UnfavoriteArticleAsync(user.Id, article.Slug);
            Assert.NotNull(afterSecond);
            Assert.False(afterSecond.Favorited);
            Assert.Equal(0, afterSecond.FavoritesCount);
        }

        await using var assertContext = _dbFixture.CreateContext();
        var favRecordExists = await assertContext.Favorites.AnyAsync(f => f.ArticleId == article.Id && f.UserId == user.Id);
        Assert.False(favRecordExists);
    }

    [Fact]
    public async Task UpdateArticle_FailsForNonAuthor()
    {
        var user = await SeedUserAsync("Jo");
        var author = await SeedUserAsync("Mo");
        var article = await SeedArticleAsync(author.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.UpdateArticleAsync(user.Id, article.Slug,
                new UpdateArticleDto(new UpdateArticleInnerDto("updated", "updated", "updated"))));

        await using var assertContext = _dbFixture.CreateContext();
        var fetchedArticle = await assertContext.Articles.SingleAsync(a => a.Slug == article.Slug);
        Assert.Equal(article.UpdatedAt, fetchedArticle.UpdatedAt);
        Assert.Equal(article.Slug, fetchedArticle.Slug);
        Assert.Equal(article.Title, fetchedArticle.Title);
        Assert.Equal(article.Description, fetchedArticle.Description);
        Assert.Equal(article.Body, fetchedArticle.Body);
    }

    [Fact]
    public async Task UpdateArticle_SucceedsForAuthor()
    {
        var author = await SeedUserAsync("Mo");
        var article = await SeedArticleAsync(author.Id);

        await using var context = _dbFixture.CreateContext();
        var service = new ArticleService(context);

        var updatedArticle = await service.UpdateArticleAsync(author.Id, article.Slug,
                new UpdateArticleDto(new UpdateArticleInnerDto("Updated Title", "updated description", "updated body")));
        Assert.NotNull(updatedArticle);
        Assert.Equal("updated-title", updatedArticle.Slug);
        Assert.Equal("Updated Title", updatedArticle.Title);
        Assert.Equal("updated description", updatedArticle.Description);
        Assert.Equal("updated body", updatedArticle.Body);

        await using var assertContext = _dbFixture.CreateContext();
        var fetchedArticle = await assertContext.Articles.SingleAsync(a => a.Id == article.Id);
        Assert.Equal(updatedArticle.UpdatedAt, fetchedArticle.UpdatedAt);
        Assert.Equal(updatedArticle.Slug, fetchedArticle.Slug);
        Assert.Equal(updatedArticle.Title, fetchedArticle.Title);
        Assert.Equal(updatedArticle.Description, fetchedArticle.Description);
        Assert.Equal(updatedArticle.Body, fetchedArticle.Body);
    }

    public Task DisposeAsync() => Task.CompletedTask;
    public async Task InitializeAsync() => await _dbFixture.ResetAsync();

    private async Task<Entities.User> SeedUserAsync(string username = "Joe")
    {
        await using var context = _dbFixture.CreateContext();
        var user = new Entities.User
        {
            Username = username,
            Email = $"{username}@test.com",
            PasswordHash = "password",
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<Entities.Article> SeedArticleAsync(int userId, string slug = "test")
    {
        await using var context = _dbFixture.CreateContext();
        var article = new Entities.Article
        {
            Slug = slug,
            Title = slug,
            Description = "test",
            Body = "test",
            AuthorId = userId,
        };
        context.Articles.Add(article);
        await context.SaveChangesAsync();
        return article;
    }

    private async Task<Entities.Favorites> SeedFavorite(int userId, int articleId)
    {
        await using var context = _dbFixture.CreateContext();
        var fav = new Entities.Favorites
        {
            UserId = userId,
            ArticleId = articleId
        };
        context.Favorites.Add(fav);
        await context.Articles
            .Where(a => a.Id == articleId)
            .ExecuteUpdateAsync(update => update.SetProperty(a => a.FavoritesCount, a => a.FavoritesCount + 1));
        await context.SaveChangesAsync();
        return fav;
    }

    private async Task<Entities.Follows> SeedFollows(int followerId, int followingId)
    {
        await using var context = _dbFixture.CreateContext();
        var follows = new Entities.Follows
        {
            FollowerId = followerId,
            FolloweeId = followingId
        };
        context.Follows.Add(follows);
        await context.SaveChangesAsync();
        return follows;
    }
}
