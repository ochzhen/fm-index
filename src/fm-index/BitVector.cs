using System;
using System.Collections;

namespace FmIndex.Abstract
 {
     [Serializable]
    internal sealed class BitVector : IBitVector
    {
        private readonly BitArray bits;

        public BitVector(bool[] arr)
        {
            Length = arr.Length;
            bits = new BitArray(arr);
        }

        public int Length { get; }

        public int RankOne(int len)
        {
            int count = 0;
            for (int i = 0; i < len; ++i)
            {
                if (bits[i])
                    count++;
            }
            return count;
        }

        public int RankZero(int len)
        {
            return len - RankOne(len);
        }
    }
}
