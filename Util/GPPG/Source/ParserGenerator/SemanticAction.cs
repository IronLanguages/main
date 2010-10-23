// Copyright (c) Microsoft Corporation. All rights reserved.


using System;
using System.Text;


namespace gpcc
{
    public class SemanticAction
    {
        private Production production;
        private int pos;
        private string commands;


        public SemanticAction(Production production, int pos, string commands)
        {
            this.production = production;
            this.pos = pos;
            this.commands = commands;
        }

        void ErrReport(string tag, string msg)
        {
            Console.Error.Write(tag); Console.Error.Write(": ");  Console.Error.WriteLine(msg);
        }


        public void GenerateCode(CodeGenerator codeGenerator)
        {
            int i = 0;
            string lineTag = "";

            if (commands.StartsWith("#line")) {
                lineTag = commands.Substring(0, commands.IndexOf('"')).Trim();
            }

            while (i < commands.Length)
            {
                switch (commands[i])
                {
                    case '/':
                        Output(i++);
                        if (commands[i] == '/') // C++ style comments
                        {
                            while (i < commands.Length && commands[i] != '\n')
                                Output(i++);
                            if (i < commands.Length)
                                Output(i++);
                        }
                        else if (commands[i] == '*') // C style comments
                        {
                            Output(i++);
                            do
                            {
                                while (i < commands.Length && commands[i] != '*')
                                    Output(i++);
                                if (i < commands.Length)
                                    Output(i++);
                            } while (i < commands.Length && commands[i] != '/');
                            if (i < commands.Length)
                                Output(i++);
                        }
                        break;

                    case '"':       // start of string literal
                        Output(i++);
                        while (i < commands.Length && commands[i] != '"')
                        {
                            if (commands[i] == '\\')
                                Output(i++);
                            if (i < commands.Length)
                                Output(i++);
                        }
                        if (i < commands.Length)
                            Output(i++);
                        break;

                    case '@':		
                        // Possible start of verbatim string literal
                        // but also possible location marker access
                        if (i + 1 < commands.Length)
                        {
                            char la = commands[i + 1]; // lookahead character
                            if (la == '$')
                            {
                                i += 2; // read past '@', '$'
                                Console.Write("yyloc");
                            }
                            else if (Char.IsDigit(la))
                            {
                                int num = (int)la - (int)'0';
                                i += 2; // read past '@', digit
                                if (i < commands.Length && char.IsDigit(commands[i]))
                                    ErrReport(lineTag, "Only indexes up \"$9\" allowed");
                                else if (num > this.production.rhs.Count)
                                    ErrReport(lineTag, String.Format("Index @{0} is out of bounds", num));
                                Console.Write("GetLocation({0})", pos - num + 1);
                            }
                            else
                            {
                                Output(i++);
                                if (la == '"')
                                {
                                    Output(i++);
                                    while (i < commands.Length && commands[i] != '"')
                                        Output(i++);
                                    if (i < commands.Length)
                                        Output(i++);
                                    break;
                                }
                            }
                        }
                        else
                            ErrReport(lineTag, "Invalid use of '@'");
                        break;

                    case '\'':      // start of char literal
                        Output(i++);
                        while (i < commands.Length && commands[i] != '\'')
                        {
                            if (commands[i] == '\\')
                                Output(i++);
                            if (i < commands.Length)
                                Output(i++);
                        }
                        if (i < commands.Length)
                            Output(i++);
                        break;

                    case '$':       // $$ or $n placeholder
                        i++;
                        if (i < commands.Length)
                        {
                            string kind = null;
                            if (commands[i] == '<') // $<kind>n
                            {
                                i++;
                                StringBuilder builder = new StringBuilder();
                                while (i < commands.Length && commands[i] != '>')
                                {
                                    builder.Append(commands[i]);
                                    i++;
                                }
                                if (i < commands.Length)
                                {
                                    i++;
                                    kind = builder.ToString();
                                }
                            }

                            if (commands[i] == '$')
                            {
                                i++;
                                if (kind == null)
                                    kind = production.lhs.kind;

                                Console.Write("yyval");

                                if (kind != null)
                                    Console.Write(".{0}", kind);
                            }
                            else if (char.IsDigit(commands[i]))
                            {
                                int num = commands[i] - '0';
                                i++;
                                if (i < commands.Length && char.IsDigit(commands[i]))
                                    ErrReport(lineTag, "Only indexes up \"$9\" allowed");
                                else if (num > this.production.rhs.Count)
                                    ErrReport(lineTag, String.Format("Index ${0} is out of bounds", num));
                                if (kind == null)
                                    kind = production.rhs[num - 1].kind;

                                Console.Write("GetValue({0})", pos - num + 1);

                                if (kind != null)
                                    Console.Write(".{0}", kind);
                            }
                        }
                        else
                            ErrReport(lineTag, "Unexpected '$'");
                        break;

                    default:
                        Output(i++);
                        break;
                }
            }
            Console.WriteLine();
        }


        private void Output(int i)
        {
            if (commands[i] == '\n')
                Console.WriteLine();
            else
                Console.Write(commands[i]);
        }
    }
}