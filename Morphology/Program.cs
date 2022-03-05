using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Morphology.Utils;

namespace Morphology
{
    public static class Program
    {
        public static string DictionaryPath => Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
            @"Resources\dict.opcorpora.txt"
        );
        
        private static void Main()
        {
            var sw = Stopwatch.StartNew();
            var morpher = SentenceMorpher.Create(FileLinesEnumerable.Create(DictionaryPath));
            Console.WriteLine($"Init took {sw.Elapsed}");
            RunLoop(morpher);
        }

        public static void RunLoop(SentenceMorpher morpher)
        {
            var input = "мама{noun,anim,femn,sing,gent} мыла РАМА{noun,inan,femn,sing,accs}";
            var sw = new Stopwatch();
            do
            {
                sw.Restart();
                var result = morpher.Morph(input);
                Console.WriteLine($"[took {sw.Elapsed}]   {result}");
            } while ((input = Console.ReadLine()) is { Length: > 0 });
        }
    }
}