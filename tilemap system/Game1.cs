using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using System.Reflection;
using System.Reflection.Metadata;
namespace tilemap_system
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        KeyboardState _keyboardState, _PreviouskeyboardState;
        MouseState _mouseState, _previousMouseState;
        Random _random = new Random();
        SpriteFont _font;

        private readonly IntTriple renderDistance = new(400, 400, 400);
        static readonly IntTriple TileArray = new(200, 200, 200);

        /* mouse Control */
        bool previousIsActive;
        float sensitivity = .035f;

        /* draw */
        Vector2 offset;
        Vector2 screenSize;
        List<Ray> drawRays = new List<Ray>();
        Vector2 FOV = new(3, 3);
        Point resolution = new(60, 60);

        //Point PlayerTileIndex;
        private List<Tiles> loadedTiles = new List<Tiles>();
        private Tiles[][][] _Tiles = new Tiles[TileArray.X][][];
        private Player _player;
        private Texture2D square;

        public enum Keybind
        {
            up = Keys.W,
            down = Keys.S,
            Left = Keys.A,
            Right = Keys.D,
            Jump = Keys.Space,
            sneak = Keys.LeftShift
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _font = Content.Load<SpriteFont>("myFont");

            screenSize = new(
                renderDistance.X * 2,
                renderDistance.Y * 2
                );
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)renderDistance.X * 2;
            _graphics.PreferredBackBufferHeight = (int)renderDistance.Y * 2;
            _graphics.ApplyChanges();

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
                            _Tiles[x][y][z] = new Tiles(x, y, z, Tiles.ID.Full);
                        }


                    }
                }
            }
            _player = new Player(_Tiles[TileArray.X / 2][TileArray.Y / 3][TileArray.Z / 2].Cube.Position);
            //_player = new Player(new(100, -100, 0));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Tiles.SetTexture(Content.Load<Texture2D>("square"));
            Player.SetTexture(Content.Load<Texture2D>("square"));
            square = Content.Load<Texture2D>("square");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            _mouseState = Mouse.GetState();
            _keyboardState = Keyboard.GetState();
            loadedTiles = Tiles.getLoaded(_player.Position, renderDistance, TileArray, _Tiles);

            //if (_mouseState.LeftButton == ButtonState.Pressed)
            //{
            //    foreach (Tiles tile in Tiles.CollidingTiles(new(_mouseState.X, _mouseState.Y, -100, 1, 1, 1000), TileArray, _Tiles))
            //        (tile).MineTile(3);
            //}

            if (General.OnRelease(_mouseState, _previousMouseState))
            {
                foreach (Tiles tile in loadedTiles)
                {
                    if (tile.Isfull)
                        tile.Update();
                }
            }
            /* enable for jumping to activate */
            //foreach (Tiles tile in Tiles.CollidingTiles(new((int)_player.Position.X, _player.Cube.Y_OP, (int)_player.Position.Z, _player.Cube.XSize, 3, _player.Cube.ZSize), TileArray, _Tiles))
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

            if (previousIsActive)
                _player._angle.X += (Mouse.GetState().X - screenSize.X / 2) * sensitivity;
            _player._angle.Y += (Mouse.GetState().Y - screenSize.Y / 2) * sensitivity;
            if (IsActive)
                Mouse.SetPosition((int)screenSize.X / 2, (int)screenSize.Y / 2);
            previousIsActive = IsActive;

            if (_player._angle.X > (float)(2 * Math.PI))
                _player._angle.X = _player._angle.X % (float)(2 * Math.PI);
            if (_player._angle.X < 0)
                _player._angle.X += 2 * (float)Math.PI;

            _player._angle.Y = MathHelper.Clamp(_player._angle.Y, -MathF.PI / 3f, MathF.PI / 3f);

            Ray ray = new(_player._angle, _player.Cube.Center);

            for (int l = 0; l < 100; l++)
            {
                ray.update(1);
                IntTriple tileIndex = General.clamp(Tiles.getTileIndex(ray._position), new(), TileArray);
                if (_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].Isfull)
                {
                    _Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].Update();

                    _Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z]._color = Color.AliceBlue;

                    if (General.OnPress(_mouseState, _previousMouseState))
                        _Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].MineTile(1000);


                    break;
                }
            }



            /* Does Player Collision & movement */
            _player.move(new(_player.Speed.X, 0, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionX(tile.Cube); }
                //(tile).MineTile(15);
            }
            _player.move(new(0, _player.Speed.Y, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionY(tile.Cube); }
                //(tile).MineTile(3);

            }
            _player.move(new(0, 0, _player.Speed.Z));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionZ(tile.Cube); }
                //(tile).MineTile(15);

            }
            /* updates Player._speed with movement and stuffn */
            _player.update(_keyboardState, _PreviouskeyboardState);


            _previousMouseState = _mouseState;
            _PreviouskeyboardState = _keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            offset = renderDistance.XY - _player.XY - new Vector2(_player.Cube.XSize / 2, _player.Cube.YSize / 2);

            //foreach (Tiles tile in Tiles.g set);

            //raycasting starts here


            int width = (int)screenSize.X / resolution.X;
            int height = (int)screenSize.Y / resolution.Y;
            if (_player.Speed != Vector3.Zero || gameTime.TotalGameTime.Seconds < 1)
            {
                drawRays.Clear();

                for (int x = -resolution.X / 2; x < resolution.X / 2; x++)
                {
                    for (int y = -resolution.Y / 2; y < resolution.Y / 2; y++)
                    {
                        drawRays.Add(new Ray(new Vector2(_player._angle.X + (float)(x) / resolution.X / 2 * FOV.X, _player._angle.Y + (float)(y) / resolution.Y / 2 * FOV.Y), _player.Cube.Center));
                    }
                }

                for (int index = 0; index < drawRays.Count; index++)
                {
                    drawRays[index]._color = Color.Blue;
                    for (int l = 0; l < 500; l++)
                    {
                        drawRays[index].update(1);
                        IntTriple tileIndex = General.clamp(Tiles.getTileIndex(drawRays[index]._position), new(0, 0, 0), TileArray);
                        if (_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z].Isfull)
                        {
                            drawRays[index]._color = (_Tiles[tileIndex.X][tileIndex.Y][tileIndex.Z]._color) * ((float)(500f - drawRays[index].DistanceTraveled) / 500f);
                            break;
                        }
                    }
                }
            }

            int rayIndex = 0;
            _spriteBatch.Begin();
            for (int x = 0; x < resolution.X; x++)
            {
                for (int y = 0; y < resolution.Y; y++)
                {
                    _spriteBatch.Draw(Tiles._texture,
                        new Rectangle(
                            x * width,
                            y * height,
                            width,
                            height
                            ),
                        null,
                        drawRays[rayIndex]._color
                        );
                    rayIndex += 1;
                }
            }
            _spriteBatch.DrawString(_font, Tiles.getTileIndex(_player.Position).X.ToString(), new(), Color.White);
            _spriteBatch.DrawString(_font, Tiles.getTileIndex(_player.Position).Y.ToString(), new(0, _font.LineSpacing), Color.White);
            _spriteBatch.DrawString(_font, Tiles.getTileIndex(_player.Position).Z.ToString(), new(0, _font.LineSpacing + _font.LineSpacing), Color.White);
            _spriteBatch.DrawString(_font, _player._angle.X.ToString(), new(0, _font.LineSpacing * 3), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
