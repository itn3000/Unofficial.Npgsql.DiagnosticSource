// Copyright (c) 2020 itn3000 <itn3000@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
// OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using Npgsql.Logging;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Unofficial.Npgsql.Logging
{
    public class DiagnosticNpgsqlLoggingProvider : INpgsqlLoggingProvider
    {
        NpgsqlLogLevel _Level;
        public DiagnosticNpgsqlLoggingProvider(NpgsqlLogLevel minlevel = NpgsqlLogLevel.Fatal)
        {
            _Level = minlevel;
        }
        public NpgsqlLogger CreateLogger(string name)
        {
            return new DiagnosticNpgsqlLogger(_Level, name);
        }
        public const string ProviderName = "Npgsql.Logging";
    }
    public class LogEventArgs
    {
        public NpgsqlLogLevel Level { get; private set; }
        public string Message { get; private set; }
        public int ConnectorId { get; private set; }
        public Exception Exception { get; private set; }
        internal LogEventArgs(NpgsqlLogLevel level, string msg, int connectorId, Exception exception)
        {
            Level = level;
            Message = msg;
            ConnectorId = connectorId;
            Exception = exception;
        }
    }
    public class CreateLoggerEventArgs
    {
        public string Name { get; set; }
        public NpgsqlLogLevel Level { get; set; }
        public static readonly string EventName = "CreateLogger";
    }
    internal class DiagnosticNpgsqlLogger : NpgsqlLogger
    {
        // private DiagnosticSource _Source = new DiagnosticListener(DiagnosticNpgsqlLoggingProvider.ProviderName);
        static readonly ConcurrentDictionary<string, DiagnosticListener> _SourceDictionary = new ConcurrentDictionary<string, DiagnosticListener>();
        private DiagnosticSource _Source;

        NpgsqlLogLevel _Level;
        // string _Name;
        public DiagnosticNpgsqlLogger(NpgsqlLogLevel level, string name)
        {
            _Level = level;
            // _Name = name;
            _Source = _SourceDictionary.GetOrAdd(name, (x) => new DiagnosticListener($"{DiagnosticNpgsqlLoggingProvider.ProviderName}.{x}"));
            if (_Source.IsEnabled(CreateLoggerEventArgs.EventName))
            {
                _Source.Write(CreateLoggerEventArgs.EventName, new CreateLoggerEventArgs() { Name = name, Level = level });
            }
        }
        public override bool IsEnabled(NpgsqlLogLevel level)
        {
            return _Level >= level;
        }


        public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception exception = null)
        {
            if (_Source.IsEnabled("Log"))
            {
                _Source.Write("Log", new LogEventArgs(level, msg, connectorId, exception));
            }
        }
    }
}