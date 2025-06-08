using Asp.Versioning;
using Conways.GameOfLife.API.Extensions;
using Conways.GameOfLife.API.Features.CreateBoard;
using Conways.GameOfLife.API.Features.FinalGeneration;
using Conways.GameOfLife.API.Features.NextGeneration;
using Conways.GameOfLife.API.Features.NextGenerations;
using Conways.GameOfLife.API.Middlewares;
using Conways.GameOfLife.Infrastructure.Extensions;
using Scalar.AspNetCore;

var v1 = new ApiVersion(majorVersion: 1, minorVersion: 0);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddPostgreHealthCheck(builder.Configuration);
builder.Services.AddBoardDbContexts(builder.Configuration);
builder.Services.AddHashIds(builder.Configuration);
builder.Services.AddCQRS();
builder.Services.AddApiExceptionHandling();
builder.Services.AddApiVersioning(v1);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultHealthChecks();
app.MapOpenApi();
app.MapOpenApiUI();
app.UseMiddleware<ExceptionMiddleware>();

var api = app.MapApiGroup(v1);

api.MapCreateBoardEndpoint(v1);
api.MapNextGenerationEndpoint(v1);
api.MapNextGenerationsEndpoint(v1);
api.MapFinalGenerationEndpoint(v1);

await app.RunAsync()
    .ConfigureAwait(continueOnCapturedContext: false);

public partial class Program
{
    protected Program()
    {
    }
}