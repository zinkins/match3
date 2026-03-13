# Game Core Class Diagram

This diagram focuses on the pure gameplay simulation in `Match3.Core.GameCore`: board state, value objects, piece catalog, random source, matching, refill, gravity, and bonuses.

```mermaid
classDiagram
direction LR

namespace Match3.Core.GameCore.ValueObjects {
    class GridPosition {
        <<struct>>
        +Row
        +Column
        +IsAdjacentTo(other) bool
    }

    class Move {
        <<struct>>
        +From
        +To
    }
}

namespace Match3.Core.GameCore.Pieces {
    class Piece {
        +Type
    }

    class PieceType {
        <<enum>>
    }

    class PieceColor {
        <<enum>>
    }

    class PieceCatalog {
        <<static>>
        +All
        +GetColor(pieceType) PieceColor
    }
}

namespace Match3.Core.GameCore.Board {
    class BoardState {
        -cells
        +Width
        +Height
        +GetCell(position) PieceType?
        +SetCell(position, pieceType)
        +GetContent(position) CellContent
        +SetContent(position, content)
        +GetBonus(position) BonusToken
        +SetBonus(position, bonus)
        +Clone() BoardState
    }

    class CellContent {
        +PieceType
        +Bonus
        +IsFreshBonus
    }

    class MatchGroup {
        +PieceType
        +Positions
    }

    class MoveValidator {
        +IsValid(move) bool
    }

    class MatchFinder {
        +FindMatches(board) IReadOnlyList~MatchGroup~
    }

    class GravityResolver {
        +Apply(board)
    }

    class RefillResolver {
        -randomSource
        +Refill(board)
    }

    class BoardGenerator {
        -randomSource
        +Generate() BoardState
    }

    class ScoreCalculator {
        +AddScore(currentScore, destroyedPieces) int
    }

    class IRandomSource {
        <<interface>>
        +Next(minInclusive, maxExclusive) int
    }

    class SystemRandomSource {
        -random
        +Next(minInclusive, maxExclusive) int
    }
}

namespace Match3.Core.GameCore.Bonuses {
    class BonusKind {
        <<enum>>
    }

    class LineOrientation {
        <<enum>>
    }

    class BonusToken {
        <<abstract>>
        +Kind
        +Position
        +Color
    }

    class LineBonus {
        +Orientation
    }

    class BombBonus {
        +Radius
    }

    class BonusFactory {
        +Create(groups, lastMovedCell) BonusToken
    }

    class BonusActivationResolver {
        -lineBehavior
        -bombBehavior
        +Resolve(board, rootBonus) BonusActivationResult
    }

    class LineBonusBehavior {
        +Activate(bonus, board) Destroyer
    }

    class BombBonusBehavior {
        +Activate(bonus, board) ExplosionResult
    }

    class Destroyer {
        +Path
        +DestroyedPositions
        +ActivatedBonuses
    }

    class ExplosionResult {
        +AffectedArea
        +DestroyedPositions
        +ActivatedBonuses
    }

    class BonusActivationResult {
        +ActivatedBonuses
        +DestroyedPositions
    }
}

Move o-- GridPosition
BoardState *-- CellContent
BoardState ..> GridPosition : indexed by
CellContent o-- BonusToken
CellContent ..> PieceType
MatchGroup o-- GridPosition
MatchGroup ..> PieceType
MoveValidator ..> Move : validates
MatchFinder ..> BoardState : scans
MatchFinder ..> MatchGroup : returns
GravityResolver ..> BoardState : compacts
RefillResolver *-- IRandomSource
RefillResolver ..> BoardState : fills
RefillResolver ..> PieceCatalog : chooses pieces
BoardGenerator *-- IRandomSource
BoardGenerator ..> BoardState : creates
BoardGenerator ..> PieceCatalog : chooses pieces
SystemRandomSource ..|> IRandomSource
Piece ..> PieceType
PieceCatalog ..> PieceType : lists
PieceCatalog ..> PieceColor : maps
BonusToken ..> BonusKind
BonusToken o-- GridPosition
BonusToken ..> PieceColor
BonusToken <|-- LineBonus
BonusToken <|-- BombBonus
LineBonus ..> LineOrientation
BonusFactory ..> MatchGroup : analyzes
BonusFactory ..> BonusToken : creates
BonusFactory ..> PieceCatalog : maps color
BonusActivationResolver *-- LineBonusBehavior
BonusActivationResolver *-- BombBonusBehavior
BonusActivationResolver ..> BoardState : mutates
BonusActivationResolver ..> BonusToken : resolves
LineBonusBehavior ..> LineBonus : activates
LineBonusBehavior ..> BoardState : mutates
LineBonusBehavior ..> Destroyer : returns
BombBonusBehavior ..> BombBonus : activates
BombBonusBehavior ..> BoardState : mutates
BombBonusBehavior ..> ExplosionResult : returns
Destroyer o-- GridPosition
ExplosionResult o-- GridPosition
BonusActivationResult o-- BonusToken
BonusActivationResult o-- GridPosition
```
