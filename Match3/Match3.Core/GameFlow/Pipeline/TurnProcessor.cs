using System;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameFlow.Pipeline;

public sealed class TurnProcessor
{
    private readonly MatchFinder matchFinder;

    public TurnProcessor()
        : this(new MatchFinder())
    {
    }

    public TurnProcessor(MatchFinder matchFinder)
    {
        this.matchFinder = matchFinder ?? throw new ArgumentNullException(nameof(matchFinder));
    }

    public bool TryProcessMove(BoardState board, Move move)
    {
        Swap(board, move);

        var matches = matchFinder.FindMatches(board);
        if (matches.Count > 0)
        {
            return true;
        }

        Swap(board, move);
        return false;
    }

    private static void Swap(BoardState board, Move move)
    {
        var from = board.GetCell(move.From);
        var to = board.GetCell(move.To);
        board.SetCell(move.From, to);
        board.SetCell(move.To, from);
    }
}
