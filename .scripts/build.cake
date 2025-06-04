///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var solutionRoot = MakeAbsolute(Directory("../"));

string startupProject = solutionRoot
    .Combine("src")
    .Combine("Conways.GameOfLife.API")
    .CombineWithFilePath("Conways.GameOfLife.API.csproj")
    .FullPath;


string infrastructureProject = solutionRoot
    .Combine("src")
    .Combine("Conways.GameOfLife.Infrastructure")
    .CombineWithFilePath("Conways.GameOfLife.Infrastructure.csproj")
    .FullPath;

var dbContextName = "BoardDbContext";
var connectionString = EnvironmentVariable("ConnectionStrings__GameOfLife");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("Does nothing. Please specify --target.")
    .Does(() =>
{
    Information("Specify --target to run a provided task.");
});

Task("Ensure-Connection-String")
    .Description("Ensures that the connection string is set in the environment variables.")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Error("The environment variable 'ConnectionStrings__GameOfLife' is not set. Please set it to your database connection string.");
        throw new CakeException("Connection string is not set.");
    }

    Information("Got connection string from environment variable: 'ConnectionStrings__GameOfLife'");
});

Task("Generate-Migrations-Bundle")
    .Description("Creates a dotnet-ef bundle to apply migrations.")
    .IsDependentOn("Ensure-Connection-String")
    .Does(() =>
{
    var bundleFolder = "./.bundle";

    var bundleExecutableName = "efbundle";

    if (IsRunningOnWindows())
    {
        bundleExecutableName += ".exe";
    }

    if (!DirectoryExists(bundleFolder))
    {
        CreateDirectory(bundleFolder);
    }

    Information("Cleaning content of dotnet-ef bundle...");

    CleanDirectory(bundleFolder, new CleanDirectorySettings { Force = true });

    var bundlePath = $"{bundleFolder}/{bundleExecutableName}";

    var args = new ProcessArgumentBuilder()
        .Append("ef")
        .Append("migrations")
        .Append("bundle")
        .Append("--no-build")
        .Append("--startup-project")
        .AppendQuoted(startupProject)
        .Append("--project")
        .AppendQuoted(infrastructureProject)
        .Append("--context")
        .AppendQuoted(dbContextName)
        .Append("--output")
        .AppendQuoted(bundlePath)
        .Append("--verbose")
        .Append("--self-contained");

    var settings = new ProcessSettings
    {
        Arguments = args,
        RedirectStandardOutput = false,
        RedirectStandardError = false,
    };

    Information("Creating dotnet-ef bundle at “{0}”", bundlePath);

    StartProcess("dotnet", settings);
});

Task("Apply-Migrations-Bundle")
    .Description("Applies migrations to the database using the dotnet-ef bundle.")
    .IsDependentOn("Generate-Migrations-Bundle")
    .Does(() =>
{
    var bundleFolder = "./.bundle";
    var bundleExecutableName = "efbundle";

    if (IsRunningOnWindows())
    {
        bundleExecutableName += ".exe";
    }

    var bundlePath = $"{bundleFolder}/{bundleExecutableName}";

    if (!FileExists(bundlePath))
    {
        throw new CakeException($"Cannot find the migrations bundle at '{bundlePath}'. Did the Bundle task run successfully?");
    }

    var args = new ProcessArgumentBuilder()
        .Append("--verbose")
        .Append("--connection")
        .AppendQuoted(connectionString);

    var settings = new ProcessSettings
    {
        Arguments = args,
    };

    Information("Executing dotnet-ef bundle at “{0}”", bundlePath);

    StartProcess(bundlePath, settings);
});

///////////////////////////////////////////////////////////////////////////////
// RUN TARGET
///////////////////////////////////////////////////////////////////////////////

RunTarget(Argument("target", "Default"));
