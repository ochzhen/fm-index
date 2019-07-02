using System;

namespace FmIndex
{
    internal static class BWT
    {
        static int alphabet = 256;
        static int[] count;
        static int[] nextSA;
        static int[] nextClasses;
        
        public static int[] Transform(string s)
        {
            int N = s.Length;
            count = new int[Math.Max(alphabet, N)];
            nextSA = new int[N];
            nextClasses = new int[N];
            int[] SA = SortChars(s);
            int[] classes = ComputeClasses(s, SA);
            int L = 1;
            while (L < s.Length)
            {
                SA = SortDoubled(s, L, SA, classes);
                classes = UpdateClasses(SA, classes, L);
                L *= 2;
            }
            for (int i = 0; i < SA.Length; ++i)
                SA[i] = (SA[i] - 1 + N) % N;
            return SA;
        }
        
        private static int[] SortChars(string s)
        {
            int N = s.Length;
            var SA = new int[N];
            for (int i = 0; i < N; ++i)
                count[s[i]]++;
            for (int i = 1; i < alphabet; ++i)
                count[i] += count[i-1];
            for (int i = N-1; i >= 0; --i)
            {
                count[s[i]]--;
                SA[count[s[i]]] = i;
            }
            ClearArray(count, alphabet);
            return SA;
        }
        
        private static int[] ComputeClasses(string s, int[] SA)
        {
            int N = s.Length;
            int[] classes = new int[N];
            classes[SA[0]] = 0;
            for (int i = 1; i < N; ++i)
            {
                if (s[SA[i]] == s[SA[i-1]])
                    classes[SA[i]] = classes[SA[i-1]];
                else
                    classes[SA[i]] = classes[SA[i-1]] + 1;
            }
            return classes;
        }
        
        private static int[] SortDoubled(string s, int L, int[] SA, int[] classes)
        {
            int N = s.Length;
            for (int i = 0; i < N; ++i)
                count[classes[i]]++;
            for (int i = 1; i < N; ++i)
                count[i] += count[i-1];
            for (int i = N-1; i >= 0; --i)
            {
                int start = (SA[i] - L + N) % N;
                int cls = classes[start];
                count[cls]--;
                nextSA[count[cls]] = start;
            }
            ClearArray(count, N);
            var res = nextSA;
            nextSA = SA;
            return res;
        }
        
        private static int[] UpdateClasses(int[] SA, int[] classes, int L)
        {
            int N = SA.Length;
            nextClasses[SA[0]] = 0;
            for (int i = 1; i < N; ++i)
            {
                int prev = SA[i-1];
                int curr = SA[i];
                int midPrev = (prev + L) % N;
                int midCurr = (curr + L) % N;
                if (classes[curr] != classes[prev] || classes[midCurr] != classes[midPrev])
                    nextClasses[curr] = nextClasses[prev] + 1;
                else
                    nextClasses[curr] = nextClasses[prev];
            }
            var res = nextClasses;
            nextClasses = classes;
            return res;
        }
        
        static void ClearArray(int[] a, int N)
        {
            for (int i = 0; i < N; ++i)
                a[i] = 0;
        }
    }
}