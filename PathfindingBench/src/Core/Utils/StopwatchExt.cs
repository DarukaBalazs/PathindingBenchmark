using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Utils
{
    /// <summary>
    /// Stopwatch-hoz apró kényelmi metódusok.
    /// </summary>
    public static class StopwatchExt
    {
        /// <summary>
        /// Elindít egy Stopwatchet, lefuttat egy Actiont, és visszaadja az eltelt időt ms-ban.
        /// </summary>
        public static double MeasureMs(Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Elindít egy Stopwatchet, lefuttat egy Func&lt;T&gt;-t,
        /// visszaadja az eredményt és az eltelt időt ms-ban.
        /// </summary>
        public static (T result, double elapsedMs) MeasureMs<T>(Func<T> func)
        {
            var sw = Stopwatch.StartNew();
            var result = func();
            sw.Stop();
            return (result, sw.Elapsed.TotalMilliseconds);
        }
    }
}
