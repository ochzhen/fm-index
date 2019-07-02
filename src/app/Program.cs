using System;
using FmIndex;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for a text:");
            string T = Console.ReadLine();
            FullTextIndex fmIndex = new FullTextIndex(T);
            while (true)
            {
                Console.WriteLine("Enter pattern:");
                string P = Console.ReadLine();
                Console.WriteLine($"Count {fmIndex.Count(P)}");
            }
        }
    }
}
