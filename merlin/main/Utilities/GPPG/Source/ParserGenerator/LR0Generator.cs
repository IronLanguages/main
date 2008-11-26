// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;


namespace gpcc
{
	public class LR0Generator
	{
		protected List<State> states = new List<State>();
		protected Grammar grammar;
		private Dictionary<Symbol, List<State>> accessedBy = new Dictionary<Symbol,List<State>>();


		public LR0Generator(Grammar grammar)
		{
			this.grammar = grammar;
		}


        public List<State> BuildStates()
		{
			// create state for root production and expand recursively
			ExpandState(grammar.rootProduction.lhs, new State(grammar.rootProduction));
            
            return states;
		}


		private void ExpandState(Symbol sym, State newState)
		{
			newState.accessedBy = sym;
			states.Add(newState);

			if (!accessedBy.ContainsKey(sym))
				accessedBy[sym] = new List<State>();
			accessedBy[sym].Add(newState);

			newState.AddClosure();
			ComputeGoto(newState);
		}


		private void ComputeGoto(State state)
		{
			foreach (ProductionItem item in state.all_items)
				if (!item.expanded && !item.isReduction())
				{
					item.expanded = true;
					Symbol s1 = item.production.rhs[item.pos];

					// Create itemset for new state ...
					List<ProductionItem> itemSet = new List<ProductionItem>();
					itemSet.Add(new ProductionItem(item.production, item.pos+1));

					foreach (ProductionItem item2 in state.all_items)
						if (!item2.expanded && !item2.isReduction())
						{
							Symbol s2 = item2.production.rhs[item2.pos];

							if (s1 == s2)
							{
								item2.expanded = true;
								itemSet.Add(new ProductionItem(item2.production, item2.pos+1));
							}
						}

					State existingState = FindExistingState(s1, itemSet);

					if (existingState == null)
					{
						State newState = new State(itemSet);
						state.AddGoto(s1, newState);
						ExpandState(s1, newState);
					}
					else
						state.AddGoto(s1, existingState);
				}
		}


		private State FindExistingState(Symbol sym, List<ProductionItem> itemSet)
		{
			if (accessedBy.ContainsKey(sym))
				foreach (State state in accessedBy[sym])
					if (ProductionItem.SameProductions(state.kernal_items, itemSet))
						return state;

			return null;
		}




		public void BuildParseTable()
		{
			foreach (State state in states)
			{
				// Add shift actions ...
				foreach (Terminal t in state.terminalTransitions)
					state.parseTable[t] = new Shift(state.Goto[t]);

				// Add reduce actions ...
				foreach (ProductionItem item in state.all_items)
					if (item.isReduction())
					{
						// Accept on everything
						if (item.production == grammar.rootProduction)
							foreach (Terminal t in grammar.terminals.Values)
								state.parseTable[t] = new Reduce(item);

						foreach (Terminal t in item.LA)
						{
							// possible conflict with existing action
							if (state.parseTable.ContainsKey(t))
							{
								ParserAction other = state.parseTable[t];

								if (other is Reduce)
								{
									Console.Error.WriteLine("Reduce/Reduce conflict, state {0}: {1} vs {2} on {3}",
										state.num, item.production.num, ((Reduce)other).item.production.num, t);

									// choose in favour of production listed first in the grammar
									if (((Reduce)other).item.production.num > item.production.num)
										state.parseTable[t] = new Reduce(item);
								}
								else
								{
									if (item.production.prec != null && t.prec != null)
									{
										if (item.production.prec.prec > t.prec.prec ||
											(item.production.prec.prec == t.prec.prec &&
											 item.production.prec.type == PrecType.left))
										{
											// resolve in favour of reduce (without error)
											state.parseTable[t] = new Reduce(item);
										}
										else
										{
											// resolve in favour of shift (without error)
										}
									}
									else
										Console.Error.WriteLine("Shift/Reduce conflict, state {0} on {1}", state.num, t);
									// choose in favour of the shift
								}
							}
							else
								state.parseTable[t] = new Reduce(item);
						}
					}
			}
		}


		public void Report()
		{
			Console.WriteLine("Grammar");

			NonTerminal lhs = null;
			foreach (Production production in grammar.productions)
			{
				if (production.lhs != lhs)
				{
					lhs = production.lhs;
					Console.WriteLine();
					Console.Write("{0,5} {1}: ", production.num, lhs);
				}
				else
					Console.Write("{0,5} {1}| ", production.num, new string(' ', lhs.ToString().Length));

				for (int i=0; i<production.rhs.Count-1; i++)
					Console.Write("{0} ", production.rhs[i].ToString());

				if (production.rhs.Count > 0)
					Console.WriteLine("{0}", production.rhs[production.rhs.Count-1]);
				else
					Console.WriteLine("/* empty */");
			}

			Console.WriteLine();

			foreach (State state in states)
				Console.WriteLine(state.ToString());
		}
	}
}