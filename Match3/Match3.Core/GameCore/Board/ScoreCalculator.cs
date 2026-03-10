namespace Match3.Core.GameCore.Board;

public sealed class ScoreCalculator
{
    private const int PointsPerDestroyedPiece = 10;

    public int AddScore(int currentScore, int destroyedPieces)
    {
        return currentScore + (destroyedPieces * PointsPerDestroyedPiece);
    }
}
