namespace XPloit.Core.Collections
{
    public class EncoderCollection : IModuleCollection<Encoder>
    {
        static EncoderCollection _Current = null;

        /// <summary>
        /// Current Exploits
        /// </summary>
        public static EncoderCollection Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new EncoderCollection();
                    _Current.Load();
                }
                return _Current;
            }
        }
    }
}