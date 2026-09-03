using System.ComponentModel.DataAnnotations;

namespace realworld_net.Dtos;

public record CreateArticleInnerDto([Required] string Title, [Required] string Description, [Required] string Body, List<string>? TagList);

public record CreateArticleDto([Required] CreateArticleInnerDto Article);
