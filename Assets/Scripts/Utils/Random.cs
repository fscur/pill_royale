using System;

namespace Pills.Assets.Utils
{
    public class Random
    {
        private UInt64 _seed = 1;

        public Random()
        {
        }
        
        public Random(ulong seed)
        {
            if (seed != 0)
                _seed = seed;
        }
        
        public int Range(int min, int max)
        {
            _seed = (_seed * 279470273uL) % 4294967291uL;
            var result = min + (int)(_seed % ((ulong)max - (ulong)min));
            return result;
        }
    }
}