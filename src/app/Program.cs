using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Diagnostics;
using FmIndex;

namespace app
{
    class Program
    {
        const int maxDisplayCount = 20;

        static void Main(string[] args)
        {
            checked
            {
                Run(args);
            }
        }

        static void Run(string[] args)
        {
            (bool load, string inputFile, string idxFile,
                bool setupLocate, bool verify, int margin) = ParseParams(args);
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

            var sw = new Stopwatch();

            while (true)
            {
                try
                {
                    Console.WriteLine("- Enter pattern:");
                    string P = Console.ReadLine();
                    sw.Restart();
                    int count = fmIndex.Count(P);
                    sw.Stop();
                    Console.WriteLine($"Count(P): length={P.Length}, {sw.ElapsedMilliseconds} ms");
                    Console.WriteLine($"== Answer: {count} occurrences");
                    
                    if (count == 0 || !setupLocate)
                        continue;

                    Console.WriteLine($"- Find positions of occurrences (max {maxDisplayCount})? Yes(Y) No(N):");
                    string cmd = Console.ReadLine().Trim().ToUpper();
                    if (cmd == "Y")
                    {
                        sw.Restart();
                        int[] positions = fmIndex.FindPositions(P, maxDisplayCount).ToArray();
                        sw.Stop();
                        Console.WriteLine($"Locate(P): length={P.Length}, {sw.ElapsedMilliseconds} ms, {positions.Length} entries");
                        Console.Write("== Positions:");
                        foreach(int pos in positions)
                            Console.Write($" {pos}");
                        Console.WriteLine();
                        if (verify)
                        {
                            Console.Write("== Verification: ||");
                            foreach(int pos in positions)
                                Console.Write($" ..{T.Substring(Math.Max(pos-margin, 0), Math.Min(P.Length + 2*margin, T.Length - Math.Max(pos-margin, 0))).Replace($"{Environment.NewLine}", " ")}.. ||");
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

        static (bool, string, string, bool, bool, int) ParseParams(string[] args)
        {
            bool load = false;
            string index = "indexes/fm-index.idx";
            string file = null;
            bool locate = false;
            bool verify = false;
            int margin = 5;
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
                else if (key == nameof(margin) && int.TryParse(value, out int marginResult))
                    margin = marginResult;
            }
            Console.WriteLine(
                $"Parameters: {nameof(load)}={load}, {nameof(file)}={file}, {nameof(index)}={index}, " +
                $"{nameof(locate)}={locate} {nameof(verify)}={verify} {nameof(margin)}={margin}");
            return (load, file, index, locate, verify, margin);
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
