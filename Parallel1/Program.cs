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
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;

        private static Dictionary<char, int> sentencesFrequency = new Dictionary<char, int>
        {
            {'!', 0},
            {'?', 0},
            {'.', 0}
        };

        private static string[] files = Directory.GetFiles(dirPath);


        private static int threadsCount = 4;
        private static Thread[] threads = new Thread[threadsCount];
        private static int filesStep = filesCount / threadsCount;
        private static int lastIndex = filesCount % threadsCount;

        private static ConcurrentDictionary<char, int>[] localSentences =
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

            string localTexts = textBuilder.ToString();
            CountLocalSentencesFrequencies(localTexts, index);
        }

        // вычисляем локальные результаты по группе файлов и записываем в локальный словарь
        static void CountLocalSentencesFrequencies(string localTexts, int localDictIndex)
        {
            localSentences[localDictIndex] = new ConcurrentDictionary<char, int>();
            localSentences[localDictIndex].TryAdd('!', 0);
            localSentences[localDictIndex].TryAdd('?', 0);
            localSentences[localDictIndex].TryAdd('.', 0);
            
            for (int i = 0; i < localTexts.Length; i++)
            {
                // так как метод ContainsKey не синхронизирован, нужно использовать дополнительную блокировку
                if (localSentences[localDictIndex].ContainsKey(localTexts[i]))
                {
                    lock ("handle")
                    {
                        if (localSentences[localDictIndex].TryGetValue(localTexts[i], out var oldValue))
                        {
                            int tmp = oldValue+1;
                            localSentences[localDictIndex].TryUpdate(localTexts[i], tmp, oldValue);
                        }
                    }
                   
                }
            }
        } 

        // вычисление глобальной статистики
        static void CountOverallWords()
        {
            // проходимся по всем локальным буферам
            for (int i = 0; i < localSentences.Length; i++)
            {
                char[] keys = localSentences[i].Keys.ToArray();
                int[] values = localSentences[i].Values.ToArray();
                
                for (int j = 0; j < keys.Length; j++)
                {
                    if (sentencesFrequency.ContainsKey(keys[j]))
                    {
                        sentencesFrequency[keys[j]] += values[j];
                    }
                    else
                    {
                        sentencesFrequency.Add(keys[j], values[j]);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
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


            Console.WriteLine("Вопросительные предложения: " + sentencesFrequency['?']);
            Console.WriteLine("Восклицательные предложения: " + sentencesFrequency['!']);
            Console.WriteLine("Утвердительные предложения: " + sentencesFrequency['.']);
        }
    }
}