using FmIndex.Abstract;

namespace FmIndex
{
    internal class WaveletTree : IOcc
    {
        private readonly string T;
        private readonly int[] bwt;

        public WaveletTree(string T, int[] bwt)
        {
            this.T = T;
            this.bwt = bwt;
        }

        public int CountOccurrencesInPrefix(char c, int k)
        {
            int count = 0;
            for (int i = 0; i < k; ++i)
            {
                if (T[bwt[i]] == c)
                    count++;
            }
            return count;
        }
    }
}