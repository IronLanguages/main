// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;


namespace gpcc
{
	public class Transition
	{
		public int N;
		public State p;
		public NonTerminal A;
		public State next;

		public Set<Terminal> DR;
		public List<Transition> includes = new List<Transition>();
		public Set<Terminal> Read;
		public Set<Terminal> Follow;


		public Transition(State p, NonTerminal A, State next)
		{
			this.p = p;
			this.A = A;
			this.next = next;
		}
	}
}