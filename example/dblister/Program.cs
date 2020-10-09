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
