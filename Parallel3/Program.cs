﻿using System;
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
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Texts";

        private static string[] files = Directory.GetFiles(dirPath);
        private static char[] separators;
        private static int filesCount = 40;
        private static ConcurrentDictionary<string, int> wordsFrequency = new ConcurrentDictionary<string, int>();
        private static ConcurrentBag<string> globalBuffer = new ConcurrentBag<string>();
        private static int readersCount = 2;
        private static int handlersCount = 4;
        private static Thread[] readers = new Thread[readersCount];
        private static Thread[] handlers = new Thread[handlersCount];
        private static int filesStep = filesCount / readersCount;
       
        
        

        static void ReadFiles(object threadIndex)
        {
            int index = (int) threadIndex;
            int startIndex = index * filesStep;
            int finishIndex = (index + 1) * filesStep; //+ (index == threadsCount - 1 ? lastIndex : 0);

            for (int i = startIndex; i < finishIndex && i < filesCount; i++)
            {
                // добавляем необработанную порцию текста в буффер
                globalBuffer.Add(File.ReadAllText(files[i]));
            }
        }

        static void HandleTexts()
        {
            string[] localWords;
            string text;
            // пока в буффере есть элементы
            while (!globalBuffer.IsEmpty)
            {
                if (globalBuffer.TryTake(out text))
                {
                    localWords = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < localWords.Length; i++)
                    {
                        wordsFrequency.AddOrUpdate(localWords[i].ToLower(), 1, (key, oldValue) => ++oldValue);
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

                if (!char.IsLetter(ch))
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

            for (int i = 0; i < readersCount; i++)
            {
                readers[i].Start(i);
            }
            // ожидаем завершения чтения их файлов
            for (int i = 0; i < readersCount; i++)
            {
                readers[i].Join();
            }

          
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

            Console.WriteLine("Всего слов: " + wordsFrequency.Count);
             
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