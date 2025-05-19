var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume("postgres_data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .AddDatabase("GameOfLife");

builder.AddProject<Projects.Conways_GameOfLife_API>("conways-gameoflife-api")
    .WithHttpHealthCheck("/healthz/ready")
    .WithHttpHealthCheck("/healthz/live")
    .WithReference(postgre)
    .WaitFor(postgre);

await builder.Build().RunAsync();