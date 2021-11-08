using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace Parallel3
{
    class Program
    {
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;

        private static string[] files = Directory.GetFiles(dirPath);
        private static char[] separators;

        private static ConcurrentDictionary<char, int> charsFrequency = new();

        private static ConcurrentQueue<string> globalBuffer = new();
        
        private static int readersCount = 2;
        private static int handlersCount = 4;
        private static Thread[] readers = new Thread[readersCount];
        private static Thread[] handlers = new Thread[handlersCount];
        
        private static int filesStep = filesCount / readersCount;
        private static int lastIndex = filesCount % readersCount;

        static void ReadFiles(object threadIndex)
        {
            int index = (int) threadIndex;
            int startIndex = index * filesStep;
            int finishIndex = (index + 1) * filesStep + (index == readersCount - 1 ? lastIndex : 0);

            for (int i = startIndex; i < finishIndex; i++)
            {
                // добавляем необработанную порцию текста в буффер
                globalBuffer.Enqueue(File.ReadAllText(files[i]));
            }
        }

        static void HandleTexts()
        { 
            //пока в буффере есть элементы
            while (!globalBuffer.IsEmpty)
            {
                if (globalBuffer.TryDequeue(out var text))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        char lowerChar = char.ToLower(text[i]);
                        if (!separators.Contains(lowerChar))
                        {
                            charsFrequency.AddOrUpdate(lowerChar, 1, (key, oldValue) => ++oldValue);
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

            for (int i = 0; i < readersCount; i++)
            {
                readers[i] = new Thread(ReadFiles);
            }

            for (int i = 0; i < handlersCount; i++)
            {
                handlers[i] = new Thread(HandleTexts);
            }

            // запускаем читателей
            for (int i = 0; i < readersCount; i++)
            {
                readers[i].Start(i);
            }

            // ожидаем завершения чтения всех файлов
            for (int i = 0; i < readersCount; i++)
            {
                readers[i].Join();
            }

            // запускаем обработчиков
            for (int i = 0; i < handlersCount; i++)
            {
                handlers[i].Start();
            }

            for (int i = 0; i < handlersCount; i++)
            {
                handlers[i].Join();
            }

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