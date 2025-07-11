using Conways.GameOfLife.Domain;
using Conways.GameOfLife.Domain.Exceptions;
using Conways.GameOfLife.Infrastructure.Extensions;
using Conways.GameOfLife.Infrastructure;
using Conways.GameOfLife.Infrastructure.Persistence;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conways.GameOfLife.API.Features.FinalGeneration;

public class FinalGenerationQueryHandler : IRequestHandler<FinalGenerationQuery, FinalGenerationResponse>
{
    private readonly BoardDbContextReadOnly _context;
    private readonly IHashids _hashids;

    public FinalGenerationQueryHandler(BoardDbContextReadOnly context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<FinalGenerationResponse> Handle(FinalGenerationQuery request, CancellationToken cancellationToken)
    {
        var boardId = _hashids.DecodeLong(request.BoardId)[0];

        var board = await _context.Set<Board>()
            .Include("_generations")
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);

        if (board is null)
        {
            throw new BoardNotFoundException(request.BoardId);
        }

        var attempts = 0;

        while (attempts < request.MaxAttempts)
        {
            var nextGeneration = board.NextGeneration();

            var stable = board.HasReachedStableState(nextGeneration);

            board.AddGeneration(board.CurrentGeneration.Number + 1, nextGeneration);

            attempts++;

            if (stable)
            {
                return new FinalGenerationResponse(request.BoardId, stable, board.CurrentGeneration.ToMatrix());
            }
        }

        throw new UnstableBoardException(request.BoardId, request.MaxAttempts);
    }
}