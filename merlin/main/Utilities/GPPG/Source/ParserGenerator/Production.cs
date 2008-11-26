// Copyright (c) Microsoft Corporation. All rights reserved.


using System;
using System.Text;
using System.Collections.Generic;


namespace gpcc
{
	public class Production
	{
		public int num;
		public NonTerminal lhs;
		public List<Symbol> rhs = new List<Symbol>();
		public SemanticAction semanticAction;
		public Precedence prec = null;


		public Production(NonTerminal lhs)
		{
			this.lhs = lhs;
			lhs.productions.Add(this);
		}


		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendFormat("{0} -> ", lhs);
			foreach (Symbol s in rhs)
				builder.AppendFormat("{0} ", s);

			return builder.ToString();
		}
	}
}