using System;

namespace FmIndex.Abstract
{
    [Serializable]
    internal class PrefixSum : IPrefixSum
    {
        private readonly int[] _counts;

        public PrefixSum(byte[] s, int alphabetSize)
        {
            _counts = ConstructCountsArray(s, alphabetSize);
        }

        private int[] ConstructCountsArray(byte[] s, int alphabetSize)
        {
            var counts = new int[alphabetSize];
            for (int i = 0; i < s.Length; ++i)
                counts[s[i]]++;
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

        int IPrefixSum.this[byte c] => _counts[c];
    }
}
