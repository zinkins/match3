# Project Class Diagram

This diagram captures the main runtime classes and the most important dependencies between `Core`, `GameFlow`, `Presentation`, and platform composition.

```mermaid
classDiagram
direction LR

namespace Match3.DesktopGL {
    class DesktopCompositionRoot {
        +CreateGame() Match3Game
    }
}

namespace Match3.Core {
    class Match3Game {
        -graphicsDeviceManager
        -canvas
        +Update(gameTime)
        +Draw(gameTime)
    }
}

namespace Match3.Core.Runtime {
    class IGameScreenHost {
        <<interface>>
        +Update(elapsed, inputState)
        +Draw(canvas)
    }
}

namespace Match3.Presentation.Composition {
    class GameFlowCompositionRoot {
        +CreateScreenFlowController() ScreenFlowController
        +CreateScreenHost(flowController) IGameScreenHost
    }
}

namespace Match3.Presentation.Screens {
    class PresentationScreenHost {
        -flowController
        -renderer
        +Update(elapsed, inputState)
        +Draw(canvas)
    }

    class ScreenFlowController {
        -layoutCalculator
        +MainMenu
        +Gameplay
        +CurrentScreen
    }

    class GameplayScreen {
        +Presenter
        +Board
        +VisualBoard
        +BoardInputHandler
        +AnimationPlayer
        +TurnAnimationBuilder
        +BoardRenderer
        +HudRenderer
        +BoardTransform
        +BoardViewState
    }
}

namespace Match3.Presentation {
    class GameplayPresenter {
        -turnProcessor
        -stateMachine
        -session
        +Score
        +ProcessMove(board, move) TurnPipelineResult
    }
}

namespace Match3.Presentation.Input {
    class BoardInputHandler {
        -boardTransform
        -selectionController
        +HandleClick(worldPosition) Move?
    }
}

namespace Match3.Presentation.Rendering {
    class SpriteBatchRenderer
    class BoardRenderer {
        +BuildSnapshot(board, transform) BoardRenderSnapshot
    }
    class HudRenderer
    class BoardTransform {
        +CellSize
        +Origin
        +GridToWorld(gridPosition) Vector2
        +TryWorldToGrid(worldPosition, gridPosition) bool
    }
}

namespace Match3.Presentation.Animation {
    class ITurnAnimationBuilder {
        <<interface>>
        +Build(context) IAnimation
    }

    class TurnAnimationBuilder {
        +Build(context) IAnimation
    }
}

namespace Match3.Presentation.Animation.Engine {
    class AnimationPlayer {
        -activeAnimations
        -reservedBindings
        +Play(animation, conflictPolicy) AnimationHandle
        +Update(deltaTime)
    }

    class BoardViewState {
        -nodesById
        -nodeIdsByCell
        -effectNodesById
        +PieceNodes
        +EffectNodes
    }

    class IAnimation {
        <<interface>>
        +Update(deltaTime)
        +IsCompleted
        +BlocksInput
    }
}

namespace Match3.Core.GameFlow.Pipeline {
    class TurnProcessor {
        -matchFinder
        -gravityResolver
        -refillResolver
        -scoreCalculator
        -bonusFactory
        -bonusActivationResolver
        +ProcessTurnPipelineWithEvents(board, move, session, stateMachine, currentScore) TurnPipelineResult
    }

    class TurnPipelineResult {
        +IsSwapApplied
        +Events
        +CascadeSteps
    }
}

namespace Match3.Core.GameFlow.Sessions {
    class GameSession {
        +RemainingTime
        +IsGameOver
        +CanAcceptInput
        +UpdateTimer(elapsed)
    }

    class SelectionController {
        +SelectedCell
        +Select(position) Move?
    }
}

namespace Match3.Core.GameFlow.StateMachine {
    class GameplayStateMachine {
        +State
        +AdvanceAfterPhase(session)
        +TransitionToIdle()
        +TransitionToGameOver()
    }
}

namespace Match3.Core.GameCore.Board {
    class BoardState {
        -cells
        +Width
        +Height
        +GetContent(position) CellContent
        +SetContent(position, content)
        +Clone() BoardState
    }

    class CellContent {
        +PieceType
        +Bonus
        +IsFreshBonus
    }

    class MatchFinder {
        +FindMatches(board) IReadOnlyList~MatchGroup~
    }

    class GravityResolver {
        +Apply(board)
    }

    class RefillResolver {
        +Refill(board)
    }

    class ScoreCalculator {
        +AddScore(currentScore, destroyedPieces) int
    }

    class BoardGenerator {
        +Generate() BoardState
    }

    class MatchGroup {
        +PieceType
        +Positions
    }
}

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

namespace Match3.Core.GameCore.Bonuses {
    class BonusFactory {
        +Create(groups, lastMovedCell) BonusToken
    }

    class BonusActivationResolver {
        -lineBehavior
        -bombBehavior
        +Resolve(board, rootBonus) BonusActivationResult
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

    class LineBonusBehavior {
        +Activate(bonus, board) Destroyer
    }

    class BombBonusBehavior {
        +Activate(bonus, board) ExplosionResult
    }
}

DesktopCompositionRoot ..> GameFlowCompositionRoot : uses
DesktopCompositionRoot ..> Match3Game : creates
GameFlowCompositionRoot ..> ScreenFlowController : creates
GameFlowCompositionRoot ..> PresentationScreenHost : creates
Match3Game ..> IGameScreenHost : uses service
PresentationScreenHost ..|> IGameScreenHost
PresentationScreenHost *-- ScreenFlowController
PresentationScreenHost *-- SpriteBatchRenderer
ScreenFlowController *-- GameplayScreen
ScreenFlowController ..> BoardGenerator : creates board
GameplayScreen *-- GameplayPresenter
GameplayScreen *-- BoardState
GameplayScreen *-- BoardInputHandler
GameplayScreen *-- AnimationPlayer
GameplayScreen *-- ITurnAnimationBuilder
GameplayScreen *-- BoardRenderer
GameplayScreen *-- HudRenderer
GameplayScreen *-- BoardTransform
GameplayScreen *-- BoardViewState
GameplayPresenter *-- TurnProcessor
GameplayPresenter *-- GameplayStateMachine
GameplayPresenter *-- GameSession
GameplayPresenter ..> TurnPipelineResult : returns
BoardInputHandler *-- SelectionController
BoardInputHandler *-- BoardTransform
SelectionController ..> Move : creates
SelectionController ..> GridPosition : uses
TurnAnimationBuilder ..|> ITurnAnimationBuilder
TurnAnimationBuilder ..> IAnimation : builds
AnimationPlayer o-- IAnimation : plays
SpriteBatchRenderer ..> GameplayScreen : draws
BoardRenderer ..> BoardState : snapshots
BoardRenderer ..> BoardTransform : projects
BoardViewState o-- GridPosition : maps by cell
TurnProcessor *-- MatchFinder
TurnProcessor *-- GravityResolver
TurnProcessor *-- RefillResolver
TurnProcessor *-- ScoreCalculator
TurnProcessor *-- BonusFactory
TurnProcessor *-- BonusActivationResolver
TurnProcessor ..> BoardState : mutates
TurnProcessor ..> Move : applies
TurnProcessor ..> GameSession : checks timer
TurnProcessor ..> GameplayStateMachine : drives phases
TurnProcessor ..> TurnPipelineResult : returns
BoardState *-- CellContent
BoardState ..> GridPosition : indexes by
CellContent o-- BonusToken
MatchFinder ..> BoardState : scans
MatchFinder ..> MatchGroup : returns
MatchGroup o-- GridPosition
BonusFactory ..> MatchGroup : analyzes
BonusFactory ..> BonusToken : creates
BonusActivationResolver *-- LineBonusBehavior
BonusActivationResolver *-- BombBonusBehavior
BonusActivationResolver ..> BonusToken : resolves
BonusActivationResolver ..> BoardState : mutates
BonusToken <|-- LineBonus
BonusToken <|-- BombBonus
LineBonusBehavior ..> LineBonus : activates
LineBonusBehavior ..> BoardState : mutates
BombBonusBehavior ..> BombBonus : activates
BombBonusBehavior ..> BoardState : mutates
Move o-- GridPosition
BonusToken o-- GridPosition
```
