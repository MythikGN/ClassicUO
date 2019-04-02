﻿#region license
//  Copyright (C) 2019 ClassicUO Development Community on Github
//
//	This project is an alternative client for the game Ultima Online.
//	The goal of this is to develop a lightweight client considering 
//	new technologies.  
//      
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;

using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI
{
    abstract class AbstractEntry
    {
        protected AbstractEntry(int maxcharlength, int width, int maxWidth)
        {
            MaxCharCount = maxcharlength;
            Width = width;
            MaxWidth = maxWidth;
        }

        public void Destroy()
        {
            RenderText?.Destroy();
            RenderText = null;
            RenderCaret?.Destroy();
            RenderCaret = null;
        }

        public RenderedText RenderText { get; protected set; }

        public RenderedText RenderCaret { get; protected set; }

        public Point CaretPosition { get; set; }
        public int CaretIndex { get; protected set; }

        public ushort Hue
        {
            get => RenderText.Hue;
            set
            {
                if (RenderText.Hue != value)
                {
                    RenderCaret.Hue = RenderText.Hue = value;
                    RenderText.CreateTexture();
                    RenderCaret.CreateTexture();
                }
            }
        }

        public virtual string Text
        {
            get => RenderText.Text;
            set
            {
                RenderText.Text = value;
                IsChanged = true;
            }
        }

        private bool _isChanged;
        public bool IsChanged
        {
            get => _isChanged;
            protected set
            {
                _selectionArea = (0, 0);
                _isChanged = value;
            }
        }

        public int MaxCharCount { get; }

        public int Width { get; }
        public int Height => RenderText.Height < 25 ? 25 : RenderText.Height;

        public int MaxWidth { get; }

        public int Offset { get; set; }

        public void RemoveChar(bool fromleft)
        {
            if (fromleft)
            {
                if (CaretIndex < 1)
                    return;
                CaretIndex--;
            }
            else
            {
                if (CaretIndex >= Text.Length)
                    return;
            }

            if (CaretIndex < Text.Length)
                Text = Text.Remove(CaretIndex, 1);
            else if (CaretIndex > Text.Length)
                Text = Text.Remove(Text.Length - 1);
        }

        public void SeekCaretPosition(int value)
        {
            CaretIndex += value;

            if (CaretIndex < 0)
                CaretIndex = 0;

            if (CaretIndex > Text.Length)
                CaretIndex = Text.Length;
            IsChanged = true;
        }

        public void SetCaretPosition(int value)
        {
            CaretIndex = value;

            if (CaretIndex < 0)
                CaretIndex = 0;

            if (CaretIndex > Text.Length)
                CaretIndex = Text.Length;
            IsChanged = true;
        }

        public void UpdateCaretPosition()
        {
            int x, y;

            if (RenderText.IsUnicode)
                (x, y) = FileManager.Fonts.GetCaretPosUnicode(RenderText.Font, RenderText.Text, CaretIndex, Width, RenderText.Align, (ushort)RenderText.FontStyle);
            else
                (x, y) = FileManager.Fonts.GetCaretPosASCII(RenderText.Font, RenderText.Text, CaretIndex, Width, RenderText.Align, (ushort)RenderText.FontStyle);
            CaretPosition = new Point(x, y);

            if (Offset > 0)
            {
                if (CaretPosition.X + Offset < 0)
                    Offset = -CaretPosition.X;
                else if (Width + -Offset < CaretPosition.X)
                    Offset = Width - CaretPosition.X;
            }
            else if (Width + Offset < CaretPosition.X)
                Offset = Width - CaretPosition.X;
            else
                Offset = 0;

            if (IsChanged)
                IsChanged = false;
        }

        public void OnDraw(Batcher2D batcher, int x, int y)
        {
            if (_isSelection)
            {
                batcher.Draw2D(CheckerTrans.TransparentTexture, _selectionArea.Item1, _selectionArea.Item2, Mouse.Position.X - _selectionArea.Item1, Mouse.Position.Y - _selectionArea.Item2, ShaderHuesTraslator.GetHueVector(222, false, 0.5f, false));
            }
        }

        public void OnMouseClick(int x, int y)
        {
            int oldPos = CaretIndex;

            if (RenderText.IsUnicode)
                CaretIndex = FileManager.Fonts.CalculateCaretPosUnicode(RenderText.Font, RenderText.Text, x, y, Width, RenderText.Align, (ushort)RenderText.FontStyle);
            else
                CaretIndex = FileManager.Fonts.CalculateCaretPosASCII(RenderText.Font, RenderText.Text, x, y, Width, RenderText.Align, (ushort)RenderText.FontStyle);

            if (oldPos != CaretIndex)
                UpdateCaretPosition();
            _selectionArea = (Mouse.Position.X, Mouse.Position.Y);
            _isSelection = true;
        }

        private (int, int) _selectionArea;
        private bool _isSelection = false;
        internal void OnSelectionEnd(int x, int y)
        {
            int endindex;
            if (RenderText.IsUnicode)
                endindex = FileManager.Fonts.CalculateCaretPosUnicode(RenderText.Font, RenderText.Text, x, y, Width, RenderText.Align, (ushort)RenderText.FontStyle);
            else
                endindex = FileManager.Fonts.CalculateCaretPosASCII(RenderText.Font, RenderText.Text, x, y, Width, RenderText.Align, (ushort)RenderText.FontStyle);
            _isSelection = false;
            if (endindex == CaretIndex)
            {
                _selectionArea = (0, 0);
                return;
            }
            CaretIndex = endindex;
            UpdateCaretPosition();
        }

        public void Clear()
        {
            Text = string.Empty;
            Offset = 0;
            CaretPosition = Point.Zero;
            CaretIndex = 0;
        }
    }
}
