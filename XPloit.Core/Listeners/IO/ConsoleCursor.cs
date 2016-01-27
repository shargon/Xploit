using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Listeners.IO
{
    public class ConsoleCursor
    {
        public enum ECursorMode
        {
            Visible,
            Hidden,
            Small
        }

        #region Console
        /// <summary>
        /// Console Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Console Height
        /// </summary>
        public int Height { get; set; }
        #endregion

        #region Cursor
        /// <summary>
        /// Cursor Left
        /// </summary>
        public int CursorX { get; set; }
        /// <summary>
        /// Cursor Top
        /// </summary>
        public int CursorY { get; set; }
        /// <summary>
        /// Mode
        /// </summary>
        public ECursorMode CursorMode { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleCursor() { }

        /// <summary>
        /// Constructo from current console
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
                CursorX = Console.CursorLeft,
                CursorY = Console.CursorTop,
                CursorMode = mode,

                Width = Console.BufferWidth,
                Height = Console.BufferHeight
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
                    CursorX--;
                }
                else
                {
                    if (CursorY > 0)
                    {
                        CursorX = Width - 1;
                        CursorY--;
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
                    CursorX++;
                }
                else
                {
                    CursorY++;
                    CursorX = 0;
                }
                count--;
            }
        }
        /// <summary>
        /// Update the current io
        /// </summary>
        /// <param name="io">IO</param>
        public void Update(IIOCommandLayer io)
        {
            if (io != null)
                io.SetCursorPosition(CursorX, CursorY);
        }
    }
}