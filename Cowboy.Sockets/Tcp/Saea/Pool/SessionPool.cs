﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cowboy.Sockets
{
    public class SessionPool : QueuedObjectPool<TcpSocketSaeaSession>
    {
        private Func<TcpSocketSaeaSession> _sessionCreator;
        private Action<TcpSocketSaeaSession> _sessionCleaner;

        public SessionPool(int batchCount, int maxFreeCount, Func<TcpSocketSaeaSession> sessionCreator, Action<TcpSocketSaeaSession> sessionCleaner)
        {
            if (batchCount <= 0)
                throw new ArgumentOutOfRangeException("batchCount");
            if (maxFreeCount <= 0)
                throw new ArgumentOutOfRangeException("maxFreeCount");
            if (sessionCreator == null)
                throw new ArgumentNullException("sessionCreator");

            _sessionCreator = sessionCreator;
            _sessionCleaner = sessionCleaner;

            if (batchCount > maxFreeCount)
            {
                batchCount = maxFreeCount;
            }

            Initialize(batchCount, maxFreeCount);
        }

        public override bool Return(TcpSocketSaeaSession session)
        {
            if (_sessionCleaner != null)
            {
                _sessionCleaner(session);
            }

            if (!base.Return(session))
            {
                CleanupItem(session);
                return false;
            }

            return true;
        }

        protected override void CleanupItem(TcpSocketSaeaSession item)
        {
            item.Close();
        }

        protected override TcpSocketSaeaSession Create()
        {
            return _sessionCreator();
        }
    }
}
