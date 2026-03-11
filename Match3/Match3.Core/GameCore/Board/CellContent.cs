using Match3.Core.GameCore.Bonuses;
using Match3.Core.GameCore.Pieces;

namespace Match3.Core.GameCore.Board;

public sealed record CellContent(PieceType PieceType, BonusToken Bonus = null);
