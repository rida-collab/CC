using System;

class RandomPasswordGenerator
{
    static void Main()
    {
        // Input from user
        Console.Write("Enter your first name: ");
        string firstName = Console.ReadLine();

        Console.Write("Enter your last name: ");
        string lastName = Console.ReadLine();

        Console.Write("Enter your 3-digit registration number: ");
        string regNumber = Console.ReadLine();

        Console.Write("Enter your favorite movie: ");
        string favMovie = Console.ReadLine();

        Console.Write("Enter your favorite food: ");
        string favFood = Console.ReadLine();

        // Combine all inputs into a single string
        string combined = firstName + lastName + regNumber + favMovie + favFood;

        // Create a random object
        Random random = new Random();

        // Create a char array from the combined string
        char[] chars = combined.ToCharArray();

        // Shuffle the characters randomly
        for (int i = 0; i < chars.Length; i++)
        {
            // Swap each character with a randomly chosen character from the rest of the array
            int j = random.Next(i, chars.Length);
            char temp = chars[i];
            chars[i] = chars[j];
            chars[j] = temp;
        }

        // Convert the shuffled char array back to a string
        string shuffledString = new string(chars);

        // Generate a random number between 1000-9999
        int randomNum = random.Next(1000, 10000);

        // Create the final password by appending the random number
        string password = shuffledString + randomNum;

        // Display the generated password
        Console.WriteLine("Your generated password is: " + password);
        Console.ReadKey();
    }
}