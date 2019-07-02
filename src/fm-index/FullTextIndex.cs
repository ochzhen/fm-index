using System;
using FmIndex.Abstract;

namespace FmIndex
{
    public class FullTextIndex
    {
        public const char SEPARATOR = ' ';
        public const int ALPHABET = 256;

        private int[] _smallerCount;
        private IOcc _occ;

        public FullTextIndex(string T)
        {
            T = $"{T.Replace(' ', '_')} ";
            int[] bwt = BWT.Transform(T);
            _smallerCount = ConstructCountArray(T, bwt);
            _occ = new WaveletTree(T, bwt);
        }

        private int[] ConstructCountArray(string T, int[] bwt)
        {
            var counts = new int[ALPHABET];
            for (int i = 0; i < bwt.Length; ++i)
                counts[T[bwt[i]]]++;
            int next = counts[0];
            counts[0] = 0;
            for (int i = 1; i < counts.Length; ++i)
            {
                int tmp = counts[i];
                counts[i] = next;
                next += tmp;
            }
            return counts;
        }

        public int Count(string P)
        {
            P = P.Replace(' ', '_');

            int i = P.Length - 1;
            int lo = _smallerCount[P[i]];
            int hi = _smallerCount[P[i] + 1];

            while (lo < hi && i > 0)
            {
                i--;
                char c = P[i];
                lo = _smallerCount[c] + _occ.CountOccurrencesInPrefix(c, lo);
                hi = _smallerCount[c] + _occ.CountOccurrencesInPrefix(c, hi);
            }

            return hi - lo;
        }
    }
}