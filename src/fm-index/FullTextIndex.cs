using System;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    public class FullTextIndex
    {
        private readonly IPrefixSum _prefixSum;
        private readonly IOcc _occ;

        public FullTextIndex(string T, char anchor, int alphabet, Action<string> writeDebug)
        {
            if (T.Contains((char)anchor))
                throw new ArgumentException("Anchor character is already contained in the text");
            T += anchor;
            
            writeDebug($"BWT starting {DateTime.Now.ToLongTimeString()}");
            int[] bwtIndexes = BWT.Transform(T);
            writeDebug($"BWT finished {DateTime.Now.ToLongTimeString()}");
            
            char[] bwt = CreateCharArray(T, bwtIndexes);

            writeDebug($"PrefixSum starting {DateTime.Now.ToLongTimeString()}");
            _prefixSum = new PrefixSum(bwt, anchor, alphabet);
            writeDebug($"PrefixSum finished {DateTime.Now.ToLongTimeString()}");

            writeDebug($"WaveletTree starting {DateTime.Now.ToLongTimeString()}");
            var waveletTree = new WaveletTree(bwt);
            _occ = waveletTree;
            writeDebug($"WaveletTree finished {DateTime.Now.ToLongTimeString()}");

            writeDebug($"Nodes in the Wavelet Tree: {waveletTree.CountNodes()}");
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
