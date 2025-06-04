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
        Rectangle _sliderSize;
        float _minValue;
        float _maxValue;
        Color _backgroundColor;
        Color _sliderColor;
        string _text;
        float _textScale;
        float _visualSliderValueMultiplier;
        public Slider(Texture2D texture, Rectangle sliderSize, float minValue, float maxValue, Color backgroundColor, Color sliderColor, string text, float textScale, float visualSliderValueMultiplier)
        {
            _texture = texture;
            _sliderSize = sliderSize;
            _minValue = minValue;
            _maxValue = maxValue;
            _backgroundColor = backgroundColor;
            _sliderColor = sliderColor;
            _text = text;
            _textScale = textScale;
            _visualSliderValueMultiplier = visualSliderValueMultiplier;
        }
        public void SliderUpdate(MouseState _mouseState, MouseState _previousMouseState, ref float sliderValue)
        {
            if (_sliderSize.Contains(_previousMouseState.Position) && _mouseState.LeftButton == ButtonState.Pressed)
            {
                sliderValue = Math.Max(Math.Min(((float)(_mouseState.X - _sliderSize.X) / _sliderSize.Width) * (_maxValue - _minValue) + _minValue, _maxValue), _minValue);
            }
        }

        public void SliderDraw(SpriteBatch _spriteBatch, float sliderValue)
        {
            string visualSliderValue = ((int)(sliderValue * _visualSliderValueMultiplier)).ToString();
            _spriteBatch.DrawString(
            Game1._font,
            visualSliderValue,
            new(_sliderSize.Right - Game1._font.MeasureString(visualSliderValue).X * _textScale, _sliderSize.Y - Game1._font.LineSpacing * _textScale),
            _backgroundColor,
            0,
            new(),
            _textScale,
            0,
            .99f
            );

            _spriteBatch.DrawString(
                Game1._font,
                _text,
                new(_sliderSize.X, _sliderSize.Y - Game1._font.LineSpacing * _textScale),
                _backgroundColor,
                0,
                new(),
                _textScale,
                0,
                .99f
                );
            _spriteBatch.Draw(
                _texture,
                _sliderSize,
                null,
                _backgroundColor,
                0,
                new(),
                0,
                1f
                );
            _spriteBatch.Draw(
                _texture,
                new(_sliderSize.X, _sliderSize.Y, (int)(_sliderSize.Width / (_maxValue - _minValue) * (sliderValue - _minValue)), _sliderSize.Height),
                null,
                _sliderColor,
                0,
                new(),
                0,
                1f
                );

        }
    }
}
