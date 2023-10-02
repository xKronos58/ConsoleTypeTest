namespace Typing_speed_test;

public abstract class Program
{
    public static bool Finished;
    
    public static async Task Main()
    {
        //Runs a test to see if the file exists
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "words.txt"))
            await GenerateWords.DownloadFileAsync();

        //Checks to make sure the file can be found after the download
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "words.txt"))
            throw new FileNotFoundException($"File not found, please run the program again to download the file");
        
        //Takes in the mode of the test
        Console.WriteLine("Please input the type of test (1 -> 3)");
        if(!int.TryParse(Console.ReadLine(), out var mode) && mode is > 1 and < 3)
            throw new FormatException($"{mode} is not a valid integer please input a number between 1 and 3");
        
        //Gives the user a countdown
        Console.Clear();
        Clock.Countdown();
        
        //Starts the timer and runs logic
        Console.WriteLine("GO!!");
        var main = Task.Factory.StartNew(Logic.Run);
        Finished = Task.Factory.StartNew(() => Clock.Time(mode)).Result;
        Task.WaitAll(main);
        
        //  Outputs the basic results
        // Console.Clear();
        Console.WriteLine("Times up!!...");
        var speed = SpeedAndAccuracy.Wpm(mode);
        var aboveOrBelow = speed == 40 ? "At" : speed > 40 ? "Above" : "Blow";
        Console.WriteLine($"You got {speed} (CPM : {0 /*CheckInput.Cpm(mode)*/})" +
                          $" words per minute that is {aboveOrBelow} Average" +
                          $"\nWith an accuracy of {SpeedAndAccuracy.Accuracy()}%");
        
    }
}

public abstract class GenerateWords
{
    /// <summary>
    /// If the word file is missing it downloads it from the github repo
    /// </summary>
    public static async Task DownloadFileAsync()
    {
        var response = await new HttpClient().GetAsync("https://raw.githubusercontent.com/xKronos58/ConsoleTypeTest/main/Typing%20speed%20test/Words.txt?token=GHSAT0AAAAAACG7JEZZ4LCU6EWFHKS56IM4ZIBMW3A");

        if (response.IsSuccessStatusCode)
            await File.WriteAllBytesAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Words.txt"), 
                await response.Content.ReadAsByteArrayAsync());
        else
            throw new HttpRequestException($"Failed to download file. Status code: {response.StatusCode}");
    }
    
    /// <summary>
    /// Builds the sentence to be typed
    /// </summary>
    /// <returns>String[12]</returns>
    public static string[] Build()
    {
        var words = new string[12];
        var cachedWords = CacheWords();
        for(var i = 0; i < words.Length; i++)
            words[i] = GenerateWord(cachedWords);
        
        return words;
    }

    /// <summary>
    /// Pulls the words from the file
    /// </summary>
    /// <returns>String[] of single words</returns>
    /// <exception cref="FileNotFoundException">FileNotFound</exception>
    private static string[] CacheWords()
        => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Words.txt")) ?
            File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Words.txt")) :
            throw new FileNotFoundException(AppDomain.CurrentDomain.BaseDirectory 
                                            + "words.txt not found please rerun the program to download the word list");

    private static readonly Random Random = new();
    
    /// <summary>
    /// Takes in the cached word list and then outputs a random word from that list
    /// </summary>
    /// <param name="list">String[] of single words</param>
    /// <returns>string</returns>
    private static string GenerateWord(IReadOnlyList<string> list)
        => list[Random.Next(0, list.Count)];
}

public abstract class Clock
{
    /// <summary>
    /// Takes the mode and runs the timer that returns true when finished
    /// </summary>
    /// <param name="mode">int used to determine how long the timer is (1 - 3)</param>
    /// <returns>true</returns>
    public static bool Time(int mode)
    {
        var modes = new[] { 15000 /* 15 Seconds */, 30000 /* 30 Seconds */, 60000 /* 60 Seconds */ };
        Thread.Sleep(modes[mode - 1]);
        return true;
    }

    /// <summary>
    /// Gives a countdown from 3 seconds
    /// </summary>
    public static void Countdown()
    {
        for(var i = 3; i > 0; i--)
        {
            Console.WriteLine(i);
            Thread.Sleep(1000);
        }
    }
}

public abstract class Logic
{
    public static readonly List<string[]> Lines = new();
    public static readonly List<string[]> Answers = new();
    
    /// <summary>
    /// Pulls all of the logic together on runs until the timer is finished 
    /// </summary>
    public static void Run()
    {
        var i = 0;
        while (!Program.Finished)
        {
            Lines.Add(GenerateWords.Build());
            Console.WriteLine("\n" + string.Join(' ', Lines[i]));
            Answers.Add(CheckInput.InputLine()!);
            i++;
        }
    }
}

/// <summary>
/// All of the methods used to check and handle input
/// </summary>
public abstract class CheckInput
{
    /// <summary>
    /// This method uses input word to get a whole line then returns what has been typed.
    /// </summary>
    /// <returns>String?[12]</returns>
    private static int _wordPos;
    public static string?[] InputLine()
    {
        var end = new string?[12];
        _wordPos = 0;
        while (!Program.Finished && _wordPos < 12)
        {
            end[_wordPos] = InputWord(_wordPos, end);
            _wordPos++;
        }

        Console.WriteLine("Line finished...");
        return end;
    }
    
    /// <summary>
    /// Takes single word input from the console. 
    /// </summary>
    /// <returns>String?</returns>
    private static string InputWord(int wordPos, string?[] end)
    {
        // Takes keyboard input individually, this allows for the word count to be updated in real time.
        // If the key is a space it submits the word.
        
        var current = new ConsoleKeyInfo();
        var final = string.Empty;

        while (current.Key != ConsoleKey.Spacebar)
        {
            if (current.Key == ConsoleKey.Backspace)
                final = HandleBackspace(final, wordPos, end);
            current = Console.ReadKey();
            final += current.KeyChar;
        }
        
        return final[..^1];
    }

    /// <summary>
    /// If the user typed a back space, checks if the word is empty,
    /// if so starts to modify the last word, else removes the last letter typed
    /// </summary>
    /// <param name="final">String, Current word</param>
    /// <param name="wordPos">Int, Position in sentence</param>
    /// <param name="end">String[], Sentence</param>
    /// <returns>String, current word</returns>
    private static string HandleBackspace(string final, int wordPos, IReadOnlyList<string?> end)
    {
        Console.Write("\b\b");
        return final.Any().Equals(null) ? HandlePreviousWordModification(wordPos, end) : final[..^1];
    }

    private static string HandlePreviousWordModification(int wordPos, IReadOnlyList<string?> end)
    {
        wordPos--;
        return end[wordPos]![..^1];     //TODO: Modification still needed
    }
}

public abstract class SpeedAndAccuracy
{

    /// <summary>
    /// Takes the Two List[string[]] and compares them to each other to get the WPM
    /// multiples it by the mode to get the correct WPM
    /// </summary>
    /// <returns>int (Speed)</returns>
    public static int Wpm(int mode)
        => (from line in Logic.Lines
               from word in line
               from answer in Logic.Answers
               from input in answer
               where word == input
               select word).Count()
           * mode switch { 1 => 4, 2 => 2, 3 => 1, _ => 2 };
    
    private static readonly int TotalWordsTyped = Logic.Answers.Sum(l => l.Length); 
    //Need to check if L is not null should be able to do this with a where statement.
    
    // Error rate as percentage = (total - errors) * 100
    public static double Accuracy()
        => (double)(TotalWordsTyped - Errors()) / TotalWordsTyped * 100;

    private static int Errors()
    {
        var errors = 0;
        Console.WriteLine("(You Wrote) - (Correct Answer)");
        for(var i = 0; i < Logic.Answers.Count; i++)
            for(var j = 0; j < Logic.Answers[i].Length; j++)
                if (Logic.Lines[i][j] != Logic.Answers[i][j] && Logic.Answers[i][j] != String.Empty)
                {
                    errors++;
                    Console.WriteLine(Logic.Answers[i][j] + " - " + Logic.Lines[i][j]);
                }
        return errors;
    }
    
    // To be implemented later but is the base for the CPM
    
    // public static int Cpm(int mode)
    //     => (from line in Logic.lines
    //         from word in line
    //         from chara in word
    //         from answer in Logic.answers
    //         from input in answer
    //         from chara2 in input
    //         where chara == chara2
    //         select chara).Count() 
    //        * mode switch { 1 => 4, 2 => 2, 3 => 1, _ => 2 };
}