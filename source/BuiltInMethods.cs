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
                        Gotochan.Variables["result"] = double.Parse(new Random().NextInt64(0, RandomMaximum + 1).ToString());
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
            async Task GetType() {
                if (Gotochan.Variables.ContainsKey("param")) {
                    Dictionary<Type, string> TypeNames = new() {
                        {typeof(string), "string"},
                        {typeof(double), "number"},
                        {typeof(bool), "bool"},
                    };
                    Type ParamType = Gotochan.Variables["param"].GetType();
                    if (TypeNames.ContainsKey(ParamType)) {
                        Gotochan.Variables["result"] = TypeNames[ParamType];
                    }
                    else {
                        Gotochan.Variables["result"] = "unknown";
                    }
                }
                else {
                    Gotochan.Variables["result"] = "null";
                }
            }
            async Task Length() {
                if (Gotochan.Variables.TryGetValue("param", out object Param)) {
                    Gotochan.Variables["result"] = Convert.ToDouble(Param.ToString().Length);
                }
                else {
                    Gotochan.Error("length param must not be null.");
                }
            }
            async Task Truncate() {
                if (Gotochan.Variables.ContainsKey("param") && double.TryParse(Gotochan.Variables["param"].ToString(), out double Param)) {
                    Gotochan.Variables["result"] = Math.Truncate(Param);
                }
                else {
                    Gotochan.Error("truncate param must be a number.");
                }
            }
            async Task Round() {
                if (Gotochan.Variables.ContainsKey("param") && double.TryParse(Gotochan.Variables["param"].ToString(), out double Param)) {
                    Gotochan.Variables["result"] = Math.Round(Param);
                }
                else {
                    Gotochan.Error("round param must be a number.");
                }
            }
            async Task Floor() {
                if (Gotochan.Variables.ContainsKey("param") && double.TryParse(Gotochan.Variables["param"].ToString(), out double Param)) {
                    Gotochan.Variables["result"] = Math.Floor(Param);
                }
                else {
                    Gotochan.Error("floor param must be a number.");
                }
            }
            async Task Ceiling() {
                if (Gotochan.Variables.ContainsKey("param") && double.TryParse(Gotochan.Variables["param"].ToString(), out double Param)) {
                    Gotochan.Variables["result"] = Math.Ceiling(Param);
                }
                else {
                    Gotochan.Error("ceiling param must be a number.");
                }
            }

            MethodsList.Add("say", Say);
            MethodsList.Add("clear", Clear);
            MethodsList.Add("gettime", GetTime);
            MethodsList.Add("wait", Wait);
            MethodsList.Add("input", Input);
            MethodsList.Add("hasinput", HasInput);
            MethodsList.Add("random", Random);
            MethodsList.Add("error", Error);
            MethodsList.Add("gettype", GetType);
            MethodsList.Add("truncate", Truncate);
            MethodsList.Add("round", Round);
            MethodsList.Add("length", Length);
            MethodsList.Add("floor", Floor);
            MethodsList.Add("ceiling", Ceiling);
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
