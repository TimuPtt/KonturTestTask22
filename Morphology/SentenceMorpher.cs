using System;
using System.Collections.Generic;
using System.Linq;

namespace Morphology
{
    public class SentenceMorpher { 
    
        private static readonly Dictionary<string, Dictionary<uint, string>> _dictionary = new(StringComparer.CurrentCultureIgnoreCase);
        private static readonly Dictionary<string, int> _tagsMap = new(StringComparer.CurrentCultureIgnoreCase);
        private static int _tagIndex = 0;

        private static class PrimeNumber
        {
            private static readonly int basePrime = 2;

            public static int GetBasePrime()
            {
                return basePrime;
            }

            public static int GetNext(int current)
            {
                if (current == basePrime)
                {
                    return 3;
                }
                if (current == 3)
                {
                    return 5;
                }
                if (current == 5)
                {
                    return 7;
                }


                var nextPrime = current + 2;

                while (CheckPrime(nextPrime) == false)
                {
                    nextPrime += 2;
                }

                return nextPrime;
            }

            public static bool CheckPrime(int number)
            {
                bool IsPrime = true;

                if (number is 2 or 3 or 5 or 7)
                {
                    return true;
                }

                for (int i = 2; i < number / 2; i++)
                {
                    if (number % i == 0)
                    {
                        IsPrime = false;
                        break;
                    }
                }

                return IsPrime;
            }
        }

        private static uint ParseTag(string dictionaryLine, Dictionary<string, int> _tagsMap)
        {

            var splitedLine = dictionaryLine.Split('\t', System.StringSplitOptions.RemoveEmptyEntries);
            var tagsMultiple = (uint)1;

            if (!_tagsMap.ContainsKey(splitedLine[0]))
            {
                var splitedTags = splitedLine[1].Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag in splitedTags)
                {
                    var lowerTag = tag.ToLower();
                    if (!_tagsMap.ContainsKey(lowerTag))
                    {   
                        if (_tagsMap.Count == 0)
                        {   
                            _tagsMap.Add(lowerTag, PrimeNumber.GetBasePrime());
                            tagsMultiple *= (uint) PrimeNumber.GetBasePrime();

                            continue;
                        }
                        else
                        {
                            var currentTag = PrimeNumber.GetNext(_tagsMap.Values.Max());
                            _tagsMap.Add(lowerTag, currentTag);
                            tagsMultiple *= (uint)currentTag;

                            continue;
                        }
                    }
                    else
                    {
                        tagsMultiple *= (uint)_tagsMap[lowerTag];
                        continue;
                    }
                }
            }
            return tagsMultiple;
        }

        private static void WriteDictonaryLines(IEnumerable<string> dictionaryLines, Dictionary<string, Dictionary<uint, string>> dictionary)
        {   
            var currentWord = string.Empty;
            var isNormalForm = false;

            foreach (var dictionaryLine in dictionaryLines)
            {
                if (string.IsNullOrWhiteSpace(dictionaryLine))
                {   
                    continue;
                }

                if (int.TryParse(dictionaryLine, out _))
                {
                    isNormalForm = true;
                    continue;
                }

                if (isNormalForm)
                {
                    currentWord = dictionaryLine.Split('\t')[0];

                    if (dictionary.ContainsKey(currentWord))
                    {
                        //dictionary[currentWord].Add(ParseTag(dictionaryLine, _tagsMap), dictionaryLine.Split('\t')[0]);
                        var parsedTag1 = ParseTag(dictionaryLine, _tagsMap);
                        if (dictionary[currentWord].ContainsKey(parsedTag1))
                        {
                            continue;
                        }
                        else
                        {
                            dictionary[currentWord].Add(parsedTag1, dictionaryLine.Split('\t')[0]);
                            continue;
                        }
                    }
                    else
                    {
                        dictionary.Add(currentWord, new Dictionary<uint, string>());
                        isNormalForm = false;
                        continue;
                    }
                }
                var splittedWord = dictionaryLine.Split('\t')[0];
                var parsedTag = ParseTag(dictionaryLine, _tagsMap);
                if (dictionary[currentWord].ContainsKey(parsedTag))
                {
                    continue;
                }
                else
                {
                    dictionary[currentWord].Add(parsedTag, splittedWord);
                }
                
            }
        }


        /// <summary>
        ///     Создает <see cref="SentenceMorpher"/> из переданного набора строк словаря.
        /// </summary>
        /// <remarks>
        ///     В этом методе должен быть код инициализации: 
        ///     чтение и преобразование входных данных для дальнейшего их использования
        /// </remarks>
        /// <param name="dictionaryLines">
        ///     Строки исходного словаря OpenCorpora в формате plain-text.
        ///     <code> СЛОВО(знак_табуляции)ЧАСТЬ РЕЧИ( )атрибут1[, ]атрибут2[, ]атрибутN </code>
        /// </param>
        public static SentenceMorpher Create(IEnumerable<string> dictionaryLines)
        {
            WriteDictonaryLines(dictionaryLines, _dictionary);
            //var currentLine = string.Empty;
            //var currentNumber = 0;

            //foreach (string dictionaryLine in dictionaryLines)
            //{
            //    if (int.TryParse(dictionaryLine, out int number))
            //    {   
            //        currentNumber = number;

            //        continue;
            //    }

            //    if (dictionary.ContainsKey(currentLine) && !string.IsNullOrEmpty(dictionaryLine))
            //    {
            //        dictionary[currentLine].Add(dictionaryLine);
            //        continue;
            //    }

            //    if (!string.IsNullOrEmpty(dictionaryLine))
            //    {
            //        if (!dictionary.ContainsKey(dictionaryLine))
            //        {
            //            dictionary.Add(dictionaryLine, new List<string>());
            //            currentLine = dictionaryLine;
            //        }
            //        else
            //        {
            //            continue;
            //        }
                        
            //    }
            //    else
            //    {
            //        currentLine = string.Empty;

            //        continue;
            //    }
            //}
            //TODO: код инициализации
            return new SentenceMorpher();
        }

        /// <summary>
        ///     Выполняет склонение предложения согласно указанному формату
        /// </summary>
        /// <param name="sentence">
        ///     Входное предложение <para/>
        ///     Формат: набор слов, разделенных пробелами.
        ///     После слова может следовать спецификатор требуемой части речи (формат описан далее),
        ///     если он отсутствует - слово требуется перенести в выходное предложение без изменений.
        ///     Спецификатор имеет следующий формат: <code>{ЧАСТЬ РЕЧИ,аттрибут1,аттрибут2,..,аттрибутN}</code>
        ///     Если для спецификации найдётся несколько совпадений - используется первое из них
        /// </param>
        public virtual string Morph(string sentence)
        {
            //sentence = sentence.ToUpper();
            ////TODO: код реализации
            //var splitedSentence = sentence.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

            //for(int i = 0; i<splitedSentence.Length; i++)
            //{
            //    var word = splitedSentence[i];

            //    if (word.Contains('{') && word.EndsWith('}'))
            //    {
            //        var splitedWord = word.Split('{');
            //        var tags = splitedWord[1];
            //        tags = tags.Substring(0, tags.Length - 1);
            //        word = string.Join('\t', new string[] { splitedWord[0], tags });
            //        foreach (var dictionaryWord in dictionary.Keys)
            //        {
            //            if (dictionaryWord.ToUpper() == word)
            //            {
            //                foreach (var taggedWord in dictionary[dictionaryWord])
            //                {
            //                    if (taggedWord.Contains(tags))
            //                    {
            //                        splitedSentence[i]  = taggedWord.Split('\t')[0];
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //var resultSentence = string.Join(' ', splitedSentence);
            //sentence = resultSentence;

            return sentence;
        }
    }
}
