﻿// Copyright (c) 2002-2017 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Threading;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.Internal.Routing
{
    internal abstract class BaseDriver : IDriver
    {
        public abstract ISession NewSession(AccessMode mode);
        public abstract void ReleaseUnmanagedResources();
        public abstract Uri Uri { get; }

        private volatile bool _disposeCalled = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing)
            {
                return;
            }
            _disposeCalled = true;
            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ISession Session()
        {
            return Session(AccessMode.Write);
        }

        public ISession Session(AccessMode mode)
        {
            if (_disposeCalled)
            {
                ThrowDriverClosedException();
            }

            var session = NewSession(mode);

            if (_disposeCalled)
            {
                session.Dispose();
                ThrowDriverClosedException();
            }
            return session;
        }

        private void ThrowDriverClosedException()
        {
            throw new ObjectDisposedException(GetType().Name, "Cannot open a new session on a driver that is already disposed.");
        }
    }
}