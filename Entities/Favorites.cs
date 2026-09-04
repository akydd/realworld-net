using Microsoft.EntityFrameworkCore;

namespace realworld_net.Entities;

[PrimaryKey(nameof(UserId), nameof(ArticleId))]
public class Favorites
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
}
