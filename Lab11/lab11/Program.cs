using System;
using System.Collections.Generic;

class Parser
{
    // Token list: tuple of (token_type, value)
    static List<(string, string)> tokens;
    static int pos = 0;

    // Symbol Table: stores variable name and value (int)
    static Dictionary<string, int> symbolTable = new Dictionary<string, int>();

    static (string, string) Peek()
    {
        if (pos < tokens.Count)
            return tokens[pos];
        return ("EOF", "");
    }

    static (string, string) Match(string expectedType)
    {
        var token = Peek();
        if (token.Item1 == expectedType)
        {
            pos++;
            return token;
        }
        else
        {
            throw new Exception($"Expected '{expectedType}' but found '{token.Item1}'");
        }
    }

    // Entry point
    public static void Parse(List<(string, string)> tokenList)
    {
        tokens = tokenList;
        pos = 0;
        Program();
        Console.WriteLine("Parsing completed successfully!");
    }

    // Grammar rules

    // Program -> Decls Stmts EOF
    static void Program()
    {
        Decls();
        Stmts();
        Match("EOF");
    }

    // Decls -> ( 'int' ID ';' )*
    static void Decls()
    {
        while (Peek().Item1 == "INT")
        {
            Match("INT");
            var idToken = Match("ID");
            var id = idToken.Item2;
            if (symbolTable.ContainsKey(id))
                throw new Exception($"Variable '{id}' already declared");
            symbolTable[id] = 0;  // default initialization
            Match("SEMI");
        }
    }

    // Stmts -> ( Stmt )*
    static void Stmts()
    {
        while (Peek().Item1 == "ID" || Peek().Item1 == "IF")
        {
            Stmt();
        }
    }

    // Stmt -> ID ( '=' Expr )? ';'
    static void Stmt()
    {
        if (Peek().Item1 == "ID")
        {
            var idToken = Match("ID");
            var id = idToken.Item2;

            if (!symbolTable.ContainsKey(id))
                throw new Exception($"Undeclared variable: {id}");

            if (Peek().Item1 == "ASSIGN")
            {
                Match("ASSIGN");
                int val = Expr();
                symbolTable[id] = val;
            }

            Match("SEMI");
        }
        else if (Peek().Item1 == "IF")
        {
            IfStmt();
        }
        else
        {
            throw new Exception($"Unexpected token {Peek().Item1} at statement start");
        }
    }

    // IfStmt -> 'if' '(' Expr ')' '{' Stmts '}'
    static void IfStmt()
    {
        Match("IF");
        Match("LPAREN");
        int cond = Expr();
        Match("RPAREN");
        Match("LBRACE");
        if (cond != 0)
        {
            Stmts();
        }
        else
        {
            // Skip statements inside if (for simplicity)
            // You can extend this to support else blocks etc.
            int braceCount = 1;
            while (braceCount > 0)
            {
                var tok = Peek();
                if (tok.Item1 == "LBRACE") braceCount++;
                else if (tok.Item1 == "RBRACE") braceCount--;
                pos++;
            }
        }
        Match("RBRACE");
    }

    // Expr -> Term ( ('+' | '-') Term )*
    static int Expr()
    {
        int val = Term();
        while (Peek().Item1 == "PLUS" || Peek().Item1 == "MINUS")
        {
            var op = Match(Peek().Item1);
            int right = Term();
            if (op.Item1 == "PLUS")
                val += right;
            else
                val -= right;
        }
        return val;
    }

    // Term -> Factor ( ('*' | '/') Factor )*
    static int Term()
    {
        int val = Factor();
        while (Peek().Item1 == "TIMES" || Peek().Item1 == "DIV")
        {
            var op = Match(Peek().Item1);
            int right = Factor();
            if (op.Item1 == "TIMES")
                val *= right;
            else
                val /= right;
        }
        return val;
    }

    // Factor -> INT_CONST | ID | '(' Expr ')'
    static int Factor()
    {
        var token = Peek();
        if (token.Item1 == "INT_CONST")
        {
            Match("INT_CONST");
            return int.Parse(token.Item2);
        }
        else if (token.Item1 == "ID")
        {
            Match("ID");
            var id = token.Item2;
            if (!symbolTable.ContainsKey(id))
                throw new Exception($"Undeclared variable: {id}");
            return symbolTable[id];
        }
        else if (token.Item1 == "LPAREN")
        {
            Match("LPAREN");
            int val = Expr();
            Match("RPAREN");
            return val;
        }
        else
        {
            throw new Exception($"Unexpected token in factor: {token.Item1}");
        }
    }

    // Test driver
    static void Main()
    {
        try
        {
            // Sample token list representing:
            // int x;
            // x = 2 + 5 + (4 * 8) + 12 / 3;
            // if (x + 1) {
            //   x = 6;
            // }
            var sampleTokens = new List<(string, string)>()
            {
                ("INT", "int"),
                ("ID", "x"),
                ("SEMI", ";"),

                ("ID", "x"),
                ("ASSIGN", "="),
                ("INT_CONST", "2"),
                ("PLUS", "+"),
                ("INT_CONST", "5"),
                ("PLUS", "+"),
                ("LPAREN", "("),
                ("INT_CONST", "4"),
                ("TIMES", "*"),
                ("INT_CONST", "8"),
                ("RPAREN", ")"),
                ("PLUS", "+"),
                ("INT_CONST", "12"),
                ("DIV", "/"),
                ("INT_CONST", "3"),
                ("SEMI", ";"),

                ("IF", "if"),
                ("LPAREN", "("),
                ("ID", "x"),
                ("PLUS", "+"),
                ("INT_CONST", "1"),
                ("RPAREN", ")"),
                ("LBRACE", "{"),

                ("ID", "x"),
                ("ASSIGN", "="),
                ("INT_CONST", "6"),
                ("SEMI", ";"),

                ("RBRACE", "}"),

                ("EOF", "")
            };

            Parse(sampleTokens);

            // Output symbol table values
            Console.WriteLine("Variable values:");
            foreach (var kvp in symbolTable)
            {
                Console.WriteLine($"{kvp.Key} = {kvp.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Parse error: " + ex.Message);
        }
    }
}
