using System;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    internal sealed class WaveletTree : IOcc
    {
        private readonly Node root;

        public WaveletTree(char[] s, int alphabet, int offset)
        {
            root = new Node(s, 0, s.Length, offset, offset + alphabet, new char[s.Length]);
        }

        public int CountInPrefix(char c, int len)
        {
            return root.Rank(c, len);
        }

        public int CountNodes()
        {
            return root.CountNodes();
        }

        [Serializable]
        private sealed class Node
        {
            private readonly IBitVector bitVector;
            private readonly int alphaStart;
            private readonly int alphaEnd;
            private readonly Node left;
            private readonly Node right;

            public Node(char[] s, int lo, int hi, int alphaStart, int alphaEnd, char[] aux)
            {
                this.alphaStart = alphaStart;
                this.alphaEnd = alphaEnd;
                if (lo == hi)
                    return;

                var bits = new bool[hi - lo];
                int idx = 0;
                for (int i = lo; i < hi; ++i)
                {
                    if (BelongsLeft(s[i]))
                        bits[idx++] = false;
                    else
                        bits[idx++] = true;
                }

                bitVector = new BitVector(bits);
                int m = StablePartition(s, lo, hi, BelongsLeft, aux);
                if (alphaEnd - alphaStart <= 2)
                    return;
                int alphaPivot = (alphaStart + alphaEnd) / 2;
                left = new Node(s, lo, m, alphaStart, alphaPivot, aux);
                right = new Node(s, m, hi, alphaPivot, alphaEnd, aux);
            }

            private bool BelongsLeft(int x)
            {
                return x < (alphaStart + alphaEnd) / 2;
            }

            private int StablePartition(char[] s, int lo, int hi, Func<int, bool> belongsLeft, char[] aux)
            {
                for (int i = lo; i < hi; ++i)
                    aux[i] = s[i];

                int idx = lo;
                for (int i = lo; i < hi; ++i)
                {
                    if (belongsLeft(aux[i]))
                        s[idx++] = aux[i];
                }

                int ans = idx;
                for (int i = lo; i < hi; ++i)
                {
                    if (!belongsLeft(aux[i]))
                        s[idx++] = aux[i];
                }

                return ans;
            }

            public int Rank(char c, int len)
            {
                if (len == 0 || bitVector == null || c < alphaStart || c >= alphaEnd)
                    return 0;
                if (IsLeaf)
                {
                    if (BelongsLeft(c))
                        return bitVector.RankZero(len);
                    else
                        return bitVector.RankOne(len);
                }
                else
                {
                    if (BelongsLeft(c))
                        return left.Rank(c, bitVector.RankZero(len));
                    else
                        return right.Rank(c, bitVector.RankOne(len));
                }
            }

            private bool IsLeaf => left == null;

            public int CountNodes()
            {
                if (IsLeaf)
                    return 1;
                return 1 + left.CountNodes() + right.CountNodes();
            }
        }
    }
}
