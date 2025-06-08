using Asp.Versioning;
using Asp.Versioning.Builder;
using Scalar.AspNetCore;

namespace Conways.GameOfLife.API.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MapOpenApiUI(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options.WithTheme(ScalarTheme.DeepSpace)
                .WithTitle("Conway's Game of Life")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithDownloadButton(showDownloadButton: true)
                .WithDotNetFlag(expose: true);
        });

        return app;
    }

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