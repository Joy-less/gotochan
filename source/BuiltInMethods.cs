using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gotochan
{
    /// <summary>
    /// This class contains built-in methods that can be accessed as labels.
    /// </summary>
    internal class BuiltInMethods
    {
        public Dictionary<string, Func<Task>> MethodsList = new();

        private readonly Gotochan Gotochan;

        public BuiltInMethods(Gotochan GotochanInstance) {
            Gotochan = GotochanInstance;

            async Task Say() {
                if (Gotochan.Variables.ContainsKey("param")) {
                    object Output = Gotochan.Variables["param"];
                    if (Output.Equals(true)) {
                        Console.Write("true");
                    }
                    else if (Output.Equals(false)) {
                        Console.Write("false");
                    }
                    else {
                        Console.Write(Output);
                    }
                }
            }
            async Task Clear() {
                Console.Clear();
            }
            async Task GetTime() {
                Gotochan.Variables["result"] = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000d;
            }
            async Task Wait() {
                if (Gotochan.Variables.ContainsKey("param") && float.TryParse(Gotochan.Variables["param"].ToString(), out float WaitDuration)) {
                    await Task.Delay((int)Math.Round(WaitDuration * 1000));
                }
                else {
                    Gotochan.Error("wait param must be a number.");
                }
            }
            async Task Input() {
                while (true) {
                    ConsoleKeyInfo GetKey = Console.ReadKey(true);
                    if (GetKey.Key == ConsoleKey.Enter) {
                        Gotochan.Variables["result"] = "\n";
                        break;
                    }
                    else if (GetKey.Key == ConsoleKey.Backspace || GetKey.KeyChar == '\r') {
                    }
                    else {
                        Gotochan.Variables["result"] = GetKey.KeyChar.ToString();
                        break;
                    }
                }
            }
            async Task HasInput() {
                Gotochan.Variables["result"] = Console.KeyAvailable;
            }
            async Task Random() {
                if (Gotochan.Variables.ContainsKey("param") && long.TryParse(Gotochan.Variables["param"].ToString(), out long RandomMaximum)) {
                    if (RandomMaximum >= 0) {
                        Gotochan.Variables["result"] = new Random().NextInt64(0, RandomMaximum + 1);
                    }
                    else {
                        Gotochan.Error("random param cannot be negative.");
                    }
                }
                else {
                    Gotochan.Error("random param must be an integer.");
                }
            }
            async Task Error() {
                object? MessageObject = Gotochan.Variables["param"];
                string? Message = MessageObject != null ? MessageObject.ToString() : null;
                Gotochan.Error(Message);
            }

            MethodsList.Add("say", Say);
            MethodsList.Add("clear", Clear);
            MethodsList.Add("gettime", GetTime);
            MethodsList.Add("wait", Wait);
            MethodsList.Add("input", Input);
            MethodsList.Add("hasinput", HasInput);
            MethodsList.Add("random", Random);
            MethodsList.Add("error", Error);
        }

        public void Error(int Line, string? Message) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.Write($"there was an error on line {Line + 1}");
            if (Message != null) {
                Console.Write(":\n    " + Message);
            }
            else {
                Console.Write(".");
            }
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
