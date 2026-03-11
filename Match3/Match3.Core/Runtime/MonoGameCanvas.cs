using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Match3.Core.Runtime;

public sealed class MonoGameCanvas : IGameCanvas, IDisposable
{
    private readonly GraphicsDevice graphicsDevice;
    private readonly SpriteBatch spriteBatch;
    private readonly Texture2D pixel;
    private readonly SpriteFont font;

    public MonoGameCanvas(GraphicsDevice graphicsDevice, ContentManager content)
    {
        this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        spriteBatch = new SpriteBatch(graphicsDevice);
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData([Color.White]);
        font = content.Load<SpriteFont>("Fonts/Hud");
    }

    public int ViewportWidth => graphicsDevice.Viewport.Width;

    public int ViewportHeight => graphicsDevice.Viewport.Height;

    public void Begin()
    {
        spriteBatch.Begin();
    }

    public void End()
    {
        spriteBatch.End();
    }

    public void DrawFilledRectangle(float x, float y, float width, float height, string tint)
    {
        spriteBatch.Draw(pixel, new Rectangle((int)x, (int)y, (int)width, (int)height), ToColor(tint));
    }

    public void DrawText(string text, float x, float y, string tint)
    {
        spriteBatch.DrawString(font, text, new Vector2(x, y), ToColor(tint));
    }

    public void Dispose()
    {
        pixel.Dispose();
        spriteBatch.Dispose();
    }

    private static Color ToColor(string tint)
    {
        return tint switch
        {
            "Red" => Color.IndianRed,
            "Green" => Color.ForestGreen,
            "Blue" => Color.CornflowerBlue,
            "Yellow" => Color.Goldenrod,
            "Purple" => Color.MediumPurple,
            "White" => Color.White,
            "Black" => Color.Black,
            "DarkGray" => Color.DarkSlateGray,
            "LightGray" => Color.LightGray,
            "Orange" => Color.Orange,
            _ => Color.White
        };
    }
}
