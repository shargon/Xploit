using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Listeners.IO
{
    public class ConsoleCursor
    {
        public enum ECursorMode
        {
            /// <summary>
            /// Hidden cursor
            /// </summary>
            Hidden,
            /// <summary>
            /// Small cursor
            /// </summary>
            Small,
            /// <summary>
            /// Big cursor
            /// </summary>
            Visible
        }

        ECursorMode _CursorMode;
        int _CursorX, _CursorY;
        int _Width, _Height;

        #region Console
        /// <summary>
        /// Console Width
        /// </summary>
        public int Width { get { return _Width; } }
        /// <summary>
        /// Console Height
        /// </summary>
        public int Height { get { return _Height; } }
        #endregion

        #region Cursor
        /// <summary>
        /// Cursor Left
        /// </summary>
        public int CursorX { get { return _CursorX; } }
        /// <summary>
        /// Cursor Top
        /// </summary>
        public int CursorY { get { return _CursorY; } }
        /// <summary>
        /// Mode
        /// </summary>
        public ECursorMode CursorMode { get { return _CursorMode; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleCursor()
        {
            _CursorMode = ECursorMode.Hidden;
            _CursorX = 0;
            _CursorY = 0;
            _Width = 0;
            _Height = 0;
        }

        /// <summary>
        /// Constructor from current console
        /// </summary>
        public static ConsoleCursor CreateFromConsole()
        {
            ECursorMode mode;

            if (Console.CursorVisible)
            {
                if (Console.CursorSize == 100) mode = ECursorMode.Visible;
                else mode = ECursorMode.Small;
            }
            else mode = ECursorMode.Hidden;

            return new ConsoleCursor()
            {
                _CursorX = Console.CursorLeft,
                _CursorY = Console.CursorTop,
                _CursorMode = mode,

                _Width = Console.BufferWidth,
                _Height = Console.BufferHeight
            };
        }
        /// <summary>
        /// Move cursor to left
        /// </summary>
        /// <param name="count">Count</param>
        public void MoveLeft(int count)
        {
            if (count <= 0) return;

            while (count > 0)
            {
                if (CursorX > 0)
                {
                    // same line
                    _CursorX--;
                }
                else
                {
                    if (CursorY > 0)
                    {
                        _CursorX = Width - 1;
                        _CursorY--;
                    }
                }
                count--;
            }
        }
        /// <summary>
        /// Move cursor to right
        /// </summary>
        /// <param name="count">Count</param>
        public void MoveRight(int count)
        {
            if (count <= 0) return;

            while (count > 0)
            {
                if (CursorX + 1 < Width)
                {
                    // same line
                    _CursorX++;
                }
                else
                {
                    _CursorY++;
                    _CursorX = 0;
                }
                count--;
            }
        }
        /// <summary>
        /// Update the current io
        /// </summary>
        /// <param name="io">IO</param>
        public void Flush(IIOCommandLayer io)
        {
            if (io != null)
                io.SetCursorPosition(CursorX, CursorY);
        }
    }
}