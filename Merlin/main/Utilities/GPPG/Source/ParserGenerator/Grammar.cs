// Copyright (c) Microsoft Corporation. All rights reserved.




using System;
using System.Collections.Generic;


namespace gpcc
{
	public class Grammar
	{
        public const string DefaultValueTypeName = "ValueType";

		public List<Production> productions = new List<Production>();
        public string unionType;
		public int NumActions = 0;
        public string header;	// before first %%
        public string prologCode;	// between %{ %} 
        public string epilogCode;	// after last %%
		public NonTerminal startSymbol;
		public Production rootProduction;
		public Dictionary<string, NonTerminal> nonTerminals = new Dictionary<string, NonTerminal>();
		public Dictionary<string, Terminal> terminals = new Dictionary<string, Terminal>();
        
        public bool IsPartial = false;
        public string OutFName = null;
        public string TokFName = null;
        public string Namespace;
        public string Visibility = "public";
        public string ParserName = "Parser";
        public string TokenName = "Tokens";
        public string ValueTypeName = null;
        public string LocationTypeName = "LexLocation";
        public string PartialMark { get { return (IsPartial ? " partial" : ""); } }


        public Grammar()
        {
            LookupTerminal(GrammarToken.Symbol, "NONE");
            LookupTerminal(GrammarToken.Symbol, "ERROR");
            LookupTerminal(GrammarToken.Symbol, "END_OF_FILE");
        }


		public Terminal LookupTerminal(GrammarToken token, string name)
		{
			if (!terminals.ContainsKey(name))
				terminals[name] = new Terminal(token == GrammarToken.Symbol, name);

			return terminals[name];
		}


		public NonTerminal LookupNonTerminal(string name)
		{
			if (!nonTerminals.ContainsKey(name))
				nonTerminals[name] = new NonTerminal(name);

			return nonTerminals[name];
		}


		public void AddProduction(Production production)
		{
			productions.Add(production);
			production.num = productions.Count;
		}


		public void CreateSpecialProduction(NonTerminal root)
		{
			rootProduction = new Production(LookupNonTerminal("$accept"));
			AddProduction(rootProduction);
			rootProduction.rhs.Add(root);
            rootProduction.rhs.Add(LookupTerminal(GrammarToken.Symbol, "END_OF_FILE"));
		}

        void MarkReachable()
        {
            Stack<NonTerminal> work = new Stack<NonTerminal>();
            rootProduction.lhs.reached = true; // by definition.
            work.Push(startSymbol);
            startSymbol.reached = true;
            while (work.Count > 0)
            {
                NonTerminal nonT = work.Pop();
                foreach (Production prod in nonT.productions)
                {
                    foreach (Symbol smbl in prod.rhs)
                    {
                        NonTerminal rhNt = smbl as NonTerminal;
                        if (rhNt != null && !rhNt.reached)
                        {
                            rhNt.reached = true;
                            work.Push(rhNt);
                        }
                    }
                }
            }
        }

        public bool CheckGrammar()
        {
            bool ok = true;
            NonTerminal nt;
            MarkReachable();
            foreach (KeyValuePair<string, NonTerminal> pair in nonTerminals)
            {
                nt = pair.Value;
                if (!nt.reached)
                    Console.Error.WriteLine(
                        "WARNING: NonTerminal symbol \"{0}\" is unreachable", pair.Key);

                if (nt.productions.Count == 0)
                {
                    ok = false;
                    Console.Error.WriteLine(
                        "FATAL: NonTerminal symbol \"{0}\" has no productions", pair.Key);
                }
            }
            return ok;    
        }
	}
}







