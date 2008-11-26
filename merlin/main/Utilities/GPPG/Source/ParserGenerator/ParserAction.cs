// Copyright (c) Microsoft Corporation. All rights reserved.

using System;


namespace gpcc
{
	public abstract class ParserAction
	{
        public abstract int ToNum();
	}


	public class Shift : ParserAction
	{
		public State next;

		public Shift(State next)
		{
			this.next = next;
		}

		public override string ToString()
		{
			return "shift, and go to state " + next.num;
		}

        public override int ToNum()
        {
            return next.num;
        }
	}


	public class Reduce : ParserAction
	{
		public ProductionItem item;

		public Reduce(ProductionItem item)
		{
			this.item = item;
		}

		public override string ToString()
		{
			return "reduce using rule " + item.production.num + " (" + item.production.lhs + ")";
		}

        public override int  ToNum()
        {
            return -item.production.num;
        }
	}
}