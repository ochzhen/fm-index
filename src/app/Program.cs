using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FmIndex;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            const string indexFilename = "fm-index.dat";
            string inputFilename = args[0];
            
            string T = File.ReadAllText(inputFilename);
            var fmIndex = new FullTextIndex(T, Console.WriteLine);
            
            var formatter = new BinaryFormatter();
            Serialize(fmIndex, formatter, indexFilename);
            fmIndex = Deserialize<FullTextIndex>(indexFilename, formatter);

            Console.WriteLine();

            while (true)
            {
                try
                {
                    Console.WriteLine("- Enter pattern:");
                    string P = Console.ReadLine();
                    Console.WriteLine($"Answer: {fmIndex.Count(P)} occurrences");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                Console.WriteLine();
            }
        }

        static void Serialize(object obj, IFormatter formatter, string fileName)
        {
            FileStream writeStream = File.Open(fileName, FileMode.Create);
            formatter.Serialize(writeStream, obj);
            writeStream.Close();
            Console.WriteLine("Serialized to " + fileName);
        }

        static T Deserialize<T>(string path, IFormatter formatter)
        {
            FileStream readStream = File.Open(path, FileMode.Open);
            var obj = (T) formatter.Deserialize(readStream);
            readStream.Close();
            Console.WriteLine($"Deserialized from {path}");
            return obj;
        }
    }
}
