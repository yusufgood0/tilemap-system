using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.VisualBasic;
namespace tilemap_system
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        KeyboardState _keyboardState, _PreviouskeyboardState;
        MouseState _mouseState, _previousMouseState;

        private IntTriple renderDistance = new(400, 400, 400);
        static readonly IntTriple TileArray = new(200, 200, 200);

        //draw
        Vector2 offset;
        Point PlayerTileIndex;

        private Tiles[][][] _Tiles = new Tiles[TileArray.X][][];
        private Player _player;
        private Texture2D square;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            //screenSize = new(
            //    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            //    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
            //    );
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)renderDistance.X*2;
            _graphics.PreferredBackBufferHeight = (int)renderDistance.Y*2;
            _graphics.ApplyChanges();
            // TODO: Add your initialization logic here

            //set up tiles
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
                        _Tiles[x][y][z] = new Tiles(new(x, y, z));
                    }
                }
            }
            _player = new Player(_Tiles[TileArray.X / 2][0][TileArray.Z / 2].Cube.Position);
            //_player = new Player(new(100, -100, 0));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Tiles.setTexture(Content.Load<Texture2D>("square"));
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

            //if (_mouseState.LeftButton == ButtonState.Pressed)
            //{
            //    foreach (Tiles tile in Tiles.CollidingTiles(new(_mouseState.X, _mouseState.Y, -100, 1, 1, 1000), TileArray, _Tiles))
            //        (tile).MineTile(3);
            //}
            if (General.OnRelease(_mouseState, _previousMouseState))
            {
                foreach (Tiles tile in Tiles.getLoaded(_player.Position, renderDistance, TileArray, _Tiles))
                {
                    tile.Update();
                }
            }
            foreach (Tiles tile in Tiles.CollidingTiles(new((int)_player.Position.X, _player.Cube.Y_OP, (int)_player.Position.Z, _player.Cube.XSize, 3, _player.Cube.ZSize), TileArray, _Tiles))
            {
                if (tile.Isfull)
                {
                    if (General.OnPress(_keyboardState, _PreviouskeyboardState, Keys.Space))
                    {
                        _player.jump();
                        break;
                    }
                }
            }

            //Does Player Collision & movement
            _player.move(new(_player.Speed.X, 0, 0)); 
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionX(tile.Cube); }
                (tile).MineTile(15);
            }
            _player.move(new(0, _player.Speed.Y, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionY(tile.Cube); }
                (tile).MineTile(3);

            }
            _player.move(new(0, 0, _player.Speed.Z));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Cube, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionZ(tile.Cube); }
                (tile).MineTile(15);

            }

            _player.update(_keyboardState, _PreviouskeyboardState); // updates Player._speed with movement and stuff


            _previousMouseState = _mouseState;
            _PreviouskeyboardState = _keyboardState;
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            offset = renderDistance.XY - _player.XY - new Vector2(_player.Cube.XSize/2, _player.Cube.YSize / 2);
            _spriteBatch.Begin();
            //foreach (Tiles tile in Tiles.CollidingTiles(new((int)(_player..X - screenSize.X / 2), (int)(_player.XY.Y - screenSize.Y / 2), (int)screenSize.X, (int)screenSize.Y), TileArray, _Tiles))
            //{
            //    if (tile.Isfull)
            //    {
            //        tile.draw(_spriteBatch, offset);
            //    }
            //}
            foreach (Tiles tile in Tiles.getLoaded(_player.Position, renderDistance, TileArray, _Tiles))
            {
                //if(tile.Cube.Z == 0)
                if (tile.Cube.Z < _player.Cube.Z)
                    tile.draw(_spriteBatch, offset);
            }
            _player.Draw(_spriteBatch, offset);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
