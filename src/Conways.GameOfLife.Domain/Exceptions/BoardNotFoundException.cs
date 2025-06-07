namespace Conways.GameOfLife.Domain.Exceptions;

public class BoardNotFoundException(string boardId)
    : Exception($"Board with Id '{boardId}' was not found");