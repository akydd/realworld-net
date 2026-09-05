namespace realworld_net.Entities;

public abstract class Auditable
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
}
