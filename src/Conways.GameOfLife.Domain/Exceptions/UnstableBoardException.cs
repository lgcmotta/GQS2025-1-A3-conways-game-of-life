namespace Conways.GameOfLife.Domain.Exceptions;

public class UnstableBoardException(string boardId, int maxAttempts)
    : Exception($"Board with id '{boardId}' failed to reach stable state after {maxAttempts} attempts");