using Conways.GameOfLife.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume("postgres_data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .AddDatabase("GameOfLife");

var migrator = builder.AddDotNetCakeEfCoreMigrator("dotnet-cake-migrator")
    .WithReference(postgre)
    .WaitFor(postgre);

builder.AddProject<Projects.Conways_GameOfLife_API>("conways-gameoflife-api")
    .WithHttpHealthCheck("/healthz/ready")
    .WithHttpHealthCheck("/healthz/live")
    .WithReference(postgre)
    .WaitFor(postgre)
    .WaitForCompletion(migrator);

await builder.Build().RunAsync();