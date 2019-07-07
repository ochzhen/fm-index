using System;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    internal sealed class CompressedSA : ICompressedSA
    {
        private readonly IBitVector _bitVector;
        private readonly int[] _arr;
        private readonly int _segmentSize;

        public CompressedSA(int[] SA)
        {
            _segmentSize = DetermineSegmentSize(SA.Length);
            (_arr, _bitVector) = InitializeDataStructures(SA);
        }

        private int DetermineSegmentSize(int n)
        {
            double x = Math.Log(n, 2);
            return (int) (x*x / Math.Log(x, 2));
        }

        private (int[] arr, IBitVector bitVector) InitializeDataStructures(int[] SA)
        {
            int size = 1 + SA.Length / _segmentSize;
            var arr = new int[size];
            int idx = 0;
            var bits = new bool[SA.Length];
            for (int i = 0; i < SA.Length; ++i)
            {
                if (SA[i] % _segmentSize == 0)
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
