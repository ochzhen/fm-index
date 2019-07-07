using System;
using System.Collections.Generic;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    public class FullTextIndex
    {
        private readonly IPrefixSum _prefixSum;
        private readonly IOcc _occ;
        private readonly IAlphabetIds _alphabetIds;
        private readonly ICompressedSA _compressedSA;

        public FullTextIndex(string T, bool setupLocate, Action<string> logInfo)
        {
            T = T.ToLower();
            logInfo($"AlphabetIds starting {DateTime.Now.ToLongTimeString()}");
            _alphabetIds = new AlphabetIds(T);
            logInfo($"- Alphabet size: {_alphabetIds.Length}");
            logInfo($"AlphabetIds finished {DateTime.Now.ToLongTimeString()}");

            char[] s = CreateTransformedToNewAlphabet(T, _alphabetIds, T.Length + 1);
            s[s.Length - 1] = _alphabetIds.Anchor;
            
            logInfo($"SuffixArray starting {DateTime.Now.ToLongTimeString()}");
            int[] SA = SuffixArray.Create(s, _alphabetIds.Length);
            logInfo($"SuffixArray finished {DateTime.Now.ToLongTimeString()}");
            
            if (setupLocate)
            {
                logInfo($"CompressedSA starting {DateTime.Now.ToLongTimeString()}");
                _compressedSA = new CompressedSA(SA);
                logInfo($"CompressedSA finished {DateTime.Now.ToLongTimeString()}");
            }
            else
            {
                logInfo($"Skipping CompressedSA");
            }

            char[] bwt = CreateBwt(SA, s);

            logInfo($"PrefixSum starting {DateTime.Now.ToLongTimeString()}");
            _prefixSum = new PrefixSum(bwt, _alphabetIds.Length + 1);
            logInfo($"PrefixSum finished {DateTime.Now.ToLongTimeString()}");

            logInfo($"WaveletTree starting {DateTime.Now.ToLongTimeString()}");
            var waveletTree = new WaveletTree(bwt);
            _occ = waveletTree;
            logInfo($"WaveletTree finished {DateTime.Now.ToLongTimeString()}");
            logInfo($"- Nodes in the Wavelet Tree: {waveletTree.CountNodes()}");
        }

        private char[] CreateTransformedToNewAlphabet(string T, IAlphabetIds alphabetIds, int size)
        {
            var s = new char[size];
            for (int i = 0; i < T.Length; ++i)
                s[i] = alphabetIds[T[i]];
            return s;
        }

        private char[] CreateBwt(int[] SA, char[] s)
        {
            int N = s.Length;
            var arr = new char[SA.Length];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = s[(SA[i] - 1 + N) % N];
            return arr;
        }

        public int Count(string P)
        {
            (int lo, int hi) = FindRange(P);
            return hi - lo;
        }

        public IEnumerable<int> FindPositions(string P, int entriesCount = 50)
        {
            if (_compressedSA == null)
                throw new InvalidOperationException("Operation for locating pattern positions is not set up");
            const int maxEntries = 100;
            entriesCount = Math.Min(entriesCount, maxEntries);
            (int lo, int hi) = FindRange(P);
            for (int i = lo; i < hi && i < lo + maxEntries; ++i)
                yield return LocateInText(i);
        }

        private (int, int) FindRange(string P)
        {
            if (string.IsNullOrEmpty(P))
                return (0, 0);
            int i = P.Length - 1;
            char c;
            if (!_alphabetIds.TryConvert(char.ToLower(P[i]), out c))
                return (0, 0);
            int lo = _prefixSum[c];
            int hi = _prefixSum[c + 1];

            while (lo < hi && i > 0)
            {
                i--;
                if (!_alphabetIds.TryConvert(char.ToLower(P[i]), out c))
                    return (0, 0);
                lo = _prefixSum[c] + _occ.CountInPrefix(c, lo);
                hi = _prefixSum[c] + _occ.CountInPrefix(c, hi);
            }

            return (lo, hi);
        }

        private int LocateInText(int idx)
        {
            int steps = 0;
            while (!_compressedSA.IsStored(idx))
            {
                idx = LF(idx);
                steps++;
            }
            return steps + _compressedSA.GetStoredPosition(idx);
        }

        private int LF(int idx)
        {
            char c = DetermineChar(idx);
            return _prefixSum[c] + _occ.CountInPrefix(c, idx + 1) - 1;
        }

        private char DetermineChar(int idx)
        {
            for (int c = 0; c < _alphabetIds.Length; ++c)
            {
                if (_occ.CountInPrefix((char)c, idx) != _occ.CountInPrefix((char)c, idx + 1))
                    return (char)c;
            }
            throw new Exception("Logic error: cannot determine character");
        }
    }
}
