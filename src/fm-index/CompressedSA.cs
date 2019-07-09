using System;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    internal sealed class CompressedSA : ICompressedSA
    {
        private readonly IBitVector _bitVector;
        private readonly int[] _arr;
        private const int SEGMENT_SIZE = 200;

        public CompressedSA(int[] SA)
        {
            (_arr, _bitVector) = InitializeDataStructures(SA);
        }

        private (int[] arr, IBitVector bitVector) InitializeDataStructures(int[] SA)
        {
            int size = 1 + SA.Length / SEGMENT_SIZE;
            var arr = new int[size];
            int idx = 0;
            var bits = new bool[SA.Length];
            for (int i = 0; i < SA.Length; ++i)
            {
                if (SA[i] % SEGMENT_SIZE == 0)
                {
                    arr[idx++] = SA[i];
                    bits[i] = true;
                }
            }
            return (arr, new BitVector(bits));
        }

        public bool IsStored(int idx)
        {
            return idx >= 0 && idx < _bitVector.Length && _bitVector[idx];
        }

        public int GetStoredPosition(int idx)
        {
            return _arr[_bitVector.RankOne(idx)];
        }
    }
}
