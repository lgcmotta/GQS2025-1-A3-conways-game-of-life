using Conways.GameOfLife.Domain;
using Conways.GameOfLife.Domain.Exceptions;
using Conways.GameOfLife.Infrastructure.Extensions;
using Conways.GameOfLife.Infrastructure;
using Conways.GameOfLife.Infrastructure.Persistence;
using HashidsNet;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conways.GameOfLife.API.Features.NextGeneration;

public class NextGenerationQueryHandler : IRequestHandler<NextGenerationQuery, NextGenerationResponse>
{
    private readonly BoardDbContextReadOnly _context;
    private readonly IHashids _hashids;

    public NextGenerationQueryHandler(BoardDbContextReadOnly context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<NextGenerationResponse> Handle(NextGenerationQuery request, CancellationToken cancellationToken)
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

        var nextGen = board.NextGeneration();

        var stable = board.HasReachedStableState(nextGen);

        return new NextGenerationResponse(request.BoardId, nextGen.ToMatrix(), stable);
    }
}