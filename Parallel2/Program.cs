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
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;

        private static string[] files = Directory.GetFiles(dirPath);

        private static ConcurrentDictionary<char, int> sentencesFrequency = new();
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
                textBuilder.Append(File.ReadAllText(files[i]));
            }

            string allTexts = textBuilder.ToString();

            for (int i = 0; i < allTexts.Length; i++)
            {
                if (sentencesFrequency.ContainsKey(allTexts[i]))
                {
                    // так как метод ContainsKey не синхронизирован, нужно использовать дополнительную блокировку
                    lock ("handle")
                    {
                        if (sentencesFrequency.TryGetValue(allTexts[i], out var oldValue))
                        {
                            sentencesFrequency.TryUpdate(allTexts[i], oldValue + 1, oldValue);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            sentencesFrequency.TryAdd('!', 0);
            sentencesFrequency.TryAdd('?', 0);
            sentencesFrequency.TryAdd('.', 0);

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

            Console.WriteLine("Времени затрачено: " + timeSpan.TotalMilliseconds);
            
            Console.WriteLine("Вопросительные предложения: " + sentencesFrequency['?']);
            Console.WriteLine("Восклицательные предложения: " + sentencesFrequency['!']);
            Console.WriteLine("Утвердительные предложения: " + sentencesFrequency['.']);
        }
    }
}