// Copyright (c) Microsoft Corporation. All rights reserved.


using System;


namespace gpcc
{
	public enum PrecType { left, right, nonassoc };
 
	public class Precedence
	{
		public PrecType type;
		public int prec;

		public Precedence(PrecType type, int prec)
		{
			this.type = type;
			this.prec = prec;
		}

		public static void Calculate(Production p)
		{
			// Precedence of a production is that of its rightmost terminal
			// unless explicitly labelled with %prec

			if (p.prec == null)
				for (int i = p.rhs.Count - 1; i >= 0; i--)
					if (p.rhs[i] is Terminal)
					{
						p.prec = ((Terminal)p.rhs[i]).prec;
						break;
					}
		}
	}
}