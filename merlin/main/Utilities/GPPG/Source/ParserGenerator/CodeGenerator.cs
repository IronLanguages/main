// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;


namespace gpcc
{
    public class CodeGenerator
    {
		public Grammar grammar;

        public void Generate(List<State> states, Grammar grammar)
        {
            StreamWriter tWrtr = null;
            StreamWriter sWrtr = null;
            TextWriter   save = Console.Out;

			this.grammar = grammar;
            if (grammar.OutFName != null)
            {
                try
                {
                    FileStream fStrm = new FileStream(grammar.OutFName, FileMode.Create);
                    sWrtr = new StreamWriter(fStrm);
                    Console.WriteLine("GPPG: sending output to {0}", grammar.OutFName);
                    Console.SetOut(sWrtr);
                }
                catch (IOException x)
                {
                    Console.Error.WriteLine("GPPG: Error. File redirect failed");
                    Console.Error.WriteLine(x.Message);
                    Console.Error.WriteLine("GPPG: Terminating ...");
                    Environment.Exit(1);
                }
            }

            if (grammar.TokFName != null) // generate token list file
            {
                try
                {
                    FileStream fStrm = new FileStream(grammar.TokFName, FileMode.Create);
                    tWrtr = new StreamWriter(fStrm);
                    tWrtr.WriteLine("// Symbolic tokens for parser");
                }
                catch (IOException x)
                {
                    Console.Error.WriteLine("GPPG: Error. Failed to create token namelist file");
                    Console.Error.WriteLine(x.Message);
                    tWrtr = null;
                }
            }

            InsertCode(grammar.header);

            if (grammar.Namespace != null)
            {
                Console.WriteLine("namespace {0}", grammar.Namespace);
                Console.WriteLine("{");
            }

			GenerateTokens(grammar.terminals, tWrtr);

            GenerateClassHeader(grammar.ParserName);
            InsertCode(grammar.prologCode);
            GenerateInitializeMethod(states, grammar.productions);
            GenerateInitializeMetadata(grammar.productions, grammar.nonTerminals);
            GenerateActionMethod(grammar.productions);
            InsertCode(grammar.epilogCode);
            GenerateClassFooter();

            if (grammar.Namespace != null)
                Console.WriteLine("}");

            if (tWrtr != null)
            {
                tWrtr.WriteLine("// End symbolic tokens for parser");
                tWrtr.Close(); // Close the optional token name stream
            }

            if (sWrtr != null)
            {
                Console.SetOut(save);
                sWrtr.Close();
            }
        }

        private void GenerateTokens(Dictionary<string, Terminal> terminals, StreamWriter writer)
        {
            Console.WriteLine("{0} enum {1} {{", grammar.Visibility, grammar.TokenName);
            bool first = true;
            foreach (Terminal terminal in terminals.Values)
                if (terminal.symbolic)
                {
                    if (!first) 
                    {
                        Console.Write(", ");
                    }

                    if (terminal.num % 8 == 1)
                    {
                        Console.WriteLine();
                        Console.Write("    ");
                    }
                    Console.Write("{0} = {1}", terminal, terminal.num);
                    
                    first = false;
                    if (writer != null) 
                    {
                        writer.WriteLine("\t{0}.{1} /* {2} */", grammar.TokenName, terminal.ToString(), terminal.num);
                    }
                }

            Console.WriteLine("};");
            Console.WriteLine();
        }

		private void GenerateValueType()
		{
			if (grammar.unionType != null)
			{
                if (grammar.ValueTypeName == null)
                    // we have a "union" type declared, but no type name declared.
                    grammar.ValueTypeName = Grammar.DefaultValueTypeName;
				Console.WriteLine("{0}{1} struct {2}", 
                    grammar.Visibility, grammar.PartialMark, grammar.ValueTypeName);
				InsertCode(grammar.unionType);
			}
			else if (grammar.ValueTypeName == null)
				grammar.ValueTypeName = "int";
            // else we have a value type name declared, but no "union"
		}

        private void GenerateScannerBaseClass()
        {
            Console.WriteLine("// Abstract base class for GPLEX scanners");
            Console.WriteLine("public abstract class ScanBase : IScanner<{0},{1}> {2}",
                grammar.ValueTypeName, grammar.LocationTypeName, "{ }");
            Console.WriteLine();
        }

		private void GenerateClassHeader(string name)
        {
            GenerateValueType();
            if (GPCG.FORGPLEX) GenerateScannerBaseClass();
            Console.WriteLine("{0}{1} class {2}", grammar.Visibility, grammar.PartialMark, name);
            Console.WriteLine("{");
        }


        private void GenerateClassFooter()
        {
            Console.WriteLine("}");
        }


        private void GenerateInitializeMethod(List<State> states, List<Production> productions)
        {
            Console.WriteLine("  private void InitializeGeneratedTables(ParserTables tables)");
            Console.WriteLine("  {");

            Console.WriteLine("    tables.ErrorToken = (int){0}.Error;", grammar.TokenName);
            Console.WriteLine("    tables.EofToken = (int){0}.EndOfFile;", grammar.TokenName);
            Console.WriteLine();

            Console.WriteLine("    tables.States = BuildStates(new short[] {");
            
            GenerateStates(states);
            
            Console.WriteLine("    });");
            Console.WriteLine();
            
            Console.Write("    tables.Rules = new int[] {");

            for (int i = 0; i < productions.Count; i++) {
                Debug.Assert(i == productions[i].num - 1);
                Console.Write(((-productions[i].lhs.num) << 16) | productions[i].rhs.Count);
                
                Console.Write(", ");
            }

            Console.WriteLine("};");

            Console.WriteLine("  }");
			Console.WriteLine();
        }

        private void GenerateInitializeMetadata(List<Production> productions, Dictionary<string, NonTerminal> nonTerminals) {
            // TODO: parameterize #if symbol
            Console.WriteLine("#if DEBUG");
            Console.WriteLine("  private void InitializeMetadata(ParserTables tables) {");

            // non-terminal names:
            Console.WriteLine("    tables.NonTerminalNames = new string[] {\"\", ");
            int length = 37;
            foreach (NonTerminal nonTerminal in nonTerminals.Values)
            {
                string ss = String.Format("\"{0}\", ", nonTerminal.ToString());
                length += ss.Length;
                Console.Write(ss);
                if (length > 70)
                {
                    Console.WriteLine();
                    Console.Write("      ");
                    length = 0;
                }
            }
            Console.WriteLine("    };");

            // rule RHS symbols:
            Console.WriteLine("    tables.RuleRhsSymbols = new short[] {");

            for (int i = 0; i < productions.Count; i++) {
                Console.Write("        ");
                for (int j = 0; j < productions[i].rhs.Count; j++) {
                    Console.Write(productions[i].rhs[j].num);
                    Console.Write(", ");
                }
                Console.WriteLine("// {0}", productions[i].num);
            }

            Console.WriteLine("    };");
            Console.WriteLine("  }");
            Console.WriteLine("#endif");
            Console.WriteLine();
        }


        private void GenerateStates(List<State> states) {
            Console.WriteLine("      {0},", states.Count);
            
            // states are numbered sequentially starting from 0:
            for (int i = 0; i < states.Count; i++) {
                Debug.Assert(i == states[i].num);
                GenerateState(states[i]);
            }
        }

        private void GenerateState(State state)
        {
            int defaultAction = state.GetDefaultAction();

            Console.Write("      ");

            // default action is always < 0 (reduction)
            if (defaultAction == 0 || state.nonTerminalTransitions.Count > 0) {

                // actions:
                if (defaultAction == 0) {
                    Console.Write("{0},", state.parseTable.Count);
                } else {
                    Console.Write("0,");
                }

                // gotos:
                Console.Write("{0},", state.nonTerminalTransitions.Count);
            }

            if (defaultAction == 0) {
                Console.Write(" /* actions: */ ");
                foreach (KeyValuePair<Terminal, ParserAction> transition in state.parseTable) {
                    Console.Write("{0},{1},", transition.Key.num, transition.Value.ToNum());
                }
            } else {
                Console.Write(" /* default action: */ ");
                Console.Write("{0},", defaultAction);
            }

            if (state.nonTerminalTransitions.Count > 0) {
                Console.Write(" /* gotos: */ ");
                foreach (Transition transition in state.nonTerminalTransitions.Values) {
                    Console.Write("{0},{1},", transition.A.num, transition.next.num);
                }
            }

            Console.WriteLine();
        }

        private void GenerateRuleRhs(Production production) {
            Console.Write("      tables.Rules[{0}].Rhs = new int[] {{", production.num, production.lhs.num);
            bool first = true;
            foreach (Symbol sym in production.rhs) {
                if (!first)
                    Console.Write(",");
                else
                    first = false;
                Console.Write("{0}", sym.num);
            }
            Console.WriteLine("};");
        }

        private void GenerateActionMethod(List<Production> productions)
        {
            // TODO: parameterize; it seems that 0 is optimal though
            const int GroupSizeLog = 0;

            int groupSize = 1 << GroupSizeLog;
            int mask = groupSize - 1;
            int groupCount = (productions.Count >> GroupSizeLog) + ((productions.Count & mask) != 0 ? 1 : 0);

            Console.WriteLine("  private void DoAction(int action)");
            Console.WriteLine("  {");

            List<int> nonEmptyProductionCounts = new List<int>();

            for (int g = 0; g < groupCount; g++) {
                int nonEmptyCount = 0;
                for (int i = 0; i < groupSize; i++) {
                    int index = (g << GroupSizeLog) + i;
                    if (index >= productions.Count) break;

                    // empty rhs with no semantic action must be present in switch (the default action should not be taken);
                    if (productions[index].semanticAction != null || productions[index].rhs.Count == 0) {
                        nonEmptyCount++;
                    }
                }
                nonEmptyProductionCounts.Add(nonEmptyCount);
            }

            Debug.Assert(nonEmptyProductionCounts.Count == groupCount);
            
            if (groupCount > 1) {
                if (groupSize > 1) {
                    Console.WriteLine("    int mod = action & 0x{0:X};", mask);
                    Console.WriteLine("    switch (action  >> {0})", GroupSizeLog);
                } else {
                    Console.WriteLine("    switch (action)", GroupSizeLog);
                }

                Console.WriteLine("    {");
                Console.WriteLine("      default: DoDefaultAction(); return;", GroupSizeLog);

                for (int g = 0; g < groupCount; g++) {
                    if (nonEmptyProductionCounts[g] > 0) {
                        Console.WriteLine("      case {0}: _{0}({1}); return;", g, groupSize > 1 ? "mod" : "");
                    }
                }

                Console.WriteLine("    }");
            } else {
                Console.WriteLine("    _0(action);");
            }

            Console.WriteLine("  }");
            Console.WriteLine();

            for (int g = 0; g < groupCount; g++) {
                if (nonEmptyProductionCounts[g] > 0) {
                    Console.WriteLine("  private void _{0}({1})", g, groupSize > 1 ? "int mod" : "");
                    Console.WriteLine("  {");

                    if (groupSize > 1) {
                        Console.WriteLine("    switch (mod)");
                        Console.WriteLine("    {");
                    }

                    for (int i = 0; i < groupSize; i++) {
                        int index = (g << GroupSizeLog) + i;
                        if (index >= productions.Count) break;

                        Production production = productions[index];
                        Debug.Assert(index == production.num - 1);

                        if (production.semanticAction != null || productions[index].rhs.Count == 0) {
                            if (groupSize > 1) {
                                Console.WriteLine("      case {0}:", i);
                            }

                            Console.WriteLine("      // " + production.ToString());

                            if (production.semanticAction != null) {
                                production.semanticAction.GenerateCode(this);
                            }

                            if (groupSize > 1) {
                                Console.WriteLine("        return;");
                            }
                        }
                    }

                    if (groupSize > 1) {
                        Console.WriteLine("      default: DoDefaultAction(); return;");
                        Console.WriteLine("    }");
                    }

                    Console.WriteLine("  }");
                    Console.WriteLine();
                }
            }
        }

        private void InsertCode(string code)
        {
            if (code != null)
            {
                StringReader reader = new StringReader(code);
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    Console.WriteLine("{0}", line);
                }
            }
        }
    }
}







