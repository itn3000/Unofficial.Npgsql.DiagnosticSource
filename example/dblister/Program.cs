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
using Npgsql;
using Unofficial.Npgsql.Logging;
using Npgsql.Logging;
using System.Diagnostics;
using System.Collections.Generic;

namespace dblister
{
    class DiagnosticEventObserver : IObserver<KeyValuePair<string, object>>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (value.Key == CreateLoggerEventArgs.EventName)
            {
                var p = (CreateLoggerEventArgs)value.Value;
                Console.WriteLine($"{value.Key}: {p.Level}, {p.Name}");
            }
            else
            {
                var p = (LogEventArgs)value.Value;
                Console.WriteLine($"{value.Key}: {p.Message}, {p.Level}, {p.ConnectorId}, {p.Exception}");
            }
        }
    }
    class DiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == DiagnosticNpgsqlLoggingProvider.ProviderName)
            {
                value.Subscribe(new DiagnosticEventObserver(), (name, p1, p2) => true);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            DiagnosticListener.AllListeners.Subscribe(new DiagnosticListenerObserver());
            NpgsqlLogManager.Provider = new DiagnosticNpgsqlLoggingProvider(NpgsqlLogLevel.Trace);
            var host = args[0];
            var port = int.Parse(args[1]);
            var user = args.Length > 2 ? args[2] : null;
            var pass = args.Length > 3 ? args[3] : null;
            var ncb = new NpgsqlConnectionStringBuilder();
            ncb.Host = host;
            ncb.Port = port;
            ncb.Database = "postgres";
            if (user != null)
            {
                ncb.Username = user;
            }
            if (pass != null)
            {
                ncb.Password = pass;
            }
            try
            {
                using (var con = new NpgsqlConnection(ncb.ToString()))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "select datname from pg_database";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"error in query: {e}");
            }
        }
    }
}
