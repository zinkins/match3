using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Match3.Core.GameCore.Board;
using Match3.Core.GameCore.ValueObjects;
using Match3.Core.GameFlow.Sessions;
using Match3.Core.GameFlow.StateMachine;
using Match3.Presentation;
using Match3.Presentation.Animation;
using Match3.Presentation.Animation.Engine;
using Match3.Presentation.Rendering;

namespace Match3.Tests;

public sealed class ArchitectureRefactorTests
{
    [Fact]
    public void CoreProject_DoesNotReferenceMonoGamePackages()
    {
        var packageReferences = LoadProjectIncludes(
            GetRepositoryPath("Match3", "Match3.Core", "Match3.Core.csproj"),
            "PackageReference");

        Assert.DoesNotContain(packageReferences, include => include.StartsWith("MonoGame", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PlatformProjects_DoNotReferenceCoreDirectly()
    {
        var platformProjects = new[]
        {
            GetRepositoryPath("Match3", "Match3.DesktopGL", "Match3.DesktopGL.csproj"),
            GetRepositoryPath("Match3", "Match3.Android", "Match3.Android.csproj"),
            GetRepositoryPath("Match3", "Match3.iOS", "Match3.iOS.csproj")
        };

        foreach (var projectPath in platformProjects)
        {
            var projectReferences = LoadProjectIncludes(projectPath, "ProjectReference");
            Assert.DoesNotContain(projectReferences, include => include.Contains("Match3.Core\\Match3.Core.csproj", System.StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void CoreAssembly_DoesNotExposeLegacyRuntimeTypes()
    {
        var coreAssembly = typeof(BoardState).Assembly;

        Assert.Null(coreAssembly.GetType("Match3.Core.Match3Game", throwOnError: false, ignoreCase: false));
        Assert.Null(coreAssembly.GetType("Match3.Core.Runtime.IGameCanvas", throwOnError: false, ignoreCase: false));
        Assert.Null(coreAssembly.GetType("Match3.Core.Runtime.IGameScreenHost", throwOnError: false, ignoreCase: false));
        Assert.Null(coreAssembly.GetType("Match3.Core.Runtime.InputState", throwOnError: false, ignoreCase: false));
    }

    [Fact]
    public void PresentationAssembly_ExposesRuntimeContracts()
    {
        var presentationAssembly = typeof(GameplayPresenter).Assembly;

        Assert.NotNull(presentationAssembly.GetType("Match3.Presentation.Runtime.IGameCanvas", throwOnError: false, ignoreCase: false));
        Assert.NotNull(presentationAssembly.GetType("Match3.Presentation.Runtime.IGameScreenHost", throwOnError: false, ignoreCase: false));
        Assert.NotNull(presentationAssembly.GetType("Match3.Presentation.Runtime.InputState", throwOnError: false, ignoreCase: false));
    }

    [Fact]
    public void ScreenFlowController_DoesNotConstructGameplayGraphInline()
    {
        var source = File.ReadAllText(GetRepositoryPath("Match3", "Match3.Presentation", "Screens", "ScreenFlowController.cs"));

        Assert.DoesNotContain("new BoardGenerator()", source);
        Assert.DoesNotContain("new TurnProcessor()", source);
        Assert.DoesNotContain("new GameplayStateMachine()", source);
    }

    [Fact]
    public void GameSession_TracksScoreThatPresenterReads()
    {
        var session = new GameSession();
        var presenter = new GameplayPresenter(new Match3.Core.GameFlow.Pipeline.TurnProcessor(), new GameplayStateMachine(), session);

        presenter.ProcessMove(CreateBoardForSwapWithMatch(), new Move(new GridPosition(0, 2), new GridPosition(1, 2)));

        var scoreProperty = typeof(GameSession).GetProperty("Score", BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(scoreProperty);

        var score = Assert.IsType<int>(scoreProperty!.GetValue(session));
        Assert.True(score > 0);
        Assert.Equal(score, presenter.Score);
    }

    [Fact]
    public void GameplayAnimationRuntime_QueueGravity_HandlesColumnsOutsideLegacyRange()
    {
        var viewState = new BoardViewState();
        var animationPlayer = new AnimationPlayer();
        var sourcePosition = new GridPosition(1, 9);
        var targetPosition = new GridPosition(4, 9);
        var sourcePiece = new RenderPiece(sourcePosition, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 10f, 20f, 32f, 32f);
        var targetPiece = new RenderPiece(targetPosition, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintRed, 10f, 116f, 32f, 32f);

        viewState.AddOrUpdate(new PieceNode(NodeId.New(), sourcePosition, new System.Numerics.Vector2(sourcePiece.X, sourcePiece.Y), new System.Numerics.Vector2(1f, 1f), 0f, 1f, sourcePiece.Tint, 0f, true));

        GameplayAnimationRuntime.QueueGravity(
            viewState,
            animationPlayer,
            new BoardRenderSnapshot([], [sourcePiece]),
            new BoardRenderSnapshot([], [targetPiece]));

        Assert.NotNull(viewState.GetPieceNode(targetPosition));
        Assert.True(animationPlayer.HasBlockingAnimations);
    }

    [Fact]
    public void GameplayAnimationRuntime_QueueSpawn_HandlesColumnsOutsideLegacyRange()
    {
        var viewState = new BoardViewState();
        var animationPlayer = new AnimationPlayer();
        var targetPosition = new GridPosition(0, 9);
        var targetPiece = new RenderPiece(targetPosition, PieceVisualConstants.ShapeSquare, PieceVisualConstants.TintBlue, 10f, 20f, 32f, 32f);

        GameplayAnimationRuntime.QueueSpawn(
            viewState,
            animationPlayer,
            new BoardRenderSnapshot([], []),
            new BoardRenderSnapshot([], [targetPiece]),
            cellSize: 32f);

        Assert.NotNull(viewState.GetPieceNode(targetPosition));
        Assert.True(animationPlayer.HasBlockingAnimations);
    }

    private static string[] LoadProjectIncludes(string projectPath, string itemName)
    {
        var document = XDocument.Load(projectPath);
        return document
            .Descendants()
            .Where(element => element.Name.LocalName == itemName)
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();
    }

    private static string GetRepositoryPath(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
            {
                return Path.Combine([current.FullName, .. parts]);
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private static BoardState CreateBoardForSwapWithMatch()
    {
        var board = new BoardState();
        var types = Match3.Core.GameCore.Pieces.PieceCatalog.All;
        for (var row = 0; row < board.Height; row++)
        {
            for (var column = 0; column < board.Width; column++)
            {
                board.SetPiece(new GridPosition(row, column), types[(row + column) % types.Count]);
            }
        }

        board.SetPiece(new GridPosition(0, 0), Match3.Core.GameCore.Pieces.PieceType.Red);
        board.SetPiece(new GridPosition(0, 1), Match3.Core.GameCore.Pieces.PieceType.Red);
        board.SetPiece(new GridPosition(0, 2), Match3.Core.GameCore.Pieces.PieceType.Blue);
        board.SetPiece(new GridPosition(1, 2), Match3.Core.GameCore.Pieces.PieceType.Red);
        return board;
    }
}
