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
        // путь до папки с текстами
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;

        // словарь, для глобальной статистики 
        private static Dictionary<char, int> charsFrequency = new();
        private static string[] files = Directory.GetFiles(dirPath);

        // массив символов, которые не нужно учитывать при подсчете
        private static char[] separators;

        private static int threadsCount = 4;

        private static Thread[] threads = new Thread[threadsCount];

        // шаг для декомпозиции по данным
        private static int filesStep = filesCount / threadsCount;
        // для последнего потока, если кол-во файлов не кратно кол-ву потоков
        private static int lastIndex = filesCount % threadsCount;

        // локальные словари
        private static Dictionary<char, int>[] localChars =
            new Dictionary<char, int>[threadsCount];

        // чтение из файлов
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
            CountLocalChars(localTexts, index);
        }

        // вычисляем локальные результаты по группе файлов и записываем в локальный словарь
        // соответствующего потока обработчика 
        static void CountLocalChars(string localTexts, int localDictIndex)
        {
            Program.localChars[localDictIndex] = new Dictionary<char, int>();
            for (int i = 0; i < localTexts.Length; i++)
            {
                char lowerChar = char.ToLower(localTexts[i]);
                // не учитываем при подсчете специальные символы, например, пробел
                if (!separators.Contains(lowerChar))
                {
                    if (localChars[localDictIndex].ContainsKey(lowerChar))
                    {
                        localChars[localDictIndex][lowerChar]++;
                    }
                    else
                    {
                        localChars[localDictIndex].Add(lowerChar, 1);
                    }
                }
            }
        }

        // вычисление глобальной статистики после работы потоков 
        static void CountAllChars()
        {
            // цикл по локальным словарям
            for (int i = 0; i < localChars.Length; i++)
            {
                char[] keys = localChars[i].Keys.ToArray();
                int[] values = localChars[i].Values.ToArray();
                for (int j = 0; j < keys.Length; j++)
                {
                    char lowerChar = char.ToLower(keys[j]);
                    // не учитываем при подсчете специальные символы, например, пробел
                    if (!separators.Contains(lowerChar))
                    {
                        if (charsFrequency.ContainsKey(lowerChar))
                        {
                            charsFrequency[lowerChar] += values[j];
                        }
                        else
                        {
                            // если такого символа в глобальном словаре нет,добавляем его
                            // а значение устанавливаем из локального словаря
                            charsFrequency.Add(lowerChar, values[j]);
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
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i] = new Thread(ReadFiles);
            }

            // запускаем читателей
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i].Start((object) i);
            }

            // ожидаем завершения чтения их файлов
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i].Join();
            }

            CountAllChars();

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