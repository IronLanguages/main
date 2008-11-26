// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;


namespace gpcc
{
	public class LALRGenerator: LR0Generator
	{
		public LALRGenerator(Grammar grammar): base(grammar)
		{
		}


		public void ComputeLookAhead()
		{
			ComputeDRs();
			ComputeReads();
			ComputeIncludes();
			ComputeFollows();
			ComputeLA();
		}


		private void ComputeDRs()
		{
			// DR(p,A) = { t | p -> A -> r -> t -> ? }

			foreach (State p in states)
				foreach (Transition pA in p.nonTerminalTransitions.Values)
					pA.DR = pA.next.terminalTransitions;
		}


		private Stack<Transition> S;

		// DeRemer and Pennello algorithm to compute Reads
		private void ComputeReads()
		{
			S = new Stack<Transition>();

			foreach (State ps in states)
				foreach (Transition x in ps.nonTerminalTransitions.Values)
					x.N = 0;

			foreach (State ps in states)
				foreach (Transition x in ps.nonTerminalTransitions.Values)
					if (x.N == 0)
						TraverseReads(x, 1);
		}


		private void TraverseReads(Transition x, int k)
		{
			S.Push(x);
			x.N = k;
			x.Read = new Set<Terminal>(x.DR);

			// foreach y such that x reads y
			foreach (Transition y in x.next.nonTerminalTransitions.Values)
				if (y.A.IsNullable())
				{
					if (y.N == 0)
						TraverseReads(y, k + 1);

					if (y.N < x.N)
						x.N = y.N;

					x.Read.AddRange(y.Read);
				}

			if (x.N == k)
				do
				{
					S.Peek().N = int.MaxValue;
					S.Peek().Read = new Set<Terminal>(x.Read);
				} while (S.Pop() != x);
		}


		private void ComputeIncludes()
		{
			// (p,A) include (q,B) iff B -> Beta A Gamma and Gamma => empty and q -> Beta -> p

			foreach (State q in states)
				foreach (Transition qB in q.nonTerminalTransitions.Values)
					foreach (Production prod in qB.A.productions)
					{
						for (int i = prod.rhs.Count - 1; i >= 0; i--)
						{
							Symbol A = prod.rhs[i];
							if (A is NonTerminal)
							{
								State p = PathTo(q, prod, i);
								p.nonTerminalTransitions[(NonTerminal)A].includes.Add(qB);
							}

							if (!A.IsNullable())
								break;
						}
					}
		}


		private State PathTo(State q, Production prod, int prefix)
		{
			// q -> prod.rhs[0] ... prod.rhs[prefix] -> ???

			for (int i = 0; i < prefix; i++)
			{
				Symbol s = prod.rhs[i];
				if (q.Goto.ContainsKey(s))
					q = q.Goto[s];
				else
					return null;
			}

			return q;
		}


		// DeRemer and Pennello algorithm to compute Follows
		private void ComputeFollows()
		{
			S = new Stack<Transition>();

			foreach (State ps in states)
				foreach (Transition x in ps.nonTerminalTransitions.Values)
					x.N = 0;

			foreach (State ps in states)
				foreach (Transition x in ps.nonTerminalTransitions.Values)
					if (x.N == 0)
						TraverseFollows(x, 1);
		}


		private void TraverseFollows(Transition x, int k)
		{
			S.Push(x);
			x.N = k;
			x.Follow = new Set<Terminal>(x.Read);

			foreach (Transition y in x.includes)
				if (x != y)
				{
					if (y.N == 0)
						TraverseFollows(y, k + 1);

					if (y.N < x.N)
						x.N = y.N;

					x.Follow.AddRange(y.Follow);
				}

			if (x.N == k)
				do
				{
					S.Peek().N = int.MaxValue;
					S.Peek().Follow = new Set<Terminal>(x.Follow);
				} while (S.Pop() != x);
		}


		private void ComputeLA()
		{
			// LA(q, A->w) = Union { Follow(p,A) | p -> w -> q }

			foreach (State q in states)
			{
				foreach (ProductionItem item in q.all_items)
				{
					if (item.isReduction())
					{
						item.LA = new Set<Terminal>();
						foreach (State p in states)
							if (PathTo(p, item.production, item.pos) == q)
							{
								NonTerminal A = item.production.lhs;
								if (p.nonTerminalTransitions.ContainsKey(A))
								{
									Transition pA = p.nonTerminalTransitions[A];
									item.LA.AddRange(pA.Follow);
								}
							}
					}
				}
			}
		}
	}
}