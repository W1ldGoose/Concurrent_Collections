using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Parallel1
{
    class Program
    {
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Chesterton_Books\Chesterton";
        private static int filesCount = 13;
       
        private static Dictionary<char, int> charsFrequency = new();
        private static string[] files = Directory.GetFiles(dirPath);
        private static char[] separators;

        private static int threadsCount = 4;
        private static Thread[] threads = new Thread[threadsCount];
        private static int filesStep = filesCount / threadsCount;
        private static int lastIndex = filesCount % threadsCount;

        private static ConcurrentDictionary<char, int>[] localWords =
            new ConcurrentDictionary<char, int>[threadsCount];


        static void ReadFiles(object threadIndex)
        {
            int index = (int) threadIndex;
            int startIndex = index * filesStep;
            int finishIndex = (index + 1) * filesStep + (index == threadsCount - 1 ? lastIndex : 0);

            StringBuilder textBuilder = new StringBuilder();

            for (int i = startIndex; i < finishIndex; i++)
            {
                // закинули все тексты из всех файлов в sb
                textBuilder.Append(File.ReadAllText(files[i]));
            }

            string localChars = textBuilder.ToString();
            CountLocalWords(localChars, index);
        }

        // вычисляем локальные результаты по группе файлов и записываем в локальный словарь
        static void CountLocalWords(string localChars, int localDictIndex)
        {
            localWords[localDictIndex] = new ConcurrentDictionary<char, int>();
            for (int i = 0; i < localChars.Length; i++)
            {
                localWords[localDictIndex].AddOrUpdate(char.ToLower(localChars[i]), 1, (key, oldValue) => ++oldValue);
            }
        }

        // вычисление глобальной статистики
        static void CountOverallWords()
        {
            int temp;
            for (int i = 0; i < localWords.Length; i++)
            {
                char[] keys = localWords[i].Keys.ToArray();
                int[] values = localWords[i].Values.ToArray();
                for (int j = 0; j < keys.Length; j++)
                {
                    char lowerChar = char.ToLower(keys[j]);
                    if (!separators.Contains(lowerChar))
                    {
                        if (charsFrequency.ContainsKey(lowerChar))
                        {
                            charsFrequency[lowerChar] += values[j];
                        }
                        else
                        {
                            charsFrequency.Add(lowerChar, values[j]);
                        }
                    }
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
                if (char.IsSeparator(ch))
                    tmp.Add(ch);
                if (char.IsWhiteSpace(ch))
                    tmp.Add(ch);
            }

            tmp.Add('\t');
            tmp.Add('\n');
            tmp.Add('\r');
            separators = tmp.ToArray();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i] = new Thread(ReadFiles);
            }

            for (int i = 0; i < threadsCount; i++)
            {
                threads[i].Start(i);
            }

            // ожидаем завершения чтения их файлов
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i].Join();
            }

            CountOverallWords();
            
           


            stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;
            Console.WriteLine("Времени затрачено: " + timeSpan.TotalMilliseconds);
            Console.WriteLine("Кол-во уникальных символов: " + charsFrequency.Count);
            Console.WriteLine("Самый частый символ: " +
                              charsFrequency.First(x => x.Value == charsFrequency.Values.Max()).Key + " " +
                              charsFrequency.Values.Max());
           
            /*foreach (var pair in wordsFrequency.OrderBy(pair => pair.Key))
            {
                Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
            }*/


        }
    }
}