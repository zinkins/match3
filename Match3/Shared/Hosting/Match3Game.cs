using System;
using Match3.Core.Localization;
using Match3.Presentation.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Match3.Platform.Hosting;

public sealed class Match3Game : Game
{
    private readonly GraphicsDeviceManager graphicsDeviceManager;
    private MonoGameCanvas canvas = null!;
    private bool wasPrimaryDown;

    public Match3Game()
    {
        graphicsDeviceManager = new GraphicsDeviceManager(this);
        IsMouseVisible = true;

        Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);
        Content.RootDirectory = "Content";

        graphicsDeviceManager.SupportedOrientations = IsMobile()
            ? DisplayOrientation.Portrait
            : DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
    }

    protected override void Initialize()
    {
        base.Initialize();
        LocalizationManager.SetCulture(LocalizationManager.DEFAULT_CULTURE_CODE);
    }

    protected override void LoadContent()
    {
        canvas = new MonoGameCanvas(GraphicsDevice, Content);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (Services.GetService(typeof(IGameScreenHost)) is IGameScreenHost screenHost)
        {
            screenHost.Update(gameTime.ElapsedGameTime, ReadInputState());
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.MonoGameOrange);

        if (Services.GetService(typeof(IGameScreenHost)) is IGameScreenHost screenHost)
        {
            canvas.Begin();
            screenHost.Draw(canvas);
            canvas.End();
        }

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            canvas?.Dispose();
        }

        base.Dispose(disposing);
    }

    private InputState ReadInputState()
    {
        var mouseState = Mouse.GetState();
        var touchCollection = TouchPanel.GetState();

        var hasTouch = touchCollection.Count > 0;
        var pointerPosition = hasTouch
            ? touchCollection[0].Position
            : new Microsoft.Xna.Framework.Vector2(mouseState.X, mouseState.Y);
        var isPrimaryDown = hasTouch
            ? touchCollection[0].State != TouchLocationState.Released
            : mouseState.LeftButton == ButtonState.Pressed;

        var inputState = new InputState(
            hasTouch || IsDesktop(),
            new System.Numerics.Vector2(pointerPosition.X, pointerPosition.Y),
            isPrimaryDown,
            wasPrimaryDown,
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height);

        wasPrimaryDown = isPrimaryDown;
        return inputState;
    }

    private static bool IsDesktop()
    {
        return OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();
    }

    private static bool IsMobile()
    {
        return OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
    }
}
