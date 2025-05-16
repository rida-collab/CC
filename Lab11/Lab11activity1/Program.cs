using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SemanticAnalyzerLab
{
    class Program
    {
        static List<List<string>> Symboltable = new List<List<string>>();
        static List<string> finalArray = new List<string>();
        static List<int> Constants = new List<int>();
        static Regex variable_Reg = new Regex(@"^[A-Za-z_][A-Za-z0-9]*$");

        static void Main(string[] args)
        {
            InitializeSymbolTable();
            InitializeFinalArray();
            PrintLexerOutput();

            for (int i = 0; i < finalArray.Count; i++)
            {
                try
                {
                    Semantic_Analysis(i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error at index {i}] {ex.Message}");
                }
            }

            Console.WriteLine("\nSemantic Analysis Completed.");
            Console.ReadLine();
        }

        static void InitializeSymbolTable()
        {
            Symboltable.Add(new List<string> { "x", "id", "int", "0" });
            Symboltable.Add(new List<string> { "y", "id", "int", "0" });
            Symboltable.Add(new List<string> { "i", "id", "int", "0" });
            Symboltable.Add(new List<string> { "l", "id", "char", "0" }); // Not used in arithmetic
        }

        static void InitializeFinalArray()
        {
            finalArray.AddRange(new string[] {
                "int", "main", "(", ")", "{",
                "int", "x", ";",
                "x", ";",
                "x", "=", "2", "+", "5", "+", "(", "4", "*", "8", ")", "+", "l", "/", "9.0", ";",
                "if", "(", "x", "+", "y", ")", "{",
                "if", "(", "x", "!=", "4", ")", "{",
                "x", "=", "6", ";",
                "y", "=", "10", ";",
                "i", "=", "11", ";",
                "}", "}", "}"
            });
        }

        static void PrintLexerOutput()
        {
            Console.WriteLine("Tokenizing source code...");
            int row = 1, col = 1;
            foreach (string token in finalArray)
            {
                if (token == "int") Console.WriteLine($"INT ({row},{col})");
                else if (token == "main") Console.WriteLine($"MAIN ({row},{col})");
                else if (token == "(") Console.WriteLine($"LPAREN ({row},{col})");
                else if (token == ")") Console.WriteLine($"RPAREN ({row},{col})");
                else if (token == "{") Console.WriteLine($"LBRACE ({row},{col})");
                else if (token == "}") Console.WriteLine($"RBRACE ({row},{col})");
                else if (token == ";") Console.WriteLine($"SEMI ({row},{col})");
                else if (token == "=") Console.WriteLine($"ASSIGN ({row},{col})");
                else if (token == "+") Console.WriteLine($"PLUS ({row},{col})");
                else if (token == "-") Console.WriteLine($"MINUS ({row},{col})");
                else if (token == "*") Console.WriteLine($"TIMES ({row},{col})");
                else if (token == "/") Console.WriteLine($"DIV ({row},{col})");
                else if (token == "!=") Console.WriteLine($"NEQ ({row},{col})");
                else if (Regex.IsMatch(token, @"^[0-9]+$")) Console.WriteLine($"INT_CONST ({row},{col}): {token}");
                else if (Regex.IsMatch(token, @"^[0-9]+\.[0-9]+$")) Console.WriteLine($"FLOAT_CONST ({row},{col}): {token}");
                else if (Regex.IsMatch(token, @"^[a-zA-Z]$")) Console.WriteLine($"CHAR_CONST ({row},{col}): {token}");
                else if (variable_Reg.Match(token).Success) Console.WriteLine($"ID ({row},{col}): {token}");
                else Console.WriteLine($"UNKNOWN ({row},{col}): {token}");

                col += token.Length + 1;
                if (token == ";") row++;
            }

            Console.WriteLine($"EOF ({row},{col})");
        }

        static void Semantic_Analysis(int k)
        {
            if (k >= finalArray.Count - 1) return;

            string token = finalArray[k];

            if (token == "+" || token == "-")
            {
                if (k - 1 >= 0 && k + 1 < finalArray.Count)
                {
                    string left = finalArray[k - 1];
                    string right = finalArray[k + 1];

                    if (variable_Reg.IsMatch(left) && variable_Reg.IsMatch(right))
                    {
                        int leftIndex = FindSymbol(left);
                        int rightIndex = FindSymbol(right);

                        if (leftIndex == -1 || rightIndex == -1) return;

                        string leftType = Symboltable[leftIndex][2];
                        string rightType = Symboltable[rightIndex][2];

                        if (leftType == "int" && rightType == "int")
                        {
                            int val1 = int.Parse(Symboltable[leftIndex][3]);
                            int val2 = int.Parse(Symboltable[rightIndex][3]);
                            int result = token == "+" ? val1 + val2 : val1 - val2;
                            Constants.Add(result);
                            Console.WriteLine($"[Semantic] Constant Folded: {left} {token} {right} = {result}");
                        }
                    }
                }
            }

            if (token == "!=")
            {
                if (k - 1 >= 0 && k + 1 < finalArray.Count)
                {
                    string left = finalArray[k - 1];
                    string right = finalArray[k + 1];

                    int li = FindSymbol(left);
                    if (li == -1) return;

                    if (int.TryParse(right, out int rightVal))
                    {
                        int storedVal = int.Parse(Symboltable[li][3]);
                        if (storedVal != rightVal)
                        {
                            Console.WriteLine($"[Semantic] Condition '{left} != {right}' is TRUE. Executing IF block.");
                        }
                        else
                        {
                            Console.WriteLine($"[Semantic] Condition '{left} != {right}' is FALSE. Removing IF block.");
                            RemoveIfBlock(k);
                        }
                    }
                }
            }
        }

        static int FindSymbol(string name)
        {
            for (int i = 0; i < Symboltable.Count; i++)
            {
                if (Symboltable[i][0] == name)
                    return i;
            }
            return -1;
        }

        static void RemoveIfBlock(int index)
        {
            int start = -1;
            int braceCount = 0;
            for (int i = index; i >= 0; i--)
            {
                if (finalArray[i] == "if")
                {
                    start = i;
                    break;
                }
            }

            for (int i = start; i < finalArray.Count; i++)
            {
                if (finalArray[i] == "{") braceCount++;
                if (finalArray[i] == "}") braceCount--;

                finalArray.RemoveAt(start);
                i--;

                if (braceCount == 0 && i > start) break;
            }

            Console.WriteLine($"[Semantic] Removed IF block starting at index {start}");
        }
    }
}
