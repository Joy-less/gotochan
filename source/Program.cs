namespace Program
{
    /// <summary>
    /// By Joyless
    /// </summary>
    public class gotochan {
        public const string Version = "1.0.3";

        private Dictionary<string, Action> BuiltInMethods = new();
        private List<object[]> Commands = new();

        private Dictionary<string, object> Variables = new();
        private Dictionary<string, int> LastGotoCallLines = new();

        private int CurrentLine;

        public gotochan() {
            InitialiseBuiltInMethods();
        }

        public void Compile(string Code) {
            Reset();
            // Remove carriage returns
            Code = Code.Replace("\r", "");
            // Remove tabs
            Code = Code.Replace("\t", "");
            // Get each line of code
            string[] CodeLines = Code.Split('\n');
            // Iterate through each line of code
            for (CurrentLine = 0; CurrentLine < CodeLines.Length; CurrentLine++) {
                try {
                    string Line = CodeLines[CurrentLine].Trim(' ');
                    // Ignore comments
                    int IndexOfComment = Line.IndexOf('#');
                    if (IndexOfComment >= 0) {
                        Line = Line.Substring(0, IndexOfComment);
                    }
                    // Check if the line is not empty
                    if (string.IsNullOrWhiteSpace(Line) == false) {
                        string[] Words = Line.Split(' ');
                        string Command = Words[0];

                        // Process the command
                        switch (Command) {
                            // Goto
                            case "goto":
                                // Check invalid word count
                                if (Words.Length == 1) {
                                    Error("goto statements must have a target.");
                                }
                                // Check if statement
                                string GotoConditionVariable = null;
                                if (Words.Length > 2) {
                                    string Parameter2 = Words[2];
                                    if (Parameter2 == "if") {
                                        if (Words.Length > 3) {
                                            string Parameter3 = Words[3];
                                            GotoConditionVariable = Parameter3;
                                        }
                                        else {
                                            Error("if statements must be followed by a variable identifier.");
                                        }
                                    }
                                    else {
                                        Error($"expected if after goto statement, got '{Parameter2}'.");
                                    }
                                }
                                // Get goto target
                                string Parameter = Words[1];
                                // Check if the line number is a built-in label
                                if (BuiltInMethods.ContainsKey(Parameter)) {
                                    Commands.Add(new object[] {"A", Parameter, GotoConditionVariable});
                                    break;
                                }
                                // Check if the line number is a custom label
                                int LineOfLabel = -1;
                                for (int i = 0; i < CodeLines.Length; i++) {
                                    if (CodeLines[i].Trim(' ').StartsWith("label " + Parameter)) {
                                        LineOfLabel = i;
                                        break;
                                    }
                                }
                                if (LineOfLabel >= 0) {
                                    Commands.Add(new object[] {"B", Parameter, LineOfLabel, GotoConditionVariable});
                                    break;
                                }
                                // Check if the line number is an offset from the current line
                                int Modifier = 0;
                                if (Parameter[0] == '+') {
                                    Modifier = 1;
                                    Parameter = Parameter.TrimStart('+');
                                }
                                else if (Parameter[0] == '-') {
                                    Modifier = -1;
                                    Parameter = Parameter.TrimStart('-');
                                }
                                // Try to get the target line number
                                if (int.TryParse(Parameter, out int GotoLine) == true) {
                                    if (Modifier == 0) {
                                        Commands.Add(new object[] {"C", GotoLine, GotoConditionVariable});
                                    }
                                    else if (Modifier == 1) {
                                        Commands.Add(new object[] {"C", CurrentLine + GotoLine, GotoConditionVariable});
                                    }
                                    else if (Modifier == -1) {
                                        Commands.Add(new object[] {"C", CurrentLine - GotoLine, GotoConditionVariable});
                                    }
                                }
                                else {
                                    Error($"the target line number must be a valid integer, integer offset or label (got '{Parameter}').");
                                }
                                break;
                            // Backto
                            case "backto":
                                if (Words.Length == 1) {
                                    Error("backto statements must be followed by a label.");
                                }
                                string BacktoLabel = Words[1];
                                Commands.Add(new object[] {"D", BacktoLabel});
                                break;
                            /*// Forget goto
                            case "forgetgoto":
                                if (Words.Length == 1) {
                                    Error("Forgetgoto statements must be followed by a label.");
                                }
                                string ForgetGotoLabel = Words[1];
                                LastGotoCallLines.Remove(ForgetGotoLabel);
                                break;*/
                            // Label
                            case "label":
                                // Check invalid word count
                                if (Words.Length == 1) {
                                    Error("labels must have an identifier.");
                                }
                                // Check if the label identifier is only letters
                                string LabelIdentifier = Words[1];
                                if (LabelIdentifier.All(char.IsLetter) == false) {
                                    Error("label identifiers can only have letters.");
                                }
                                Commands.Add(null);
                                break;
                            // Set variable
                            default:
                                // Check invalid word count
                                if (Words.Length == 1) {
                                    Error("variable names must be followed by a set operator.");
                                }
                                else if (Words.Length == 2) {
                                    Error("set operators must be followed by a value.");
                                }
                                else if (Words.Length == 4) {
                                    Error("comparison operator must be followed by a value.");
                                }
                                string Operator = Words[1];
                                string Value = Words[2];
                                // Set operator
                                if (Operator == "=") {
                                    // Set variable to equation result
                                    if (Words.Length > 4) {
                                        string SecondOperator = Words[3];
                                        string SecondValue = Words[4];
                                        Commands.Add(new object[] {"E", Command, Value, SecondOperator, SecondValue});
                                    }
                                    // Set variable to value
                                    else {
                                        Commands.Add(new object[] {"F", Command, Value});
                                    }
                                }
                                // Add operator
                                else if (Operator == "+=") {
                                    // Add value to variable
                                    Commands.Add(new object[] {"G", Command, Value});
                                }
                                // Subtract operator
                                else if (Operator == "-=") {
                                    // Add value to variable
                                    Commands.Add(new object[] {"H", Command, Value});
                                }
                                // Unknown operator
                                else {
                                    Error($"unknown set operator: '{Operator}'.");
                                }
                                break;
                        }
                    }
                    else {
                        Commands.Add(null);
                    }
                }
                catch {
                    Error();
                }
            }
        }

        public void Run() {
            Reset();
            CurrentLine = 0;
            try {
                while (CurrentLine < Commands.Count) {
                    if (Commands.Count > CurrentLine && Commands[CurrentLine] != null) {
                        object[] CommandInfo = Commands[CurrentLine];
                        string Command = (string)CommandInfo[0];

                        // Goto built in label
                        if (Command == "A") {
                            string Label = (string)CommandInfo[1];
                            string IfVariable = (string)CommandInfo[2];
                            ProcessGoto(Label, IfVariable, BuiltInMethods[Label]);
                        }
                        // Goto custom label
                        else if (Command == "B") {
                            string Label = (string)CommandInfo[1];
                            int LineOfLabel = (int)CommandInfo[2];
                            string IfVariable = (string)CommandInfo[3];
                            ProcessGoto(Label, IfVariable, delegate {CurrentLine = LineOfLabel;});
                        }
                        // Goto line
                        else if (Command == "C") {
                            int TargetLine = (int)CommandInfo[1];
                            string IfVariable = (string)CommandInfo[2];
                            ProcessGoto(null, IfVariable, delegate { CurrentLine = TargetLine - 1; });
                        }
                        // Backto label
                        else if (Command == "D") {
                            string BacktoLabel = (string)CommandInfo[1];
                            if (LastGotoCallLines.ContainsKey(BacktoLabel)) {
                                CurrentLine = LastGotoCallLines[BacktoLabel];
                            }
                            else {
                                Error($"goto has never been called on label with identifier '{BacktoLabel}'.");
                            }
                        }
                        // Set variable to equation result
                        else if (Command == "E") {
                            string VariableIdentifier = (string)CommandInfo[1];
                            string Value = (string)CommandInfo[2];
                            string SecondOperator = (string)CommandInfo[3];
                            string SecondValue = (string)CommandInfo[4];
                            Variables[VariableIdentifier] = CompareValues(GetValue(Value), SecondOperator, GetValue(SecondValue));
                        }
                        // Set variable to value
                        else if (Command == "F") {
                            string VariableIdentifier = (string)CommandInfo[1];
                            string Value = (string)CommandInfo[2];
                            object DynamicValue = GetValue(Value);
                            if (DynamicValue != null) {
                                Variables[VariableIdentifier] = DynamicValue;
                            }
                            else {
                                Variables.Remove(Command);
                            }
                        }
                        // Add value to variable
                        else if (Command == "G") {
                            string VariableIdentifier = (string)CommandInfo[1];
                            string Value = (string)CommandInfo[2];
                            Variables[VariableIdentifier] = AddValues(Variables.ContainsKey(VariableIdentifier) ? Variables[VariableIdentifier] : null, GetValue(Value));
                        }
                        // Subtract value from variable
                        else if (Command == "H") {
                            string VariableIdentifier = (string)CommandInfo[1];
                            string Value = (string)CommandInfo[2];
                            Variables[VariableIdentifier] = SubtractValues(Variables.ContainsKey(VariableIdentifier) ? Variables[VariableIdentifier] : null, GetValue(Value));
                        }
                    }
                    CurrentLine++;
                }
            }
            catch {
                Error();
            }
        }

        public void DisplayCompiledCode() {
            int CommandNumber = -1;
            foreach (object[] CommandInfo in Commands) {
                CommandNumber++;
                Console.Write($"{CommandNumber}. ");
                if (CommandInfo != null) {
                    foreach (object Info in CommandInfo) {
                        Console.Write(Info);
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }
        }

        private void ProcessGoto(string Label, string ConditionVariable, Action GotoAction) {
            // Set the current line as the last goto call line
            if (Label != null) {
                if (LastGotoCallLines.ContainsKey(Label) == false) {
                    LastGotoCallLines.Add(Label, CurrentLine);
                }
                else {
                    LastGotoCallLines[Label] = CurrentLine;
                }
            }
            // Run the goto if there is no condition
            if (ConditionVariable == null) {
                GotoAction();
            }
            // Run the goto if the condition is true
            else if (Variables.ContainsKey(ConditionVariable) && Variables[ConditionVariable].GetType() == typeof(bool)) {
                if ((bool)Variables[ConditionVariable] == true) {
                    GotoAction();
                }
            }
            else {
                Error($"no boolean variable found with identifier '{ConditionVariable}'.");
            }
        }

        private void Reset() {
            Variables.Clear();
            LastGotoCallLines.Clear();
        }

        private void InitialiseBuiltInMethods() {
            void Say() {
                if (Variables.ContainsKey("param")) {
                    object Output = Variables["param"];
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
                Variables["result"] = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000d;
            }
            void Wait() {
                if (Variables.ContainsKey("param") && float.TryParse(Variables["param"].ToString(), out float WaitDuration)) {
                    Thread.Sleep((int)Math.Round(WaitDuration * 1000));
                }
                else {
                    Error("wait param must be a number.");
                }
            }
            void Input() {
                while (true) {
                    ConsoleKeyInfo GetKey = Console.ReadKey(true);
                    if (GetKey.Key == ConsoleKey.Enter) {
                        Variables["result"] = "\n";
                        break;
                    }
                    else if (GetKey.Key == ConsoleKey.Backspace || GetKey.KeyChar == '\r') {
                    }
                    else {
                        Variables["result"] = GetKey.KeyChar.ToString();
                        break;
                    }
                }
            }
            void HasInput() {
                Variables["result"] = Console.KeyAvailable;
            }
            void Random() {
                if (Variables.ContainsKey("param") && long.TryParse(Variables["param"].ToString(), out long RandomMaximum)) {
                    if (RandomMaximum >= 0) {
                        Variables["result"] = new Random().NextInt64(0, RandomMaximum + 1);
                    }
                    else {
                        Error("random param cannot be negative.");
                    }
                }
                else {
                    Error("random param must be an integer.");
                }
            }

            BuiltInMethods.Add("say", Say);
            BuiltInMethods.Add("clear", Clear);
            BuiltInMethods.Add("gettime", GetTime);
            BuiltInMethods.Add("wait", Wait);
            BuiltInMethods.Add("input", Input);
            BuiltInMethods.Add("hasinput", HasInput);
            BuiltInMethods.Add("random", Random);
        }

        private object GetValue(string Value) {
            // String
            if (Value.StartsWith("~")) {
                return Value.TrimStart('~').Replace('~', ' ').Replace("\\n", "\n").Replace("\\h", "#");
            }
            // Long
            else if (long.TryParse(Value, out long LongResult)) {
                return LongResult;
            }
            // Double
            else if (double.TryParse(Value, out double DoubleResult)) {
                return DoubleResult;
            }
            // Bool
            else if (Value == "yes") {
                return true;
            }
            else if (Value == "no") {
                return false;
            }
            // Null
            else if (Value == "null") {
                return null;
            }
            // Variable
            else if (Variables.ContainsKey(Value)) {
                return Variables[Value];
            }
            // Empty variable
            else if (Value.All(char.IsLetter)) {
                return null;
            }
            // Unknown
            else {
                Error($"unknown data type of value '{Value}'.");
                return null;
            }
        }

        private object AddValues(object Value1, object Value2) {
            return OperateValues(Value1, Value2, 1);
        }
        private object SubtractValues(object Value1, object Value2) {
            return OperateValues(Value1, Value2, -1);
        }
        private object OperateValues(object Value1, object Value2, int MultiplySecondValueBy) {
            // Operate null
            if (Value1 == null || Value2 == null) {
                Error("Cannot add or subtract null.");
            }
            // Operate long and long
            else if (Value1.GetType() == typeof(long) && Value2.GetType() == typeof(long)) {
                Value1 = (long)Value1 + (long)Value2 * MultiplySecondValueBy;
            }
            // Operate double and double
            else if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                Value1 = (double)Value1 + (double)Value2 * MultiplySecondValueBy;
            }
            // Operate long and double
            else if (double.TryParse(Value1.ToString(), out double Value1Double) && double.TryParse(Value2.ToString(), out double Value2Double)) {
                Value1 = Value1Double + Value2Double * MultiplySecondValueBy;
            }
            // Add object to string
            else if (MultiplySecondValueBy == 1 && (Value1.GetType() == typeof(string) || Value2.GetType() == typeof(string))) {
                Value1 = Value1.ToString() + Value2.ToString();
            }
            // Error
            else {
                Error($"cannot {(MultiplySecondValueBy == 1 ? "add" : "subtract")} values: '{Value1}', '{Value2}'.");
            }
            return Value1;
        }

        private bool CompareValues(object Value1, string ComparisonOperator, object Value2) {
            bool ComparisonResult = false;
            // Compare equality
            if (ComparisonOperator == "==") {
                ComparisonResult = Value1.Equals(Value2);
            }
            else if (ComparisonOperator == "!=") {
                ComparisonResult = !Value1.Equals(Value2);
            }
            // Arithmetic operators
            else if (ComparisonOperator == ">" || ComparisonOperator == "<") {
                if (float.TryParse(Value1.ToString(), out float Value1Float) && float.TryParse(Value2.ToString(), out float Value2Float)) {
                    // Greater than
                    if (ComparisonOperator == ">") {
                        ComparisonResult = Value1Float > Value2Float;
                    }
                    // Less than
                    else {
                        ComparisonResult = Value1Float < Value2Float;
                    }
                }
            }
            // Error
            else {
                Error($"unknown comparison operator: '{ComparisonOperator}'.");
            }
            return ComparisonResult;
        }

        private void Error(string ErrorMessage = null) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.Write($"there was an error on line {CurrentLine + 1}");
            if (ErrorMessage != null) {
                Console.Write(":\n    " + ErrorMessage);
            }
            else {
                Console.Write(".");
            }
            Console.ReadLine();
            Environment.Exit(0);
        }
    }

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
            Console.WriteLine($"gotochan v{gotochan.Version}");
            Console.WriteLine();

            // Read the code
            string Code = File.ReadAllText(args[0]);

            // Compile the code
            gotochan gotochanInstance = new();
            gotochanInstance.Compile(Code);

            // Run the code
            ClearInput();
            gotochanInstance.Run();

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
        private static void CompileBenchmark(string Code) {
            gotochan gotochanInstance = new();
            long StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // Start benchmark
            gotochanInstance.Compile(Code);
            gotochanInstance.DisplayCompiledCode();
            Console.WriteLine($"Compiled in {(DateTimeOffset.Now.ToUnixTimeMilliseconds() - StartTime) / 1000d} seconds."); // End benchmark
            Console.WriteLine("-------------------------");
        }
    }
}
