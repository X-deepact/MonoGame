using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

public class Game1 : Game
{
    public static bool debuggingTiles = false;
    public float debbugingTimeScale = 1.0f;

    // Screen settings
    public static int screenWidth;
    public static int screenHeight;
    public static int screenWidthHalved;
    public static int screenHeightHalved;

    public GraphicsDeviceManager graphics;
    public static GraphicsDevice graphicsDevice;
    public static SpriteBatch spriteBatch;
    public static GameTime CurrentGameTime;
    public static float ElapsedTime;

    // Player and controller management
    public static Player player1, player2;
    public static Controllers[] playerControllers;
    public static int player1ControllerIndex = 0;
    public static int player2ControllerIndex = 1;
    public static int playerCount = 1;

    // Camera management
    private Camera2D cameraPlayer1;
    private Camera2D cameraPlayer2;
    public static Camera2D currentCamera;

    // Game state management
    public enum GameState
    {
        Menu,
        Playing,
        Settings,
        LevelSelection
    }
    private GameState currentGameState;

    public static KeyboardState previousKeys, currentKeys;
    public static GamePadState previousGamePad, currentGamePad;
    public static bool isPaused = false;

    // Menu variables
    public static SpriteFont menuFont;    // Font for the menu
    public static SpriteFont HUDFont;     // Font for the HUD
    public static SpriteFont smallFont;   // Font for debugging
    private bool isExitMenuOpen = false;

    public static Vector2[] HudResourcePositions = new Vector2[10];
    private List<ControlAction> actions = new List<ControlAction>();

    /// <summary>
    /// Populate the list of possible ControlActions so we can rebind them.
    /// </summary>
    private void InitializeControlActions()
    {
        foreach (ControlAction action in Enum.GetValues(typeof(ControlAction)))
        {
            actions.Add(action);
        }
    }

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        screenWidthHalved = screenWidth / 2;
        screenHeightHalved = screenHeight / 2;
    }

    protected override void Initialize()
    {
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.PreferredBackBufferHeight = screenHeight;
        graphics.ApplyChanges();
        graphicsDevice = graphics.GraphicsDevice;
        currentGameState = GameState.Menu; // Start with the menu 

        // Load or create default controllers
        playerControllers = new Controllers[4];
        playerControllers = ControlsManager.LoadAllPlayerControls(); // This should load (or create) the 4 possible setups

        InitializeControlActions();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        AssetManager.LoadContent(Content);

        menuFont = Content.Load<SpriteFont>("Font/MenuFont");
        HUDFont = Content.Load<SpriteFont>("Font/HUDFont");
        smallFont = Content.Load<SpriteFont>("Font/smallFont");
        
        // Media player options
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        InitialiseGame(0);
    }

    protected override void Update(GameTime gameTime)
    {
        Game1.CurrentGameTime = gameTime;

        Game1.ElapsedTime = (float)Game1.CurrentGameTime.ElapsedGameTime.TotalSeconds;

        CheckAlwaysActiveKeys();

        CheckNonPlayerGameKeys();
        UpdateGame();

        previousKeys = currentKeys;
        base.Update(gameTime);
    }

    private void CheckAlwaysActiveKeys()
    {
        // Capture keyboard/gamepad state each frame
        previousKeys = currentKeys;
        currentKeys = Keyboard.GetState();

        previousGamePad = currentGamePad;
        currentGamePad = GamePad.GetState(PlayerIndex.One);

        // Toggle full screen
        if (currentKeys.IsKeyDown(Keys.F5) && previousKeys.IsKeyUp(Keys.F5))
        {
            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = screenWidth;
                graphics.PreferredBackBufferHeight = screenHeight;
            }
            else
            {
                graphics.PreferredBackBufferWidth =
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - TileMap.TileSize;
                graphics.PreferredBackBufferHeight =
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - TileMap.TileSize;
            }
            graphics.ApplyChanges();
        }
        if (currentKeys.IsKeyDown(Keys.F12) && previousKeys.IsKeyUp(Keys.F12))
        {
            Exit();
        }
    }


    private void InitialiseGame(int LevelToPlay)
    {
        GameManager.InitializeManagers(GraphicsDevice);
        GameManager.InitializeTileMap(GraphicsDevice);
 
        if (playerCount == 1)
        {
            cameraPlayer1 = new Camera2D(screenWidth, screenHeight);
            player1 = new Player(
                playerControllers[player1ControllerIndex],
                AssetManager.Player1,
                new Vector2(TileMap.TileMapWidthPixels / 2, TileMap.TileSize * 64 / 2),
                cameraPlayer1,
                1
            );
        }
        else if (playerCount == 2)
        {
            cameraPlayer1 = new Camera2D(screenWidthHalved, screenHeight);
            cameraPlayer2 = new Camera2D(screenWidthHalved, screenHeight);

            player1 = new Player(
                playerControllers[player1ControllerIndex],
                AssetManager.Player1,
                new Vector2(TileMap.TileMapWidthPixels / 2 - 600, TileMap.TileMapHeightPixels / 2),
                cameraPlayer1,
                1            );
        }
    }

    public void CheckNonPlayerGameKeys()
    {
        if ((currentKeys.IsKeyDown(Keys.Escape) && previousKeys.IsKeyUp(Keys.Escape)) ||
            (currentGamePad.IsButtonDown(Buttons.Start) && !previousGamePad.IsButtonDown(Buttons.Start)))
        {
            if (currentGameState == GameState.Playing)
            {
                isExitMenuOpen = !isExitMenuOpen;
                isPaused = isExitMenuOpen;
            }
        }

#if DEBUG    
        if (currentKeys.IsKeyDown(Keys.T) && previousKeys.IsKeyUp(Keys.T))
        {
            debuggingTiles = !debuggingTiles;
        }
#endif
    }

    private void UpdateGame()
    {
        if (player1 != null)
        {
            UpdatePlayerAndCamera(player1, cameraPlayer1);
        }

        if (player2 != null)
        {
            if (player2.IsDead)
            {
                UpdatePlayerAndCamera(player2, cameraPlayer2);
            }

        }
    }

    private void UpdatePlayerAndCamera(Player player, Camera2D camera)
    {
        player.Update();
        camera.Position = player.Position;

        player.Position =new Vector2(
        Math.Clamp(
            player.Position.X,
            TileMap.TileSize + AssetManager.Player1[0].Width / 2,
            TileMap.TileMapWidthPixels - TileMap.TileSize - AssetManager.Player1[0].Width / 2)
           ,
         Math.Clamp(
            player.Position.Y,
            TileMap.TileSize + AssetManager.Player1[0].Height / 2,
            TileMap.TileMapHeightPixels - TileMap.TileSize - AssetManager.Player1[0].Height / 2)
         );

        int viewportWidth = camera.ViewportWidth;
        int viewportHeight = camera.ViewportHeight;

        camera.Position = new Vector2(
            Math.Clamp(camera.Position.X, viewportWidth / 2, TileMap.TileMapWidthPixels - viewportWidth / 2),
            Math.Clamp(camera.Position.Y, viewportHeight / 2, TileMap.TileMapHeightPixels - viewportHeight / 2)
        );

        camera.Update();
    }

    public static void BeginPlayerViewportDraw(SpriteBatch spriteBatch, Camera2D camera)
    {
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.Default,
            RasterizerState.CullCounterClockwise,
            effect: null,
            transformMatrix: camera.GetViewMatrix()
        );
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(32, 32, 32));

        DrawGame();

        base.Draw(gameTime);
    }

    private void DrawGame()
    {
        if (playerCount == 1)
        {
            DrawPlayerViewport(0, 0, screenWidth, screenHeight, cameraPlayer1, player1, null);
        }
        else if (playerCount == 2)
        {
            DrawPlayerViewport(0, 0, screenWidthHalved, screenHeight, cameraPlayer1, player1, player2);
            DrawPlayerViewport(screenWidthHalved, 0, screenWidthHalved, screenHeight, cameraPlayer2, player2, player1);
        }

        GraphicsDevice.Viewport = new Viewport(0, 0, screenWidth, screenHeight);

        if (playerCount == 2)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(AssetManager.WhiteTexture, new Rectangle(screenWidthHalved - 2, 0, 4, screenHeight), Color.LightSeaGreen);
            spriteBatch.End();
        }

        if (isPaused)
        {
            spriteBatch.Begin();
            Texture2D overlayTexture = new Texture2D(GraphicsDevice, 1, 1);
            overlayTexture.SetData(new[] { new Color(0, 0, 0, 0.5f) });
            spriteBatch.Draw(overlayTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            spriteBatch.End();
        }
    }

    private void DrawPlayerViewport(int x, int y, int width, int height, Camera2D camera, Player player, Player otherPlayer)
    {
        Viewport viewport = new Viewport(x, y, width, height);
        GraphicsDevice.Viewport = viewport;
        currentCamera = camera;

        Game1.BeginPlayerViewportDraw(spriteBatch,camera);
        GameManager.TileMap.Draw(spriteBatch, camera.Position, player, otherPlayer, camera.Zoom, Color.White);
     
        player?.Draw(spriteBatch);
        otherPlayer?.Draw(spriteBatch);

        spriteBatch.End();
    }

}
