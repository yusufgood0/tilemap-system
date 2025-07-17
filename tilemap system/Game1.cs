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
using System.Linq;
using System.Threading.Tasks;
using tilemap_system.tilemap_system;
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
        private readonly IntTriple renderDistance = new(1, 1, 1);
        static readonly IntTriple TileArraySize = new(200, 200, 200);

        /* mouse Control */
        bool _previousIsMouseVisible;
        float sensitivity = .01f;

        /* draw */
        const int fps = 144;
        static readonly Timer drawTimer = new Timer(1000 / fps); // in milliseconds
        static int frameCount = 0;
        static readonly Timer fpsCheckInterval = new Timer(1000); // 5 second timer
        static int displayfps; // used for displaying fps in the game

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
        private static Tile selectedTile = null;
        //private static Tile[,,] loadedTiles = new List<Tile>();
        //private static Tile[,,] _Tiles;
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
        public enum GameState
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

            _player = new Player(World.GetTileFromWorldPos(new(1000, 0, 1000)).Cube.Position);
            //_player = new Player(new(100, -100, 0));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            crosshair = Content.Load<Texture2D>("minecraftCrosshair");
            crosshairDrawPos = new Vector2((int)(screenSize.X / 2 - crosshair.Width / 2 * crosshairScale), (int)(screenSize.Y / 2 - crosshair.Height / 2 * crosshairScale));
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Tile.SetTexture(Content.Load<Texture2D>("square"));
            Player.SetTexture(Content.Load<Texture2D>("square"));
            square = Content.Load<Texture2D>("square");
            TileInfo.ItemSetup(new TileInfo[] {

                new TileInfo("Empty", Color.White),
                new TileInfo("Empty", Color.Green),
                new TileInfo("Empty", Color.Gray)

            });


            Item.ItemSetup(new Item[] {

                new Item("Empty", Content.Load<Texture2D>("square"), null),
                new Item("Grass", Content.Load<Texture2D>("grassblockItem"), (int)Tile.ID.Grass),
                new Item("Stone", Content.Load<Texture2D>("stoneItem"), (int)Tile.ID.Stone)

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
        internal void Play(out GameState gameState)
        {
            gameState = GameState.Playing;
            IsMouseVisible = false;
        }
        internal void Pause(out GameState gameState)
        {
            gameState = GameState.Paused;
            IsMouseVisible = true;
        }
        private void GetSelectedTile(out Tile? selectedTile, ref Ray ray, int reach)
        {
            selectedTile = null;
            for (int l = 0; l < reach; l++)
            {
                ray.update(3);
                Tile tile = World.GetTileFromWorldPos(ray._position);
                if (tile.Isfull)
                {
                    selectedTile = tile;
                    break;
                }
            }
        }
        private void PlaceBlockFromHotbar(ref Tile tile)
        {
            if (!Tile.CollidingTiles(_player.Cube).Contains(tile) //ensures the player is not placing a block inside themself
                        && hotbar.GetBlockID(selectedItem) != null) //makes sure your not placing a null value
            {
                tile.setType((Tile.ID)hotbar.GetBlockID(selectedItem));
                hotbar.RemoveItems(selectedItem, 1);
            }
        }
        protected override void Update(GameTime gameTime)
        {
            eyePosition = new Vector3(_player.Cube.Center.X, _player.Cube.Y_OP - cameraHeight, _player.Cube.Center.Z);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            _mouseState = Mouse.GetState();
            _keyboardState = Keyboard.GetState();
            //loadedTiles = Tile.getLoaded(_player.Position, loadDistance, TileArraySize, _Tiles);
            //Chunk currentChunk = World.GetChunk(new IntDouble(_player.Position));

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
            /* opens inventory on button press */
            if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.inventory))
            {
                if (gameState == GameState.Playing)
                {
                    gameState = GameState.Inventory;
                    IsMouseVisible = true;
                }
                else
                {
                    Play(out gameState);
                }
            }
            /* pauses game on button press */
            if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.pause))
            {
                if (gameState == GameState.Playing)
                {
                    Pause(out gameState);
                }
                else
                {
                    Play(out gameState);
                }
            }

            if (!_previousIsMouseVisible)
            {
                _player._angle.X += (Mouse.GetState().X - screenSize.X / 2) * sensitivity;
                _player._angle.Y += (Mouse.GetState().Y - screenSize.Y / 2) * sensitivity;
            }
            if (!IsMouseVisible)
            {
                Mouse.SetPosition((int)screenSize.X / 2, (int)screenSize.Y / 2);
            }
            /*
            for (int i = 0; i < currentChunk.Count; i++)
            {
                if (loadedTiles[i].Isfull && loadedTiles[i] != selectedTile)
                    loadedTiles[i].Heal();
            }
            */
            /* enable for jumping to activate */
            //foreach (Tile tile in Tile.CollidingTiles(new((int)_player.Position.X, _player.Cube.Y_OP, (int)_player.Position.Z, _player.Cube.XSize, 3, _player.Cube.ZSize))) //checks if players onground
            //{
            //    if (tile.Isfull)
            //    {
            //        if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.Jump))
            //        {
            //            _player.jump();
            //            break;
            //        }
            //    }
            //}

            /*clamps player angles */
            General.Bound(ref _player._angle.X, (float)Math.Tau);
            _player._angle.Y = MathHelper.Clamp(_player._angle.Y, -MathF.PI / 2f, MathF.PI / 2f);

            /* checks inventory interaction when inventory is open */
            if (General.OnLeftPress(_mouseState, _previousMouseState) && gameState == GameState.Inventory)
            {
                inventory.CheckSlotInteractions(_mouseState, inventoryRects, ref mouseHeldItem);
                hotbar.CheckSlotInteractions(_mouseState, hotbarRects, ref mouseHeldItem);
            }


            //if (gameState == GameState.Playing)
            //{
            //    /* does mining tiles */
            //    if (selectedTile != null)
            //    {
            //        if (General.OnLeftPress(_mouseState, _previousMouseState) && _player.isCreative)
            //            selectedTile.MineTile(1000);
            //        else if (_mouseState.LeftButton == ButtonState.Pressed && _player.isSurvival)
            //            selectedTile.MineTile(5);
            //    }
            //    Ray ray = new(_player._angle, eyePosition);

            //    /* updates what tile you have selected */
            //    GetSelectedTile(out selectedTile, ref ray, 160); //this is really slow now

            //    /* on right button press, moves the ray back one unit and fills the block its in with block from your hotbar */
            //    if (_mouseState.RightButton == ButtonState.Pressed)
            //    {
            //        ray.reverse(1);
            //        IntTriple tileIndex = General.Clamp(Tile.getTileIndex(ray._position), new(), TileArraySize);
            //        PlaceBlockFromHotbar(ref World.GetTile(new (ray._position)));
            //    }
            //}


            /* Does Player Collision & movement */
            _player.move(new(_player.Speed.X, 0, 0));
            //foreach (Tile tile in Tile.CollidingTiles(_player.Cube))
            //{
            //    if (tile.Isfull) { _player.CollisionX(tile.Cube); }
            //}
            _player.move(new(0, _player.Speed.Y, 0));
            //foreach (Tile tile in Tile.CollidingTiles(_player.Cube))
            //{
            //    if (tile.Isfull) { _player.CollisionY(tile.Cube); }
            //}
            _player.move(new(0, 0, _player.Speed.Z));
            //foreach (Tile tile in Tile.CollidingTiles(_player.Cube))
            //{
            //    if (tile.Isfull) { _player.CollisionZ(tile.Cube); }
            //}

            /* updates Player._speed with movement and checks for keypresses etc */
            _player.update(_keyboardState, _PreviouskeyboardState);

            /* makes sure the sliders are checking if theyre being pressed */
            if (gameState == GameState.Paused)
            {
                menuSliders[0].SliderUpdate(_mouseState, _previousMouseState, ref FOV.X);
                menuSliders[0].SliderUpdate(_mouseState, _previousMouseState, ref FOV.Y);
            }

            if (fpsCheckInterval.IsActive)
            {
                displayfps = (int)Math.Round(frameCount / fpsCheckInterval.getTimeSeconds(), 1);
                fpsCheckInterval.Reset();
                frameCount = 0;
            }

            /* 
             * updates previous mousestates with old button presses
             * this is useful for checking the exact frame when something is pressed
             */
            _previousIsMouseVisible = IsMouseVisible;
            _previousMouseState = _mouseState;
            _PreviouskeyboardState = _keyboardState;

            base.Update(gameTime);
        }
        async Task CastRays()
        {
            drawRays.Clear();

            float fX = (resolution.X / 2f) / (float)Math.Tan(FOV.X / 2f);
            float fY = (resolution.Y / 2f) / (float)Math.Tan(FOV.Y / 2f);

            Matrix rotYaw = Matrix.CreateRotationY(_player._angle.X - (float)Math.PI);
            Matrix rotPitch = Matrix.CreateRotationX(_player._angle.Y);
            Matrix viewRot = rotPitch * rotYaw;

            IntTriple clampMax = renderDistance + Tile.getTileIndex(_player.Position);
            IntTriple clampMin = -clampMax;
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
            await Task.Run(() =>
            {
                for (int index = 0; index < drawRays.Count; index++)
                {
                    drawRays[index]._color = Color.Blue;
                    for (int l = 0; l < renderDistance.X * 2; l++)
                    {
                        IntTriple tileIndex = Tile.getTileIndex(drawRays[index].Update());
                        if (!tileIndex.inBound(clampMin, clampMax))
                        {
                            break;
                        }
                        Tile tile = World.GetTileFromIndex(tileIndex);
                        if (tile.Isfull)
                        {
                            drawRays[index]._color = (tile.color) * ((float)(500f - drawRays[index].lowestDistance) / 500f);
                            break;
                        }
                    }
                }
            });
        }
        bool firstRayCast = false;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //a relic of old visualization code, not needed anymore
            //offset = loadDistance.XY - _player.XY - new Vector2(_player.Cube.XSize / 2, _player.Cube.YSize / 2); 


            if (drawTimer.IsActive && false)
            {
                drawTimer.Reset();
                frameCount++;
                //raycasting starts here
                if (CastRays().IsCompleted)
                firstRayCast = true;

                //if (gameTime.TotalGameTime.Seconds < 0.5 || true)
                //{
                //}
            }
            int rayIndex = 0;
            _spriteBatch.Begin();
            //draws tiles from previous raycasting
            if (firstRayCast)
                for (int x = 0; x < resolution.X; x++)
                {
                    for (int y = 0; y < resolution.Y; y++)
                    {
                        _spriteBatch.Draw(Tile._texture,
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
            Tile tile = World.GetTileFromWorldPos(_player.Position);
            _spriteBatch.DrawString(_font, (tile.position.X / Tile.ZSize).ToString(), new(), Color.White);
            _spriteBatch.DrawString(_font, (tile.position.Y / Tile.YSize).ToString(), new(0, _font.LineSpacing), Color.White);
            _spriteBatch.DrawString(_font, (tile.position.Z / Tile.XSize).ToString(), new(0, _font.LineSpacing * 2), Color.White);
            _spriteBatch.DrawString(_font, _player.Position.X.ToString(), new(0, _font.LineSpacing * 4), Color.White);
            _spriteBatch.DrawString(_font, _player.Position.Y.ToString(), new(0, _font.LineSpacing * 5), Color.White);
            _spriteBatch.DrawString(_font, _player.Position.Z.ToString(), new(0, _font.LineSpacing * 6), Color.White);
            _spriteBatch.DrawString(_font, World.GetChunkIndex(_player.Position).X.ToString(), new(0, _font.LineSpacing * 7), Color.White);
            _spriteBatch.DrawString(_font, World.GetChunkIndex(_player.Position).Z.ToString(), new(0, _font.LineSpacing * 8), Color.White);
            _spriteBatch.DrawString(_font, displayfps.ToString(), new(0, _font.LineSpacing * 3), Color.White);

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
                if (hotbar.getItemValue(index) != storageInfo.ItemValue.Empty)
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
                            if (inventory.getItemValue(index) != storageInfo.ItemValue.Empty)
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
                if (mouseHeldItem.getItemValue(0) != storageInfo.ItemValue.Empty)
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

