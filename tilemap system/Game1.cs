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
        static readonly Point TileArray = new Point(10000, 10000);
        static readonly int TileArrayX = 10000;
        static readonly int TileArrayY = 10000;

        //draw
        Vector2 offset;
        Point Range = new(10, 10);
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
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)screenSize.X;
            _graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            _graphics.ApplyChanges();
            // TODO: Add your initialization logic here

            //set up tiles
            for (int x = 0; x < _Tiles.Length; x++)
            {
                _Tiles[x] = new Tiles[TileArrayX];
            }
                        
            for (int x = 0; x < TileArrayY; x++)
            {
                for (int y = 0; y < TileArrayX; y++)
                {
                    _Tiles[x][y] = new Tiles(new (x, y));
                }
            }
            _player = new Player(new Vector2());

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
            
            foreach (Tiles tile in Tiles.getLoaded(_player.GetPosition(), new((int)screenSize.X / Tiles.getSize(), (int)screenSize.Y / Tiles.getSize()), TileArray, _Tiles))
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

            foreach (Tiles tile in Tiles.getLoaded(_player.GetPosition(), new ((int)screenSize.X/Tiles.getSize(), (int)screenSize.Y / Tiles.getSize()), TileArray, _Tiles)){
                tile.draw(_spriteBatch, offset);
            }
            _player.Draw(_spriteBatch, screenSize);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
