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
        KeyboardState _keyboardState = new();
        MouseState _mouseState = new();

        static Vector2 screenSize = new();
        static readonly Point TileArray = new Point(1000, 1000);

        //draw
        Vector2 offset;
        Point PlayerTileIndex;

        private Tiles[][] _Tiles = new Tiles[TileArray.Y][];
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
            for (int x = 0; x < _Tiles.Length; x++)
            {
                _Tiles[x] = new Tiles[TileArray.X];
            }
                        
            for (int x = 0; x < TileArray.Y; x++)
            {
                for (int y = 0; y < TileArray.X; y++)
                {
                    _Tiles[x][y] = new Tiles(new (x, y));
                }
            }
            _player = new Player(_Tiles[TileArray.X / 2][TileArray.Y / 2].GetRectangle().Center);

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

            _player.update(_keyboardState);
            if (_mouseState.LeftButton == ButtonState.Pressed)
            {
                (Tiles.getTile(new(_mouseState.X - offset.X, _mouseState.Y - offset.Y), _Tiles)).MineTile(3);
            }
            
            foreach (Tiles tile in Tiles.getLoaded(_player.GetPosition(), new((int)screenSize.X/2, (int)screenSize.Y / 2), TileArray, _Tiles))
            {
                tile.Update();
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            offset = screenSize / 2 -_player.GetPosition();
            _spriteBatch.Begin();
            foreach (Tiles tile in Tiles.CollidingTiles(new((int)(_player.GetPosition().X-screenSize.X/2), (int)(_player.GetPosition().Y-screenSize.Y / 2), (int)screenSize.X, (int)screenSize.Y), TileArray, _Tiles))
            {
                tile.draw(_spriteBatch, offset);
            }
            //foreach (Tiles tile in Tiles.CollidingTiles(_player.GetRectangle(), TileArray, _Tiles))
            //{
            //    tile.draw(_spriteBatch, offset);
            //}
            //foreach (Tiles tile in Tiles.getLoaded(_player.GetPosition(), new((int)screenSize.X / 2, (int)screenSize.Y / 2), TileArray, _Tiles))
            //{
            //    tile.draw(_spriteBatch, offset);
            //}
            _player.Draw(_spriteBatch, screenSize);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
