using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XPloit.Core.Sockets;
using XPloit.Core.Sockets.Protocols;
using XPloit.Core.Sockets.Messages;
using System.Threading;
using XPloit.Core.Sockets.Interfaces;
using System.Text;
using System.Collections.Generic;

namespace XPloit.Test
{
    [TestClass]
    public class SocketTest
    {
        static bool isover = false;
        [TestMethod]
        public void TestSocketProtocol()
        {
            using (XPloitSocket client = new XPloitSocket(new XPloitSocketProtocol(Encoding.UTF8, XPloitSocketProtocol.EProtocolMode.Int32), "127.0.0.1:77") { TimeOut = TimeSpan.Zero })
            using (XPloitSocket server = new XPloitSocket(new XPloitSocketProtocol(Encoding.UTF8, XPloitSocketProtocol.EProtocolMode.Int32), 77) { TimeOut = TimeSpan.Zero })
            {
                server.OnConnect += s_OnConnect;
                server.OnMessage += s_OnMessage;
                server.Start();

                client.OnMessage += client_OnMessage;
                client.Start();

                while (!isover) Thread.Sleep(1);
            }
        }

        void client_OnMessage(XPloitSocket sender, XPloitSocketClient cl, IXPloitSocketMsg msg)
        {
            // Client receive message
            cl.Send(new XPloitMsgLogin() { User = "client", Password = "toServer" });
        }
        void s_OnMessage(XPloitSocket sender, XPloitSocketClient cl, IXPloitSocketMsg msg)
        {
            // Server receive msg
            isover = true;
        }
        void s_OnConnect(XPloitSocket sender, XPloitSocketClient cl)
        {
            cl.Send(new XPloitMsgLogin() { User = "server", Password = "toClient" });
        }


        [TestMethod]
        public void TestTelnetProtocol()
        {
            using (XPloitSocket server = new XPloitSocket(new XPloitTelnetProtocol(Encoding.UTF8), 23) { TimeOut = TimeSpan.Zero })
            {
                server.OnConnect += s_OnConnect2;
                server.OnMessage += s_OnMessage2;
                server.Start();

                while (!isover) Thread.Sleep(1);
            }
        }
        void s_OnMessage2(XPloitSocket sender, XPloitSocketClient cl, IXPloitSocketMsg msg)
        {
            // Server receive msg
            XPloitMsgString msgS = (XPloitMsgString)msg;

            //XPloitTelnetProtocol.Send(cl, XPloitTelnetProtocol.GetColorMessage());
            //XPloitTelnetProtocol.Send(cl, new byte[] { 255, 247 });
            XPloitTelnetProtocol.Send(cl, "Received: " + msgS.Data + Environment.NewLine);

            //isover = true;
        }
        void s_OnConnect2(XPloitSocket sender, XPloitSocketClient cl)
        {
            //XPloitTelnetProtocol.Send(cl, new byte[] { 255, 247 });
            //XPloitTelnetProtocol.Send(cl, XPloitTelnetProtocol.GetColorMessage());
            XPloitTelnetProtocol.Send(cl, "server to client" + Environment.NewLine);
        }
    }
}
