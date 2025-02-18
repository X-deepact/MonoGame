using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

public enum TileTypes
{
    Base, Grass, GrassSoilCornerNE,
    GrassSoilCornerNW, GrassSoilCornerSE,
    GrassSoilCornerSW, GrassSoilE,
    GrassSoilN, GrassSoilS, GrassSoilW,
    Rock, Soil,
    SoilGrassCornerNE, SoilGrassCornerNW,
    SoilGrassCornerSE, SoilGrassCornerSW,
    SoilWaterCornerNE, SoilWaterCornerNW,
    SoilWaterCornerSE, SoilWaterCornerSW,
    Water,
    WaterSoilCornerNE, WaterSoilCornerNW,
    WaterSoilCornerSE, WaterSoilCornerSW,
    WaterSoilE, WaterSoilN,WaterSoilS,
    WaterSoilW
}


public class TileMap
{
    private int[,] map;
    private Vector2[,] tilePositions;
    private Texture2D tileTextures;

    // Must add up to 1.0f
    private float waterPercent = 0.4f;
    private float soilPercent = 0.3f;
    private float grassPercent = 0.3f;

    public static int TileSize = 128; // Size of each tile when drawn
    public static int TileMapMaxWidth = 256;
    public static int TileMapMaxHeight = 256;
    public static int TileSizeHalve = TileSize / 2;

    public static int BaseStartX { get; private set; }
    public static int BaseStartY { get; private set; }
    public static int BaseEndX { get; private set; }
    public static int BaseEndY { get; private set; }


    // Redirection boundary for aliens
    public static Vector2 MapCenter;
    public static int BoundaryMargin = 1;
    public static Vector2 RedirectionTopLeft;
    public static Vector2 RedirectionBottomRight;

    public Color FireFruitLightColor = Color.Red * 0.6f; // Slightly transparent red
    public float FireFruitLightRadius = 400f; // Adjust based on desired glow radius
    public static int TileMapHeightPixels { get; private set; }
    public static int TileMapWidthPixels { get; private set; }

  
    public TileMap(int width, int height, Texture2D tilemap)
    {
        tileTextures = tilemap;
        TileMapWidthPixels = width * TileMap.TileSize;
        TileMapHeightPixels = height * TileMap.TileSize;
    }

    private void PrecomputeTilePositions(int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tilePositions[x, y] = new Vector2(x * TileMap.TileSize, y * TileMap.TileSize);
            }
        }
    }

    private Rectangle GetTileSourceRectangle(TileTypes tileType)
    {
        int tileIndex = (int)tileType;

        return new Rectangle(tileIndex * TileSize, 0, TileSize, TileSize);
    }

    public int[,] Generate(int width, int height, int baseSize, float scale)
    {
        // Setup and initialise variables
        map = new int[width, height];
        tilePositions = new Vector2[width, height];
        PrecomputeTilePositions(width, height);
        MapCenter = new Vector2(width * TileMap.TileSize / 2, height * TileMap.TileSize / 2);
        RedirectionTopLeft = new Vector2(-BoundaryMargin * TileMap.TileSize, -BoundaryMargin * TileMap.TileSize);
        RedirectionBottomRight = new Vector2((width + BoundaryMargin) * TileMap.TileSize, (height + BoundaryMargin) * TileMap.TileSize);

        // Validate percentages (they should add up to 1.0)
        float totalPercentage = waterPercent + soilPercent + grassPercent;
        if (Math.Abs(totalPercentage - 1.0f) > 0.001f)
        {
            throw new ArgumentException("Percentages must add up to 1 (100%)");
        }

        // Calculate Perlin noise thresholds based on percentages
        float perlinNoiseRange = 1.3f;
        float waterThreshold = waterPercent * perlinNoiseRange - perlinNoiseRange / 2;
        float soilThreshold = waterThreshold + soilPercent * perlinNoiseRange;
        float grassThreshold = soilThreshold + grassPercent * perlinNoiseRange;

        // Generate Perlin noise map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sample = PerlinNoise.Noise(x * scale, y * scale);

                // Assign tile type based on dynamic thresholds
                if (sample < waterThreshold)
                    map[x, y] = (int)TileTypes.Water;
                else if (sample < soilThreshold)
                    map[x, y] = (int)TileTypes.Soil;
                else 
                    map[x, y] = (int)TileTypes.Grass;
            }
        }

        baseSize += 4; // Add on the base border

        // Create a block of farmland in the center of the map
        int startX = (width - baseSize) / 2;
        int startY = (height - baseSize) / 2;

        // Define the actual corners of the Base area
        int baseStartX = startX + 2;
        int baseStartY = startY + 2;
        int baseEndX = startX + baseSize - 3;  // -1 for index, -2 for outer Grass
        int baseEndY = startY + baseSize - 3;  // -1 for index, -2 for outer Grass

        // Store for later use
        TileMap.BaseStartX = baseStartX;
        TileMap.BaseStartY = baseStartY;
        TileMap.BaseEndX = baseEndX;
        TileMap.BaseEndY = baseEndY;

        for (int y = startY; y < startY + baseSize; y++)
        {
            for (int x = startX; x < startX + baseSize; x++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    // First layer - Works better with 2 layers soil
                    if (x == startX || x == startX + baseSize - 1 ||
                        y == startY || y == startY + baseSize - 1)
                    {
                        map[x, y] = (int)TileTypes.Soil;
                    }
                    // Second layer: Soil
                    else if (x == startX + 1 || x == startX + baseSize - 2 ||
                             y == startY + 1 || y == startY + baseSize - 2)
                    {
                        map[x, y] = (int)TileTypes.Soil;
                    }
                    // Inner area: Base
                    else
                    {
                        map[x, y] = (int)TileTypes.Base;
                    }
                }
            }
        }

        // Set edges to rock/mountain tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = (int)TileTypes.Rock;
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Remove Noise
                bool n = IsTileType(x, y + 1, TileTypes.Soil);
                bool s = IsTileType(x, y - 1, TileTypes.Soil);
                bool w = IsTileType(x - 1, y, TileTypes.Soil);
                bool e = IsTileType(x + 1, y, TileTypes.Soil);

                bool sw = IsTileType(x - 1, y - 1, TileTypes.Soil);
                bool nw = IsTileType(x - 1, y + 1, TileTypes.Soil);
                bool se = IsTileType(x + 1, y - 1, TileTypes.Soil);
                bool ne = IsTileType(x + 1, y + 1, TileTypes.Soil);

                if ((w && sw && s && se && e) ||
                    (s && se && e && ne && n) ||
                    (e && ne && n && nw && w) ||
                    (n && nw && w && sw && s))
                {
                    map[x, y] = (int)TileTypes.Soil;
                }
            }
        }
        return map;
    }

    private Vector2 GetFrontOfShipTile(Player player)
    {
        Vector2 shipMiddle = player.Position;
        float distanceToFrontTile = AssetManager.Player1[0].Width;
        Vector2 shipFront = shipMiddle + player.direction * distanceToFrontTile;
        return shipFront;
    }

    public int likeToDebugX = -1, likeToDebugY = -1;
    private TileTypes SelectTileType(int x, int y)
    {
        TileTypes currentTile = (TileTypes)map[x, y];

        if (Game1.debuggingTiles) 
            return currentTile;


        if(likeToDebugX ==x && likeToDebugY == y)
        {
            // Breakpoint here.. to debug a tile
        }

        if (currentTile == TileTypes.Soil)
        {
        }
        else if (currentTile == TileTypes.Grass)
        {
            // Check for transitions with Soil
            TileTypes transitionType = TileTypes.Soil;
            TileTypes tileType = DetermineTransitionTile(
                x, y, currentTile, transitionType);
            if (tileType != currentTile)
                return tileType;
        }
        else if (currentTile == TileTypes.Water)
        {
            // Check for transitions with Soil
            TileTypes transitionType = TileTypes.Soil;
            TileTypes tileType = DetermineTransitionTile(
                x, y, currentTile, transitionType);
            if (tileType != currentTile)
                return tileType;
        }

        return currentTile;
    }

    private TileTypes DetermineTransitionTile(
            int x, int y, TileTypes currentTile, TileTypes transitionType)
    {
        bool north = IsTileType(x, y + 1, transitionType);
        bool south = IsTileType(x, y - 1, transitionType);
        bool west = IsTileType(x - 1, y, transitionType);
        bool east = IsTileType(x + 1, y, transitionType);

        bool sw = IsTileType(x - 1, y - 1, transitionType);
        bool nw = IsTileType(x - 1, y + 1, transitionType);
        bool se = IsTileType(x + 1, y - 1, transitionType);
        bool ne = IsTileType(x + 1, y + 1, transitionType);

        bool northCurrent = IsTileType(x, y + 1, currentTile);
        bool southCurrent = IsTileType(x, y - 1, currentTile);
        bool westCurrent = IsTileType(x - 1, y, currentTile);
        bool eastCurrent = IsTileType(x + 1, y, currentTile);

        bool swCurrent = IsTileType(x - 1, y - 1, currentTile);
        bool nwCurrent = IsTileType(x - 1, y + 1, currentTile);
        bool seCurrent = IsTileType(x + 1, y - 1, currentTile);
        bool neCurrent = IsTileType(x + 1, y + 1, currentTile);

        // Determine the appropriate transition tile
        if ((north && south && east && west))
        {
            return currentTile; // Surrounded, no transition needed
        }

        // --- Corner Conditions ---
        if (seCurrent && north && west)
        {
            return GetCornerTransition(currentTile, transitionType, "NW");
        }
        if (swCurrent && north && east)
        {
            return GetCornerTransition(currentTile, transitionType, "NE");
        }
        if (neCurrent && south && west)
        {
            return GetCornerTransition(currentTile, transitionType, "SW");
        }
        if (nwCurrent && south && east)
        {
            return GetCornerTransition(currentTile, transitionType, "SE");
        }


        // --- Single Side Conditions ---
        if (north && southCurrent)
        {
            return GetEdgeTransition(currentTile, transitionType, "N");
        }
        if (south && northCurrent)
        {
            return GetEdgeTransition(currentTile, transitionType, "S");
        }
        if (west && eastCurrent)
        {
            return GetEdgeTransition(currentTile, transitionType, "W");
        }
        if (east && westCurrent)
        {
            return GetEdgeTransition(currentTile, transitionType, "E");
        }

        if (se && northCurrent && westCurrent)
        {
            return GetCornerTransition(transitionType, currentTile, "NW");
        }
        if (sw && northCurrent && eastCurrent)
        {
            return GetCornerTransition(transitionType, currentTile, "NE");
        }
        if (ne && southCurrent && westCurrent)
        {
            return GetCornerTransition(transitionType, currentTile, "SW");
        }
        if (nw && southCurrent && eastCurrent)
        {
            return GetCornerTransition(transitionType, currentTile, "SE");
        }

        return currentTile; // Default to current tile
    }

    private TileTypes GetEdgeTransition(
TileTypes currentTile, TileTypes transitionType, string edge)
    {
        if (
            //(currentTile == TileTypes.Soil && transitionType == TileTypes.Water) ||
            (currentTile == TileTypes.Water && transitionType == TileTypes.Soil))
        {
            switch (edge)
            {
                case "N": return TileTypes.WaterSoilN;
                case "S": return TileTypes.WaterSoilS;
                case "W": return TileTypes.WaterSoilW;
                case "E": return TileTypes.WaterSoilE;
                default: return currentTile;
            }
        }
        else if (
                    //(currentTile == TileTypes.Soil && transitionType == TileTypes.Grass) ||
                    (currentTile == TileTypes.Grass && transitionType == TileTypes.Soil))
        {
            switch (edge)
            {
                case "N": return TileTypes.GrassSoilN;
                case "S": return TileTypes.GrassSoilS;
                case "W": return TileTypes.GrassSoilW;
                case "E": return TileTypes.GrassSoilE;
                default: return currentTile;
            }
        }
        else
        {
            return currentTile;
        }
    }

    private bool IsTileType(int x, int y, TileTypes type)
    {
        if (x < 0 || x >= map.GetLength(0) ||
            y < 0 || y >= map.GetLength(1))
            return false;

        return (TileTypes)map[x, y] == type || (TileTypes)map[x, y] == TileTypes.Rock; 
    }

    public TileTypes GetTileType(int x, int y)
    {
        if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
        {
            return (TileTypes)map[x, y];
        }

        return TileTypes.Water; // Default to water if out of bounds (or other handling)
    }

    private TileTypes GetCornerTransition(
                TileTypes currentTile, TileTypes transitionType, string corner)
    {
        if ((currentTile == TileTypes.Water && transitionType == TileTypes.Soil))
        {
            switch (corner)
            {
                case "NW": return TileTypes.WaterSoilCornerNW;
                case "NE": return TileTypes.WaterSoilCornerNE;
                case "SW": return TileTypes.WaterSoilCornerSW;
                case "SE": return TileTypes.WaterSoilCornerSE;
                default: return currentTile;
            }
        }
        else if ((currentTile == TileTypes.Soil && transitionType == TileTypes.Water))
        {
            switch (corner)
            {
                case "NW": return TileTypes.SoilWaterCornerNW;
                case "NE": return TileTypes.SoilWaterCornerNE;
                case "SW": return TileTypes.SoilWaterCornerSW;
                case "SE": return TileTypes.SoilWaterCornerSE;
                default: return currentTile;
            }
        }
        else if ((currentTile == TileTypes.Soil && transitionType == TileTypes.Grass))
        {
            switch (corner)
            {
                case "NW": return TileTypes.SoilGrassCornerNW;
                case "NE": return TileTypes.SoilGrassCornerNE;
                case "SW": return TileTypes.SoilGrassCornerSW;
                case "SE": return TileTypes.SoilGrassCornerSE;
                default: return currentTile;
            }
        }
        else if ((currentTile == TileTypes.Grass && transitionType == TileTypes.Soil))
        {
            switch (corner)
            {
                case "NW": return TileTypes.GrassSoilCornerNW;
                case "NE": return TileTypes.GrassSoilCornerNE;
                case "SW": return TileTypes.GrassSoilCornerSW;
                case "SE": return TileTypes.GrassSoilCornerSE;
                default: return currentTile;
            }
        }
        else
        {
            return currentTile;
        }
    }

    public bool IsValidLandChunk(Vector2 position, int chunkRadius, out Vector2 foundPosition)
    {
        // Get the center tile coordinates for the given position
        int centerX = (int)(position.X / TileMap.TileSize);
        int centerY = (int)(position.Y / TileMap.TileSize);

        // Iterate over a square of tiles within the specified radius
        for (int radius = 0; radius <= chunkRadius; radius++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int tileX = centerX + x;
                    int tileY = centerY + y;

                    // Ensure the tile coordinates are within the map bounds
                    if (tileX >= 0 && tileX < map.GetLength(0) && tileY >= 0 && tileY < map.GetLength(1))
                    {
                        TileTypes tileType = GetTileType(tileX, tileY);

                        // Check if the tile is valid land (not water or rock)
                        if (tileType != TileTypes.Water && tileType != TileTypes.Rock)
                        {
                            foundPosition = new Vector2(tileX * TileSize, tileY * TileSize); // Return the found position
                            return true; // Valid land chunk found
                        }
                    }
                }
            }
        }

        // If no valid land chunk is found, set foundPosition to Vector2.Zero and return false
        foundPosition = Vector2.Zero;
        return false;
    }

    public void SetTileType(int x, int y, TileTypes type)
    {
        if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
        {
            map[x, y] = (int)type;
        }
    }

    public Vector2 GetTileCoordinates(float worldX, float worldY)
    {
        float tileX = worldX / TileMap.TileSize;
        float tileY = worldY / TileMap.TileSize;
        return new Vector2(tileX, tileY);
    }

    public void ConvertTilesToType(int centerX, int centerY, TileTypes tileType, int blockSize)
    {
        // Calculate the radius based on the block size
        int radius = blockSize / 2;

        // Iterate over the area defined by blockSize and set the tile type
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                // Check if the current coordinates are within the boundaries of the map
                if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
                {
                    // Prevent overriding base tiles
                    if ((TileTypes)map[x, y] != TileTypes.Base)
                    {
                        SetTileType(x, y, tileType);
                    }
                }
            }
        }
    }


    public Vector2 GetRandomMapCoordinates()
    {
        return (new Vector2(Random.Shared.Next(4, 60) * TileMap.TileSize,
            Random.Shared.Next(4, 60) * TileMap.TileSize));
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition, Player player1, Player player2, float zoom, Color nightColor)
    {
        Vector2 player1Front = player1 != null ? GetFrontOfShipTile(player1) : Vector2.Zero;
        Vector2 player2Front = player2 != null ? GetFrontOfShipTile(player2) : Vector2.Zero;

        float viewableScreenWidth = Game1.screenWidth / zoom;
        float viewableScreenHeight = Game1.screenHeight / zoom;

        int startX = Math.Max(0, (int)Math.Floor((cameraPosition.X - viewableScreenWidth / 2) / TileMap.TileSize));
        int startY = Math.Max(0, (int)Math.Floor((cameraPosition.Y - viewableScreenHeight / 2) / TileMap.TileSize));
        int endX = Math.Min(map.GetLength(0), startX + (int)Math.Ceiling(viewableScreenWidth / TileMap.TileSize) + 2);
        int endY = Math.Min(map.GetLength(1), startY + (int)Math.Ceiling(viewableScreenHeight / TileMap.TileSize) + 2);

        if (player1 != null)
        {
            player1.tilePointedToX = (int)(player1Front.X / TileMap.TileSize);
            player1.tilePointedToY = (int)(player1Front.Y / TileMap.TileSize);
        }

        if (player2 != null)
        {
            player2.tilePointedToX = (int)(player2Front.X / TileMap.TileSize);
            player2.tilePointedToY = (int)(player2Front.Y / TileMap.TileSize);
        }

        Rectangle tempRectangle = new Rectangle(0, 0, TileMap.TileSize, TileMap.TileSize);

        // --- First loop: Draw tiles ---
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                Vector2 drawPosition = tilePositions[x, y];
                TileTypes tileType = SelectTileType(x, y);
                Rectangle sourceRectangle = GetTileSourceRectangle(tileType);

                tempRectangle.X = (int)drawPosition.X;
                tempRectangle.Y = (int)drawPosition.Y;

                Color tintColor = Color.White;

                if (player1 != null && x == player1.tilePointedToX && y == player1.tilePointedToY)
                {
                    tintColor = Color.OrangeRed * 0.9f;
                }
                else if (player2 != null && x == player2.tilePointedToX && y == player2.tilePointedToY)
                {
                    tintColor = Color.MediumPurple * 0.9f;
                }

                spriteBatch.Draw(tileTextures, tempRectangle, sourceRectangle, tintColor);
            }
        }

        // --- Final loop: Draw player selection frames ---
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                Vector2 drawPosition = tilePositions[x, y];

                if (player1 != null && x == player1.tilePointedToX && y == player1.tilePointedToY)
                {
                    tempRectangle.X = (int)drawPosition.X;
                    tempRectangle.Y = (int)drawPosition.Y;
                    spriteBatch.Draw(AssetManager.PlayerTileSelected, tempRectangle, Color.White * 0.8f);
                }
                else if (player2 != null && x == player2.tilePointedToX && y == player2.tilePointedToY)
                {
                    tempRectangle.X = (int)drawPosition.X;
                    tempRectangle.Y = (int)drawPosition.Y;
                    spriteBatch.Draw(AssetManager.PlayerTileSelected, tempRectangle, Color.White * 0.8f);
                }
            }
        }

        // Debugging tile numbers
        if (Game1.debuggingTiles)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Vector2 drawPosition = tilePositions[x, y] + new Vector2(TileMap.TileSize / 4, TileMap.TileSize / 4);
                    string tileNumber = $"({x},{y})";

                    spriteBatch.DrawString(Game1.smallFont, tileNumber, drawPosition, Color.White);
                }
            }
        }
    }

    public void CheckWithinBoundary(ref Vector2 position)
    {
        position.X = MathHelper.Clamp(position.X, RedirectionTopLeft.X, RedirectionBottomRight.X);
        position.Y = MathHelper.Clamp(position.Y, RedirectionTopLeft.Y, RedirectionBottomRight.Y);
    }
}
