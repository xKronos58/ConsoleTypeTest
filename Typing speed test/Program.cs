using System.Diagnostics;
using System.Net;

public class Program
{
    public static void Main()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Words.txt"))
            GenerateWords.DownloadFile();
    }
}

public class GenerateWords
{
    public static void DownloadFile()
    {
        var client = new WebClient();
        client.DownloadFile("https://raw.githubusercontent.com/dwyl/english-words/master/words.txt", "words.txt");
    }
    public static string[] Build(int mode)
    {
        var words = new string[mode switch
        {
            1 => 12,
            2 => 90,
            _ => 0
        }];
        var cachedWords = CacheWords();
        for(var i = 0; 0 < words.Length; i++)
            words[i] = GenerateWord(cachedWords);
        
        return words;
    }

    /// <summary>
    /// Pulls the words from the file
    /// </summary>
    /// <returns>String[] of single words</returns>
    /// <exception cref="FileNotFoundException">FileNotFound</exception>
    private static string[] CacheWords()
        => File.Exists(AppDomain.CurrentDomain.BaseDirectory + "words.txt") ?
            File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "words.txt") :
            throw new FileNotFoundException(AppDomain.CurrentDomain.BaseDirectory + "words.txt not found");
    
    /// <summary>
    /// Takes in the cached word list and then outputs a random word from that list
    /// </summary>
    /// <param name="list">String[] of single words</param>
    /// <returns>string</returns>
    private static string GenerateWord(IReadOnlyList<string> list)
        => list[new Random().Next(0, list.Count)];
}

public class clock
{
    
}

public class checkInput
{
    
}

public class displayResults
{
    
}

/*
 *  Class 1 Generate the random words
 *  Class 2 Run a constant clock
 *  Class 3 Check the input
 *  Class 4 Display the results
 *  Program Run the program
 */