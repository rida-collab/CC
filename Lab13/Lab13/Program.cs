using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Program
{
    static List<Symbol> SymbolTable = new();
    static List<Token> Tokens = new();
    static int current = 0;

    class Symbol
    {
        public int Index;
        public string Name;
        public string Type;
        public string Value;
        public int Line;

        public Symbol(int index, string name, string type, string value, int line)
        {
            Index = index;
            Name = name;
            Type = type;
            Value = value;
            Line = line;
        }
    }

    class Token
    {
        public string Type;
        public string Lexeme;
        public int Line;

        public Token(string type, string lexeme, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Enter your code (press Enter twice to finish):");
        string input = ReadMultilineInput();

        AnalyzeInput(input);
        Console.WriteLine("\n---------------- PARSING ----------------");
        Parse();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static string ReadMultilineInput()
    {
        string input = "";
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            input += line + "\n";
        }
        return input;
    }

    static void AnalyzeInput(string userInput)
    {
        List<string> keywords = new() { "int", "float", "print" };
        Regex variable_Reg = new(@"^[A-Za-z_][A-Za-z0-9_]*$");
        Regex constant_Reg = new(@"^[0-9]+(\.[0-9]+)?$");
        Regex operator_Reg = new(@"^[+\-*/=]$");
        Regex punctuation_Reg = new(@"^[.,;:{}()\[\]]$");

        List<string> lines = new(userInput.Split('\n'));
        int line_num = 0;
        int symbolIndex = 1;

        foreach (string rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine)) continue;
            line_num++;
            List<string> tokens = Tokenize(rawLine);

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                if (keywords.Contains(token))
                    Tokens.Add(new Token("keyword", token, line_num));
                else if (constant_Reg.IsMatch(token))
                    Tokens.Add(new Token("constant", token, line_num));
                else if (operator_Reg.IsMatch(token))
                    Tokens.Add(new Token("operator", token, line_num));
                else if (punctuation_Reg.IsMatch(token))
                    Tokens.Add(new Token("punctuation", token, line_num));
                else if (variable_Reg.IsMatch(token))
                {
                    if (!SymbolExists(token))
                    {
                        string type = (i > 0 && keywords.Contains(tokens[i - 1])) ? tokens[i - 1] : "unknown";
                        string value = (i + 2 < tokens.Count && tokens[i + 1] == "=") ? tokens[i + 2] : "";
                        SymbolTable.Add(new Symbol(symbolIndex++, token, type, value, line_num));
                    }
                    Tokens.Add(new Token("identifier", token, line_num));
                }
                else
                    Tokens.Add(new Token("unknown", token, line_num));
            }
        }

        Console.WriteLine("\nTOKENS:");
        foreach (var t in Tokens)
            Console.WriteLine($"Line {t.Line}: {t.Type} -> {t.Lexeme}");

        Console.WriteLine("\nSYMBOL TABLE:");
        Console.WriteLine("Index | Name | Type | Value | Line");
        foreach (var s in SymbolTable)
            Console.WriteLine($"{s.Index,5} | {s.Name,-5} | {s.Type,-5} | {s.Value,-5} | {s.Line}");
    }

    static List<string> Tokenize(string line)
    {
        List<string> tokens = new();
        string current = "";
        foreach (char c in line)
        {
            if (char.IsWhiteSpace(c))
            {
                if (current != "") { tokens.Add(current); current = ""; }
            }
            else if ("+-*/=.,;:{}()[]".Contains(c))
            {
                if (current != "") { tokens.Add(current); current = ""; }
                tokens.Add(c.ToString());
            }
            else
            {
                current += c;
            }
        }
        if (current != "") tokens.Add(current);
        return tokens;
    }

    static void Parse()
    {
        while (current < Tokens.Count)
        {
            if (!Statement())
            {
                var t = Peek();
                Console.WriteLine($"[Syntax Error] Line {t?.Line}: Unexpected token '{t?.Lexeme}'");
                current++;
            }
        }
    }

    static bool Statement()
    {
        return Declaration() || Assignment() || Print();
    }

    static bool Declaration()
    {
        int start = current;
        if (Match("keyword", "int") || Match("keyword", "float"))
        {
            string type = Tokens[start].Lexeme;
            if (Match("identifier"))
            {
                string name = Tokens[current - 1].Lexeme;
                string value = "";
                if (Match("operator", "="))
                {
                    if (!Expression()) return false;
                    value = Tokens[current - 1].Lexeme;
                }
                if (Match("punctuation", ";"))
                {
                    Console.WriteLine($"Matched Declaration: {type} {name} {(value != "" ? "= " + value : "")};");
                    return true;
                }
                else Console.WriteLine($"[Syntax Error] Line {Tokens[current - 1].Line}: Missing ';'");
            }
            else Console.WriteLine($"[Syntax Error] Line {Tokens[current].Line}: Expected identifier after type.");
        }
        current = start;
        return false;
    }

    static bool Assignment()
    {
        int start = current;
        if (Match("identifier"))
        {
            string name = Tokens[current - 1].Lexeme;
            if (Match("operator", "="))
            {
                if (!Expression()) return false;
                string value = Tokens[current - 1].Lexeme;
                if (Match("punctuation", ";"))
                {
                    Console.WriteLine($"Matched Assignment: {name} = {value};");
                    return true;
                }
                else Console.WriteLine($"[Syntax Error] Line {Tokens[current - 1].Line}: Missing ';'");
            }
        }
        current = start;
        return false;
    }

    static bool Print()
    {
        int start = current;
        if (Match("keyword", "print"))
        {
            if (Match("identifier"))
            {
                string id = Tokens[current - 1].Lexeme;
                if (Match("punctuation", ";"))
                {
                    Console.WriteLine($"Matched Print: print {id};");
                    return true;
                }
                else Console.WriteLine($"[Syntax Error] Line {Tokens[current - 1].Line}: Missing ';'");
            }
        }
        current = start;
        return false;
    }

    static bool Expression()
    {
        return Match("identifier") || Match("constant");
    }

    static bool Match(string type, string lexeme = null)
    {
        if (current >= Tokens.Count) return false;
        if (Tokens[current].Type == type && (lexeme == null || Tokens[current].Lexeme == lexeme))
        {
            current++;
            return true;
        }
        return false;
    }

    static Token Peek()
    {
        return current < Tokens.Count ? Tokens[current] : null;
    }

    static bool SymbolExists(string name)
    {
        return SymbolTable.Exists(s => s.Name == name);
    }
}