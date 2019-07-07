using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using FmIndex;

namespace app
{
    class Program
    {
        const int maxDisplayCount = 20;

        static void Main(string[] args)
        {
            (bool load, string inputFile, string idxFile, bool setupLocate, bool verify) = ParseParams(args);
            FullTextIndex fmIndex;
            string T = null;

            if (load)
            {
                Console.WriteLine($"Loading fm-index from {idxFile}");
                fmIndex = Deserialize<FullTextIndex>(idxFile, new BinaryFormatter());
            }
            else
            {
                Console.WriteLine($"Reading input from {inputFile}");
                T = File.ReadAllText(inputFile);
                fmIndex = new FullTextIndex(T, setupLocate, Console.WriteLine);
                Serialize(fmIndex, new BinaryFormatter(), idxFile);
            }

            if (verify && T == null)
                T = File.ReadAllText(inputFile);

            Console.WriteLine();

            while (true)
            {
                try
                {
                    Console.WriteLine("- Enter pattern:");
                    string P = Console.ReadLine();
                    int count = fmIndex.Count(P);
                    Console.WriteLine($"== Answer: {count} occurrences");
                    
                    if (count == 0 || !setupLocate)
                        continue;

                    Console.WriteLine($"- Find positions of occurrences (max {maxDisplayCount})? Yes(Y) No(N):");
                    string cmd = Console.ReadLine().Trim().ToUpper();
                    if (cmd == "Y")
                    {
                        int[] positions = fmIndex.FindPositions(P, maxDisplayCount).ToArray();
                        Console.Write("== Positions:");
                        foreach(int pos in positions)
                            Console.Write($" {pos}");
                        Console.WriteLine();
                        if (verify)
                        {
                            Console.Write("== Verification: ||");
                            foreach(int pos in positions)
                                Console.Write($"{T.Substring(pos, P.Length)}||");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                Console.WriteLine();
            }
        }

        static (bool, string, string, bool, bool) ParseParams(string[] args)
        {
            bool load = false;
            string index = "fm-index.idx";
            string file = null;
            bool locate = false;
            bool verify = false;
            for (int i = 0; i < args.Length; ++i)
            {
                (string key, string value) = ParseParameter(args[i]);
                if (key == nameof(load) && bool.TryParse(value, out bool loadResult))
                    load = loadResult;
                else if (key == nameof(index))
                    index = value;
                else if (key == nameof(file))
                    file = value;
                else if (key == nameof(locate) && bool.TryParse(value, out bool locateResult))
                    locate = locateResult;
                else if (key == nameof(verify) && bool.TryParse(value, out bool verifyResult))
                    verify = verifyResult;
            }
            Console.WriteLine($"Parameters: {nameof(load)}={load}, {nameof(file)}={file}, {nameof(index)}={index}, {nameof(locate)}={locate} {nameof(verify)}={verify}");
            return (load, file, index, locate, verify);
        }

        static (string, string) ParseParameter(string s)
        {
            if (!s.Contains('='))
                return (null, null);
            string[] parts = s.Split('=');
            return (parts[0].ToLower(), parts[1]);
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
