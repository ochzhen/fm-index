using System;
using FmIndex.Abstract;

namespace FmIndex
{
    [Serializable]
    internal sealed class AlphabetIds : IAlphabetIds
    {
        private readonly int _alphabetSize;
        private readonly short[] _arr;

        public AlphabetIds(string s)
        {
            (int minChar, int maxChar) = FindMinMaxChars(s);
            if (minChar == 0)
                throw new ArgumentException($"Cannot assign anchor because zero character is present");
            if (maxChar > 255)
                throw new ArgumentException($"Max character value is too large: {maxChar}");
            
            short[] arr = CreateInitializedArray(maxChar + 1);
            MarkContainedChars(arr, s);
            _alphabetSize = CountChars(arr);
            AssignNewValues(arr);
            
            _arr = arr;
        }

        private (int, int) FindMinMaxChars(string s)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] < min)
                    min = s[i];
                if (s[i] > max)
                    max = s[i];
            }
            return (min, max);
        }

        private short[] CreateInitializedArray(int size)
        {
            var arr = new short[size];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = -1;
            return arr;
        }

        private void MarkContainedChars(short[] arr, string s)
        {
            arr[0] = 1;
            for (int i = 0; i < s.Length; ++i)
            {
                if (arr[s[i]] == -1)
                    arr[s[i]] = 1;
            }
        }

        private int CountChars(short[] arr)
        {
            int count = 0;
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i] == 1)
                    count++;
            }
            return count;
        }

        private void AssignNewValues(short[] arr)
        {
            short id = 0;
            for (int i = 0; i < arr.Length; ++i)
            {
                if (arr[i] == 1)
                    arr[i] = id++;
            }
        }

        public int Length => _alphabetSize;

        public char Anchor => (char)0;

        public char this[char c]
        {
            get
            {
                if (_arr[c] == -1)
                    throw new ArgumentException("Character is not present in derived alphabet");
                return (char) _arr[c];
            }
        }

        public bool TryConvert(char c, out char value)
        {
            if (c < 0 || c >= _arr.Length || _arr[c] == -1)
            {
                value = default;
                return false;
            }

            value = (char) _arr[c];
            return true;
        }
    }
}
