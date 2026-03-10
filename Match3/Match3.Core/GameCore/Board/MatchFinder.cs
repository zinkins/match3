using System.Collections.Generic;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class MatchFinder
{
    public IReadOnlyList<MatchGroup> FindMatches(BoardState board)
    {
        var groups = new List<MatchGroup>();
        groups.AddRange(FindHorizontal(board));
        groups.AddRange(FindVertical(board));
        return groups;
    }

    private static IEnumerable<MatchGroup> FindHorizontal(BoardState board)
    {
        for (var row = 0; row < board.Height; row++)
        {
            var runStartColumn = 0;

            while (runStartColumn < board.Width)
            {
                var piece = board.GetCell(new GridPosition(row, runStartColumn));
                if (piece is null)
                {
                    runStartColumn++;
                    continue;
                }

                var runEndColumn = runStartColumn + 1;
                while (runEndColumn < board.Width &&
                       board.GetCell(new GridPosition(row, runEndColumn)) == piece)
                {
                    runEndColumn++;
                }

                var runLength = runEndColumn - runStartColumn;
                if (runLength >= 3)
                {
                    var positions = new List<GridPosition>(runLength);
                    for (var column = runStartColumn; column < runEndColumn; column++)
                    {
                        positions.Add(new GridPosition(row, column));
                    }

                    yield return new MatchGroup(piece.Value, positions);
                }

                runStartColumn = runEndColumn;
            }
        }
    }

    private static IEnumerable<MatchGroup> FindVertical(BoardState board)
    {
        for (var column = 0; column < board.Width; column++)
        {
            var runStartRow = 0;

            while (runStartRow < board.Height)
            {
                var piece = board.GetCell(new GridPosition(runStartRow, column));
                if (piece is null)
                {
                    runStartRow++;
                    continue;
                }

                var runEndRow = runStartRow + 1;
                while (runEndRow < board.Height &&
                       board.GetCell(new GridPosition(runEndRow, column)) == piece)
                {
                    runEndRow++;
                }

                var runLength = runEndRow - runStartRow;
                if (runLength >= 3)
                {
                    var positions = new List<GridPosition>(runLength);
                    for (var row = runStartRow; row < runEndRow; row++)
                    {
                        positions.Add(new GridPosition(row, column));
                    }

                    yield return new MatchGroup(piece.Value, positions);
                }

                runStartRow = runEndRow;
            }
        }
    }
}
