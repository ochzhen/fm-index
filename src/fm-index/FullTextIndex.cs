using System;
using System.Linq;
using FmIndex.Abstract;

namespace FmIndex
{
    public class FullTextIndex
    {
        private IPrefixSum _prefixSum;
        private IOcc _occ;

        public FullTextIndex(string T, int alphabet, int offset, char anchor)
        {
            if (T.Contains(anchor))
                throw new ArgumentException("Anchor character is already contained in the text");
            T += anchor;
            int[] bwtIndexes = BWT.Transform(T);
            char[] bwt = CreateCharArray(T, bwtIndexes);
            _prefixSum = new PrefixSum(bwt, alphabet, offset);
            _occ = new WaveletTree(bwt);
        }

        private char[] CreateCharArray(string s, int[] bwt)
        {
            var arr = new char[bwt.Length];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = s[bwt[i]];
            return arr;
        }

        public int Count(string P)
        {
            int i = P.Length - 1;
            int lo = _prefixSum[P[i]];
            int hi = _prefixSum[P[i] + 1];

            while (lo < hi && i > 0)
            {
                i--;
                char c = P[i];
                lo = _prefixSum[c] + _occ.CountInPrefix(c, lo);
                hi = _prefixSum[c] + _occ.CountInPrefix(c, hi);
            }

            return hi - lo;
        }
    }
}
