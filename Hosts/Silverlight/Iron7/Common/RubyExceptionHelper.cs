using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Scripting.Hosting;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IronRuby.Runtime;
using Microsoft.Scripting;

namespace Iron7.Common
{
    public abstract class RubyExceptionHelperBase
    {
        protected static string SafeGetLine(string[] codePerLine, int lineNumber)
        {
            // line numbers are 1-based
            if (lineNumber > 0 && lineNumber <= codePerLine.Length)
                return codePerLine[lineNumber - 1].Trim();

            return "?";
        }

        public abstract string LongErrorText(string code);
    }

    public class RubySyntaxExceptionHelper : RubyExceptionHelperBase
    {
        private SyntaxErrorException exception;

        public RubySyntaxExceptionHelper(SyntaxErrorException exception)
        {
            this.exception = exception;
        }

        public override string LongErrorText(string code)
        {
            var toReturn = new StringBuilder();
            toReturn.AppendLine("Error:");
            toReturn.AppendLine(exception.GetType().Name);
            toReturn.AppendLine();
            toReturn.AppendLine("Message:");
            toReturn.AppendLine(exception.Message);
            toReturn.AppendLine();
            toReturn.AppendLine("Location:");
            toReturn.AppendLine(exception.RawSpan.ToString());
            toReturn.AppendLine();
            toReturn.AppendLine("Near:");
            var codePerLine = code.Split('\n');
            toReturn.AppendLine(SafeGetLine(codePerLine, exception.Line));
            return toReturn.ToString();
        }
    }

    public class RubyExceptionHelper : RubyExceptionHelperBase
    {
        public class LineAndMethod
        {
            public int LineNumber { get; set; }
            public string MethodName { get; set; }
        }

        public string MessageType { get; protected set; }
        public string MessageBrief { get; protected set; }
        public List<LineAndMethod> StackEntries { get; private set; }
        public string FullStack { get; private set; }

        public RubyExceptionHelper(ScriptEngine engine, Exception exc)
        {
            Parse(engine, exc);
        }

        private void Parse(ScriptEngine engine, Exception exc)
        {
            var service = engine.GetService<Microsoft.Scripting.Hosting.ExceptionOperations>();
            string briefMessage;
            string errorTypeName;
            service.GetExceptionMessage(exc, out briefMessage, out errorTypeName);
            var formattedMessage = service.FormatException(exc);
            /*
            var stackFrames = service.GetStackFrames(exc);
            var ctxt = Microsoft.Scripting.Hosting.Providers.HostingHelpers.GetLanguageContext(engine);
            var sink = ctxt.GetCompilerErrorSink();
            var rubyContext = (IronRuby.Runtime.RubyContext)ctxt;
            RubyExceptionData data = RubyExceptionData.GetInstance(exc);
            IronRuby.Builtins.RubyArray backtrace = data.Backtrace;
            var s = 12;
            */

            switch (errorTypeName)
            {
                case "TypeInitializationException":
                    MessageType = "Sorry - Iron7 is unable to use dynamic ruby delegates";
                    break;
                default:
                    MessageType = errorTypeName;
                    break;
            }
            MessageBrief = briefMessage;

            var lines = formattedMessage.Replace("\r","").Split('\n');
            AttemptParseLineNumber(lines);
            CaptureRelevantStack(lines);
        }

        private void CaptureRelevantStack(string[] lines)
        {
            StringBuilder output = new StringBuilder();
            foreach (var l in lines)
            {
                if (l.EndsWith("`GenericScriptAction'"))
                    break;
                output.AppendLine(l);
            }
            FullStack = output.ToString();
        }

        const string MagicEvalLine = "from .eval.:(?<lineNumber>\\d+):in `(?<methodName>.*?)'";
        private static Regex MagicEvalRegex = new Regex(MagicEvalLine); //, RegexOptions..Compiled);
        private bool TryParseLineNumber(string line, out LineAndMethod lineAndMethod)
        {
            lineAndMethod = null;
            var match = MagicEvalRegex.Match(line);
            if (false == match.Success)
                return false;

            int lineNumber = -1;
            if (false == int.TryParse(match.Groups[1].Value, out lineNumber))
                return false;

            lineAndMethod = new LineAndMethod()
            {
                LineNumber = lineNumber,
                MethodName = match.Groups[2].Value
            };

            return true;
            /*
            var trimmed = line.Trim();
            if (false == trimmed.StartsWith(MagicEvalLine))
                return false;

            var nextColon = trimmed.IndexOf(':', MagicEvalLine.Length);
            if (nextColon < 0)
                return false;

            trimmed = trimmed.Substring(MagicEvalLine.Length, nextColon - MagicEvalLine.Length);
            return int.TryParse(trimmed, out lineNumber);
             */
        }

        private void AttemptParseLineNumber(string[] lines)
        {
            StackEntries = new List<LineAndMethod>();
            foreach (var l in lines)
            {
                LineAndMethod lineAndMethod;
                if (TryParseLineNumber(l, out lineAndMethod))
                {
                    StackEntries.Add(lineAndMethod);
                }
            }
        }

        public override string LongErrorText(string code)
        {
            var toReturn = new StringBuilder();
            toReturn.AppendLine("Error:");
            toReturn.AppendLine(this.MessageType);
            toReturn.AppendLine();
            var codePerLine = code.Split('\n');
            if (StackEntries.Count > 0)
            {
                toReturn.AppendLine("Stack:");
                foreach (var stack in StackEntries)
                {
                    string safeLine = SafeGetLine(codePerLine, stack.LineNumber);
                    toReturn.AppendLine(string.Format(":{0}:{1}:{2}", stack.LineNumber, stack.MethodName, safeLine));
                }
                toReturn.AppendLine();
            }
            toReturn.AppendLine("Message:");
            toReturn.AppendLine(this.MessageBrief);
            toReturn.AppendLine();
            toReturn.AppendLine("Full:");
            toReturn.AppendLine(this.FullStack);

            return toReturn.ToString();
        }


        /*
        private static string SafeGetStartOfLine(string line)
        {
            line = line.Trim();

            if (line.Length < 15)
            {
                return line;
            }
            else
            {
                return line.Substring(0, 13).Trim() + "...";
            }
        }
        */
    }
}
