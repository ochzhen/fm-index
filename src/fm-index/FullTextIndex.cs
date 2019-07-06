using System;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    public class FullTextIndex
    {
        private readonly IPrefixSum _prefixSum;
        private readonly IOcc _occ;
        private readonly IAlphabetIds _alphabetIds;

        public FullTextIndex(string T, Action<string> writeDebug)
        {
            writeDebug($"AlphabetIds starting {DateTime.Now.ToLongTimeString()}");
            _alphabetIds = new AlphabetIds(T);
            writeDebug($"- Alphabet size: {_alphabetIds.Length}");
            writeDebug($"AlphabetIds finished {DateTime.Now.ToLongTimeString()}");

            char[] s = CreateTransformedToNewAlphabet(T, _alphabetIds, T.Length + 1);
            s[s.Length - 1] = _alphabetIds.Anchor;
            
            writeDebug($"BWT starting {DateTime.Now.ToLongTimeString()}");
            int[] bwtIndexes = BWT.Transform(s, _alphabetIds.Length);
            writeDebug($"BWT finished {DateTime.Now.ToLongTimeString()}");
            
            char[] bwt = CreateBwt(s, bwtIndexes);

            writeDebug($"PrefixSum starting {DateTime.Now.ToLongTimeString()}");
            _prefixSum = new PrefixSum(bwt, _alphabetIds.Length + 1);
            writeDebug($"PrefixSum finished {DateTime.Now.ToLongTimeString()}");

            writeDebug($"WaveletTree starting {DateTime.Now.ToLongTimeString()}");
            var waveletTree = new WaveletTree(bwt);
            _occ = waveletTree;
            writeDebug($"WaveletTree finished {DateTime.Now.ToLongTimeString()}");

            writeDebug($"- Nodes in the Wavelet Tree: {waveletTree.CountNodes()}");
        }

        private char[] CreateTransformedToNewAlphabet(string T, IAlphabetIds alphabetIds, int size)
        {
            var s = new char[size];
            for (int i = 0; i < T.Length; ++i)
                s[i] = alphabetIds[T[i]];
            return s;
        }

        private char[] CreateBwt(char[] s, int[] bwt)
        {
            var arr = new char[bwt.Length];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = s[bwt[i]];
            return arr;
        }

        public int Count(string P)
        {
            int i = P.Length - 1;
            char c;
            if (!_alphabetIds.TryConvert(P[i], out c))
                return 0;
            int lo = _prefixSum[c];
            int hi = _prefixSum[c + 1];

            while (lo < hi && i > 0)
            {
                i--;
                if (!_alphabetIds.TryConvert(P[i], out c))
                    return 0;
                lo = _prefixSum[c] + _occ.CountInPrefix(c, lo);
                hi = _prefixSum[c] + _occ.CountInPrefix(c, hi);
            }

            return hi - lo;
        }
    }
}
