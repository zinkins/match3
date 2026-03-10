using System;
using System.Collections.Generic;

namespace Match3.Core.GameCore.Pieces;

public static class PieceCatalog
{
    public static IReadOnlyList<PieceType> All { get; } =
    [
        PieceType.Red,
        PieceType.Green,
        PieceType.Blue,
        PieceType.Yellow,
        PieceType.Purple
    ];

    public static PieceColor GetColor(PieceType type)
    {
        return type switch
        {
            PieceType.Red => PieceColor.Red,
            PieceType.Green => PieceColor.Green,
            PieceType.Blue => PieceColor.Blue,
            PieceType.Yellow => PieceColor.Yellow,
            PieceType.Purple => PieceColor.Purple,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown piece type.")
        };
    }
}
