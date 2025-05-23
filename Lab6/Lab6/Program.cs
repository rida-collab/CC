using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FirstSetConsoleApp
{
    class Program
    {
        static Dictionary<string, string> productionRules = new Dictionary<string, string>();
        static Dictionary<string, HashSet<string>> firstSets = new Dictionary<string, HashSet<string>>();

        static void Main(string[] args)
        {
            Console.WriteLine("Enter grammar rules in the form A > B C");
            Console.WriteLine("Enter 'end' to finish input\n");

            while (true)
            {
                Console.Write("Rule: ");
                string input = Console.ReadLine();
                if (input == null || input.Trim().ToLower() == "end")
                    break;

                var temp = input.Split('>');
                if (temp.Length != 2 || string.IsNullOrWhiteSpace(temp[0]) || string.IsNullOrWhiteSpace(temp[1]))
                {
                    Console.WriteLine("Invalid format. Use A > B C");
                    continue;
                }

                string lhs = temp[0].Trim();
                string rhs = temp[1].Trim();

                if (!Regex.IsMatch(lhs, @"^[A-Z]$"))
                {
                    Console.WriteLine("Non-terminals must be single uppercase letters.");
                    continue;
                }

                if (!productionRules.ContainsKey(lhs))
                    productionRules[lhs] = rhs;
                else
                    productionRules[lhs] += "|" + rhs;
            }

            Console.WriteLine("\nCalculating FIRST sets...\n");

            foreach (var rule in productionRules)
            {
                string lhs = rule.Key;
                string[] alternatives = rule.Value.Split('|');

                foreach (string alt in alternatives)
                {
                    string[] symbols = alt.Trim().Split(' ');
                    var first = CalculateFirst(symbols, 0);

                    if (!firstSets.ContainsKey(lhs))
                        firstSets[lhs] = new HashSet<string>();

                    foreach (var f in first)
                        firstSets[lhs].Add(f);
                }
            }

            foreach (var entry in firstSets)
            {
                Console.Write("First(" + entry.Key + ") = { ");
                Console.WriteLine(string.Join(", ", entry.Value) + " }");
            }

            Console.WriteLine("\nDone.");
        }

        static HashSet<string> CalculateFirst(string[] symbols, int index)
        {
            HashSet<string> result = new HashSet<string>();

            if (index >= symbols.Length)
            {
                result.Add("~"); // epsilon
                return result;
            }

            string current = symbols[index];

            // Terminal or epsilon
            if (!productionRules.ContainsKey(current))
            {
                result.Add(current);
                return result;
            }

            // Non-terminal
            string[] alternatives = productionRules[current].Split('|');
            foreach (string alt in alternatives)
            {
                string[] rhsSymbols = alt.Trim().Split(' ');
                var tempFirst = CalculateFirst(rhsSymbols, 0);

                foreach (var f in tempFirst)
                {
                    if (f == "~")
                    {
                        var nextFirst = CalculateFirst(symbols, index + 1);
                        foreach (var nf in nextFirst)
                            result.Add(nf);
                    }
                    else
                    {
                        result.Add(f);
                    }
                }
            }

            return result;
        }
    }
}
