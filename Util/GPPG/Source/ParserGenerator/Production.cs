// Copyright (c) Microsoft Corporation. All rights reserved.


using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;


namespace gpcc
{
	public class Production
	{
		public int num;
		public readonly NonTerminal/*!*/ lhs;
        public readonly List<Symbol>/*!*/ rhs = new List<Symbol>();
		public SemanticAction semanticAction;
		public Precedence prec = null;

		public Production(NonTerminal/*!*/ lhs)
		{
            Debug.Assert(lhs != null);
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