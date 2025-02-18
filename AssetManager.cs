using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class AssetManager
{
    /// <summary>
    /// Player images
    /// </summary>
    public static Texture2D[] Player1 { get; private set; }
    public static Texture2D PlayerTileSelected { get; private set; }
    public static Texture2D WhiteTexture { get; private set; }
    public static Texture2D Container { get; private set; }
    public static Texture2D TileTextures { get; private set; }


    // -----------------------
    // HELPER METHOD
    // -----------------------
    private static Texture2D[] LoadTextureArray(
        ContentManager content,
        string filePrefix,
        int count,
        int startIndex = 1,
        string numericFormat = "D2"
    )
    {
        Texture2D[] textures = new Texture2D[count];
        for (int i = 0; i < count; i++)
        {
            int actualIndex = i + startIndex;
            string frameName = $"{filePrefix}{actualIndex.ToString(numericFormat)}";
            textures[i] = content.Load<Texture2D>(frameName);
        }
        return textures;
    }

    private static Texture2D[] LoadTextureArray(ContentManager content, string filePrefix, int count)
    {
        Texture2D[] textures = new Texture2D[count];
        for (int i = 0; i < count; i++)
        {
            textures[i] = content.Load<Texture2D>($"{filePrefix}{(i + 1).ToString("D2")}");
        }
        return textures;
    }


    public static void LoadContent(ContentManager content)
    {
        TileTextures = content.Load<Texture2D>("Tiles/shlee_tile_sheet_01");

        Player1 = LoadTextureArray(content, "Player/P1_", 20, 1, "D4");

        PlayerTileSelected = content.Load<Texture2D>($"Tiles/selected");
    }
}
