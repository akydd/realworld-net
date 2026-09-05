using Microsoft.AspNetCore.Mvc;

namespace realworld_net.Dtos;

public record ArticleFilter
{
    public string? Tag { get; init; }
    public string? Author { get; init; }
    [FromQuery(Name = "favorited")] public string? FavoritedBy { get; init; }
    public int Limit { get; init; } = 20;
    public int Offset { get; init; } = 0;
}
