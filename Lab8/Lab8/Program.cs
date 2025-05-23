using System;
using System.Collections.Generic;

namespace DFA_Abcc
{
    class Program
    {
        static void Main(string[] args)
        {
            string initialState = "S0";
            string finalState = "S4";

            // Define the DFA transitions
            var dict = new Dictionary<string, Dictionary<char, string>>
            {
                { "S0", new Dictionary<char, string> { { 'a', "S1" }, { 'b', "Se" }, { 'c', "Se" } } },
                { "S1", new Dictionary<char, string> { { 'a', "Se" }, { 'b', "S2" }, { 'c', "Se" } } },
                { "S2", new Dictionary<char, string> { { 'a', "Se" }, { 'b', "Se" }, { 'c', "S3" } } },
                { "S3", new Dictionary<char, string> { { 'a', "Se" }, { 'b', "Se" }, { 'c', "S4" } } },
                { "S4", new Dictionary<char, string> { { 'a', "Se" }, { 'b', "Se" }, { 'c', "Se" } } },
                { "Se", new Dictionary<char, string> { { 'a', "Se" }, { 'b', "Se" }, { 'c', "Se" } } }
            };

            // Get input from the user
            Console.Write("Enter the input string: ");
            string strInput = Console.ReadLine();
            char[] charInput = strInput.ToCharArray();
            string state = initialState;

            // Process each character in the input
            for (int j = 0; j < charInput.Length; j++)
            {
                char check = charInput[j];
                if (!dict[state].ContainsKey(check))
                {
                    state = "Se"; // Invalid character
                    break;
                }
                state = dict[state][check];
            }

            // Check if the final state is reached
            if (state.Equals(finalState))
                Console.WriteLine("RESULT OKAY");
            else
                Console.WriteLine("ERROR");

            Console.ReadKey();
        }
    }
}