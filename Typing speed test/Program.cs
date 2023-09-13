using System.Net;
using Console = System.Console;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

public class Program
{
    public static bool Finished = false;
    public static void Main()
    {
        //Runs a test to see if the file exists
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "words.txt"))
            GenerateWords.DownloadFile();

        //Checks to make sure the file can be found after the download
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "words.txt"))
            throw new FileNotFoundException("File not found");
        
        //Takes in the mode of the test
        Console.WriteLine("Please input the type of test (1 -> 3)");
        if(!int.TryParse(Console.ReadLine(), out var mode))
            throw new FormatException($"{mode} is not a valid integer");

        //Starts the timer and runs logic
        Console.WriteLine("GO!!");
        var main = Task.Factory.StartNew(() => Logic.Run());
        Finished = Task.Factory.StartNew(() => Clock.Time(mode)).Result;

        //  Outputs the basic results
        Console.Clear();
        Console.WriteLine("Finished...");
        Console.WriteLine($"You got {CheckInput.Wpm(mode)} words per minute");
        
    }
}

public class GenerateWords
{
    /// <summary>
    /// If the word file is missing it downloads it from the github repo
    /// </summary>
    public static void DownloadFile()
        => new WebClient().DownloadFile(
            "https://raw.githubusercontent.com/xKronos58/ConsoleTypeTest/main/Typing%20speed%20test/Words.txt?token=GHSAT0AAAAAACG7JEZZ4LCU6EWFHKS56IM4ZIBMW3A", 
            /*        TODO: Link needs to be fixed!         */
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Words.txt"));
    
    /// <summary>
    /// Builds the sentence to be typed
    /// </summary>
    /// <returns></returns>
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
            throw new FileNotFoundException(AppDomain.CurrentDomain.BaseDirectory + "words.txt not found");


    static Random random = new();
    /// <summary>
    /// Takes in the cached word list and then outputs a random word from that list
    /// </summary>
    /// <param name="list">String[] of single words</param>
    /// <returns>string</returns>
    private static string GenerateWord(IReadOnlyList<string> list)
        => list[random.Next(0, list.Count)];
}

public class Clock
{
    /// <summary>
    /// Takes the mode and runs the timer that returns true when finished
    /// </summary>
    /// <param name="mode">int used to determine how long the timer is (1 - 3)</param>
    /// <returns>true</returns>
    public static bool Time(int mode)
    {
        Thread.Sleep(mode switch
        {
            1 => 15000, // 15 seconds
            2 => 30000, // 30 seconds
            3 => 60000, // 1 minute
            _ => 30000, // Defaults to 30 seconds if the input is invalid
        });
        return true;
    }
}

public class Logic
{
    public static List<string[]> lines = new();
    public static List<string[]> answers = new();
    
    /// <summary>
    /// Pulls all of the logic together on runs until the timer is finished 
    /// </summary>
    public static void Run()
    {
        var i = 0;
        while (!Program.Finished)
        {
            lines.Add(GenerateWords.Build());
            Console.WriteLine(string.Join(' ', lines[i]));
            answers.Add(CheckInput.Input()!);
            i++;
        }
    }
}

public class CheckInput
{
    public static string[]? Input()
    {
        // TODO: I want to do it via Console.readKey for an accurate (In color) display,
        // as well as to allow for per word counting rather than per line
        // Should also allow for constant display of the timer
        
        
        
        return Console.ReadLine()?.Split(' ')!;
    }
    
    /// <summary>
    /// Takes the Two List[string[]] and compares them to each other to get the WPM
    /// divides it by the mode to get the correct WPM
    /// </summary>
    /// <returns>int - Speed</returns>
    public static int Wpm(int mode)
        => (from line in Logic.lines 
            from word in line 
            from answer in Logic.answers 
            from input in answer 
            where word == input select word).Count() 
           / mode switch { 1 => 4, 2 => 2, 3 => 1, _ => 2};
}

public class DisplayResults
{
    
}


/// <summary>
/// Obsolete code used for testing and reference
/// </summary>
[Obsolete]
class ThreadTesting
{
    /// <summary>
    /// Obsolete code used for testing and reference
    /// </summary>
    [Obsolete]
    public static void ThreadTest(string[] args)
    {
        Task task1 = Task.Factory.StartNew(() => doStuff("Task1"));
        Task task2 = Task.Factory.StartNew(() => doStuff("Test"));
        Task task3 = Task.Factory.StartNew(() => doStuff("Task3"));
        Task.WaitAll(task1, task2, task3);

        Console.WriteLine("All tasks are completed");
        Console.ReadLine();
    }

    /// <summary>
    /// Obsolete code used for testing and reference
    /// </summary>
    [Obsolete]
    static void doStuff(string strName)
    {
        for (int i = 1; i <= 3; i++)
        {
            Console.WriteLine(strName + " " + i.ToString());
            Thread.Yield();
        }
    }
}

/*
 *  Class 1 Generate the random words
 *  Class 2 Run a constant clock
 *  Class 3 Check the input
 *  Class 4 Display the results
 *  Program Run the program
 */