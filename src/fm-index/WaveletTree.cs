using FmIndex.Abstract;

namespace FmIndex
{
    internal class WaveletTree : IOcc
    {
        private readonly char[] s;
        // private readonly IBitVector bitVector;

        public WaveletTree(char[] s)
        {
            this.s = s;
        }

        public int CountInPrefix(char c, int len)
        {
            int count = 0;
            for (int i = 0; i < len; ++i)
            {
                if (s[i] == c)
                    count++;
            }
            return count;
        }
    }
}
