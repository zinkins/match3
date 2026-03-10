using System.Collections.Generic;
using Match3.Core.GameCore.Pieces;
using Match3.Core.GameCore.ValueObjects;

namespace Match3.Core.GameCore.Board;

public sealed class MatchGroup(PieceType pieceType, IReadOnlyList<GridPosition> positions)
{
    public PieceType PieceType { get; } = pieceType;
    public IReadOnlyList<GridPosition> Positions { get; } = positions;
}
