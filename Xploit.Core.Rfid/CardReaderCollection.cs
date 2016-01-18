using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Xploit.Core.Rfid;
using Xploit.Core.Rfid.Interfaces;
using Xploit.Core.Rfid.Mifare;

namespace XPloit.Core.Rfid
{
    public class CardReaderCollection : IEnumerable<CardReader>, IDisposable
    {
        internal IntPtr _hContext = IntPtr.Zero;
        BackgroundWorker _worker = null;
        Dictionary<string, CardReader> _readers = new Dictionary<string, CardReader>();

        public delegate void delOnCardAction(CardReader reader, HandledEventArgs e);
        public delegate void delOnCardReaded(ICard card, HandledEventArgs e);

        public event delOnCardAction OnCardInsert;
        public event delOnCardAction OnCardRemove;
        public event delOnCardReaded OnCardReaded;

        /// <summary>
        /// Devuelve el número de lectores existentes en la colección
        /// </summary>
        public int Count { get { return _readers.Values.Count; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="readers">Lectores</param>
        public CardReaderCollection()
        {
            // Establish context
            CardReader.API.SCardFunctionReturnCodes result = (CardReader.API.SCardFunctionReturnCodes)
                CardReader.API.SCardEstablishContext(CardReader.API.SCARD_SCOPE_SYSTEM/*SCARD_SCOPE_USER*/, IntPtr.Zero, IntPtr.Zero, out _hContext);


            long ret = 0;
            IntPtr hContext;
            UInt32 pcchReaders = 0;
            ArrayList readersList = new ArrayList();

            //establish context
            ret = CardReader.API.SCardEstablishContext(CardReader.API.SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out hContext);

            //get readers buffer len
            ret = CardReader.API.SCardListReaders(hContext, IntPtr.Zero, null, ref pcchReaders);
            byte[] mszReaders = new byte[pcchReaders];

            // fill readers' buffer
            ret = CardReader.API.SCardListReaders(hContext, IntPtr.Zero, mszReaders, ref pcchReaders);

            ret = CardReader.API.SCardReleaseContext(hContext);

            // Remember that readers is a multistring with a double trailing \0
            // This is much easier and faster to do the allocation like this than the looping way

            ASCIIEncoding ascii = new ASCIIEncoding();
            string currbuff = ascii.GetString(mszReaders);
            int len = (int)pcchReaders;

            List<CardReader> r = new List<CardReader>();

            foreach (string reader in MultiStringToArray(currbuff.ToCharArray()))
                _readers.Add(reader, new CardReader(reader, _hContext));
        }

        static string[] MultiStringToArray(char[] multistring)
        {
            List<string> stringList = new List<string>();
            int i = 0;
            while (i < multistring.Length)
            {
                int j = i;
                if (multistring[j++] == '\0') break;
                while (j < multistring.Length)
                {
                    if (multistring[j++] == '\0')
                    {
                        stringList.Add(new string(multistring, i, j - i - 1));
                        i = j;
                        break;
                    }
                }
            }

            return stringList.ToArray();
        }


        /// <summary>
        /// Obtiene el lector en base a su nombre
        /// </summary>
        /// <param name="name">Nombre del lector</param>
        public CardReader this[string name]
        {
            get
            {
                CardReader r;
                if (_readers.TryGetValue(name, out r))
                    return r;
                return null;
            }
        }

        public IEnumerator<CardReader> GetEnumerator() { return _readers.Values.GetEnumerator(); }
        IEnumerator System.Collections.IEnumerable.GetEnumerator() { return _readers.Values.GetEnumerator(); }
        /// <summary>
        /// Liberación de recursos
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_readers != null)
            {
                foreach (CardReader r in _readers.Values)
                    r.Dispose();

                _readers.Clear();
            }
            if (_hContext != IntPtr.Zero)
            {
                int ret = CardReader.API.SCardReleaseContext(_hContext);
                _hContext = IntPtr.Zero;
            }
            if (_worker != null)
            {
                _worker.CancelAsync();
                _worker.Dispose();
                _worker = null;
            }
        }
        /// <summary>
        /// Devuelve el primer lector o NULL
        /// </summary>
        public CardReader GetFirstOrNull()
        {
            foreach (CardReader r in _readers.Values)
                return r;

            return null;
        }


        void WaitChangeStatus(object sender, DoWorkEventArgs e)
        {
            CardReader.API.SCARD_READERSTATE[] states = new CardReader.API.SCARD_READERSTATE[Count];
            CardReader[] readers = new CardReader[states.Length];

            int x = 0;
            foreach (CardReader r in this)
            {
                states[x].Reader = r.Name;
                readers[x] = r;
                x++;
            }

            while (!e.Cancel)
            {
                CardReader.API.SCardFunctionReturnCodes result;

                // Obtain a lock when we use the context pointer, 
                // which may be modified in the Dispose() method.

                lock (this)
                {
                    if (_hContext == IntPtr.Zero) return;

                    //This thread will be executed every 1000ms. 
                    //The thread also blocks for 1000ms, meaning 
                    //that the application may keep on running for 
                    //one extra second after the user has closed 
                    //the Main Form.

                    result =
                        (CardReader.API.SCardFunctionReturnCodes)CardReader.API.GetStatusChange(this._hContext, 1000, states, states.Length);
                }

                if ((result == CardReader.API.SCardFunctionReturnCodes.SCARD_E_TIMEOUT))
                {
                    // Time out has passed, but there is no new info. Just go on with the loop
                    continue;
                }
                else
                {
                    for (int xx = 0, m = states.Length; xx < m; xx++)
                    {
                        CardReader.API.SCARD_READERSTATE st = states[xx];
                        CardReader r = readers[xx];
                        CardReader.API.CardState eventState = st.EventState;

                        bool isChanged = (eventState & CardReader.API.CardState.Changed) == CardReader.API.CardState.Changed;

                        if (isChanged)
                        {
                            bool isPresent = (eventState & CardReader.API.CardState.Present) == CardReader.API.CardState.Present;
                            bool isInUse = (eventState & CardReader.API.CardState.Empty) == CardReader.API.CardState.InUse;
                            bool isUnavailable = (eventState & CardReader.API.CardState.Empty) == CardReader.API.CardState.Unavailable;

                            if (isPresent && !isInUse && !isUnavailable)
                            {
                                // Hay tarjeta
                                if (!r._IsCardInserted)
                                {
                                    r._IsCardInserted = true;

                                    HandledEventArgs h = new HandledEventArgs(false);

                                    if (OnCardInsert != null)
                                        OnCardInsert(r, h);

                                    // No se ha ejecutado una acción propia, la leemos nosotros
                                    if (!h.Handled && OnCardReaded != null)
                                    {
                                        // Tarjeta leida
                                        if (r.Connect())
                                        {
                                            ConfigMifareRead cfg = null;
                                            //cfg = new ConfigMifareRead()
                                            //{
                                            //    KeysOne  = new byte[] { 0xB0 ,0xB1 ,0xB2 ,0xB3 ,0xB4 ,0xB5 },
                                            //    KeysZero = new byte[] { 0xB0 ,0xB1 ,0xB2 ,0xB3 ,0xB4 ,0xB5 },
                                            //};

                                            //for (byte xyx = 0; xyx < 16; xyx++)
                                            //{
                                            //    cfg[xyx].ReadDataBlockStart = ConfigMifareReadSector.EBlockRange.DataBlock01;
                                            //    cfg[xyx].ReadDataBlockEnd = ConfigMifareReadSector.EBlockRange.DataBlock03;
                                            //    cfg[xyx].ReadTrailBlock = true;
                                            //    cfg[xyx].Login = new LoginMifareMethod()
                                            //    {
                                            //        KeyNum = ConfigMifareRead.EKeyNum.One,
                                            //        KeyType = ConfigMifareRead.EKeyType.A,
                                            //    };
                                            //}

                                            ICard card;
                                            if (r.GetCard(out card, st.RGBAttribute(), cfg))
                                            {
                                                r.Disconnect();

                                                OnCardReaded(card, h);
                                            }
                                            else
                                            {
                                                // Algún error ha ocurrido ... (en DniE tarda en reconocerlo)
                                                r.Disconnect();
                                                r._IsCardInserted = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool isEmpty = (eventState & CardReader.API.CardState.Empty) == CardReader.API.CardState.Empty;

                                if (isEmpty)
                                {
                                    // No hay tarjeta
                                    if (r._IsCardInserted)
                                    {
                                        r._IsCardInserted = false;

                                        if (OnCardRemove != null)
                                        {
                                            HandledEventArgs h = new HandledEventArgs(false);
                                            OnCardRemove(r, h);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Crea el hilo que espera a los eventos de escucha
        /// </summary>
        public void WaitChange()
        {
            if (Count <= 0) return;
            if (_worker != null) return;

            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += WaitChangeStatus;
            _worker.RunWorkerAsync();
        }
    }
}