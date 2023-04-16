namespace gotochan
{
    /// <summary>
    /// This class reads a gotochan file and runs it.
    /// </summary>
    public class Program
    {
        private static void Main(string[] args) {
            // Check that there is a valid gotochan file to be run
            if (args.Length == 0) {
                return;
            }
            else if (File.Exists(args[0]) == false) {
                return;
            }

            // Display version information
            Console.WriteLine($"gotochan v{Gotochan.Version}");
            Console.WriteLine();

            // Read the code
            string Code = File.ReadAllText(args[0]);

            // Parse the code
            Gotochan GotochanInstance = new();
            List<object[]> Commands = GotochanInstance.Parse(Code);

            // Run the code
            ClearInput();
            GotochanInstance.Run(Commands).Wait();

            // End of code, wait for user to press enter
            Console.WriteLine("\n");
            Console.Write("end of program");
            ClearInput();
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }
        private static void ClearInput() {
            while (Console.KeyAvailable) {
                Console.ReadKey(true);
            }
        }
        private static void ParseBenchmark(string Code) {
            long StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // Start benchmark
            Gotochan GotochanInstance = new();
            List<object[]> Commands = GotochanInstance.Parse(Code);
            GotochanInstance.DisplayParsedCommands(Commands);
            Console.WriteLine($"Parsed in {(DateTimeOffset.Now.ToUnixTimeMilliseconds() - StartTime) / 1000d} seconds."); // End benchmark
            Console.WriteLine("-------------------------");
        }
    }
}
