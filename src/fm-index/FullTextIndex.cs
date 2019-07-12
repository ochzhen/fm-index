using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            logInfo($"Text length: {T.Length}");
            var sw = new Stopwatch();

            logInfo($"AlphabetIds starting...");
            sw.Start();
            _alphabetIds = new AlphabetIds(T);
            sw.Stop();
            logInfo($"- Alphabet size: {_alphabetIds.Length}");
            logInfo($"AlphabetIds finished: {sw.ElapsedMilliseconds} ms, {sw.Elapsed}");

            byte[] s = CreateTransformedToNewAlphabet(T, _alphabetIds, T.Length + 1);
            s[s.Length - 1] = _alphabetIds.Anchor;
            
            logInfo($"SuffixArray starting...");
            sw.Restart();
            int[] SA = SuffixArray.Create(s, _alphabetIds.Length);
            sw.Stop();
            logInfo($"SuffixArray finished: {sw.ElapsedMilliseconds} ms, {sw.Elapsed}");
            
            if (setupLocate)
            {
                logInfo($"CompressedSA starting...");
                sw.Restart();
                _compressedSA = new CompressedSA(SA);
                sw.Stop();
                logInfo($"CompressedSA finished: {sw.ElapsedMilliseconds} ms, {sw.Elapsed}");
            }
            else
            {
                logInfo($"Skipping CompressedSA");
            }

            byte[] bwt = CreateBwt(SA, s);

            logInfo($"PrefixSum starting...");
            sw.Restart();
            _prefixSum = new PrefixSum(bwt, _alphabetIds.Length + 1);
            sw.Stop();
            logInfo($"PrefixSum finished: {sw.ElapsedMilliseconds} ms, {sw.Elapsed}");

            logInfo($"WaveletTree starting...");
            sw.Restart();
            var waveletTree = new WaveletTree(bwt);
            sw.Stop();
            logInfo($"WaveletTree finished: {sw.ElapsedMilliseconds} ms, {sw.Elapsed}");
            logInfo($"- Nodes in the Wavelet Tree: {waveletTree.CountNodes()}");
            _occ = waveletTree;
        }

        private byte[] CreateTransformedToNewAlphabet(string T, IAlphabetIds alphabetIds, int size)
        {
            var s = new byte[size];
            for (int i = 0; i < T.Length; ++i)
                s[i] = alphabetIds[T[i]];
            return s;
        }

        private byte[] CreateBwt(int[] SA, byte[] s)
        {
            int N = s.Length;
            var arr = new byte[SA.Length];
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
            byte c;
            if (!_alphabetIds.TryConvert(char.ToLower(P[i]), out c))
                return (0, 0);
            int lo = _prefixSum[c];
            int hi = _prefixSum[(byte)(c + 1)];

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
            byte c = DetermineChar(idx);
            return _prefixSum[c] + _occ.CountInPrefix(c, idx + 1) - 1;
        }

        private byte DetermineChar(int idx)
        {
            for (byte c = 0; c < _alphabetIds.Length; ++c)
            {
                if (_occ.CountInPrefix(c, idx) != _occ.CountInPrefix(c, idx + 1))
                    return c;
            }
            throw new Exception("Logic error: cannot determine character");
        }
    }
}
