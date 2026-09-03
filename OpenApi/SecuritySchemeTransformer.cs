using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace realworld_net.OpenApi;

/// <summary>
/// Declares the RealWorld "Token &lt;jwt&gt;" auth as an apiKey security scheme in the
/// OpenAPI document so Scalar renders an Authorize input. RealWorld uses the
/// <c>Authorization: Token &lt;jwt&gt;</c> header rather than the standard <c>Bearer</c>
/// scheme, so this is modelled as an apiKey where the full header value is entered.
/// </summary>
internal sealed class SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeId = "Token";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "RealWorld JWT auth — enter the full header value including the scheme, e.g. `Token eyJ...`",
        };

        return Task.CompletedTask;
    }
}

/// <summary>
/// Adds the security requirement only to operations whose endpoint carries
/// <c>[Authorize]</c> (and not <c>[AllowAnonymous]</c>), so anonymous endpoints such
/// as register/login are not marked as requiring auth.
/// </summary>
internal sealed class AuthorizeOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var requiresAuth = metadata.OfType<IAuthorizeData>().Any()
            && !metadata.OfType<IAllowAnonymous>().Any();

        if (requiresAuth)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SecuritySchemeDocumentTransformer.SchemeId, context.Document)] = [],
            });
        }

        return Task.CompletedTask;
    }
}
