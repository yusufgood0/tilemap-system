using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Threading;
using first_game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tilemap_system;

namespace first_game
{
    internal struct Slider
    {
        Texture2D _texture;
        Rectangle _sliderRectangle;
        float _minValue;
        float _maxValue;
        Color _backgroundColor;
        Color _sliderColor;
        string _Title;
        float _textScale;
        float _visualSliderValueMultiplier;
        public Slider(Texture2D texture, Rectangle sliderRectangle, float minValue, float maxValue, Color backgroundColor, Color sliderColor, string Title, float textScale, float visualSliderValueMultiplier)
        {
            _texture = texture;
            _sliderRectangle = sliderRectangle;
            _minValue = minValue;
            _maxValue = maxValue;
            _backgroundColor = backgroundColor;
            _sliderColor = sliderColor;
            _Title = Title;
            _textScale = textScale;
            _visualSliderValueMultiplier = visualSliderValueMultiplier;
        }
        public void SliderUpdate(MouseState _mouseState, MouseState _previousMouseState, ref float sliderValue)
        {
            if (_sliderRectangle.Contains(_previousMouseState.Position) && _mouseState.LeftButton == ButtonState.Pressed)
            {
                sliderValue = Math.Max(Math.Min(((float)(_mouseState.X - _sliderRectangle.X) / _sliderRectangle.Width) * (_maxValue - _minValue) + _minValue, _maxValue), _minValue);
            }
        }

        public void SliderDraw(SpriteBatch _spriteBatch, float sliderValue)
        {
            string visualSliderValue = ((int)(sliderValue * _visualSliderValueMultiplier)).ToString();
            _spriteBatch.DrawString(
                Game1._font,
                visualSliderValue,
                new(_sliderRectangle.Right - Game1._font.MeasureString(visualSliderValue).X * _textScale, _sliderRectangle.Y - Game1._font.LineSpacing * _textScale),
                _backgroundColor,
                0,
                new(),
                _textScale,
                0,
                DepthLayers.PauseMenuSliderBackGround
                );
            _spriteBatch.DrawString(
                Game1._font,
                _Title,
                new(_sliderRectangle.X, _sliderRectangle.Y - Game1._font.LineSpacing * _textScale),
                _backgroundColor,
                0,
                new(),
                _textScale,
                0,
                DepthLayers.PauseMenuSliderString
                );
            _spriteBatch.Draw(
                _texture,
                _sliderRectangle,
                null,
                _backgroundColor,
                0,
                new(),
                0,
                DepthLayers.PauseMenuSliderBackGround
                );
            _spriteBatch.Draw(
                _texture,
                new(_sliderRectangle.X, _sliderRectangle.Y, (int)(_sliderRectangle.Width / (_maxValue - _minValue) * (sliderValue - _minValue)), _sliderRectangle.Height),
                null,
                _sliderColor,
                0,
                new(),
                0,
                DepthLayers.PauseMenuSliderForeGround
                );

        }
    }
}
