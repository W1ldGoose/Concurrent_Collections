using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel2
{
    class Program
    {
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;
        
        private static string[] files = Directory.GetFiles(dirPath);
        private static char[] separators;

        private static ConcurrentDictionary<string, int> wordsFrequency = new ConcurrentDictionary<string, int>();
        private static int threadsCount = 4;
        private static Thread[] threads = new Thread[threadsCount];
        private static int filesStep = filesCount / threadsCount;
        private static int lastIndex = filesCount % threadsCount;
        

        static void ReadFiles(object threadIndex)
        {
            int index = (int) threadIndex;
            int startIndex = index * filesStep;
            int finishIndex = (index + 1) * filesStep + (index == threadsCount - 1 ? lastIndex : 0);

            StringBuilder textBuilder = new StringBuilder();

            for (int i = startIndex; i < finishIndex; i++)
            {
                // закинули все тексты из всех файлов в sb
                textBuilder.AppendLine(File.ReadAllText(files[i]));
            }

            // разделяем на слова
            string[] allLocalWords = textBuilder.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < allLocalWords.Length; i++)
            {
                wordsFrequency.AddOrUpdate(allLocalWords[i].ToLower(), 1, (key, oldValue) => ++oldValue);
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
            
            

          stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;

            Console.WriteLine("Всего слов: " + wordsFrequency.Count);
            // Console.WriteLine("Самое частое слово: " + wordsFrequency.First(x => x.Value == wordsFrequency.Values.Max()).Key, wordsFrequency.Values.Max());
            Console.WriteLine("Самое частое слово: " +
                              wordsFrequency.First(x => x.Value == wordsFrequency.Values.Max()).Key + " " +
                              wordsFrequency.Values.Max());
            /*foreach (var pair in wordsFrequency.OrderBy(pair => pair.Key))
            {
                Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
            }*/

            Console.WriteLine("Time: " + timeSpan.TotalMilliseconds);
        }
    }
}