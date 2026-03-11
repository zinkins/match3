using Match3.Core.Localization;
using Match3.Core.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace Match3.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    public class Match3Game : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;
        private MonoGameCanvas canvas = null!;
        private bool wasPrimaryDown;

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public Match3Game()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            IsMouseVisible = true;

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";

            // Configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            canvas = new MonoGameCanvas(GraphicsDevice, Content);
            base.LoadContent();
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for game updates.
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Services.GetService(typeof(IGameScreenHost)) is IGameScreenHost screenHost)
            {
                screenHost.Update(gameTime.ElapsedGameTime, ReadInputState());
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for rendering.
        /// </param>
        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the MonoGame orange color before drawing.
            GraphicsDevice.Clear(Color.MonoGameOrange);

            if (canvas is not null &&
                Services.GetService(typeof(IGameScreenHost)) is IGameScreenHost screenHost)
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

        private Runtime.InputState ReadInputState()
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

            var inputState = new Runtime.InputState(
                hasTouch || IsDesktop,
                new System.Numerics.Vector2(pointerPosition.X, pointerPosition.Y),
                isPrimaryDown,
                wasPrimaryDown,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);

            wasPrimaryDown = isPrimaryDown;
            return inputState;
        }
    }
}
