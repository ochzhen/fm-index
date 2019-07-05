using System;
using System.Numerics;

namespace FmIndex.Abstract
 {
     [Serializable]
    internal sealed class BitVector : IBitVector
    {
        private readonly ulong[] buckets;
        private readonly int[] blocks;

        public int Length { get; }

        public BitVector(bool[] arr)
        {
            Length = arr.Length;
            buckets = CreateBuckets(arr);
            blocks = CreateBlockArray(buckets, arr.Length);
        }

        private ulong[] CreateBuckets(bool[] arr)
        {
            int bucketsCount = arr.Length / 64;
            if (arr.Length % 64 != 0)
                bucketsCount++;
            var buckets = new ulong[bucketsCount];
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i])
                {
                    int bucket = i / 64;
                    int idx = i % 64;
                    buckets[bucket] |= ((ulong)1 << (64-idx-1));
                }
            }
            return buckets;
        }

        private int[] CreateBlockArray(ulong[] buckets, int size)
        {
            const int bucketsPerBlock = 9;
            const int bitsPerBlock = 64 * bucketsPerBlock;
            var blocks = new int[size / bitsPerBlock];
            for (int i = 0; i < blocks.Length; ++i)
            {
                for (int j = i * bucketsPerBlock; j < (i + 1) * bucketsPerBlock; ++j)
                    blocks[i] += BitOperations.PopCount(buckets[j]);
            }
            for (int i = 1; i < blocks.Length; ++i)
                blocks[i] += blocks[i-1];
            return blocks;
        }

        public int RankOne(int len)
        {
            const int bucketsPerBlock = 9;
            const int bitsPerBlock = 64 * bucketsPerBlock;
            int currentBlock = len / bitsPerBlock;
            int currentBucket = len / 64;
            int count = 0;
            int i = currentBlock * bucketsPerBlock;
            len -= bitsPerBlock * currentBlock;
            if (currentBlock > 0)
                count += blocks[currentBlock - 1];
            while (i < currentBucket)
            {
                count += BitOperations.PopCount(buckets[i]);
                len -= 64;
                i++;
            }
            if (len > 0)
                count += BitOperations.PopCount(buckets[currentBucket] >> (64 - len));
            return count;
        }

        public int RankZero(int len)
        {
            return len - RankOne(len);
        }
    }
}
