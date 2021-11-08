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

       

        static void ReadFiles()
        {
            StringBuilder textBuilder = new StringBuilder();
            for (int i = 0; i < filesCount; i++)
            {
                // закинули все тексты из всех файлов в sb
                textBuilder.Append(File.ReadAllText(files[i]));
            }

            string allTexts = textBuilder.ToString();
            for (int i = 0; i < allTexts.Length; i++)
            {
                char lowerChar = char.ToLower(allTexts[i]);
                
                if (sentencesFrequency.ContainsKey(lowerChar))
                {
                    sentencesFrequency[lowerChar]++;
                }
            }
        }

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ReadFiles();
            stopwatch.Stop();

            TimeSpan timeSpan = stopwatch.Elapsed;
            Console.WriteLine("Времени затрачено: " + timeSpan.TotalMilliseconds);
            
            Console.WriteLine("Вопросительные предложения: "+ sentencesFrequency['?']);
            Console.WriteLine("Восклицательные предложения: "+ sentencesFrequency['!']);
            Console.WriteLine("Утвердительные предложения: "+ sentencesFrequency['.']);

        }
    }
}