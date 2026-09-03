using Microsoft.EntityFrameworkCore;

namespace realworld_net.Entities;

[PrimaryKey(nameof(FollowerId), nameof(FolloweeId))]
public class Follows
{
    public int FollowerId { get; set; }
    public User Follower { get; set; } = null!;
    public int FolloweeId { get; set; }
    public User Followee { get; set; } = null!;
}
