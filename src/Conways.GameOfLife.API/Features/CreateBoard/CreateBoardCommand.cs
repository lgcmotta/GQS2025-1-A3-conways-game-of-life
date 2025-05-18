using Conways.GameOfLife.Domain.Core;
using MediatR;

namespace Conways.GameOfLife.API.Features.CreateBoard;

public record GenerationDto(List<List<bool>> Cells);

public record CreateBoardCommand(GenerationDto FirstGeneration) : IRequest<CreateBoardResponse>, ICommand;
