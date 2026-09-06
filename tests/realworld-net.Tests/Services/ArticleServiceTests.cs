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

    public Task DisposeAsync() => Task.CompletedTask;
    public async Task InitializeAsync() => await _dbFixture.ResetAsync();

    private async Task<Entities.User> SeedUserAsync(string username = "Joe")
    {
        await using var context = _dbFixture.CreateContext();
        var user = new Entities.User
        {
            Username = username,
            Email = $"{username}@test.com",
            PasswordHash = "password"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
