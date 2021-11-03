using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using DictionaryLib;

namespace Rummy500
{
    //Borrowed from Fantius's answer on Stack q# 6416050
    public class Trie
    {
        public struct Letter
        {
            public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            public int Index;

            public static implicit operator Letter(char c)
            {
                return new Letter() { Index = Chars.IndexOf(c) };
            }

            public char ToChar()
            {
                return Chars[Index];
            }

            public override string ToString()
            {
                return Chars[Index].ToString();
            }
        }

        public class Node
        {
            public string Word;
            public bool IsTerminal
            {
                get => Word != null;
            }
            public Dictionary<char, Node> Edges = new Dictionary<char, Node>();
        }

        public Node Root = new Node();
        
        public Trie(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var node = Root;
                for (int j = 1; j <= word.Length; j++)
                {
                    var letter = word[j - 1];
                    Node next;
                    if (!node.Edges.TryGetValue(letter, out next))
                    {
                        next = new Node();
                        if( j == word.Length)
                        {
                            next.Word = word;
                        }
                        node.Edges.Add(letter, next);
                    }
                    node = next;
                }
            }
        }

        public bool IsWord(string word)
        {
            bool CheckThree(string threeSeg)
            {
                return Root.Edges.ContainsKey(word[0])
                   && Root.Edges[word[0]].Edges.ContainsKey(word[1])
                   && Root.Edges[word[0]].Edges[word[1]].Edges.ContainsKey(word[2]);
            }
            bool checkFour(string fourSeg)
            {
                return CheckThree(fourSeg)
                    && Root.Edges[word[0]].Edges[word[1]].Edges[word[2]].Edges.ContainsKey(word[3]);
            }
            bool checkSeven(string sevenSeg)
            {
                return checkFour(sevenSeg)
                    && Root.Edges[word[0]].Edges[word[1]].Edges[word[2]].Edges[word[3]].Edges.ContainsKey(word[4])
                    && Root.Edges[word[0]].Edges[word[1]].Edges[word[2]].Edges[word[3]].Edges[word[4]].Edges.ContainsKey(word[5])
                    && Root.Edges[word[0]].Edges[word[1]].Edges[word[2]].Edges[word[3]].Edges[word[4]].Edges[word[5]].Edges.ContainsKey(word[6]);
            }
            if (word.Length < 4)
            {
                return CheckThree(word);
            } else if (word.Length < 5)
            {
                return checkFour(word);
            } else
            {
                return checkSeven(word);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var testRows = new List<char[]>();
            testRows.Add("HCERYIIACLOSP".ToArray());
            testRows.Add("EMBARKECHIRES".ToArray());
            testRows.Add("SMELONDREDGNI".ToArray());
            testRows.Add("TWAYSQUALCLUN".ToArray());

            //Setup
            bool testMode = false;
            bool useSimpleDicto = false;
            bool useTries = true;
            List<string> validSevenLetterWords = new List<string>();
            string filePath = "..\\..\\..\\words.txt";
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var thisLine = streamReader.ReadLine();
                        Regex reg = new Regex("^[a-zA-Z]*$");
                        if (reg.IsMatch(thisLine) && thisLine.Length == 7)
                        {
                            validSevenLetterWords.Add(thisLine.ToUpper());
                        }
                    }
                }
            }
            var dictoLib = new DictionaryLib.DictionaryLib(DictionaryType.Small);

            int topScore = 0;
            char[] headerRow = new char[] { 'A', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K' };
            char[] suitCol = new char[] { '\u2660', '\u2665', '\u2666', '\u2663' };
            List<char[]> bestRows = new List<char[]>();
            List<(string, int,string)> bestAnswers = new List<(string, int,string)>();

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var sevenTries = new Trie(validSevenLetterWords.ToArray());
            Console.WriteLine($"Bananas Test = {sevenTries.IsWord("BANANAS")}");
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            int totalRuns = 0;
            while (topScore < 1000)
            {
                totalRuns++;
                List<(string, int, string)> foundWords = new List<(string, int, string)>();
                List<char[]> rows = new List<char[]>();
                if (testMode)
                {
                    rows = testRows;
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        rows.Add(BuildRow(validSevenLetterWords));
                    }
                }
                
                Console.Clear();
                Console.WriteLine("RULES");
                Console.WriteLine("Make 7 letter words using either a string of 3 or 4 consecutive letters left to right");
                Console.WriteLine("AND a string of 3 or 4 letters from the same vertical group. The sets can be placed in");
                Console.WriteLine("either order, and the vertical group can be rearranged in any order within the set, but");
                Console.WriteLine("the letters from the horizontal grouping must be used in the order they appear.");
                Console.WriteLine();

                if(topScore != 0)
                {
                    Console.WriteLine($"BEST SCORE:{topScore}");
                    Console.WriteLine("BEST GRID:");
                    Console.WriteLine("  A 2 3 4 5 6 7 8 9 T J Q K");
                    Console.WriteLine("---------------------------");
                    for (int i = 0; i < 4; i++)
                    {
                        var row = bestRows[i];
                        Console.WriteLine(suitCol[i] + " " + string.Join(" ", row));
                    }
                    Console.WriteLine("BEST ANSWERS:");
                    foreach (var score in bestAnswers)
                    {
                        Console.WriteLine($"{score.Item2} - {score.Item1} - {score.Item3}");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("GRID:");
                Console.WriteLine("  A 2 3 4 5 6 7 8 9 T J Q K");
                Console.WriteLine("---------------------------");

                for (int i = 0; i < 4; i++)
                {
                    var row = rows[i];
                    Console.WriteLine(suitCol[i] + " " + string.Join(" ", row));
                }

                if (useTries)
                {
                    for (int ri = 0; ri < rows.Count(); ri++)
                    {
                        var row = rows[ri];
                        for (int i = 0; i < 11; i++)
                        {
                            string threeCharStarter = new string(row[i..(i + 3)]);
                            if (sevenTries.IsWord(threeCharStarter))
                            {
                                //check permutations of vert 4 enders
                                for(int c = 0; c < 13; c++)
                                {
                                    if (c != i && c != i + 1 && c != i + 2) //we need a full empty column
                                    {
                                        var colCars = rows.Select(it => it[c]).ToArray();
                                        foreach(var perm in verticalPermutationsFour(colCars)) //maybe call a distinct and save lines?
                                        {
                                            var word = threeCharStarter + perm;
                                            if (sevenTries.IsWord(word))
                                            {
                                                var score = (IndexToScore(i) + IndexToScore(i + 1) + IndexToScore(i + 2) + (4 * IndexToScore(c)));
                                                var area = $"{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[c]}-{headerRow[c]}-{headerRow[c]}-{headerRow[c]}";
                                                foundWords.Add((word, score, area));
                                            }
                                        }
                                    }
                                }
                            }

                            if (i < 10)
                            {
                                string fourCharStarter =  new string(row[i..(i + 4)]);
                                if (sevenTries.IsWord(fourCharStarter))
                                {
                                    //check permutations of vert 3 enders
                                    Dictionary<int, List<string>> dicto = new Dictionary<int, List<string>>();
                                    for (int c = 0; c < 13; c++)
                                    {
                                        if ((c != i) && (c != i + 1) && (c != i + 2) && (c != i + 3))
                                        {
                                            var colChars = (rows.Select(it => it[c]).ToArray());
                                            foreach(var perm in verticalPermutationsThree(colChars))
                                            {
                                                var word = fourCharStarter + perm;
                                                if (sevenTries.IsWord(word))
                                                {
                                                    var score = (IndexToScore(i) + IndexToScore(i + 1) + IndexToScore(i + 2) + IndexToScore(i + 3) + (3 * IndexToScore(c)));
                                                    var area = $"{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[i + 3]}-{headerRow[c]}-{headerRow[c]}-{headerRow[c]}";
                                                    foundWords.Add((word, score, area));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var list = new List<char[]>();
                                            for (int r = 0; r < rows.Count(); r++)
                                            {
                                                if (r != ri)
                                                {
                                                    list.Add(rows[r]);
                                                }
                                            }
                                            var colChars = list.Select(it => it[c]).ToArray();
                                            foreach (var perm in verticalPermutationsThree(colChars))
                                            {
                                                var word = fourCharStarter + perm;
                                                if (sevenTries.IsWord(word))
                                                {
                                                    var score = (IndexToScore(i) + IndexToScore(i + 1) + IndexToScore(i + 2) + IndexToScore(i + 3) + (3 * IndexToScore(c)));
                                                    var area = $"{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[i + 3]}-{headerRow[c]}-{headerRow[c]}-{headerRow[c]}";
                                                    foundWords.Add((word, score, area));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //SOLVER
                //3 letter horis and 4 letter verts
                foreach (var row in rows)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        var threeCharSeg = new string(row[i..(i + 3)]);

                        if (validSevenLetterWords.Any(it => it.EndsWith(threeCharSeg)))
                        {
                            Dictionary<int, List<string>> dicto = new Dictionary<int, List<string>>();
                            for (int c = 0; c < 13; c++)
                            {
                                if (c != i && c != i + 1 && c != 2)
                                {
                                    var colChars = (rows.Select(it => it[c]).ToArray());
                                    dicto.Add(c, verticalPermutationsFour(colChars));
                                }
                            }

                            foreach (var kv in dicto)
                            {
                                foreach (var fourChar in kv.Value)
                                {
                                    var possibleAnswer = $"{threeCharSeg}{fourChar}";
                                    var otherAnswer = $"{fourChar}{threeCharSeg}";
                                    var score = (IndexToScore(i) + IndexToScore(i + 1) + IndexToScore(i + 2) + (4 * IndexToScore(kv.Key)));
                                    
                                    if (WordCheck(possibleAnswer, useSimpleDicto, dictoLib, validSevenLetterWords, useTries, sevenTries))
                                    {
                                        if (!foundWords.Any(it => it.Item1 == possibleAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((possibleAnswer, score, $"{headerRow[i]}-{headerRow[i+1]}-{headerRow[i+2]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}"));
                                        }
                                    }
                                    else if (WordCheck(otherAnswer, useSimpleDicto, dictoLib, validSevenLetterWords, useTries, sevenTries))
                                    {
                                        if (!foundWords.Any(it => it.Item1 == otherAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((otherAnswer, score, $"{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //4 letter horis and 3 letter verts

                for (int ri = 0; ri < rows.Count(); ri++)
                {
                    var row = rows[ri];
                    for (int i = 0; i < 10; i++)
                    {
                        var fourCharSeg = new string(row[i..(i + 4)]);

                        if (useSimpleDicto || validSevenLetterWords.Any(it => it.EndsWith(fourCharSeg)))
                        {
                            Dictionary<int, List<string>> dicto = new Dictionary<int, List<string>>();
                            for (int c = 0; c < 13; c++)
                            {
                                if ((c != i) && (c != i + 1) && (c != i + 2) && (c != i + 3))
                                {
                                    var colChars = (rows.Select(it => it[c]).ToArray());
                                    dicto.Add(c, verticalPermutationsThree(colChars));
                                }
                                else
                                {
                                    var list = new List<char[]>();
                                    for (int r = 0; r < rows.Count(); r++)
                                    {
                                        list.Add(rows[r]);
                                    }
                                    list.RemoveAt(ri);
                                    var colChars = list.Select(it => it[c]).ToArray();
                                    dicto.Add(c, verticalPermutationsThree(colChars));
                                }
                            }

                            foreach (var kv in dicto)
                            {
                                foreach (var threeChar in kv.Value)
                                {
                                    var possibleAnswer = $"{threeChar}{fourCharSeg}";
                                    var otherAnswer = $"{fourCharSeg}{threeChar}";
                                    var score = (IndexToScore(i) + IndexToScore(i + 1) + IndexToScore(i + 2) + IndexToScore(i + 3) + (3 * IndexToScore(kv.Key)));
                                    if (WordCheck(possibleAnswer, useSimpleDicto, dictoLib, validSevenLetterWords))
                                    if (WordCheck(possibleAnswer, useSimpleDicto, dictoLib, validSevenLetterWords, useTries, sevenTries))
                                    {
                                        if(!foundWords.Any(it => it.Item1 == possibleAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((possibleAnswer, score, $"{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[i + 3]}"));
                                        }
                                    }
                                    else if (WordCheck(otherAnswer, useSimpleDicto, dictoLib, validSevenLetterWords, useTries, sevenTries))
                                    {
                                        if (!foundWords.Any(it => it.Item1 == otherAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((otherAnswer, score, $"{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[i + 3]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                var thisScore = foundWords.Sum(it => it.Item2);
                if (thisScore > topScore)
                {
                    topScore = thisScore;
                    bestAnswers = foundWords;
                    bestRows = rows;
                }
                foundWords.ForEach(it => Console.WriteLine($"{it.Item2} - {it.Item1} - {it.Item3}"));
                Console.WriteLine($"Score - {thisScore}");
            }
            timer.Stop();
            Console.WriteLine($"Elapsed time: {timer.Elapsed}");
            Console.WriteLine($"{timer.Elapsed} / {totalRuns} Tries = {timer.Elapsed / totalRuns} avg run time");
        }

        static List<string> verticalPermutationsFour(char[] column)
        {
            List<string> output = new List<string>();

            output.Add($"{column[0]}{column[1]}{column[2]}{column[3]}");
            output.Add($"{column[0]}{column[1]}{column[3]}{column[2]}");
            output.Add($"{column[0]}{column[2]}{column[3]}{column[1]}");
            output.Add($"{column[0]}{column[2]}{column[1]}{column[3]}");
            output.Add($"{column[0]}{column[3]}{column[1]}{column[2]}");
            output.Add($"{column[0]}{column[3]}{column[2]}{column[1]}");

            output.Add($"{column[1]}{column[0]}{column[2]}{column[3]}");
            output.Add($"{column[1]}{column[0]}{column[3]}{column[2]}");
            output.Add($"{column[1]}{column[2]}{column[0]}{column[3]}");
            output.Add($"{column[1]}{column[2]}{column[3]}{column[0]}");
            output.Add($"{column[1]}{column[3]}{column[0]}{column[2]}");
            output.Add($"{column[1]}{column[3]}{column[2]}{column[0]}");

            output.Add($"{column[2]}{column[0]}{column[1]}{column[3]}");
            output.Add($"{column[2]}{column[0]}{column[3]}{column[1]}");
            output.Add($"{column[2]}{column[1]}{column[0]}{column[3]}");
            output.Add($"{column[2]}{column[1]}{column[3]}{column[0]}");
            output.Add($"{column[2]}{column[3]}{column[0]}{column[1]}");
            output.Add($"{column[2]}{column[3]}{column[1]}{column[0]}");

            output.Add($"{column[3]}{column[0]}{column[1]}{column[2]}");
            output.Add($"{column[3]}{column[0]}{column[2]}{column[1]}");
            output.Add($"{column[3]}{column[1]}{column[0]}{column[2]}");
            output.Add($"{column[3]}{column[1]}{column[2]}{column[0]}");
            output.Add($"{column[3]}{column[2]}{column[0]}{column[1]}");
            output.Add($"{column[3]}{column[2]}{column[1]}{column[0]}");

            return output.Distinct().ToList();
        }
        static List<string> verticalPermutationsThree(char[] column)
        {
            List<string> output = new List<string>();
            if (column.Length == 3)
            {
                output.Add($"{column[0]}{column[1]}{column[2]}");
                output.Add($"{column[0]}{column[2]}{column[1]}");
                output.Add($"{column[1]}{column[2]}{column[0]}");
                output.Add($"{column[1]}{column[0]}{column[2]}");
                output.Add($"{column[2]}{column[1]}{column[0]}");
                output.Add($"{column[2]}{column[0]}{column[1]}");
            }
            else if (column.Length == 4)
            {
                output.Add($"{column[0]}{column[1]}{column[2]}");
                output.Add($"{column[0]}{column[1]}{column[3]}");
                output.Add($"{column[0]}{column[2]}{column[1]}");
                output.Add($"{column[0]}{column[2]}{column[3]}");
                output.Add($"{column[0]}{column[3]}{column[1]}");
                output.Add($"{column[0]}{column[3]}{column[2]}");

                output.Add($"{column[1]}{column[0]}{column[2]}");
                output.Add($"{column[1]}{column[0]}{column[3]}");
                output.Add($"{column[1]}{column[2]}{column[0]}");
                output.Add($"{column[1]}{column[2]}{column[3]}");
                output.Add($"{column[1]}{column[3]}{column[0]}");
                output.Add($"{column[1]}{column[3]}{column[2]}");

                output.Add($"{column[2]}{column[0]}{column[1]}");
                output.Add($"{column[2]}{column[0]}{column[3]}");
                output.Add($"{column[2]}{column[1]}{column[0]}");
                output.Add($"{column[2]}{column[1]}{column[3]}");
                output.Add($"{column[2]}{column[3]}{column[0]}");
                output.Add($"{column[2]}{column[3]}{column[1]}");

                output.Add($"{column[3]}{column[0]}{column[1]}");
                output.Add($"{column[3]}{column[0]}{column[2]}");
                output.Add($"{column[3]}{column[1]}{column[0]}");
                output.Add($"{column[3]}{column[1]}{column[2]}");
                output.Add($"{column[3]}{column[2]}{column[0]}");
                output.Add($"{column[3]}{column[2]}{column[1]}");
            }

            return output.Distinct().ToList();
        }

        static int IndexToScore(int i)
        {
            return i + 1 >= 10 ? 10 : i + 1;
        }

        static char[] BuildRow(List<string> validSevenLetterWords)
        {
            var sevenChars = "";

            string.Join("", validSevenLetterWords)
                .ToArray()
                .OrderBy(it => it)
                .GroupBy(it => it)
                .Select(it => new
                {
                    Letter = it.Key,
                    Count = it.Count()
                })
                .ToList()
                .ForEach(it =>
                {
                    sevenChars += String.Concat(Enumerable.Repeat(it.Letter, it.Count));
                });

            var rando = new Random();
            char[] output = new char[13];

            for (int i = 0; i < 13; i++)
            {
                output[i] = sevenChars[rando.Next(sevenChars.Length)];
            }
            return output;
        }

        static bool WordCheck(string input, bool useSimpleDicto, DictionaryLib.DictionaryLib simpleDicto, List<string> validSevenLetterWords, bool useTries, Trie tries)
        {
            if (useSimpleDicto)
            {
                try
                {
                    return simpleDicto.IsWord(input);
                }
                catch (Exception ex)
                {
                    simpleDicto = new DictionaryLib.DictionaryLib(DictionaryType.Small);
                    if (useTries)
                    {
                        return tries.IsWord(input);
                    }
                    return validSevenLetterWords.Contains(input);
                }
            }
            else
            {
                if (useTries)
                {
                    return tries.IsWord(input);
                }
                return validSevenLetterWords.Contains(input);
            }
        }
    }
}
