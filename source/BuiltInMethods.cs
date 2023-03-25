namespace gotochan
{
    /// <summary>
    /// This class contains built-in methods that can be accessed as labels.
    /// </summary>
    internal class BuiltInMethods
    {
        public Dictionary<string, Action> MethodsList = new();

        private readonly Gotochan Gotochan;

        public BuiltInMethods(Gotochan GotochanInstance) {
            Gotochan = GotochanInstance;

            void Say() {
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
            void Clear() {
                Console.Clear();
            }
            void GetTime() {
                Gotochan.Variables["result"] = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000d;
            }
            void Wait() {
                if (Gotochan.Variables.ContainsKey("param") && float.TryParse(Gotochan.Variables["param"].ToString(), out float WaitDuration)) {
                    Thread.Sleep((int)Math.Round(WaitDuration * 1000));
                }
                else {
                    Gotochan.Error("wait param must be a number.");
                }
            }
            void Input() {
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
            void HasInput() {
                Gotochan.Variables["result"] = Console.KeyAvailable;
            }
            void Random() {
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

            MethodsList.Add("say", Say);
            MethodsList.Add("clear", Clear);
            MethodsList.Add("gettime", GetTime);
            MethodsList.Add("wait", Wait);
            MethodsList.Add("input", Input);
            MethodsList.Add("hasinput", HasInput);
            MethodsList.Add("random", Random);
        }
    }
}
