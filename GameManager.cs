using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

public static class GameManager
{
    public static TileMap TileMap { get; private set; }

    public static void InitializeTileMap(GraphicsDevice graphicsDevice)
    {
        TileMap = new TileMap(
            64,
            64, AssetManager.TileTextures
        );

        TileMap.Generate(
            64,
            64,
            8,
            0.08f);

    }
    public static void InitializeManagers(GraphicsDevice graphicsDevice)
    {
    }
}