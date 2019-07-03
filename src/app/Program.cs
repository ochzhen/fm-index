using System;
using FmIndex;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter text:");
            string T = Console.ReadLine();
            FullTextIndex fmIndex = new FullTextIndex(T, 256, 0, (char) 0);
            while (true)
            {
                Console.WriteLine("Enter pattern:");
                string P = Console.ReadLine();
                Console.WriteLine($"Count {fmIndex.Count(P)}");
            }
        }
    }
}
