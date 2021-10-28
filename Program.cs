using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using DictionaryLib;

namespace Rummy500
{
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

            while (topScore < 1000)
            {
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

                //SOLVER
                //3 letter horis and 4 letter verts
                foreach (var row in rows)
                {
                    List<string> foundAnswers = new List<string>();
                    for (int i = 0; i < 11; i++)
                    {
                        var threeCharSeg = new string(row[i..(i + 3)]);

                        if (useSimpleDicto || validSevenLetterWords.Any(it => it.StartsWith(threeCharSeg) || it.EndsWith(threeCharSeg)))
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
                                    
                                    if (WordCheck(possibleAnswer, useSimpleDicto, dictoLib, validSevenLetterWords))
                                    {
                                        if (!foundWords.Any(it => it.Item1 == possibleAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((possibleAnswer, score, $"{headerRow[i]}-{headerRow[i+1]}-{headerRow[i+2]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}"));
                                        }
                                    }
                                    else if (WordCheck(otherAnswer, useSimpleDicto, dictoLib, validSevenLetterWords))
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
                    List<string> foundAnswers = new List<string>();
                    for (int i = 0; i < 10; i++)
                    {
                        var fourCharSeg = new string(row[i..(i + 4)]);

                        if (useSimpleDicto || validSevenLetterWords.Any(it => it.StartsWith(fourCharSeg) || it.EndsWith(fourCharSeg)))
                        {
                            Dictionary<int, List<string>> dicto = new Dictionary<int, List<string>>();
                            for (int c = 0; c < 13; c++)
                            {
                                if (c != i && c != i + 1 && c != 2 && c != i + 3)
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
                                    {
                                        if(!foundWords.Any(it => it.Item1 == possibleAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((possibleAnswer, score, $"{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[i+3]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}"));
                                        }
                                    }
                                    else if (WordCheck(otherAnswer, useSimpleDicto, dictoLib, validSevenLetterWords))
                                    {
                                        if (!foundWords.Any(it => it.Item1 == otherAnswer && it.Item2 >= score))
                                        {
                                            foundWords.Add((otherAnswer, score, $"{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[kv.Key]}-{headerRow[i]}-{headerRow[i + 1]}-{headerRow[i + 2]}-{headerRow[i + 3]}"));
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
            Dictionary<int, char[]> letterFreq = new Dictionary<int, char[]>() {
                {120, new char[] {'E' } },
                {90, new char[] {'T' } },
                {80, new char[] {'A', 'I', 'N', 'O', 'S' } },
                {64, new char[] {'H' } },
                {62, new char[] {'R' } },
                {44, new char[] {'D' } },
                {40, new char[] {'L' } },
                {34, new char[] {'U' } },
                {30, new char[] {'C', 'M' } },
                {25, new char[] {'F' } },
                {20, new char[] {'W', 'Y' } },
                {17, new char[] {'T' } },
                {16, new char[] {'B' } },
                {12, new char[] {'V' } },
                {8, new char[] {'K' } },
                {5, new char[] {'Q' } },
                {4, new char[] {'J', 'X' } },
                {2, new char[] {'Z' } }
            };

            var sevenChars = "";
            var allSevenLetters = string.Join("", validSevenLetterWords);
            var letterFreqs = allSevenLetters.ToArray().OrderBy(it => it).GroupBy(it => it).Select(it => new { Letter = it.Key, Count = it.Count() });
            foreach(var letterGroup in letterFreqs)
            {
                sevenChars += String.Concat(Enumerable.Repeat(letterGroup.Letter, letterGroup.Count));
            }


            string chars = "";

            foreach(var kv in letterFreq)
            {
                for(int i = 0; i < kv.Key; i++)
                {
                    foreach(var ch in kv.Value)
                    {
                        chars += ch;
                    }
                }
            }

            var rando = new Random();
            char[] output = new char[13];

            for (int i = 0; i < 13; i++)
            {
                output[i] = sevenChars[rando.Next(sevenChars.Length)];
            }
            return output;
        }

        static bool WordCheck(string input, bool useSimpleDicto, DictionaryLib.DictionaryLib simpleDicto, List<string> validSevenLetterWords)
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
                    return validSevenLetterWords.Contains(input);
                }
            }
            else
            {
                return validSevenLetterWords.Contains(input);
            }
        }
    }
}
