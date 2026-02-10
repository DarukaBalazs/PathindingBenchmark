using System;
using System.Globalization;
using System.IO;
using System.Text;
using Harness.Model;

namespace Harness.Output
{
    public sealed class CsvResultWriter : IDisposable
    {
        private readonly string _filePath;
        private readonly char _separator;
        private bool _headerWritten;
        private readonly object _lock = new();

        public CsvResultWriter(string filePath, char separator = ';')
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _separator = separator;

            _headerWritten = File.Exists(_filePath) && new FileInfo(_filePath).Length > 0;
        }

        public void Append(BenchmarkResultRow row)
        {
            if (row is null) throw new ArgumentNullException(nameof(row));

            lock (_lock)
            {
                using var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream, Encoding.UTF8);

                if (!_headerWritten)
                {
                    writer.WriteLine(BuildHeader());
                    _headerWritten = true;
                }

                writer.WriteLine(BuildLine(row));
            }
        }

        private string BuildHeader()
        {
            var cols = new[]
            {
                "Algorithm",
                "ScenarioName",
                "RunIndex",
                "MapWidth",
                "MapHeight",
                "ObstacleDensity",
                "MapType",
                "AllowDiagonal",
                "Start",
                "Goal",
                "LinearDistance",
                "WeightW",
                "TimeLimitMs",
                "MaxExpansions",
                "TieBreakLowG",
                "Found",
                "PathCost",
                "PathLength",
                "Expansions",
                "ElapsedMs",
                "AllocBytes"
            };

            return string.Join(_separator, cols);
        }

        private string BuildLine(BenchmarkResultRow r)
        {
            string F(object value) =>
                value switch
                {
                    bool b => b ? "1" : "0",
                    IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                    null => string.Empty,
                    _ => value.ToString() ?? string.Empty
                };

            var cols = new[]
            {
                Escape(F(r.Algorithm)),
                Escape(F(r.ScenarioName)),
                F(r.RunIndex),
                F(r.MapWidth),
                F(r.MapHeight),
                F(r.ObstacleDensity),
                Escape(F(r.MapType)),
                F(r.AllowDiagonal),
                Escape(F(r.Start)),
                Escape(F(r.Goal)),
                F(r.LinearDistance),
                F(r.WeightW),
                F(r.TimeLimitMs),
                F(r.MaxExpansions),
                F(r.TieBreakLowG),
                F(r.Found),
                F(r.PathCost),
                F(r.PathLength),
                F(r.Expansions),
                F(r.ElapsedMs),
                F(r.AllocBytes)
            };

            return string.Join(_separator, cols);
        }

        private string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.IndexOfAny(new[] { _separator, '"', '\n', '\r' }) < 0)
                return s;

            var escaped = s.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        public void Dispose()
        {
            
        }
    }
}