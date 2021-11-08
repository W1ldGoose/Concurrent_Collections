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


        private static ConcurrentDictionary<char, int> sentencesFrequency = new();

        private static ConcurrentStack<string> globalBuffer = new();

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
                globalBuffer.Push(File.ReadAllText(files[i]));
            }
        }

        static void HandleTexts()
        {
            //пока в буффере есть элементы
            while (!globalBuffer.IsEmpty)
            {
                if (globalBuffer.TryPop(out var text))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (sentencesFrequency.ContainsKey(text[i]))
                        {
                            lock ("handle")
                            {
                                if (sentencesFrequency.TryGetValue(text[i], out var oldValue))
                                {
                                    sentencesFrequency.TryUpdate(text[i], oldValue + 1, oldValue);
                                } 
                            }
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

            Console.WriteLine("Вопросительные предложения: " + sentencesFrequency['?']);
            Console.WriteLine("Восклицательные предложения: " + sentencesFrequency['!']);
            Console.WriteLine("Утвердительные предложения: " + sentencesFrequency['.']);
        }
    }
}