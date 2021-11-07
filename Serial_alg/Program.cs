using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial_alg
{
    class Program
    {
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;
        private static Dictionary<string, int> wordsFrequency = new();
        private static string[] files = Directory.GetFiles(dirPath);

        private static char[] separators;

        static void ReadFiles()
        {
            StringBuilder textBuilder = new StringBuilder();
            for (int i = 0; i < filesCount; i++)
            {
                // закинули все тексты из всех файлов в sb
                textBuilder.Append(File.ReadAllText(files[i]));
            }

            string[] allWords = textBuilder.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < allWords.Length; i++)
            {
                string lowerWord = allWords[i].ToLower();
                if (wordsFrequency.ContainsKey(lowerWord))
                {
                    wordsFrequency[lowerWord]++;
                }
                else
                {
                    wordsFrequency.Add(lowerWord, 1);
                }
            }
        }

        static void Main(string[] args)
        {
            List<char> tmp = new List<char>();
            for (int ctr = (int) (Char.MinValue);
                ctr <= (int) (Char.MaxValue);
                ctr++)
            {
                char ch = (Char) ctr;

                if (!char.IsLetter(ch))
                    tmp.Add(ch);
            }

            tmp.Add('\t');
            tmp.Add('\n');
            tmp.Add('\r');
            separators = tmp.ToArray();
           
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

             ReadFiles();
            stopwatch.Stop();

            TimeSpan timeSpan = stopwatch.Elapsed;
            Console.WriteLine("Всего слов: " + wordsFrequency.Count);
            Console.WriteLine("Самое частое слово: " + wordsFrequency.First(x => x.Value == wordsFrequency.Values.Max())
                .Key + " " + wordsFrequency.Values.Max());
            /*foreach (var pair in wordsFrequency.OrderBy(pair => pair.Key))
            {
                Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
            }*/

            Console.WriteLine("Time: " + timeSpan.TotalMilliseconds);
        }
    }
}