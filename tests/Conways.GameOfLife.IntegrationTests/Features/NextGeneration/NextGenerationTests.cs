namespace Conways.GameOfLife.IntegrationTests.Features.NextGeneration;

public class NextGenerationTests
{
    private readonly ConwaysGameOfLifeWebApplicationFactory _factory;

    public NextGenerationTests(ConwaysGameOfLifeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> SeedBoard(bool[,] firstGeneration)
    {
        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<BoardDbContext>();
        var hashIds = scope.ServiceProvider.GetRequiredService<IHashids>();

        var board = new Board(firstGeneration);

        await context.AddAsync(board);

        await context.SaveChangesAsync();

        return hashIds.EncodeLong(board.Id);
    }

    public static TheoryData<string?> GetBoardIdsForValidationFailedException()
    {
        return
        [
            null!,
            string.Empty
        ];
    }

    [Theory]
    [MemberData(nameof(GetBoardIdsForValidationFailedException))]
    public async Task NextGeneration_WhenBoardIdIsNullOrEmpty_ShouldThrowValidationFailedException(string? boardId)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        async Task RequestQuery() =>
            await mediator.Send(new NextGenerationQuery(boardId!), TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(RequestQuery);
    }

    [Fact]
    public async Task NextGeneration_WhenBoardDoesNotExists_ShouldThrowBoardNotFoundException()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var hashIds = scope.ServiceProvider.GetRequiredService<IHashids>();

        var boardId = hashIds.EncodeLong(1234);

        // Act
        async Task RequestQuery() =>
            await mediator.Send(new NextGenerationQuery(boardId), TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<BoardNotFoundException>(RequestQuery);
    }

    [Fact]
    public async Task NextGeneration_WhenBoardExists_ShouldReturnNextGenerationResponse()
    {
        // Arrange
        var firstGeneration = new[,] { { true, true, false }, { false, true, false }, { false, false, false } };

        var expectedNextState = new bool[][] { [true, true, false], [true, true, false], [false, false, false] };

        var boardId = await SeedBoard(firstGeneration);

        using var scope = _factory.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Act
        var response = await mediator.Send(new NextGenerationQuery(boardId), TestContext.Current.CancellationToken);

        // Assert
        response.Generation.Should().BeEquivalentTo(expectedNextState);
    }

    [Fact]
    public async Task NextGeneration_WhenRequestingUsingAPI_ShouldRespondWithExpectedNextGeneration()
    {
        // Arrange
        var firstGeneration = new[,] { { true, true, false }, { false, true, false }, { false, false, false } };

        var expectedNextState = new bool[][] { [true, true, false], [true, true, false], [false, false, false] };

        var boardId = await SeedBoard(firstGeneration);

        var client = _factory.CreateHttpClient();

        // Act
        var response = await client.GetAsync($"/api/v1/boards/{boardId}/generations/next",
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<NextGenerationResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        body.Should().NotBeNull();
        body.Generation.Should().BeEquivalentTo(expectedNextState);
    }

    [Fact]
    public async Task NextGeneration_WhenBoardDoesNotExistsRequestingUsingAPI_ShouldRespondNotFound()
    {
        // Arrange
        var client = _factory.CreateHttpClient();

        using var scope = _factory.Services.CreateScope();

        var hashIds = scope.ServiceProvider.GetRequiredService<IHashids>();

        var boardId = hashIds.EncodeLong(1234);

        // Act
        var response = await client.GetAsync($"/api/v1/boards/{boardId}/generations/next",
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<TracedProblemDetails>(
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().NotBeNull();
        body.Detail.Should().Contain($"Board with Id '{boardId}' was not found");
    }
}