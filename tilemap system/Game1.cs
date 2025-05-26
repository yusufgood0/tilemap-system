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
        MouseState _mouseState = new();

        static Vector2 screenSize = new();
        static readonly IntTriple TileArray = new (10, 3, 3);

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
            screenSize = new(
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
                );
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = (int)screenSize.X;
            _graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            _graphics.ApplyChanges();
            // TODO: Add your initialization logic here

            //set up tiles
            for (int x = 0; x < TileArray.X; x++)
            {
                _Tiles[x] = new Tiles[TileArray.Y][];
                for (int y = 0; y < TileArray.Y; y++)
                {
                    _Tiles[x][y] = new Tiles[TileArray.Z];
                }
            }
            

            for (int x = 0; x < TileArray.X; x++)
            {
                for (int y = 0; y < TileArray.Y; y++)
                {
                    for (int z = 0; z < TileArray.Y; z++)
                    {
                        _Tiles[x][y][z] = new Tiles(new(x, y, z));
                    }
                }
            }
            //_player = new Player(_Tiles[TileArray.X / 2][0].Cube.Position);
            _player = new Player(new(100, -100, 0));
            
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

            if (_mouseState.LeftButton == ButtonState.Pressed)
            {
                (Tiles.getTile(new(_mouseState.X - offset.X, _mouseState.Y - offset.Y, 0), _Tiles)).MineTile(3);
            }
            
            foreach (Tiles tile in Tiles.CollidingTiles(new((int)(_player.XY.X - screenSize.X / 2), (int)(_player.XY.Y - screenSize.Y / 2), (int)screenSize.X, (int)screenSize.Y), TileArray, _Tiles))
            //foreach (Tiles tile in Tiles.getLoaded(_player.XY, new((int)screenSize.X/2, (int)screenSize.Y / 2), TileArray, _Tiles))
                {
                tile.Update();
            }
            foreach (Tiles tile in Tiles.CollidingTiles(new(_player.Rectangle.X, _player.Rectangle.Bottom, _player.Rectangle.Width, 3), TileArray, _Tiles))
            {
                if (tile.Isfull)
                {
                    if (General.OnPress(_keyboardState, _PreviouskeyboardState, Keys.Space)) { 
                        _player.jump();
                        break;
                    }
                }
            }


            _player.move(new(_player.Speed.X, 0, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Rectangle, TileArray, _Tiles))
            {
                if (tile.Isfull) { _player.CollisionX(tile.Cube); }
            }
            _player.move(new(0, _player.Speed.Y, 0));
            foreach (Tiles tile in Tiles.CollidingTiles(_player.Rectangle, TileArray, _Tiles)){
                if (tile.Isfull) { _player.CollisionY(tile.Cube); } 
            }
            //_player.move(new(0, 0, _player.Speed.Z));
            //foreach (Tiles tile in Tiles.CollidingTiles(_player.Rectangle, TileArray, _Tiles)){
            //    if (tile.Isfull) { _player.CollisionZ(tile.Cube); }
            //}

            _player.update(_keyboardState, _PreviouskeyboardState);



            _PreviouskeyboardState = _keyboardState;
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            offset = screenSize / 2 -_player.XY;
            _spriteBatch.Begin();
            foreach (Tiles tile in Tiles.CollidingTiles(new((int)(_player.XY.X - screenSize.X / 2), (int)(_player.XY.Y - screenSize.Y / 2), (int)screenSize.X, (int)screenSize.Y), TileArray, _Tiles))
            {
                if (tile.Isfull)
                {
                    tile.draw(_spriteBatch, offset);
                }
            }
            //foreach (Tiles tile in Tiles.getLoaded(_player.GetPosition(), new((int)screenSize.X / 2, (int)screenSize.Y / 2), TileArray, _Tiles))
            //{
            //    tile.draw(_spriteBatch, offset);
            //}
            _player.Draw(_spriteBatch, offset);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
