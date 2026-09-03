using System.ComponentModel.DataAnnotations;

namespace realworld_net.Dtos;

public record UpdateArticleInnerDto(string? Title, string? Description, string? Body);

public record UpdateArticleDto([Required] UpdateArticleInnerDto Article);
