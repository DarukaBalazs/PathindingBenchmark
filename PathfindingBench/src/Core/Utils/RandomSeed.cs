using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Core.Utils
{
    public static class RandomSeed
    {
        /// <summary>
        /// Determinisztikus Random példány adott seed alapján.
        /// </summary>
        public static Random Create(int seed) => new Random(seed);

        /// <summary>
        /// Új származtatott seed (pl. algo, szcenárió szerint offsetelve).
        /// </summary>
        public static int Derive(int baseSeed, int offset)
            => unchecked(baseSeed * 397 ^ offset);
    }
}
