// Copyright (c) Microsoft Corporation. All rights reserved.


using System;
using System.Collections.Generic;
using System.Text;


namespace gpcc
{
	public abstract class Symbol
	{
		private string name;
		public string kind;

		public abstract int num
		{
			get;
		}


		public Symbol(string name)
		{
			this.name = name;
		}


		public override string ToString()
		{
			return name;
		}


		public abstract bool IsNullable();
	}


	public class Terminal : Symbol
	{
        static int count = 0;
		static int max = 0;

		public Precedence prec = null;
		private int n;
		public bool symbolic;

		public override int num
		{
			get
			{
				if (symbolic)
					return max + n;
				else
					return n;
			}
		}

		public Terminal(bool symbolic, string name)
			: base(symbolic ? name : "'" + name.Replace("\n", @"\n") + "'")
		{
			this.symbolic = symbolic;

			if (symbolic)
				this.n = ++count;
			else
			{
				this.n = (int)name[0];
				if (n > max) max = n;
			}
		}


		public override bool IsNullable()
		{
			return false;
		}

        public override string ToString() {
            string name = base.ToString();

            // any lower case => return as is:
            for (int i = 0; i < name.Length; i++) {
                if (Char.IsLower(name[i])) return name;
            }

            // capitalize otherwise, remove underscores:
            StringBuilder result = new StringBuilder();
            bool doLower = false;
            for (int i = 0, j = 0; i < name.Length; i++, j++) {
                if (name[i] == '_') {
                    doLower = false;
                } else {
                    if (doLower) {
                        result.Append(Char.ToLower(name[i]));
                    } else {
                        result.Append(name[i]);
                        doLower = true;
                    }
                }
            }
            return result.ToString();
        }
    }



    public class NonTerminal : Symbol
	{
        public bool reached = false;
        static int count = 0;
		private int n;
		public List<Production> productions = new List<Production>();


		public NonTerminal(string name)
			: base(name)
		{
            n = ++count;
		}

		public override int num
		{
			get
			{
				return -n;
			}
		}

		private object isNullable;
		public override bool IsNullable()
		{
			if (isNullable == null)
			{
				isNullable = false;
				foreach (Production p in productions)
				{
					bool nullable = true;
					foreach (Symbol rhs in p.rhs)
						if (!rhs.IsNullable())
						{
							nullable = false;
							break;
						}
					if (nullable)
					{
						isNullable = true;
						break;
					}
				}
			}

			return (bool)isNullable;
		}
	}
}