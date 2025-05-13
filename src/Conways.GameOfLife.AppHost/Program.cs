var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres("postgres")
    .WithDataVolume("postgres_data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .AddDatabase("GameOfLife");

builder.AddProject<Projects.Conways_GameOfLife_API>("conways-gameoflife-api")
    .WithReference(postgre)
    .WaitFor(postgre)
    .WithHttpHealthCheck("/healthz/ready")
    .WithHttpHealthCheck("/healthz/live");

await builder.Build().RunAsync();