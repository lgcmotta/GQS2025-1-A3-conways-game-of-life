var builder = DistributedApplication.CreateBuilder(args);

var postgre = builder.AddPostgres("Postgres")
    .WithPgAdmin()
    .AddDatabase("ConwaysGameOfLife");

builder.AddProject<Projects.Conways_GameOfLife_API>("gameoflife-api")
    .WithReference(postgre)
    .WaitFor(postgre);

await builder.Build().RunAsync();