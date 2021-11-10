using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;


namespace Parallel3
{
    class Program
    {
        // путь до папки с текстами
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;
        private static string[] files = Directory.GetFiles(dirPath);

        // массив символов, которые не нужно учитывать при подсчете
        private static char[] separators;

        // словарь для посчета статистики
        private static ConcurrentDictionary<char, int> charsFrequency = new();

        // глобальный буфер - очередь
        private static ConcurrentQueue<string> globalBuffer = new();

        // кол-во читателей
        private static int readersCount = 2;

        // кол-во писателей
        private static int writersCount = 4;
        private static Thread[] readers = new Thread[readersCount];
        private static Thread[] writers = new Thread[writersCount];

        // шаг для декомпозиции по данным
        private static int filesStep = filesCount / readersCount;

        // для последнего потока, если кол-во файлов не кратно кол-ву потоков
        private static int lastIndex = filesCount % readersCount;

        private static bool isReadingFinished = false;

        // чтение из файлов
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

        static void CountChars()
        {
            //пока в буффере есть элементы и чтение из файлов не завершилось
            while (!globalBuffer.IsEmpty || !isReadingFinished)
            {
                // пытаемся взять очередной текст из буфера, если получается,
                // элемент из буфера удаляется и помещается в переменную text
                if (globalBuffer.TryDequeue(out var text))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        char lowerChar = char.ToLower(text[i]);
                        // не учитываем при подсчете специальные символы, например, пробел
                        if (!separators.Contains(lowerChar))
                        {
                            // если такого символ нет, он добавяется в словарь, иначе увеличиваем значение
                            charsFrequency.AddOrUpdate(lowerChar, 1, (key, oldValue) => ++oldValue);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            List<char> tmp = new List<char>();
            // добавляем в массив separators символы, которые не нужно учитывать
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
            // запускаем таймер
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // инициализируем читателей
            for (int i = 0; i < readersCount; i++)
            {
                readers[i] = new Thread(ReadFiles);
            }

            // инициализируем обработчиков
            for (int i = 0; i < writersCount; i++)
            {
                writers[i] = new Thread(CountChars);
            }

            // запускаем читателей
            for (int i = 0; i < readersCount; i++)
            {
                readers[i].Start(i);
            }

            // запускаем обработчиков
            for (int i = 0; i < writersCount; i++)
            {
                writers[i].Start();
            }

            // ожидаем завершения чтения всех файлов
            for (int i = 0; i < readersCount; i++)
            {
                readers[i].Join();
            }

            isReadingFinished = true;

            // ожидаем завершение работы 
            for (int i = 0; i < writersCount; i++)
            {
                writers[i].Join();
            }

            // останавливаем таймер
            stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;

            Console.WriteLine("Времени затрачено: " + timeSpan.TotalMilliseconds);

            Console.WriteLine("Кол-во уникальных символов: " + charsFrequency.Count);
            Console.WriteLine("Самый частый символ: " +
                              charsFrequency.First(x => x.Value == charsFrequency.Values.Max()).Key + " " +
                              charsFrequency.Values.Max());
        }
    }
}