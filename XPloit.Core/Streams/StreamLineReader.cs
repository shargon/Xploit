using System;
using System.IO;
using System.Text;

namespace XPloit.Core.Streams
{
    public class StreamLineReader : IDisposable
    {
        const int BufferLength = 1024;

        Stream _Base;
        int _Read = 0, _Index = 0;
        byte[] _Bff = new byte[BufferLength];

        long _CurrentPosition = 0;
        int _CurrentLine = 0;

        /// <summary>
        /// CurrentLine number
        /// </summary>
        public long CurrentPosition { get { return _CurrentPosition; } }
        /// <summary>
        /// CurrentLine number
        /// </summary>
        public int CurrentLine { get { return _CurrentLine; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Stream</param>
        public StreamLineReader(Stream stream) { _Base = stream; }
        /// <summary>
        /// Count lines and goto line number
        /// </summary>
        /// <param name="goToLine">Goto Line number</param>
        /// <returns>Return true if goTo sucessfully</returns>
        public bool GoToLine(int goToLine) { return IGetCount(goToLine, true) == goToLine; }
        /// <summary>
        /// Count lines and goto line number
        /// </summary>
        /// <param name="goToLine">Goto Line number</param>
        /// <returns>Return the Count of lines</returns>
        public int GetCount(int goToLine) { return IGetCount(goToLine, false); }
        /// <summary>
        /// Internal method for goto&Count
        /// </summary>
        /// <param name="goToLine">Goto Line number</param>
        /// <param name="stopWhenLine">Stop when found the selected line number</param>
        /// <returns>Return the Count of lines</returns>
        int IGetCount(int goToLine, bool stopWhenLine)
        {
            _Base.Seek(0, SeekOrigin.Begin);
            _CurrentPosition = 0;
            _CurrentLine = 0;
            _Index = 0;
            _Read = 0;

            long savePosition = _Base.Length;

            do
            {
                if (_CurrentLine == goToLine)
                {
                    savePosition = _CurrentPosition;
                    if (stopWhenLine) return _CurrentLine;
                }
            }
            while (ReadLine() != null);

            // GoToPosition

            int count = _CurrentLine;

            _CurrentLine = goToLine;
            _Base.Seek(savePosition, SeekOrigin.Begin);

            return count;
        }
        /// <summary>
        /// Read Line
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            bool found = false;

            StringBuilder sb = new StringBuilder();
            while (!found)
            {
                if (_Read <= 0)
                {
                    // Read next block
                    _Index = 0;
                    _Read = _Base.Read(_Bff, 0, BufferLength);
                    if (_Read == 0)
                    {
                        if (sb.Length > 0) break;
                        return null;
                    }
                }

                for (int max = _Index + _Read; _Index < max; )
                {
                    char ch = (char)_Bff[_Index];
                    _Read--; _Index++;
                    _CurrentPosition++;

                    if (ch == '\0' || ch == '\n')
                    {
                        found = true;
                        break;
                    }
                    else if (ch == '\r') continue;
                    else sb.Append(ch);
                }
            }

            _CurrentLine++;
            return sb.ToString();
        }
        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            if (_Base != null)
            {
                _Base.Close();
                _Base.Dispose();
                _Base = null;
            }
        }
    }
}