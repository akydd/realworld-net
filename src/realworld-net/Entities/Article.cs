using Microsoft.EntityFrameworkCore;

namespace realworld_net.Entities;

[Index(nameof(Slug), IsUnique = true)]
public class Article : Auditable
{
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Body { get; set; }
    public int FavoritesCount { get; set; }
}
