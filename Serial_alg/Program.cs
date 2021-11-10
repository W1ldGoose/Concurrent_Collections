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
        // путь до папки с текстами
        private static string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                        @"\Chesterton_Books\Chesterton";

        private static int filesCount = 13;
        
        private static Dictionary<char, int> charsFrequency = new();
        private static string[] files = Directory.GetFiles(dirPath);

        // массив символов, которые не нужно учитывать при подсчете
        private static char[] separators;
        
        // чтение из файлов
        static void ReadFiles()
        {
            StringBuilder textBuilder = new StringBuilder();
            for (int i = 0; i < filesCount; i++)
            {
                // закинули все тексты из всех файлов в sb
                textBuilder.Append(File.ReadAllText(files[i]));
            }
            
            string allTexts = textBuilder.ToString();
            // проходимся по всем символам в текстах
            for (int i = 0; i < allTexts.Length; i++)
            {
                char lowerChar = char.ToLower(allTexts[i]);
                // не учитываем при подсчете специальные символы, например, пробел
                if (!separators.Contains(lowerChar))
                {
                    if (charsFrequency.ContainsKey(lowerChar))
                    {
                        charsFrequency[lowerChar]++;
                    }
                    else
                    {
                        charsFrequency.Add(lowerChar, 1);
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

            ReadFiles();
            // останавливаем таймер
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