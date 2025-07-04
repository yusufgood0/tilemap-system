using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using System.Reflection;
using System.Reflection.Metadata;
using first_game;
using System.Xml.Linq;
namespace tilemap_system
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        KeyboardState _keyboardState, _PreviouskeyboardState;
        MouseState _mouseState, _previousMouseState;
        public readonly Random _random = new Random();
        public static SpriteFont _font;

        private readonly IntTriple loadDistance = new(200, 200, 200);
        private readonly IntTriple renderDistance = new(100, 25, 100);
        static readonly IntTriple TileArray = new(200, 200, 200);

        /* mouse Control */
        bool previousIsActive;
        float sensitivity = .01f;

        /* draw */
        const int fps = 30;
        static Timer drawTimer = new Timer(1000 / fps); // in milliseconds

        int pixelWidth;
        int pixelHeight;
        static Texture2D crosshair;
        static Vector2 crosshairDrawPos;
        const float crosshairScale = 0.1f;
        static Vector2 offset;
        static Vector2 screenSize;
        static List<DDA_ray> drawRays = new List<DDA_ray>();
        static Vector2 FOV = new(2, 2);
        static readonly Point resolution = new(100, 100);
        const float cameraHeight = 60;
        Vector3 eyePosition;

        /* Inventory */
        int helditemSize;

        int selectedItem;
        storageInfo hotbar;
        const int hotbarSize = 80;
        Rectangle[] hotbarRects;
        Point hotbarPosition;

        storageInfo inventory;
        int rowCount;
        Rectangle[] inventoryRects;

        storageInfo mouseHeldItem;

        //Point PlayerTileIndex;
        private static Tiles selectedTile = null;
        private static List<Tiles> loadedTiles = new List<Tiles>();
        private static Tiles[][][] _Tiles = new Tiles[TileArray.X][][];
        private static Player _player;
        public static Texture2D square;
        private static Slider[] menuSliders = new Slider[1];

        float scrollwheelChange;
        public enum Keybind
        {
            up = Keys.W,
            down = Keys.S,
            Left = Keys.A,
            Right = Keys.D,
            Jump = Keys.Space,
            sneak = Keys.LeftShift,
            inventory = Keys.E,
            pause = Keys.Escape,
        }
        GameState gameState = GameState.Playing;
        enum GameState
        {
            Playing,
            Paused,
            Inventory
        }

        static readonly Rectangle sensitivitySliderRect = new((int)(Game1.screenSize.X * .1f), (int)(Game1.screenSize.Y * .4f), (int)(Game1.screenSize.X * .3f), (int)(Game1.screenSize.Y * .05f));

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            _font = Content.Load<SpriteFont>("myFont");

            screenSize = new(
                1000,
                1000
                );
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)screenSize.X;
            _graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            _graphics.ApplyChanges();

            helditemSize = (int)(screenSize.X * 0.4);
            pixelWidth = (int)screenSize.X / resolution.X;
            pixelHeight = (int)screenSize.Y / resolution.Y;
            /* set up tiles */
            for (int x = 0; x < _Tiles.Length; x++)
            {
                _Tiles[x] = new Tiles[TileArray.Y][];
                for (int y = 0; y < _Tiles[x].Length; y++)
                {
                    _Tiles[x][y] = new Tiles[TileArray.Z];
                }
            }

            for (int x = 0; x < TileArray.X; x++)
            {
                for (int y = 0; y < TileArray.Y; y++)
                {
                    for (int z = 0; z < TileArray.Z; z++)
                    {

                        if (y < TileArray.Y / 3)
                        {
                            _Tiles[x][y][z] = new Tiles(x, y, z, Tiles.ID.Empty);
                        }
                        else if (x % 2 == 1)
                        {
                            _Tiles[x][y][z] = new Tiles(x, y, z, Tiles.ID.Grass);
                        }
                        else
                        {
                            _Tiles[x][y][z] = new Tiles(x, y, z, Tiles.ID.Stone);
                        }
                    }
                }
            }
            _player = new Player(_Tiles[TileArray.X / 2][TileArray.Y / 3 - 20][TileArray.Z / 2].Cube.Position);
            //_player = new Player(new(100, -100, 0));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            crosshair = Content.Load<Texture2D>("minecraftCrosshair");
            crosshairDrawPos = new Vector2((int)(screenSize.X / 2 - crosshair.Width / 2 * crosshairScale), (int)(screenSize.Y / 2 - crosshair.Height / 2 * crosshairScale));
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Tiles.SetTexture(Content.Load<Texture2D>("square"));
            Player.SetTexture(Content.Load<Texture2D>("square"));
            square = Content.Load<Texture2D>("square");
            Item.ItemSetup(new Item[] {

                new Item("Empty", Content.Load<Texture2D>("square"), null),
                new Item("Grass", Content.Load<Texture2D>("grassblockItem"), (int)Tiles.ID.Grass),
                new Item("Stone", Content.Load<Texture2D>("stoneItem"), (int)Tiles.ID.Stone)

                });

            selectedItem = 0;
            hotbar = new(9);
            inventory = new(27);
            mouseHeldItem = new(1);

            hotbarPosition = new Point((int)(screenSize.X / 2 - (hotbarSize * hotbar.Count) / 2), (int)(screenSize.Y - hotbarSize * 4));

            hotbarRects = new Rectangle[hotbar.Count];
            for (int i = 0; i < hotbar.Count; i++)
            {
                hotbarRects[i] = new Rectangle(hotbarPosition.X + i * hotbarSize, hotbarPosition.Y + hotbarSize * 3, hotbarSize, hotbarSize);
            }

            rowCount = inventory.Count / hotbar.Count;
            inventoryRects = new Rectangle[inventory.Count];
            int index = 0;
            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < hotbar.Count; x++)
                {
                    inventoryRects[index] = new Rectangle(hotbarPosition.X + x * hotbarSize, hotbarPosition.Y + y * hotbarSize, hotbarSize, hotbarSize);
                    index++;
                }
            }

            menuSliders[0] = new Slider(square,
            new((int)(Game1.screenSize.X * .6f), (int)(Game1.screenSize.Y * .4f), (int)(Game1.screenSize.X * .3f), (int)(Game1.screenSize.Y * .05f)),
            0f,
            (float)Math.PI * 2,
            //4,
            Color.Black,
            Color.Red,
            "FOV",
            1f,
            10
            );
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            eyePosition = new Vector3(_player.Cube.Center.X, _player.Cube.Y_OP - cameraHeight, _player.Cube.Center.Z);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            _mouseState = Mouse.GetState();
            _keyboardState = Keyboard.GetState();
            loadedTiles = Tiles.getLoaded(_player.Position, loadDistance, TileArray, _Tiles);

            scrollwheelChange = _mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollwheelChange != 0)
            {
                if (scrollwheelChange > 0)
                {
                    selectedItem = General.Bound(selectedItem + 1, hotbar.Count);
                }
                else if (scrollwheelChange < 0)
                {
                    selectedItem = General.Bound(selectedItem - 1, hotbar.Count);
                }
            }


            if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.inventory))
            {
                if (gameState == GameState.Playing)
                {
                    gameState = GameState.Inventory;
                    IsMouseVisible = true;
                }
                else
                {
                    gameState = GameState.Playing;
                    IsMouseVisible = false;
                }
            }
            if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.pause))
            {
                switch (gameState)
                {
                    case GameState.Paused:
                        gameState = GameState.Playing;
                        IsMouseVisible = false;

                        break;
                    default:
                        gameState = GameState.Paused;
                        IsMouseVisible = true;
                        break;
                }
            }
            if (gameState == GameState.Playing)
            {
                if (previousIsActive)
                {
                    _player._angle.X += (Mouse.GetState().X - screenSize.X / 2) * sensitivity;
                    _player._angle.Y += (Mouse.GetState().Y - screenSize.Y / 2) * sensitivity;
                    if (_keyboardState.IsKeyDown(Keys.Up))
                    {
                        _player._angle.Y -= 0.1f;
                    }
                    if (_keyboardState.IsKeyDown(Keys.Down))
                    {
                        _player._angle.Y += 0.1f;
                    }
                    if (_keyboardState.IsKeyDown(Keys.Right))
                    {
                        _player._angle.X += 0.1f;
                    }
                    if (_keyboardState.IsKeyDown(Keys.Left))
                    {
                        _player._angle.X -= 0.1f;
                    }
                }
                if (IsActive)
                    Mouse.SetPosition((int)screenSize.X / 2, (int)screenSize.Y / 2);
                previousIsActive = IsActive;
            }
            //if (_mouseState.LeftButton == ButtonState.Pressed)
            //{
            //    foreach (Tiles tile in Tiles.CollidingTiles(new(_mouseState.X, _mouseState.Y, -100, 1, 1, 1000), TileArray, _Tiles))
            //        (tile).MineTile(3);
            //}

            for (int i = 0; i < loadedTiles.Count; i++)
            {
                if (loadedTiles[i].Isfull && loadedTiles[i] != selectedTile)
                    loadedTiles[i].Heal();
            }
            /* enable for jumping to activate */
            foreach (Tiles tile in Tiles.CollidingTiles(new((int)_player.Position.X, _player.Cube.Y_OP, (int)_player.Position.Z, _player.Cube.XSize, 3, _player.Cube.ZSize), TileArray, _Tiles)) //checks if players onground
            {
                if (tile.Isfull)
                {
                    if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.Jump))
                    {
                        _player.jump();
                        break;
                    }
                }
            }


            _player._angle.X = General.Bound(_player._angle.X, (float)Math.Tau);
            //if (_player._angle.X > (float)Math.Tau)
            //    _player._angle.X = _player._angle.X % (float)Math.Tau;
            //if (_player._angle.X < 0)
            //    _player._angle.X += (float)Math.Tau;

            _player._angle.Y = MathHelper.Clamp(_player._angle.Y, -MathF.PI / 2f, MathF.PI / 2f);

            Ray ray = new(_player._angle, eyePosition);

            if (gameState == GameState.Inventory)
            {
                if (General.OnLeftPress(_mouseState, _previousMouseState) && gameState == GameState.Inventory)
                {
                    for (int i = 0; i < inventoryRects.Length; i++)
                    {
                        if (inventoryRects[i].Contains(_mouseState.X, _mouseState.Y))
                        {
                            storageInfo.Interact(ref mouseHeldItem, 0, ref inventory, i);
                        }
                    }
                    for (int i = 0; i < hotbarRects.Length; i++)
                    {
                        if (hotbarRects[i].Contains(_mouseState.X, _mouseState.Y))
                        {
                            storageInfo.Interact(ref mouseHeldItem, 0, ref hotbar, i);
                        }
                    }
                }
            }

            if (gameState == GameState.Playing)
            {

                if (selectedTile != null)
                {
                    if (General.OnLeftPress(_mouseState, _previousMouseState) && _player.isCreative)
                        selectedTile.MineTile(1000);
                    else if (_mouseState.LeftButton == ButtonState.Pressed && !_player.isCreative)
                        selectedTile.MineTile(5);
                }
                selectedTile = null;

                for (int l = 0; l < 160; l++)
                {
                    ray.update(1);
                    IntTriple tileIndex = General.Clamp(Tiles.getTileIndex(ray._position), new(), TileArray);
                    if (_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].Isfull)
                    {
                        if (_mouseState.RightButton == ButtonState.Pressed)
                        {
                            ray.reverse(1);
                            tileIndex = General.Clamp(Tiles.getTileIndex(ray._position), new(), TileArray);
                            if (!Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles).Contains(_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z]) && hotbar.GetBlockID(selectedItem) != null)
                            {
                                _Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].setType((Tiles.ID)hotbar.GetBlockID(selectedItem));
                                hotbar.RemoveItems(selectedItem, 1);
                            }
                        }
                        selectedTile = _Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z];
                        selectedTile._color = Color.AliceBlue;

                        break;
                    }
                }
            }


            /* Does Player Collision & movement */
            _player.move(new(_player.Speed.X, 0, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionX(tile.Cube); }
            }
            _player.move(new(0, _player.Speed.Y, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionY(tile.Cube); }
            }
            _player.move(new(0, 0, _player.Speed.Z));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionZ(tile.Cube); }
            }
            /* updates Player._speed with movement and stuffn */
            _player.update(_keyboardState, _PreviouskeyboardState);

            if (gameState == GameState.Paused)
            {
                menuSliders[0].SliderUpdate(_mouseState, _previousMouseState, ref FOV.X);
                menuSliders[0].SliderUpdate(_mouseState, _previousMouseState, ref FOV.Y);
                //Slider(_mouseState, _previousMouseState, ref FOV.X, FOVRange, Constants.FOVSliderRect);
                //Slider(_mouseState, _previousMouseState, ref FOV.Y, FOVRange, Constants.FOVSliderRect);
                //Slider(_mouseState, _previousMouseState, ref detail, Constants.minDetail, Constants.maxDetail, Constants.detailSliderRect);
            }

            _previousMouseState = _mouseState;
            _PreviouskeyboardState = _keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (drawTimer.IsActive)
            {
                drawTimer.Reset();

                GraphicsDevice.Clear(Color.Black);
                offset = loadDistance.XY - _player.XY - new Vector2(_player.Cube.XSize / 2, _player.Cube.YSize / 2);

                //foreach (Tiles tile in Tiles.g set);

                //raycasting starts here

                if (gameTime.TotalGameTime.Seconds < 0.5 || true)
                {
                    drawRays.Clear();

                    float fX = (resolution.X / 2f) / (float)Math.Tan(FOV.X / 2f);
                    float fY = (resolution.Y / 2f) / (float)Math.Tan(FOV.Y / 2f);

                    Matrix rotYaw = Matrix.CreateRotationY(_player._angle.X - (float)Math.PI);
                    Matrix rotPitch = Matrix.CreateRotationX(_player._angle.Y);
                    Matrix viewRot = rotPitch * rotYaw;

                    IntTriple clampMin = General.Clamp(-renderDistance + Tiles.getTileIndex(_player.Position), new(), TileArray);
                    IntTriple clampMax = General.Clamp(renderDistance + Tiles.getTileIndex(_player.Position), new(), TileArray);
                    for (int x = -resolution.X / 2; x < resolution.X / 2; x++)
                    {
                        for (int y = -resolution.Y / 2; y < resolution.Y / 2; y++)
                        {
                            // Aspect ratio correction
                            Vector3 rayDir = new Vector3(-x, y * (fX / fY), -fX);
                            rayDir.Normalize();

                            // Rotate the ray direction into world space
                            Vector3 worldDir = Vector3.Transform(rayDir, viewRot);

                            drawRays.Add(new DDA_ray(eyePosition, eyePosition + worldDir));
                        }
                    }
                    for (int index = 0; index < drawRays.Count; index++)
                    {
                        drawRays[index]._color = Color.Blue;
                        for (int l = 0; l < renderDistance.X * 2; l++)
                        {
                            IntTriple tileIndex = Tiles.getTileIndex(drawRays[index].Update());
                            if (!tileIndex.inBound(clampMin, clampMax))
                            {
                                break;
                            }
                            if (_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].Isfull)
                            {
                                drawRays[index]._color = (_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z]._color) * ((float)(500f - drawRays[index].lowestDistance) / 500f);
                                break;
                            }
                        }
                    }
                }

                //draws tiles from previous raycasting
                int rayIndex = 0;
                _spriteBatch.Begin();
                for (int x = 0; x < resolution.X; x++)
                {
                    for (int y = 0; y < resolution.Y; y++)
                    {
                        _spriteBatch.Draw(Tiles._texture,
                            new Rectangle(
                                x * pixelWidth,
                                y * pixelHeight,
                                pixelWidth,
                                pixelHeight
                                ),
                            null,
                            drawRays[rayIndex]._color,
                            0,
                            new Vector2(),
                            SpriteEffects.None,
                            0.99f

                            );
                        rayIndex += 1;
                    }
                }

                //draws crosshair
                _spriteBatch.Draw(crosshair, crosshairDrawPos, null, Color.White, 0, new Vector2(), crosshairScale, 0, 1);

                //draws debug
                _spriteBatch.DrawString(_font, Tiles.getTileIndex(_player.Position).X.ToString(), new(), Color.White);
                _spriteBatch.DrawString(_font, Tiles.getTileIndex(_player.Position).Y.ToString(), new(0, _font.LineSpacing), Color.White);
                _spriteBatch.DrawString(_font, Tiles.getTileIndex(_player.Position).Z.ToString(), new(0, _font.LineSpacing + _font.LineSpacing), Color.White);
                _spriteBatch.DrawString(_font, _player._angle.X.ToString(), new(0, _font.LineSpacing * 3), Color.White);
                _spriteBatch.Draw(hotbar.GetItemTexture(selectedItem), new Rectangle((int)screenSize.X, (int)screenSize.Y - helditemSize / 4, helditemSize, helditemSize), null, Color.White, (float)Math.Tau * 0.95f, new(helditemSize / 2, helditemSize / 2), SpriteEffects.None, 1);
                
                //draws hotbar
                for (int index = 0; index < hotbar.Count; index++)
                {
                    Color color = Color.Gray;
                    if (selectedItem == index)
                    {
                        color = Color.White;
                    }
                    _spriteBatch.Draw(square, hotbarRects[index], null, color, 0, new(), 0, 0.99f);
                    if (hotbar.getItem(index) != Item._items[0])
                    {
                        _spriteBatch.Draw(hotbar.GetItemTexture(index), hotbarRects[index], null, Color.White, 0, new(), 0, 1);
                        _spriteBatch.DrawString(_font, hotbar.getStackCount(index).ToString(), new(hotbarRects[index].X, hotbarRects[index].Y), Color.White);
                    }

                }

                //draws inventory
                if (GameState.Inventory == gameState)
                {
                    int index = 0;

                    for (int y = 0; y < rowCount; y++)
                    {
                        for (int x = 0; x < hotbar.Count; x++)
                        {
                            if (index < inventory.Count)
                            {
                                _spriteBatch.Draw(square, inventoryRects[index], null, Color.Gray, 0, new(), 0, 0.99f);
                                if (inventory.getItem(index) != Item._items[0])
                                {
                                    _spriteBatch.Draw(inventory.GetItemTexture(index), inventoryRects[index], null, Color.White, 0, new(), 0, 1);
                                    _spriteBatch.DrawString(_font, inventory.getStackCount(index).ToString(), new(inventoryRects[index].X, inventoryRects[index].Y), Color.White);
                                }
                            }
                            else
                            {
                                break;
                            }
                            index++;
                        }
                    }
                    if (mouseHeldItem.getItem(0) != Item._items[0])
                        _spriteBatch.Draw(mouseHeldItem.GetItemTexture(0), new(_mouseState.X - hotbarSize / 2, _mouseState.Y - hotbarSize / 2, hotbarSize, hotbarSize), null, Color.White, 0, new(), 0, 1);

                }

                //draws pause menu
                if (GameState.Paused == gameState)
                {
                    _spriteBatch.Draw(square, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), null, new Color(128, 128, 128, 128), 0, new(), 0, 1);
                    menuSliders[0].SliderDraw(_spriteBatch, FOV.X);
                }

                _spriteBatch.End();

                base.Draw(gameTime);
            }
        }
    }
}
