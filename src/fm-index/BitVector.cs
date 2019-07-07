using System;
using System.Numerics;

namespace FmIndex.Abstract
 {
     [Serializable]
    internal sealed class BitVector : IBitVector
    {
        private const int BUCKET_SIZE = 64;
        private const int BLOCK_SIZE = 9;

        private readonly ulong[] _buckets;
        private readonly int[] _blocks;

        public int Length { get; }

        public BitVector(bool[] arr)
        {
            Length = arr.Length;
            _buckets = CreateBuckets(arr);
            _blocks = CreateBlockArray(_buckets, arr.Length);
        }

        private ulong[] CreateBuckets(bool[] arr)
        {
            int bucketsCount = arr.Length / BUCKET_SIZE;
            if (arr.Length % BUCKET_SIZE != 0)
                bucketsCount++;
            var buckets = new ulong[bucketsCount];
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i])
                {
                    int bucket = i / BUCKET_SIZE;
                    int idx = i % BUCKET_SIZE;
                    ulong mask = (ulong)1 << (BUCKET_SIZE-idx-1);
                    buckets[bucket] |= mask;
                }
            }
            return buckets;
        }

        private int[] CreateBlockArray(ulong[] buckets, int size)
        {
            const int bitsPerBlock = BUCKET_SIZE * BLOCK_SIZE;
            var blocks = new int[size / bitsPerBlock];
            for (int i = 0; i < blocks.Length; ++i)
            {
                for (int j = i * BLOCK_SIZE; j < (i + 1) * BLOCK_SIZE; ++j)
                    blocks[i] += BitOperations.PopCount(buckets[j]);
            }
            for (int i = 1; i < blocks.Length; ++i)
                blocks[i] += blocks[i-1];
            return blocks;
        }

        public bool this[int idx] 
        { 
            get
            {
                int currentBucket = idx / BUCKET_SIZE;
                idx -= currentBucket * BUCKET_SIZE;
                ulong mask = (ulong)1 << (BUCKET_SIZE-idx-1);
                return (_buckets[currentBucket] & mask) > 0;
            }
        }

        public int RankOne(int len)
        {
            const int bitsPerBlock = BUCKET_SIZE * BLOCK_SIZE;
            int currentBlock = len / bitsPerBlock;
            int currentBucket = len / BUCKET_SIZE;
            int count = 0;
            int i = currentBlock * BLOCK_SIZE;
            len -= bitsPerBlock * currentBlock;
            if (currentBlock > 0)
                count += _blocks[currentBlock - 1];
            while (i < currentBucket)
            {
                count += BitOperations.PopCount(_buckets[i]);
                len -= BUCKET_SIZE;
                i++;
            }
            if (len > 0)
                count += BitOperations.PopCount(_buckets[currentBucket] >> (BUCKET_SIZE - len));
            return count;
        }

        public int RankZero(int len)
        {
            return len - RankOne(len);
        }
    }
}
