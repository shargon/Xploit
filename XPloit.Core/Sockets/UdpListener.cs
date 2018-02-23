﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace XPloit.Core.Sockets
{
    // production ready but not yet ready for public use
    // --> internal
    internal class UdpListener : IDisposable
    {
        private class MyAsyncResult : IAsyncResult
        {
            public IAsyncResult AsyncResult;
            public AsyncCallback Callback;
            public object State;

            public EndPoint EndPoint;
            public byte[] Buffer;

            public object AsyncState { get { return State; } }
            public WaitHandle AsyncWaitHandle { get { return AsyncResult.AsyncWaitHandle; } }
            public bool CompletedSynchronously { get { return AsyncResult.CompletedSynchronously; } }
            public bool IsCompleted { get { return AsyncResult.IsCompleted; } }
        }

        private readonly Socket _socket;
        private readonly IPEndPoint _endPoint;

        public UdpListener(IPAddress address, int port) : this(new IPEndPoint(address, port)) { }

        public UdpListener(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(_endPoint);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            MyAsyncResult result =
                new MyAsyncResult()
                {
                    Buffer = new byte[65535],
                    EndPoint = _endPoint,
                    Callback = callback,
                    State = state
                };

            result.AsyncResult = _socket.BeginReceiveFrom(result.Buffer, 0, 65535, SocketFlags.None, ref result.EndPoint, OnSocketCallback, result);

            return result;
        }

        private static void OnSocketCallback(IAsyncResult asyncResult)
        {
            MyAsyncResult receiveAsyncResult = asyncResult.AsyncState as MyAsyncResult;
            if ((receiveAsyncResult != null) && (receiveAsyncResult.Callback != null))
            {
                receiveAsyncResult.AsyncResult = asyncResult;
                receiveAsyncResult.Callback(receiveAsyncResult);
            }
        }

        public byte[] EndReceive(IAsyncResult asyncResult, out IPEndPoint endPoint)
        {
            MyAsyncResult receiveAsyncResult = asyncResult as MyAsyncResult;

            if (receiveAsyncResult == null)
                throw new ArgumentException("Invalid Async Result", "asyncResult");

            int length = _socket.EndReceiveFrom(receiveAsyncResult.AsyncResult, ref receiveAsyncResult.EndPoint);

            endPoint = receiveAsyncResult.EndPoint as IPEndPoint;

            if (length == 65535)
            {
                return receiveAsyncResult.Buffer;
            }
            else
            {
                byte[] result = new byte[length];
                Buffer.BlockCopy(receiveAsyncResult.Buffer, 0, result, 0, length);
                return result;
            }
        }

        public IAsyncResult BeginSend(byte[] buffer, int offset, int length, IPEndPoint endPoint, AsyncCallback callback, object state)
        {
            MyAsyncResult result =
                new MyAsyncResult()
                {
                    Callback = callback,
                    State = state
                };

            result.AsyncResult = _socket.BeginSendTo(buffer, offset, length, SocketFlags.None, endPoint, OnSocketCallback, result);

            return result;
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            MyAsyncResult receiveAsyncResult = asyncResult as MyAsyncResult;

            if (receiveAsyncResult == null)
                throw new ArgumentException("Invalid Async Result", "asyncResult");

            return _socket.EndSendTo(receiveAsyncResult.AsyncResult);
        }


        public void Dispose() { ((IDisposable)_socket).Dispose(); }
    }
}