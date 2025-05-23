using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.VisualBasic;
using System.Diagnostics;
namespace tilemap_system
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        KeyboardState _keyboardState = new KeyboardState();

        private Point range = new((int)screenSize.X / Tiles.getSize()/2, (int)screenSize.Y / Tiles.getSize()/2);
        static int rows = 100;
        static int columns = 100;
        private Tiles[][] _Tiles = new Tiles[columns][];
        private Player _player;
        private Texture2D square;


        static Vector2 screenSize = new(800, 800);
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferHeight = (int)screenSize.X;
            _graphics.PreferredBackBufferWidth = (int)screenSize.Y;
            _graphics.ApplyChanges();
            //set up tiles
            for (int x = 0; x < _Tiles.Length; x++)
            {
                _Tiles[x] = new Tiles[rows];
            }

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    _Tiles[x][y] = new Tiles(new Rectangle(50 * x, 50 * y, 50, 50));
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

            _keyboardState = Keyboard.GetState();

            _player.update(_keyboardState);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Vector2 offset = screenSize / 2 - _player.GetPosition();
            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            Point CameraTileIndex = Tiles.getTileIndex(_player.GetPosition(), _Tiles);

            //for (int x = Math.Max(CameraTileIndex.X - range.X + 1, 0); x < Math.Min(CameraTileIndex.X + range.Y, columns); x++)
            //{
            //    for (int y = Math.Max(CameraTileIndex.Y - range.Y + 1, 0); y < Math.Min(CameraTileIndex.Y + range.Y, rows); y++)
            //    {
            //        _Tiles[x][y].draw(_spriteBatch, offset);
            //    }
            //}
            //((Tiles)Tiles.getTile(_player.GetPosition(), _Tiles)).draw(_spriteBatch);


            _player.Draw(_spriteBatch, screenSize);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
