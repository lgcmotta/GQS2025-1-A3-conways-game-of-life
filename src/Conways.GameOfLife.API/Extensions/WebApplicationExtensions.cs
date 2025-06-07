using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Conways.GameOfLife.API.Extensions;

internal static class WebApplicationExtensions
{
    internal static RouteGroupBuilder MapApiGroup(this WebApplication app, ApiVersion version)
    {
        var versionSet = app.BuildApiVersionSet(version);

        return app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);
    }

    private static ApiVersionSet BuildApiVersionSet(this WebApplication app, ApiVersion version)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(version)
            .ReportApiVersions()
            .Build();
    }
}