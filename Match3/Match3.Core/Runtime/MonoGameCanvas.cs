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
    private readonly Texture2D circle;
    private readonly Texture2D diamond;
    private readonly SpriteFont font;

    public MonoGameCanvas(GraphicsDevice graphicsDevice, ContentManager content)
    {
        this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        spriteBatch = new SpriteBatch(graphicsDevice);
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData([Color.White]);
        circle = CreateCircleTexture(graphicsDevice, 64);
        diamond = CreateDiamondTexture(graphicsDevice, 64);
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

    public void DrawShape(string shape, float x, float y, float width, float height, string tint, float rotationRadians = 0f)
    {
        var texture = pixel;
        var rotation = rotationRadians;

        if (shape == "Circle")
        {
            texture = circle;
        }
        else if (shape == "Diamond")
        {
            texture = diamond;
        }

        var center = new Vector2(x + (width / 2f), y + (height / 2f));
        var scale = new Vector2(width / texture.Width, height / texture.Height);
        var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        spriteBatch.Draw(texture, center, null, ToColor(tint), rotation, origin, scale, SpriteEffects.None, 0f);
    }

    public void DrawText(string text, float x, float y, string tint)
    {
        spriteBatch.DrawString(font, text, new Vector2(x, y), ToColor(tint));
    }

    public void Dispose()
    {
        circle.Dispose();
        diamond.Dispose();
        pixel.Dispose();
        spriteBatch.Dispose();
    }

    private static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter)
    {
        var texture = new Texture2D(graphicsDevice, diameter, diameter);
        var data = new Color[diameter * diameter];
        var radius = diameter / 2f;
        var center = new Vector2(radius, radius);

        for (var y = 0; y < diameter; y++)
        {
            for (var x = 0; x < diameter; x++)
            {
                var index = (y * diameter) + x;
                data[index] = Vector2.Distance(new Vector2(x, y), center) <= radius
                    ? Color.White
                    : Color.Transparent;
            }
        }

        texture.SetData(data);
        return texture;
    }

    private static Texture2D CreateDiamondTexture(GraphicsDevice graphicsDevice, int size)
    {
        var texture = new Texture2D(graphicsDevice, size, size);
        var data = new Color[size * size];
        var half = (size - 1) / 2f;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var index = (y * size) + x;
                var dx = MathF.Abs(x - half) / half;
                var dy = MathF.Abs(y - half) / half;
                data[index] = dx + dy <= 1f
                    ? Color.White
                    : Color.Transparent;
            }
        }

        texture.SetData(data);
        return texture;
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