namespace Conways.GameOfLife.IntegrationTests.Features.CreateBoard;

public class CreateBoardTests
{
    private readonly ConwaysGameOfLifeWebApplicationFactory _factory;

    public CreateBoardTests(ConwaysGameOfLifeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBoard_WhenBoardIsValidSize_ShouldSaveAndEncodeBoardId()
    {
        // Arrange
        var firstGeneration = new bool[][] { [false, true, false], [true, true, false], [false, false, false] };

        using var scope = _factory.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        var response =
            await mediator.Send(new CreateBoardCommand(firstGeneration), TestContext.Current.CancellationToken);

        // Assert
        response.BoardId.Should().NotBeEmpty();
        response.BoardId.Should().HaveLength(11);
    }

    [Fact]
    public async Task CreateBoard_WhenBoardIsNot3x3Matrix_ShouldThrowValidationFailedException()
    {
        // Arrange
        var firstGeneration = new bool[][] { [false, true], [true, true, false], [false, false] };

        using var scope = _factory.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        async Task SendCommand() =>
            await mediator.Send(new CreateBoardCommand(firstGeneration), TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(SendCommand);
    }

    [Fact]
    public async Task CreateBoard_WhenUploadingBoardUsingAPI_ShouldRespondWithEncodedBoardId()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = _factory.Server.BaseAddress
        });

        var firstGeneration = new bool[][] { [false, true, false], [true, true, false], [false, false, false] };

        var content = JsonContent.Create(new CreateBoardCommand(firstGeneration), new MediaTypeHeaderValue(MediaTypeNames.Application.Json, "utf-8"), JsonSerializerOptions.Web);

        // Act
        var response = await client.PostAsync("/api/v1/boards", content, TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<CreateBoardResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        body.Should().NotBeNull();
        body!.BoardId.Should().NotBeEmpty();
        body.BoardId.Should().HaveLength(11);
    }
}