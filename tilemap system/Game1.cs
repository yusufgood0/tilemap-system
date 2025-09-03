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
using System.Threading;
using System.Linq.Expressions;
using System.IO;
using System.Diagnostics;
namespace tilemap_system
{
    public class Game1 : Game
    {

        /* Game Setup */
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        KeyboardState _keyboardState, _PreviouskeyboardState;
        MouseState _mouseState, _previousMouseState;
        public readonly Random _random = new();
        public static SpriteFont _font;

        /* Debug */
        private static readonly string _logDirectory = Path.Combine(Environment.CurrentDirectory, "HomemadeMinecraft", "log.txt");
        //private static readonly StreamWriter _logWriter = new(_logDirectory, true);

        /* Mouse Control */
        bool _previousIsMouseVisible;
        float sensitivity = .01f;

        /* Draw */
        const int fps = 60;
        static readonly Stopwatch drawTimer = new(1000 / fps); // in milliseconds
        static object frameCountLock = new();
        static int frameCount = 0;
        static readonly Stopwatch fpsCheckInterval = new(1000); // 1 second timer
        static int displayfps; // used for displaying fps in the game

        /* Crosshair */
        static Texture2D crosshair;
        static Vector2 crosshairDrawPos;
        const float crosshairScale = 0.1f;

        /* Raycasting */
        static readonly int renderDistance = 100; // In Tiles
        const float cameraHeight = 60;
        static Vector3 eyePosition;
        static DDA_ray[,] drawRays = new DDA_ray[resolution.X, resolution.Y];

        /* General Drawing */
        public static Point screenSize;
        public static Vector2 FOV = new(2, 2);

        /* Resolution and pixel size */
        static readonly Point resolution = new(128, 80); //in pixels (cannot be an odd number) ratio 8:5 recommended
        int pixelWidth = 10;
        int pixelHeight = 10;

        /* Inventory */
        Container[] _activeContainers; // used for storing the containers and their rectangles for drawing and interaction
        Rectangle[][] _activeContainerRects; // used for storing the containers and their rectangles for drawing and interaction

        // Hand size for drawing
        int helditemSize;


        // Container Draw
        const int hotbarSize = 80;
        int rowCount;


        // Hotbar
        int selectedItem;
        Container hotbar;
        Rectangle[] hotbarRects;

        // Inventory
        Container inventory;
        Rectangle[] inventoryRects;

        // Container
        // Container will be provided on a use case scenario (ex: chests)
        Rectangle[] containerRects;

        // Item held by mouse for interaction
        ItemSlot mouseHeldItem;

        /* Tile Interactions*/
        private static IntTriple _selectedTile = new();
        private static object _selectedTileLock = new();
        private static object _mineTimeLock = new(); //how long the player has been mining a Tile
        private static Stopwatch _mineTime = new(); //how long the player has been mining a Tile
        private static Stopwatch _PlaceBlockTimer = new(200); //used for timing tile placement
        private static float _minePercentage = 1; //percentage of the tile mined, used for visualizing mining progress

        /* Loading Objects */
        static Player _player;
        public static Texture2D _square;
        static List<DroppedItem> _droppedItems = new List<DroppedItem>();
        static Slider[] _menuSliders = new Slider[2];

        /* for input */
        float scrollwheelChange;

        GameState gameState = GameState.Playing;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _player.SavePosition();
                World.SaveLoadedChunks();
            }
            base.Dispose(disposing);
        }
        public static void Log(string message)
        {

            File.WriteAllText(_logDirectory, message); // Clears the log file before writing new messages
            //_logWriter.WriteLine($"{DateTime.Now}: {message}");
            //_logWriter.Flush();
        }
        protected override void Initialize()
        {
            if (File.Exists(_logDirectory))
            {
                File.Delete(_logDirectory); // Deletes the log file if it exists
            }
            else
            {
                Directory.CreateDirectory(_logDirectory);
            }
            File.Create(_logDirectory).Close(); // Creates a new log file

            _font = Content.Load<SpriteFont>("myFont");

            screenSize = new(
                resolution.X * pixelWidth,
                resolution.Y * pixelHeight
                );
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)screenSize.X;
            _graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            _graphics.ApplyChanges();

            drawRays = new DDA_ray[resolution.X, resolution.Y];

            helditemSize = (int)(screenSize.Y * 0.4);

            _player = new Player(Tile.GetPosition(1000, 0, 1000));
            World.Initilize(_player.Position);

            Thread ChunkLoadingThread = new(Game1.ChunkLoadingThread);
            ChunkLoadingThread.IsBackground = true; // Makes it a background thread
            ChunkLoadingThread.Start();

            Thread renderingThread = new(RenderingThread);
            renderingThread.IsBackground = true; // Makes it a background thread
            renderingThread.Start();

            //_player = new Player(new(100, -100, 0));

            base.Initialize();
        }
        protected override void LoadContent()
        {
            crosshair = Content.Load<Texture2D>("minecraftCrosshair");
            crosshairDrawPos = new Vector2((int)(screenSize.X / 2 - crosshair.Width / 2 * crosshairScale), (int)(screenSize.Y / 2 - crosshair.Height / 2 * crosshairScale));
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Player.SetTexture(Content.Load<Texture2D>("square"));
            _square = Content.Load<Texture2D>("square");


            TileInfo.Initialize(); // Initializes the tile info
            Item.Initialize(GraphicsDevice, _square); // Initializes the item info

            using (Stream stream = File.Create("myTexture"))
            {
                _square.SaveAsPng(stream, _square.Width, _square.Height);
                stream.Flush();
            }

            selectedItem = 0;
            hotbar = new(9);
            inventory = new(27);
            mouseHeldItem = new();

            Point InventoryDrawPosition = new Point((int)(screenSize.X / 2 - (hotbarSize * hotbar.GetSlotCount) / 2), (int)(screenSize.Y - hotbarSize * 4));

            hotbarRects = new Rectangle[hotbar.GetSlotCount];
            for (int x = 0; x < hotbar.GetSlotCount; x++)
            {
                hotbarRects[x] = new Rectangle(InventoryDrawPosition.X + x * hotbarSize, InventoryDrawPosition.Y + hotbarSize * 3, hotbarSize, hotbarSize);
            }

            rowCount = inventory.GetSlotCount / hotbar.GetSlotCount;
            inventoryRects = new Rectangle[inventory.GetSlotCount];
            int index = 0;
            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < hotbar.GetSlotCount; x++)
                {
                    inventoryRects[index] = new Rectangle(InventoryDrawPosition.X + x * hotbarSize, InventoryDrawPosition.Y + y * hotbarSize - 10, hotbarSize, hotbarSize);
                    index++;
                }
            }
            containerRects = new Rectangle[inventory.GetSlotCount];
            index = 0;
            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < hotbar.GetSlotCount; x++)
                {
                    containerRects[index] = new Rectangle(InventoryDrawPosition.X + x * hotbarSize, InventoryDrawPosition.Y + y * hotbarSize - 20 - hotbarSize * 3, hotbarSize, hotbarSize);
                    index++;
                }
            }

            _activeContainerRects = new Rectangle[][] { hotbarRects };
            _activeContainers = new Container[] { hotbar };

            _menuSliders[0] = new Slider(_square,
                new((int)(Game1.screenSize.X * .6f), (int)(Game1.screenSize.Y * .4f), (int)(Game1.screenSize.X * .3f), (int)(Game1.screenSize.Y * .05f)),
                0.5f,
                2,
                Color.Black,
                Color.Red,
                "FOV",
                1f,
                10
                );
            _menuSliders[1] = new Slider(_square,
                new((int)(Game1.screenSize.X * .1f), (int)(Game1.screenSize.Y * .4f), (int)(Game1.screenSize.X * .3f), (int)(Game1.screenSize.Y * .05f)),
                0.002f,
                0.02f,
                Color.Black,
                Color.Red,
                "Sensitivity",
                1f,
                5000
                );
            // TODO: use this.Content to load your game content here
        }
        internal void Play(out GameState gameState)
        {
            gameState = GameState.Playing;
            IsMouseVisible = false;

            _activeContainerRects = new Rectangle[][] { hotbarRects };
            _activeContainers = new Container[] { hotbar };
        }
        internal void Pause(out GameState gameState)
        {
            gameState = GameState.Paused;
            IsMouseVisible = true;
        }
        private bool GetSelectedTile(ref IntTriple selectedTile, out Ray ray, int reach)
        {
            selectedTile = new();
            ray = new(_player._angle, eyePosition);
            for (int l = 0; l < reach; l++)
            {
                if (World.GetTileCopyFromIndex(Tile.GetTileIndex(ray._position), out Tile tile) && tile.Isfull)
                {
                    selectedTile = Tile.GetTileIndex(ray._position);
                    return true;
                }
                ray.update(1);
            }
            return false;
        }
        private void PlaceBlockFromHotbar(IntTriple index)
        {
            Tile.TileID? newBlockID = hotbar.GetSlot(selectedItem).ItemType.ItemBlockID;
            if (!Tile.CollidingTilesTriple(_player.Cube).Contains(index) && newBlockID != null) //makes sure your not placing a null value
            {
                ref Tile tile = ref World.GetTileRefFromIndex(index);
                if (!tile.Isfull)
                {
                    tile.SetType((Tile.TileID)newBlockID);
                    hotbar.GetSlot(selectedItem) = hotbar.GetSlot(selectedItem) - 1;
                }
            }
        }
        protected override void Update(GameTime gameTime)
        {
            ref ItemSlot selectedSlot = ref hotbar.GetSlot(selectedItem);

            eyePosition = new Vector3(_player.Cube.Center.X, _player.Cube.Y_OP - cameraHeight, _player.Cube.Center.Z);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            _mouseState = Mouse.GetState();
            _keyboardState = Keyboard.GetState();

            Item.SaveTextures();

            scrollwheelChange = _mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollwheelChange != 0)
            {
                if (scrollwheelChange > 0)
                {
                    selectedItem = General.Bound(selectedItem + 1, hotbar.GetSlotCount);
                }
                else if (scrollwheelChange < 0)
                {
                    selectedItem = General.Bound(selectedItem - 1, hotbar.GetSlotCount);
                }
            }

            /* opens inventory on button press */
            if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.inventory))
            {
                if (gameState == GameState.Playing)
                {
                    gameState = GameState.Inventory;
                    IsMouseVisible = true;
                    _activeContainerRects = new Rectangle[][] { hotbarRects, inventoryRects };
                    _activeContainers = new Container[] { hotbar, inventory };
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

            _player.SafeControlAngleWithMouse(_previousIsMouseVisible, IsMouseVisible, screenSize, sensitivity);

            /* enable for jumping to activate */
            if (Tile.IsCollision(new((int)_player.Position.X, _player.Cube.Y_OP, (int)_player.Position.Z, _player.Cube.XSize, 3, _player.Cube.ZSize)))
            {
                if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.Jump))
                {
                    _player.Jump();
                }
            }

            /*clamps player angles */
            General.Bound(ref _player._angle.X, (float)Math.Tau);
            _player._angle.Y = MathHelper.Clamp(_player._angle.Y, -MathF.PI / 2f, MathF.PI / 2f);

            /* checks inventory interaction when inventory is open */
            if (gameState == GameState.Inventory)
            {
                Container.CheckInteraction(
                    ref mouseHeldItem,
                    ref _activeContainers,
                    ref _activeContainerRects,
                    _mouseState,
                    _previousMouseState);
                hotbar = _activeContainers[0];
                inventory = _activeContainers[1];
            }

            var pickupContainers = new Container[] { hotbar, inventory };
            lock (_droppedItems)
            {
                for (int i = 0; i < _droppedItems.Count; i++)
                {
                    _droppedItems[i].Physics();
                    if (_droppedItems[i].GetLifespan > 300000) // 300000ms is 5 minutes
                    {
                        _droppedItems.RemoveAt(i--);
                        continue;
                    }
                    float distance = Vector3.Distance(_droppedItems[i].Position, _player.Cube.Center);
                    if (distance < 80 && _droppedItems[i].GetLifespan > 1000)
                    {
                        if (_droppedItems[i].Pickup(ref pickupContainers))
                        {
                            _droppedItems.RemoveAt(i--);
                            continue;
                        }
                    }
                }
            }
            hotbar = pickupContainers[0];
            inventory = pickupContainers[1];

            if (gameState == GameState.Playing)
            {
                /* updates what tile you have selected, does tile mining and placing */
                lock (_selectedTileLock)
                {
                    if (GetSelectedTile(ref _selectedTile, out Ray ray, 160))
                    {
                        ref Tile tile = ref World.GetTileRefFromIndex(_selectedTile);
                        /* does mining tiles */
                        lock (_mineTimeLock)
                        {
                            if (General.OnLeftPress(_mouseState, _previousMouseState) && _player.IsCreative)
                            {
                                (tile).MineTile(Int32.MaxValue, Item.GetItem(ItemValue.Empty), out _minePercentage);
                            }
                            else if (_mouseState.LeftButton == ButtonState.Pressed && _player.IsSurvival)
                            {
                                if (tile.MineTile(_mineTime.GetTimeMilliseconds(), selectedSlot.ItemType, out _minePercentage))
                                {
                                    _minePercentage = 0;
                                    _mineTime.Reset();
                                }
                            }
                            else
                            {
                                _minePercentage = 0;
                                _mineTime.Reset();
                            }
                        }
                        /* on right button press, moves the ray back one unit and fills the block its in with block from your hotbar */
                        if (_mouseState.RightButton == ButtonState.Pressed && (_PlaceBlockTimer.IsActive || _previousMouseState.RightButton == ButtonState.Released))
                        {
                            //if (tile.GetTileInfo.)
                            _PlaceBlockTimer.Reset();
                            ray.reverse(1);
                            IntTriple tileIndex = Tile.GetTileIndex(ray._position);
                            PlaceBlockFromHotbar(tileIndex);

                        }
                    }
                }
                if (General.OnPress(_keyboardState, _PreviouskeyboardState, (Keys)Keybind.DropItem) && selectedSlot.Amount > 0)
                {
                    if (_keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        lock (_droppedItems)
                        {
                            _droppedItems.Add(selectedSlot.Drop(eyePosition, _player.dirVector * 4));
                        }
                    }
                    else
                    {
                        lock (_droppedItems)
                        {
                            _droppedItems.Add(selectedSlot.DropOne(eyePosition, _player.dirVector * 4));
                        }
                    }
                }
            }
            else
            {
                _mineTime.Reset();
            }


            /* Does Player Collision & movement */
            _player.Move(new(_player.Speed.X, 0, 0));
            foreach (IntTriple index in Tile.CollidingTilesTriple(_player.Cube))
            {
                if (World.GetTileCopyFromIndex(index, out Tile tile) && tile.Isfull)
                {
                    _player.CollisionX(Tile.GetCube(index));
                }
            }
            _player.Move(new(0, _player.Speed.Y, 0));
            foreach (IntTriple index in Tile.CollidingTilesTriple(_player.Cube))
            {
                if (World.GetTileCopyFromIndex(index, out Tile tile) && tile.Isfull)
                {
                    _player.CollisionY(Tile.GetCube(index));
                }
            }
            _player.Move(new(0, 0, _player.Speed.Z));
            foreach (IntTriple index in Tile.CollidingTilesTriple(_player.Cube))
            {
                if (World.GetTileCopyFromIndex(index, out Tile tile) && tile.Isfull)
                {
                    _player.CollisionZ(Tile.GetCube(index));
                }
            }

            /* updates Player._speed with movement and checks for keypresses etc */
            _player.update(_keyboardState);

            /* makes sure the sliders are checking if theyre being pressed */
            if (gameState == GameState.Paused)
            {
                _menuSliders[0].SliderUpdate(_mouseState, _previousMouseState, ref FOV.X);
                _menuSliders[0].SliderUpdate(_mouseState, _previousMouseState, ref FOV.Y);
                _menuSliders[1].SliderUpdate(_mouseState, _previousMouseState, ref sensitivity);
            }

            if (fpsCheckInterval.IsActive)
            {
                displayfps = (int)Math.Round(frameCount / fpsCheckInterval.GetTimeSeconds(), 1);
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
        static void ChunkLoadingThread()
        {
            while (true)
            {
                World.LoadChunks(_player.Position);
                Thread.Sleep(10);
            }
        }
        static void RenderingThread()
        {
            while (true)
            {
                lock (frameCountLock)
                {
                    frameCount++;
                }
                lock (_droppedItems)
                {
                    for (int i = 0; i < _droppedItems.Count; i++)
                    {
                        _droppedItems[i].UpdateDrawInfo(
                            screenSize,
                            FOV.X,
                            eyePosition,
                            _player._angle.Y,
                            _player._angle.X
                            );
                    }
                }
                var startTime = DateTime.Now;
                CastRays();
                Debug.WriteLine((DateTime.Now-startTime).TotalMilliseconds);
                Thread.Sleep(1000 / fps);
            }
        }


        static Point halfResolution = new(resolution.X / 2, resolution.Y / 2);


        static unsafe void CastRays()
        {
            Matrix rotYaw = Matrix.CreateRotationY(_player._angle.X - (float)Math.PI);
            Matrix rotPitch = Matrix.CreateRotationX(_player._angle.Y);
            Matrix viewRot = rotPitch * rotYaw;

            float fX = (resolution.X / 2f) / (float)Math.Tan(FOV.X / 2f);
            float fY = (resolution.Y / 2f) / (float)Math.Tan(FOV.Y / 2f);
            //IntTriple clampMax = renderDistance + Tile.getTileIndex(_player.Position);
            //IntTriple clampMin = -renderDistance + Tile.getTileIndex(_player.Position);

            // Pre-calculate all rays first
            DDA_ray[] tempRays = new DDA_ray[resolution.X * resolution.Y]; // One dimentional for less bound checks
            for (int x = -halfResolution.X; x < halfResolution.X; x++)
            {
                for (int y = -halfResolution.Y; y < halfResolution.Y; y++)
                {
                    Vector3 rayDir = new(-x, y * (fX / fY), -fX);
                    rayDir.Normalize();
                    Vector3 worldDir = Vector3.Transform(rayDir, viewRot);
                    tempRays[(y + halfResolution.Y) * resolution.X + (x + halfResolution.X)] = (new DDA_ray(eyePosition, eyePosition + worldDir));
                }
            }

            // load in a copy of selectedTile
            IntTriple selectedTile;
            lock (_selectedTileLock)
            {
                selectedTile = _selectedTile;
            }

            // load in a copy of your minepercentage
            float MineTimeValue;
            lock (_mineTimeLock)
            {
                MineTimeValue = 1.001f - _minePercentage;
            }

            Parallel.For(0, resolution.X, x =>
            //for (int x = 0; x < resolution.X; y++)
            {
                //Parallel.For(0, resolution.Y, x =>
                for (int y = 0; y < resolution.Y; y++)
                {
                    int index = y * resolution.X + x;
                    fixed (DDA_ray* ray = &tempRays[index])
                    {
                        for (int l = 0; l < renderDistance; l++)
                        {
                            IntTriple TileIndex = Tile.GetTileIndex(ray->Update());
                            if (World.GetTileCopyFromIndex(TileIndex, out Tile tile))
                            {
                                if (tile.Isfull)
                                {
                                    ray->_color = General.Multiply(tile.Color, (500f - MathF.Sqrt(ray->LowestDistanceSquared)) / 100000f);

                                    if (TileIndex == selectedTile)
                                    {
                                        ray->_color *= MineTimeValue;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                //);
            }
            );

            lock (drawRays)
            {
                for (int y = 0; y < resolution.Y; y++)
                {
                    for (int x = 0; x < resolution.X; x++)
                    {
                        drawRays[x, y] = tempRays[y * resolution.X + x];
                    }
                }
            }
            firstRayCast = true;
        }
        static bool firstRayCast = false;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.FrontToBack);

            //draws tiles from previous raycasting
            lock (drawRays)
            {
                if (firstRayCast)
                    for (int x = 0; x < resolution.X; x++)
                    {
                        for (int y = 0; y < resolution.Y; y++)
                        {
                            _spriteBatch.Draw(_square,
                            new Rectangle(
                                x * pixelWidth,
                                y * pixelHeight,
                                pixelWidth,
                                pixelHeight
                                ),
                            null,
                            drawRays[x, y]._color,
                            0,
                            new Vector2(),
                            SpriteEffects.None,
                            DepthLayers.WorldMin + DepthLayers.WorldMax / drawRays[x, y].LowestDistanceSquared
                            );
                        }
                    }
            }
            //draws crosshair
            _spriteBatch.Draw(crosshair, crosshairDrawPos, null, Color.White, 0, new Vector2(), crosshairScale, 0, 1);

            //draws debug
            _spriteBatch.DrawString(_font, ((int)(_player.Position.X / Tile.Size)).ToString(), new(), Color.White);
            _spriteBatch.DrawString(_font, ((int)(_player.Position.Y / Tile.Size)).ToString(), new(0, _font.LineSpacing), Color.White);
            _spriteBatch.DrawString(_font, ((int)(_player.Position.Z / Tile.Size)).ToString(), new(0, _font.LineSpacing * 2), Color.White);
            _spriteBatch.DrawString(_font, _player.Position.X.ToString(), new(0, _font.LineSpacing * 4), Color.White);
            _spriteBatch.DrawString(_font, _player.Position.Y.ToString(), new(0, _font.LineSpacing * 5), Color.White);
            _spriteBatch.DrawString(_font, _player.Position.Z.ToString(), new(0, _font.LineSpacing * 6), Color.White);

            /* writes selected tile to screen for debugging */
            if (World.GetTileCopyFromIndex(_selectedTile, out Tile tile))
            {
                _spriteBatch.DrawString(_font, (tile.GetType).ToString(), new(0, _font.LineSpacing * 3), Color.White);
            }

            /* draw held item */
            if (hotbar.GetSlot(selectedItem).ItemType.ItemTexture != null)
            {
                _spriteBatch.Draw(hotbar.GetSlot(selectedItem).ItemType.ItemTexture, new Rectangle((int)screenSize.X - helditemSize / 2, (int)screenSize.Y - helditemSize / 2, helditemSize, helditemSize), null, Color.White, (float)Math.Tau * .95f, new(helditemSize / 2, helditemSize / 2), SpriteEffects.None, 1);
            }

            if (GameState.Inventory == gameState)
            {
                mouseHeldItem.DrawSlot(_spriteBatch, _font, new(_mouseState.X - hotbarSize / 2, _mouseState.Y - hotbarSize / 2, hotbarSize, hotbarSize));
            }

            /* draws all slots active and interactable item slots */
            Container.DrawAllSlots(
                _spriteBatch,
                _square,
                _font,
                ref _activeContainers,
                ref _activeContainerRects
            );

            /* draws selected hotbar slot to be brighter */
            _spriteBatch.Draw(_square, _activeContainerRects[0][selectedItem], null, Color.White * 0.5f, 0, new(), SpriteEffects.None, DepthLayers.SelectedItemHighlight);

            /* old code
            //draws hotbar
            for (int index = 0; index < hotbar.GetSlotCount; index++)
            {
                Color color = Color.Gray;
                if (selectedItem == index)
                {
                    color = Color.White;
                }
                if (Container.SlotsToProcess.Contains((1, index)))
                {
                    color = Color.Yellow;
                }
                _spriteBatch.Draw(square, hotbarRects[index], null, color, 0, new(), 0, 0.99f);
                if (hotbar.GetSlot(selectedItem).Amount > 0)
                {
                    _spriteBatch.Draw(hotbar.GetSlot(selectedItem).ItemValue.ItemTexture, hotbarRects[index], null, Color.White, 0, new(), 0, 1);
                    _spriteBatch.DrawString(_font, hotbar.GetSlot(selectedItem).Amount.ToString(), new(hotbarRects[index].X, hotbarRects[index].Y), Color.White);
                }

            }

            //draws inventory
            if (GameState.Inventory == gameState)
            {
                int index = 0;

                for (int y = 0; y < rowCount; y++)
                {
                    for (int x = 0; x < hotbar.GetSlotCount; x++)
                    {
                        if (index < inventory.GetSlotCount)
                        {
                            Color color = Color.Gray;
                            if (Container.SlotsToProcess.Contains((0, index)))
                            {
                                color = Color.Yellow;
                            }
                            _spriteBatch.Draw(square, inventoryRects[index], null, color, 0, new(), 0, 0.99f);
                            if (inventory.getItemValue(index) != StorageInfo.ItemValue.Empty)
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
                if (mouseHeldItem.getItemValue(0) != StorageInfo.ItemValue.Empty)
                {
                    _spriteBatch.Draw(mouseHeldItem.GetItemTexture(0), new(_mouseState.X - hotbarSize / 2, _mouseState.Y - hotbarSize / 2, hotbarSize, hotbarSize), null, Color.White * .5f, 0, new(), 0, 1);
                    _spriteBatch.DrawString(_font, mouseHeldItem.getStackCount(0).ToString(), new(_mouseState.X - hotbarSize / 2, _mouseState.Y - hotbarSize / 2), Color.White * .5f);
                }

            }
            */

            //draws pause menu
            if (GameState.Paused == gameState)
            {
                _spriteBatch.Draw(_square, new Rectangle(0, 0, screenSize.X, screenSize.Y), null, new Color(64, 64, 64, 64), 0, new(), 0, DepthLayers.PauseScreenFilter);
                _menuSliders[0].SliderDraw(_spriteBatch, FOV.X);
                _menuSliders[1].SliderDraw(_spriteBatch, sensitivity);
            }
            lock (_droppedItems)
            {
                for (int i = 0; i < _droppedItems.Count; i++)
                {
                    if (DDA_ray.CheckLineOfSight(_droppedItems[i].Position, _player.Position))
                    {
                        _droppedItems[i].Draw(_spriteBatch);
                    }
                }
            }
            _spriteBatch.End();

            base.Draw(gameTime);

        }
    }
}

