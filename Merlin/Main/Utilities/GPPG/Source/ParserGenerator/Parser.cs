// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;


namespace gpcc
{
	public class Parser
	{
		private Grammar grammar;
		private GrammarToken token;
		private Scanner scanner;
        private string baseName;


		public Grammar Parse(string filename)
		{
			scanner = new Scanner(filename);
			grammar = new Grammar();
            baseName = System.IO.Path.GetFileNameWithoutExtension(filename);
            if (GPCG.DEFINES) grammar.TokFName = baseName + ".tokens";
			Advance();
            ParseHeader();
			ParseDeclarations();
			ParseProductions();
			ParseEpilog();
			return grammar;
		}

        private void ParseHeader() 
        {
            grammar.header = scanner.yylval;
            Advance();
        }

		private void ParseDeclarations()
		{
			int prec = 0;

			while (token != GrammarToken.EndOfSection && token != GrammarToken.Eof)
			{
				switch (token)
				{
					case GrammarToken.Prolog:
						{
							grammar.prologCode += scanner.yylval;
							Advance();
							break;
						}

					case GrammarToken.Start:
						{
							Advance();
							if (token == GrammarToken.Symbol)
							{
								grammar.startSymbol = grammar.LookupNonTerminal(scanner.yylval);
								Advance();
							}
							break;
						}

					case GrammarToken.Left:
						{
							Advance();
							prec += 10;
							while (token == GrammarToken.Symbol || token == GrammarToken.Literal)
							{
								Terminal t = grammar.LookupTerminal(token, scanner.yylval);
								t.prec = new Precedence(PrecType.left, prec);
								Advance();
							}
							break;
						}

					case GrammarToken.Right:
						{
							Advance();
							prec += 10;
							while (token == GrammarToken.Symbol || token == GrammarToken.Literal)
							{
								Terminal t = grammar.LookupTerminal(token, scanner.yylval);
								t.prec = new Precedence(PrecType.right, prec);
								Advance();
							}
							break;
						}

					case GrammarToken.NonAssoc:
						{
							Advance();
							prec += 10;
							while (token == GrammarToken.Symbol || token == GrammarToken.Literal)
							{
								Terminal t = grammar.LookupTerminal(token, scanner.yylval);
								t.prec = new Precedence(PrecType.nonassoc, prec);
								Advance();
							}
							break;
						}

					case GrammarToken.Token:
						{
							Advance();
							string kind = null;
							if (token == GrammarToken.Kind)
							{
								kind = scanner.yylval;
								Advance();
							}
							while (token == GrammarToken.Symbol)
							{
                                Terminal terminal = grammar.LookupTerminal(token, scanner.yylval);
								terminal.kind = kind;
								Advance();
							}
							break;
						}
					case GrammarToken.Type:
						{
							Advance();
							string kind = null;
							if (token == GrammarToken.Kind)
							{
								kind = scanner.yylval;
								Advance();
							}
							while (token == GrammarToken.Symbol)
							{
								NonTerminal nonterminal = grammar.LookupNonTerminal(scanner.yylval);
								nonterminal.kind = kind;
								Advance();
							}
							break;
						}
					case GrammarToken.Union:
						{
							grammar.unionType = scanner.yylval;
							Advance();
							break;
						}
                    case GrammarToken.Namespace:
                        {
                            Advance();
                            grammar.Namespace = scanner.yylval;
                            Advance();
                            while (scanner.yylval == ".")
                            {
                                Advance();
                                grammar.Namespace += "." + scanner.yylval;
                                Advance();
                            }
                            break;
                        }

                    case GrammarToken.Output:
                        {
                            grammar.OutFName = scanner.yylval;
                            Advance();
                            break;
                        }
                    case GrammarToken.Defines:
                        {
                            grammar.TokFName = baseName + ".tokens";
                            Advance();
                            break;
                        }
                    case GrammarToken.Partial:
                        {
                            Advance();
                            grammar.IsPartial = true;
                            break;
                        }
                    case GrammarToken.ParserName:
                        {
                            Advance();
                            grammar.ParserName = scanner.yylval;
                            Advance();
                            break;
                        }
                    case GrammarToken.TokenName:
                        {
                            Advance();
                            grammar.TokenName = scanner.yylval;
                            Advance();
                            break;
                        }
                    case GrammarToken.LocationTypeName:
                        {
                            Advance();
                            grammar.LocationTypeName = scanner.yylval;
                            Advance();
                            break;
                        }
                    case GrammarToken.ValueTypeName:
                        {
                            Advance();
                            grammar.ValueTypeName = scanner.yylval;
                            Advance();
                            break;
                        }
                    case GrammarToken.Visibility:
                        {
                            Advance();
                            grammar.Visibility = scanner.yylval;
                            Advance();
                            break;
                        }
                    case GrammarToken.Locations:
                        {
                            Advance(); // does nothing in this version: location tracking is always on.
                            break;
                        }
					default:
						{
							scanner.ReportError("Unexpected token {0} in declaration section", token);
							Advance();
							break;
						}
				}
			}
			Advance();
		}


		private void ParseProductions()
		{
			while (token != GrammarToken.EndOfSection && token != GrammarToken.Eof)
			{
				while (token == GrammarToken.Symbol)
					ParseProduction();
                if (token != GrammarToken.Symbol && token != GrammarToken.EndOfSection && token != GrammarToken.Eof)
                {
                    scanner.ReportError("Unexpected symbol, skipping");
                    //do { Advance(); }
                    //while (token != GrammarToken.Symbol && token != GrammarToken.EndOfSection && token != GrammarToken.Eof);
                }
			}
			Advance();
		}


		private void ParseProduction()
		{
			NonTerminal lhs = null;

			if (token == GrammarToken.Symbol)
			{
				lhs = grammar.LookupNonTerminal(scanner.yylval);

				if (grammar.startSymbol == null)
					grammar.startSymbol = lhs;

				if (grammar.productions.Count == 0)
					grammar.CreateSpecialProduction(grammar.startSymbol);
			}
			else
				scanner.ReportError("lhs symbol expected");

			Advance();

			if (token != GrammarToken.Colon)
				scanner.ReportError("Colon expected");
			else
				Advance();

			ParseRhs(lhs);
			while (token == GrammarToken.Divider)
			{
				Advance();
				ParseRhs(lhs);
			}

			if (token != GrammarToken.SemiColon)
				scanner.ReportError("Semicolon expected");
			else
				Advance();
		}


		private void ParseRhs(NonTerminal lhs)
		{
			Production production = new Production(lhs);
			int pos = 0;

			while (token == GrammarToken.Symbol || token == GrammarToken.Literal || 
				   token == GrammarToken.Action || token == GrammarToken.Prec)
			{
				switch (token)
				{
					case GrammarToken.Literal:
					{
						production.rhs.Add(grammar.LookupTerminal(token, scanner.yylval));
						Advance();
						pos++;
						break;
					}
					case GrammarToken.Symbol:
					{
						if (grammar.terminals.ContainsKey(scanner.yylval))
							production.rhs.Add(grammar.terminals[scanner.yylval]);
						else
							production.rhs.Add(grammar.LookupNonTerminal(scanner.yylval));
						Advance();
						pos++;
						break;
					}
					case GrammarToken.Prec:
					{
						Advance();
						if (token == GrammarToken.Symbol)
						{
							production.prec = grammar.LookupTerminal(token, scanner.yylval).prec;
							Advance();
						}
						else
							scanner.ReportError("Expected symbol after %prec");
						break;
					}
					case GrammarToken.Action:
					{
						SemanticAction semanticAction = new SemanticAction(production, pos, scanner.yylval);
						Advance();

						if (token == GrammarToken.Divider || token == GrammarToken.SemiColon || token == GrammarToken.Prec) // reduce action
							production.semanticAction = semanticAction;
						else
						{
							NonTerminal node = grammar.LookupNonTerminal("@" + (++grammar.NumActions).ToString());
							Production nullProduction = new Production(node);
							nullProduction.semanticAction = semanticAction;
							grammar.AddProduction(nullProduction);
							production.rhs.Add(node);
						}
						pos++;
						break;
					}
				}
			}

			grammar.AddProduction(production);

			Precedence.Calculate(production);
		}


		private void ParseEpilog()
		{
            grammar.epilogCode = scanner.yylval;

			Advance();

			if (token != GrammarToken.Eof)
				scanner.ReportError("Expected EOF");
		}


		private void Advance()
		{
			token = scanner.Next();
			//Console.WriteLine("Token: ({0}:{1})", token, scanner.yylval);
		}
	}
}








