using System;
using System.Collections.Generic;

namespace FirstFollowSet
{
    class Program
    {
        static char[,] production = new char[10, 10]; // Stores productions
        static int limit; // Number of productions
        static Dictionary<char, HashSet<char>> first = new Dictionary<char, HashSet<char>>(); // FIRST sets
        static Dictionary<char, HashSet<char>> follow = new Dictionary<char, HashSet<char>>(); // FOLLOW sets

        static void Main(string[] args)
        {
            // Initialize production array with '-'
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    production[i, j] = '-';

            // Input number of productions (at least 6)
            Console.Write("Enter Total Number of Productions (minimum 6): ");
            limit = Convert.ToInt32(Console.ReadLine());
            if (limit < 6)
            {
                Console.WriteLine("Error: At least 6 productions are required.");
                return;
            }

            // Input productions
            for (int i = 0; i < limit; i++)
            {
                Console.Write($"Enter Production {i + 1} (e.g., S -> AB): ");
                string temp = Console.ReadLine().Replace(" ", ""); // Remove spaces
                for (int j = 0; j < temp.Length; j++)
                {
                    production[i, j] = temp[j];
                }
            }

            // Initialize FIRST and FOLLOW sets for all non-terminals
            for (int i = 0; i < limit; i++)
            {
                char nonTerminal = production[i, 0];
                if (!first.ContainsKey(nonTerminal))
                    first[nonTerminal] = new HashSet<char>();
                if (!follow.ContainsKey(nonTerminal))
                    follow[nonTerminal] = new HashSet<char>();
            }

            // Compute FIRST sets
            foreach (var nonTerminal in first.Keys)
            {
                FindFirst(nonTerminal);
            }

            // Compute FOLLOW sets
            ComputeFollowSets();

            // Print FIRST sets
            Console.WriteLine("\nFIRST Sets:");
            foreach (var nonTerminal in first.Keys)
            {
                Console.Write($"FIRST({nonTerminal}) = {{ ");
                Console.Write(string.Join(", ", first[nonTerminal]));
                Console.WriteLine(" }");
            }

            // Print FOLLOW sets
            Console.WriteLine("\nFOLLOW Sets:");
            foreach (var nonTerminal in follow.Keys)
            {
                Console.Write($"FOLLOW({nonTerminal}) = {{ ");
                Console.Write(string.Join(", ", follow[nonTerminal]));
                Console.WriteLine(" }");
            }

            // Allow user to query FOLLOW set for a specific non-terminal
            char option;
            do
            {
                Console.Write("\nEnter non-terminal to find FOLLOW (or any other key to exit): ");
                char ch = Console.ReadKey().KeyChar;
                Console.WriteLine();
                if (follow.ContainsKey(ch))
                {
                    Console.Write($"FOLLOW({ch}) = {{ ");
                    Console.Write(string.Join(", ", follow[ch]));
                    Console.WriteLine(" }");
                }
                else
                {
                    Console.WriteLine("Invalid non-terminal.");
                }
                Console.Write("Continue? (Y/N): ");
                option = Console.ReadKey().KeyChar;
                Console.WriteLine();
            } while (option == 'y' || option == 'Y');

            Console.ReadKey();
        }

        // Compute FIRST set for a non-terminal
        static void FindFirst(char nonTerminal)
        {
            for (int i = 0; i < limit; i++)
            {
                if (production[i, 0] == nonTerminal)
                {
                    int j = 3; // Start after "->"
                    if (production[i, j] == '$') // Epsilon production
                    {
                        first[nonTerminal].Add('$');
                    }
                    else if (IsTerminal(production[i, j]))
                    {
                        first[nonTerminal].Add(production[i, j]);
                    }
                    else if (IsNonTerminal(production[i, j]))
                    {
                        FindFirst(production[i, j]);
                        foreach (var terminal in first[production[i, j]])
                        {
                            if (terminal != '$')
                                first[nonTerminal].Add(terminal);
                        }
                    }
                }
            }
        }

        // Compute FOLLOW sets for all non-terminals
        static void ComputeFollowSets()
        {
            // Add '$' to FOLLOW of the start symbol (first production's LHS)
            follow[production[0, 0]].Add('$');

            // Iterate multiple times to ensure all FOLLOW sets are complete
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < limit; i++)
                {
                    char lhs = production[i, 0]; // Left-hand side of production
                    int j = 3; // Start after "->"
                    while (production[i, j] != '-' && production[i, j] != '\0')
                    {
                        char current = production[i, j];
                        if (IsNonTerminal(current))
                        {
                            // Look at the next symbol
                            if (production[i, j + 1] == '-' || production[i, j + 1] == '\0')
                            {
                                // If current is at the end, add FOLLOW(lhs) to FOLLOW(current)
                                foreach (var terminal in follow[lhs])
                                {
                                    if (follow[current].Add(terminal))
                                        changed = true;
                                }
                            }
                            else if (IsTerminal(production[i, j + 1]))
                            {
                                // If next is a terminal, add it to FOLLOW(current)
                                if (follow[current].Add(production[i, j + 1]))
                                    changed = true;
                            }
                            else if (IsNonTerminal(production[i, j + 1]))
                            {
                                // If next is a non-terminal, add FIRST(next) (except '$') to FOLLOW(current)
                                foreach (var terminal in first[production[i, j + 1]])
                                {
                                    if (terminal != '$' && follow[current].Add(terminal))
                                        changed = true;
                                }
                                // If FIRST(next) contains '$', add FOLLOW(lhs) to FOLLOW(current)
                                if (first[production[i, j + 1]].Contains('$'))
                                {
                                    foreach (var terminal in follow[lhs])
                                    {
                                        if (follow[current].Add(terminal))
                                            changed = true;
                                    }
                                }
                            }
                        }
                        j++;
                    }
                }
            } while (changed); // Repeat until no new terminals are added
        }

        // Check if a character is a terminal (lowercase or special character)
        static bool IsTerminal(char c)
        {
            return (c >= 'a' && c <= 'z') || c == '$';
        }

        // Check if a character is a non-terminal (uppercase)
        static bool IsNonTerminal(char c)
        {
            return c >= 'A' && c <= 'Z';
        }
    }
}