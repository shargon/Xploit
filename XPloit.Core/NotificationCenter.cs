using System;
using System.Collections.Generic;

namespace XPloit.Core
{
    public class NotificationCenter
    {
        static Dictionary<string, List<EventHandler>> _Events = new Dictionary<string, List<EventHandler>>();

        /// <summary>
        /// Lanza el evento
        /// </summary>
        /// <param name="msg">Evento tipo</param>
        /// <param name="sender">Objeto que envia el evento</param>
        /// <param name="e">Parametros</param>
        /// <returns>Devuelve el numero de eventos lanzados</returns>
        public static int Raise(string msg, object sender, EventArgs e)
        {
            List<EventHandler> lev;
            if (_Events.TryGetValue(msg, out lev))
            {
                foreach (EventHandler ev in lev)
                    ev.Invoke(sender, e);
                return lev.Count;
            }
            return 0;
        }
        /// <summary>
        /// Se subscribe al evento
        /// </summary>
        /// <param name="msg">Evento tipo</param>
        /// <param name="ev">Evento a lanzar</param>
        public static void Subscribe(string msg, EventHandler ev)
        {
            lock (_Events)
            {
                List<EventHandler> lev;
                if (_Events.TryGetValue(msg, out lev))
                    lev.Add(ev);
                else
                {
                    lev = new List<EventHandler>();
                    lev.Add(ev);
                    _Events.Add(msg, lev);
                }
            }
        }
        /// <summary>
        /// Se Desubscribe al evento
        /// </summary>
        /// <param name="msg">Evento tipo</param>
        /// <param name="ev">Evento a lanzar</param>
        /// <returns>Devuelve true si se realiza la dessubscripcion</returns>
        public static bool Unsubscribe(string msg, EventHandler ev)
        {
            lock (_Events)
            {
                List<EventHandler> lev;
                if (_Events.TryGetValue(msg, out lev))
                {
                    bool dv = lev.Remove(ev);
                    if (lev.Count == 0)
                        _Events.Remove(msg);
                    return dv;
                }
                return false;
            }
        }
    }
}