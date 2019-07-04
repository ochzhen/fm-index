using System;

namespace FmIndex.Abstract
{
    [Serializable]
    internal class PrefixSum : IPrefixSum
    {
        private readonly int[] _counts;
        private readonly int _offset;
        private readonly int _alphabet;

        public PrefixSum(char[] s, int alphabet, int offset)
        {
            _offset = offset;
            _alphabet = alphabet;
            _counts = ConstructCountsArray(s);
        }

        private int[] ConstructCountsArray(char[] s)
        {
            var counts = new int[_alphabet];
            for (int i = 0; i < s.Length; ++i)
                counts[s[i] - _offset]++;
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

        int IPrefixSum.this[int c] => _counts[c - _offset];
    }
}
