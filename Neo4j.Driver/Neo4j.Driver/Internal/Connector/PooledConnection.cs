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
using System.Collections.Generic;
using Neo4j.Driver.Internal.Result;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.Internal.Connector
{
    internal class PooledConnection : IPooledConnection
    {
        private readonly Action<Guid> _releaseAction;
        private readonly IConnection _connection;

        public PooledConnection(IConnection connection, Action<Guid> releaseAction = null)
        {
            _connection = connection;
            _releaseAction = releaseAction ?? (x => { });

            //Adds call back error handler
            AddConnectionErrorHander(new PooledConnectionErrorHandler(OnNeo4jError));
        }
        public Guid Id { get; } = Guid.NewGuid();

        public void Init()
        {
            _connection.Init();
        }

        public void ClearConnection()
        {
            Reset();
            Sync();
        }

        public void Sync()
        {
            _connection.Sync();
        }

        public void Send()
        {
            _connection.Send();
        }

        public void ReceiveOne()
        {
            _connection.ReceiveOne();
        }

        public void Run(string statement, IDictionary<string, object> parameters = null, IMessageResponseCollector resultBuilder = null, bool pullAll = true)
        {
            _connection.Run(statement, parameters, resultBuilder, pullAll);
        }

        public void Reset()
        {
            _connection.Reset();
        }

        public void AckFailure()
        {
            _connection.AckFailure();
        }

        public void ResetAsync()
        {
            _connection.ResetAsync();
        }

        public bool IsOpen => _connection.IsOpen && !HasUnrecoverableError;
        public IServerInfo Server => _connection.Server;

        /// <summary>
        /// Close the connection and all resources all for good
        /// </summary>
        public void Close()
        {
            _connection.Close();
        }

        public void AddConnectionErrorHander(IConnectionErrorHandler handler)
        {
            _connection.AddConnectionErrorHander(handler);
        }

        /// <summary>
        /// Disposing a pooled connection will try to release the connection resource back to pool
        /// </summary>
        public void Dispose()
        {
            _releaseAction(Id);
        }

        public bool HasUnrecoverableError { private set; get; }

        private Neo4jException OnNeo4jError(Neo4jException error)
        {
            if (error.IsRecoverableError())
            {
                _connection.AckFailure();
            }
            else
            {
                HasUnrecoverableError = true;
            }
            return error;
        }

        internal class PooledConnectionErrorHandler : IConnectionErrorHandler
        {
            private readonly Func<Neo4jException, Neo4jException> _onNeo4jErrorFunc;
            private readonly Func<Exception, Exception> _onConnErrorFunc;

            public PooledConnectionErrorHandler(
                Func<Neo4jException, Neo4jException> onNeo4JErrorFunc,
                Func<Exception, Exception> onConnectionErrorFunc = null)
            {
                _onNeo4jErrorFunc = onNeo4JErrorFunc;
                _onConnErrorFunc = onConnectionErrorFunc;
            }

            public Exception OnConnectionError(Exception e)
            {
                return _onConnErrorFunc == null ? e : _onConnErrorFunc.Invoke(e);
            }

            public Neo4jException OnNeo4jError(Neo4jException e)
            {
                return _onNeo4jErrorFunc.Invoke(e);
            }
        }
    }
}