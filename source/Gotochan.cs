using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace gotochan
{
    /// <summary>
    /// This class contains the code that parses and runs a gotochan program.
    /// </summary>
    public class Gotochan {
        public const string Version = "1.1.0";

        private readonly BuiltInMethods BuiltInMethods;

        internal Dictionary<string, object> Variables = new();
        private Dictionary<string, int> LastGotoCallLines = new();

        private int CurrentLine;

        public Gotochan() {
            BuiltInMethods = new(this);
        }

        public List<object[]> Parse(string Code) {
            Reset();
            List<object[]> Commands = new();
            // Remove carriage returns
            Code = Code.Replace("\r", "");
            // Remove tabs
            Code = Code.Replace("\t", "");
            // Get each line of code
            string[] CodeLines = Code.Split('\n');
            // Iterate through each line of code
            for (CurrentLine = 0; CurrentLine < CodeLines.Length; CurrentLine++) {
                try {
                    string Line = CodeLines[CurrentLine];
                    // Ignore comments
                    int IndexOfComment = Line.IndexOf('#');
                    if (IndexOfComment >= 0) {
                        Line = Line.Substring(0, IndexOfComment);
                    }
                    // Trim spaces
                    Line = Line.Trim(' ');
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
                                string? GotoConditionVariable = null;
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
                                if (BuiltInMethods.MethodsList.ContainsKey(Parameter)) {
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
                                        Commands.Add(new object[] {"C", GotoLine - 1, GotoConditionVariable});
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
                                // Check if the label identifier is valid
                                string LabelIdentifier = Words[1];
                                if (IsVariableOrLabelNameValid(LabelIdentifier) == false) {
                                    Error($"'{LabelIdentifier}' is not a valid label identifier.");
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
                                // Check if variable name is valid
                                if (IsVariableOrLabelNameValid(Command) == false) {
                                    Error($"variable identifier '{Command}' is invalid.");
                                }
                                // Get operator and value
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
                                else {
                                    // Add / Subtract / Multiply / Divide / Modulo / Exponentiate
                                    Dictionary<string, string> Functions = new() {
                                        {"+=", "G"},
                                        {"-=", "H"},
                                        {"*=", "I"},
                                        {"/=", "J"},
                                        {"%=", "K"},
                                        {"^=", "L"},
                                    };
                                    if (Functions.TryGetValue(Words[1], out string Function)) {
                                        Commands.Add(new object[] {Function, Command, Value});
                                    }
                                    else {
                                        Error($"unknown variable operator: '{Operator}'.");
                                    }
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
            return Commands;
        }

        public async Task Run(List<object[]> Commands) {
            Reset();
            CurrentLine = 0;

            Dictionary<string, Func<object, object, object>> VariableOperationCommands = new() {
                {"G", AddValues},
                {"H", SubtractValues},
                {"I", MultiplyValues},
                {"J", DivideValues},
                {"K", ModuloValues},
                {"L", ExponentiateValues},
            };

            try {
                while (CurrentLine < Commands.Count) {
                    if (Commands[CurrentLine] != null) {
                        object[] CommandInfo = Commands[CurrentLine];
                        string Command = (string)CommandInfo[0];

                        // Goto built in label
                        if (Command == "A") {
                            string Label = (string)CommandInfo[1];
                            string IfVariable = (string)CommandInfo[2];
                            if (ProcessGoto(Label, IfVariable) == true) {
                                await BuiltInMethods.MethodsList[Label]();
                            }
                        }
                        // Goto custom label
                        else if (Command == "B") {
                            string Label = (string)CommandInfo[1];
                            int LineOfLabel = (int)CommandInfo[2];
                            string IfVariable = (string)CommandInfo[3];
                            if (ProcessGoto(Label, IfVariable) == true) {
                                CurrentLine = LineOfLabel;
                            }
                        }
                        // Goto line
                        else if (Command == "C") {
                            int TargetLine = (int)CommandInfo[1];
                            string IfVariable = (string)CommandInfo[2];
                            if (ProcessGoto(null, IfVariable) == true) {
                                CurrentLine = TargetLine - 1;
                            }
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
                            string ValueString = (string)CommandInfo[2];
                            string SecondOperator = (string)CommandInfo[3];
                            string SecondValueString = (string)CommandInfo[4];
                            object? Value = InitialiseValueFromString(ValueString);
                            object? SecondValue = InitialiseValueFromString(SecondValueString);
                            Variables[VariableIdentifier] = CompareValues(Value, SecondOperator, SecondValue);
                        }
                        // Set variable to value
                        else if (Command == "F") {
                            string VariableIdentifier = (string)CommandInfo[1];
                            string Value = (string)CommandInfo[2];
                            object? DynamicValue = InitialiseValueFromString(Value);
                            if (DynamicValue != null) {
                                Variables[VariableIdentifier] = DynamicValue;
                            }
                            else {
                                Variables.Remove(VariableIdentifier);
                            }
                        }
                        // Operate variable
                        else if (VariableOperationCommands.ContainsKey(Command) == true) {
                            // Get variable and value
                            string VariableIdentifier = (string)CommandInfo[1];
                            string ValueString = (string)CommandInfo[2];
                            Variables.TryGetValue(VariableIdentifier, out object? VariableValue);
                            object? Value = InitialiseValueFromString(ValueString);
                            Value ??= "null";
                            VariableValue ??= "null";
                            Variables[VariableIdentifier] = VariableOperationCommands[Command](VariableValue, Value);
                        }
                    }
                    CurrentLine++;
                }
            }
            catch {
                Error();
            }
        }

        public void DisplayParsedCommands(List<object[]> Commands) {
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

        private bool ProcessGoto(string Label, string ConditionVariable) {
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
                return true;
            }
            // Run the goto if the condition is true
            else if (Variables.TryGetValue(ConditionVariable, out object? ConditionValue)) {
                if (Variables[ConditionVariable].Equals(true)) {
                    return true;
                }
                else if (Variables[ConditionVariable].Equals(false) == false) {
                    Error($"{ConditionVariable} is not boolean.");
                }
            }
            else {
                Error($"no boolean variable found with identifier '{ConditionVariable}'.");
            }
            return false;
        }

        private void Reset() {
            Variables.Clear();
            LastGotoCallLines.Clear();
        }

        private object? InitialiseValueFromString(string Value) {
            // String
            if (Value.StartsWith('~')) {
                return Value.Substring(1).Replace('~', ' ').Replace("\\n", "\n").Replace("\\h", "#");
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
            else if (IsVariableOrLabelNameValid(Value)) {
                return null;
            }
            // Unknown
            else {
                Error($"unknown data type of value '{Value}'.");
                return null;
            }
        }

        private object AddValues(object Value1, object Value2) {
            // Add string
            if (Value1.GetType() == typeof(string) || Value2.GetType() == typeof(string)) {
                return Value1.ToString() + Value2.ToString();
            }
            // Add double and double
            else if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                return (double)Value1 + (double)Value2;
            }
            // Error
            else {
                Error($"cannot add objects of type {Value1.GetType()} and {Value2.GetType()}.");
                return false;
            }
        }
        private object SubtractValues(object Value1, object Value2) {
            // Subtract double and double
            if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                return (double)Value1 - (double)Value2;
            }
            // Error
            else {
                Error($"cannot subtract values of type '{Value1.GetType()}' and '{Value2.GetType()}'");
                return false;
            }
        }
        private object MultiplyValues(object Value1, object Value2) {
            // Multiply double and double
            if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                return (double)Value1 * (double)Value2;
            }
            // Error
            else {
                Error($"cannot multiply values of type '{Value1.GetType()}' and '{Value2.GetType()}'.");
                return false;
            }
        }
        private object DivideValues(object Value1, object Value2) {
            // Divide double and double
            if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                return (double)Value1 / (double)Value2;
            }
            // Error
            else {
                Error($"cannot divide values of type '{Value1.GetType()}' and '{Value2.GetType()}'.");
                return false;
            }
        }
        private object ModuloValues(object Value1, object Value2) {
            // Modulo double and double
            if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                return (double)Value1 % (double)Value2;
            }
            // Error
            else {
                Error($"cannot modulo values of type '{Value1.GetType()}' and '{Value2.GetType()}'.");
                return false;
            }
        }
        private object ExponentiateValues(object Value1, object Value2) {
            // Modulo double and double
            if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                return Math.Pow((double)Value1, (double)Value2);
            }
            // Error
            else {
                Error($"cannot exponentiate values of type '{Value1.GetType()}' and '{Value2.GetType()}'.");
                return false;
            }
        }

        private bool CompareValues(object? Value1, string ComparisonOperator, object? Value2) {
            bool ComparisonResult = false;
            // Compare null equality
            if (Value1 == null || Value2 == null) {
                if (ComparisonOperator == "==") {
                    ComparisonResult = Value1 == Value2;
                }
                else if (ComparisonOperator == "!=") {
                    ComparisonResult = Value1 != Value2;
                }
                else {
                    Error($"cannot compare null values with operator '{ComparisonOperator}'.");
                }
            }
            // Compare equality
            else if (ComparisonOperator == "==") {
                ComparisonResult = Value1.Equals(Value2);
            }
            else if (ComparisonOperator == "!=") {
                ComparisonResult = !Value1.Equals(Value2);
            }
            // Arithmetic operators
            else if (ComparisonOperator == ">" || ComparisonOperator == "<" || ComparisonOperator == ">=" || ComparisonOperator == "<=") {
                if (Value1.GetType() == typeof(double) && Value2.GetType() == typeof(double)) {
                    // Greater than
                    if (ComparisonOperator == ">") {
                        ComparisonResult = (double)Value1 > (double)Value2;
                    }
                    // Less than
                    else if (ComparisonOperator == "<") {
                        ComparisonResult = (double)Value1 < (double)Value2;
                    }
                    // Greater than or equal to
                    else if (ComparisonOperator == ">=") {
                        ComparisonResult = (double)Value1 >= (double)Value2;
                    }
                    // Less than or equal to
                    else if (ComparisonOperator == "<=") {
                        ComparisonResult = (double)Value1 <= (double)Value2;
                    }
                }
                else {
                    Error($"cannot compare types '{Value1.GetType()}' and '{Value2.GetType()}' with operator '{ComparisonOperator}'.");
                }
            }
            // Error
            else {
                Error($"unknown comparison operator: '{ComparisonOperator}'.");
            }
            return ComparisonResult;
        }

        private static bool IsVariableOrLabelNameValid(string VariableName) {
            for (int i = 0; i < VariableName.Length; i++) {
                if (char.IsLetter(VariableName[i]) || (i != 0 && char.IsDigit(VariableName[i]))) {
                    continue;
                }
                return false;
            }
            return true;
        }

        internal void Error(string? Message = null) {
            BuiltInMethods.Error(CurrentLine, Message);
        }
    }
}