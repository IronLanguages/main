using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Linq;

namespace SymplSample {

    public class Parser {
    
		// ParseFile returns an array of top-level expressions parsed in the
		// TextReader.
		//
        public SymplExpr[] ParseFile(TextReader reader) {
            if (reader == null) {
                throw new ArgumentException("Reader must not be null.");
            }
            Lexer lexer = new Lexer(reader);
            var body = new List<SymplExpr>();
            var token = lexer.GetToken();
            while (token != SyntaxToken.EOF) {
                lexer.PutToken(token);
                body.Add(ParseExprAux(lexer));
                token = lexer.GetToken();
            }
            return body.ToArray();
        }

		// Parse returns a single expression parsed from the TextReader.
		//
        public SymplExpr ParseExpr(TextReader reader) {
            if (reader == null) {
                throw new ArgumentException("Reader must not be null.");
            }
            return ParseExprAux(new Lexer(reader));
        }
		
		
		// _parseExpr parses an expression from the Lexer passed in.
		//
        private SymplExpr ParseExprAux(Lexer lexer) {
            Token token = lexer.GetToken();
            SymplExpr res = null;
            if (token == SyntaxToken.EOF) {
                throw new SymplParseException(
                    "Unexpected EOF encountered while parsing expression.");
            }
            if (token == SyntaxToken.Quote) {
                lexer.PutToken(token);
                res = ParseQuoteExpr(lexer);
            } else if (token == SyntaxToken.Paren) {
                lexer.PutToken(token);
                res = ParseForm(lexer);
            } else if (token is IdOrKeywordToken) {
                // If we encounter literal kwd constants, they get turned into ID
                // Exprs.  Code that accepts Id Exprs, needs to check if the token is
                // kwd or not when it matters.
                if ((token is KeywordToken) && !(token == KeywordToken.Nil) &&
                    !(token == KeywordToken.True) && !(token == KeywordToken.False)) {
                    throw new InvalidCastException("Keyword cannot be an expression");
                } else {
                    res = new SymplIdExpr((IdOrKeywordToken)token);
                }
            } else if (token is LiteralToken) {
                res = new SymplLiteralExpr(((LiteralToken)token).Value);
            }
            // check for dotted expr
            if (res != null) {
                Token next = lexer.GetToken();
                lexer.PutToken(next);
                if (next == SyntaxToken.Dot) {
                    return ParseDottedExpr(lexer, res);
                } else {
                    return res;
                }
            }
            throw new SymplParseException(
                "Unexpected token when expecting " +
                "beginning of expression -- " + token.ToString() + " ... " + 
                lexer.GetToken().ToString() + lexer.GetToken().ToString() + 
                lexer.GetToken().ToString() + lexer.GetToken().ToString());
        }

		// _parseForm parses a parenthetic form.  If the first token after the paren
		// is a keyword, then it something like defun, loop, if, try, etc.  If the
		// first sub expr is another parenthetic form, then it must be an expression
		// that returns a callable object.
		//
        private SymplExpr ParseForm(Lexer lexer) {
            Token token = lexer.GetToken();
            if (token != SyntaxToken.Paren) {
                throw new SymplParseException(
                    "List expression must start with '('.");
            }
            token = lexer.GetToken();
            if (token is IdOrKeywordToken) {
                lexer.PutToken(token);
                if (token is KeywordToken) {
                    // Defun, Let*, Set, Import, ...
                    return ParseKeywordForm(lexer);
                } else {
                    return ParseFunctionCall(lexer);
                }
            } else {
                lexer.PutToken(token);
                return ParseFunctionCall(lexer);
            }
        }

		// _parseKeywordForm parses parenthetic built in forms such as defun, if,
		// loop, etc.
		//
        private SymplExpr ParseKeywordForm(Lexer lexer) {
            Token name = lexer.GetToken();
            if (!(name is KeywordToken)) {
                throw new SymplParseException(
                    "Internal error: parsing keyword form?");
            }
            lexer.PutToken(name);
            if (name == KeywordToken.Import) {
                return ParseImport(lexer);
            } else if (name == KeywordToken.Defun) {
                return ParseDefun(lexer);
            } else if (name == KeywordToken.Lambda) {
                return ParseLambda(lexer);
            } else if (name == KeywordToken.Set) {
                return ParseSet(lexer);
            } else if (name == KeywordToken.LetStar) {
                return ParseLetStar(lexer);
            } else if (name == KeywordToken.Block) {
                return ParseBlock(lexer);
            } else if (name == KeywordToken.Eq) {
                return ParseEq(lexer);
            } else if (name == KeywordToken.Cons) {
                return ParseCons(lexer);
            } else if (name == KeywordToken.List) {
                return ParseListCall(lexer);
            } else if (name == KeywordToken.If) {
                return ParseIf(lexer);
            } else if (name == KeywordToken.New) {
                return ParseNew(lexer);
            } else if (name == KeywordToken.Loop) {
                return ParseLoop(lexer);
            } else if (name == KeywordToken.Break) {
                return ParseBreak(lexer);
            } else if (name == KeywordToken.Elt) {
                return ParseElt(lexer);
            } else if (name == KeywordToken.Add ||
                       name == KeywordToken.Substract ||
                       name == KeywordToken.Muliply ||
                       name == KeywordToken.Divide ||
                       name == KeywordToken.Equal ||
                       name == KeywordToken.NotEqual ||
                       name == KeywordToken.GreaterThan ||
                       name == KeywordToken.LessThan ||
                       name == KeywordToken.And ||
                       name == KeywordToken.Or) {
                return ParseExprTreeBinaryOp(lexer);
            } else if (name == KeywordToken.Not) {
                return ParseExprTreeUnaryOp(lexer);
            }
            throw new SymplParseException(
                "Internal: unrecognized keyword form?");
        }

        private SymplExpr ParseDefun(Lexer lexer) {
            Token token = lexer.GetToken();
            if (token != KeywordToken.Defun) {
                throw new SymplParseException("Internal: parsing Defun?");
            }
            IdOrKeywordToken name = lexer.GetToken() as IdOrKeywordToken;
            if (name == null || name.IsKeywordToken) {
                throw new SymplParseException(
                    "Defun must have an ID for name -- " + name.ToString());
            }
            var parms = ParseParams(lexer, "Defun");
            var body = ParseBody(lexer, "Hit EOF in function body" +
                                 name.Name);
            return new SymplDefunExpr(name.Name, parms, body);
        }

        private SymplExpr ParseLambda(Lexer lexer) {
            Token token = lexer.GetToken();
            if (token != KeywordToken.Lambda) {
                throw new SymplParseException("Internal: parsing Lambda?");
            }
            var parms = ParseParams(lexer, "Lambda");
            var body = ParseBody(lexer, "Hit EOF in function body");
            return new SymplLambdaExpr(parms, body);
        }

        // _parseParams parses sequence of vars for Defuns and Lambdas, and always
        // returns a list of IdTokens.
        //
        private IdOrKeywordToken[] ParseParams(Lexer lexer, string definer) {
            Token token = lexer.GetToken();
            if (token != SyntaxToken.Paren) {
                throw new SymplParseException(
                    definer + " must have param list following name.");
            }
            lexer.PutToken(token);
            return EnsureListOfIds(ParseList(lexer, "param list.").Elements, false,
                                   definer + " params must be valid IDs.");
        }

        // _parseBody parses sequence of expressions as for Defun, Let, etc., and
        // always returns a list, even if empty.  It gobbles the close paren too.
        //
        private SymplExpr[] ParseBody(Lexer lexer, string error) {
            Token token = lexer.GetToken();
            List<SymplExpr> body = new List<SymplExpr>();
            while (token != SyntaxToken.EOF && token != SyntaxToken.CloseParen) {
                lexer.PutToken(token);
                body.Add(ParseExprAux(lexer));
                token = lexer.GetToken();
            }
            if (token == SyntaxToken.EOF) {
                throw new SymplParseException(error);
            }
            return body.ToArray();
        }

        // (import id[.id]*  [{id | (id [id]*)}  [{id | (id [id]*)}]]  )
        // (import file-or-dotted-Ids name-or-list-of-members reanme-or-list-of)
        //
        private SymplExpr ParseImport(Lexer lexer) {
            if (lexer.GetToken() != KeywordToken.Import) {
                throw new SymplParseException(
                    "Internal: parsing Import call?");
            }
            IdOrKeywordToken[] ns_or_module = ParseImportNameOrModule(lexer);
            IdOrKeywordToken[] members = ParseImportNames(lexer, "member names", true);
            IdOrKeywordToken[] as_names = ParseImportNames(lexer, "renames", false);
            if (members.Length != as_names.Length && as_names.Length != 0) {
                throw new SymplParseException(
                    "Import as-names must be same form as member names.");
            }
            if (lexer.GetToken() != SyntaxToken.CloseParen) {
                throw new SymplParseException(
                    "Import must end with closing paren.");
            }
            return new SymplImportExpr(ns_or_module, members, as_names);
        }

        // Parses dotted namespaces or Sympl.Globals members to import.
        //
        private IdOrKeywordToken[] ParseImportNameOrModule(Lexer lexer) {
            Token token = lexer.GetToken();
            if (!(token is IdOrKeywordToken)) { // Keywords are ok here.
                throw new SymplParseException(
                    "Id must follow Import symbol");
            }
            Token dot = lexer.GetToken();
            List<IdOrKeywordToken> ns_or_module = new List<IdOrKeywordToken>();
            if (dot == SyntaxToken.Dot) {
                lexer.PutToken(dot);
                var tmp = ParseDottedExpr(
                              lexer, new SymplIdExpr((IdOrKeywordToken)token));
                foreach (var e in tmp.Exprs) {
                    if (!(e is SymplIdExpr)) { // Keywords are ok here.
                        throw new SymplParseException(
                            "Import targets must be dotted identifiers." +
                            e.ToString() + ns_or_module.ToString());
                    }
                    ns_or_module.Add(((SymplIdExpr)e).IdToken);
                }
                token = lexer.GetToken();
            } else {
                ns_or_module.Add((IdOrKeywordToken)token);
                token = dot;
            }
            lexer.PutToken(token);
            return ns_or_module.ToArray();
        }

       // Parses list of member names to import from the object represented in the
       // result of _parseImportNameOrModule, which will be a file module or object
       // from Sympl.Globals.  This is also used to parse the list of renames for
       // these same members.
       //
        private IdOrKeywordToken[] ParseImportNames(Lexer lexer, string nameKinds,
                                                    bool allowKeywords) {
            Token token = lexer.GetToken();
            List<IdOrKeywordToken> names = new List<IdOrKeywordToken>();
            if (token is IdOrKeywordToken) {
                IdOrKeywordToken idToken = (IdOrKeywordToken)token;
                if (!idToken.IsKeywordToken) {
                    names.Add(idToken);
                }
            } else if (token == SyntaxToken.Paren) {
                lexer.PutToken(token);
                object[] memberTokens = ParseList(lexer, "Import " + nameKinds + ".")
                                           .Elements;
                IdOrKeywordToken[] memberIdTokens = 
                    EnsureListOfIds(memberTokens, allowKeywords,
                                    "Import " + nameKinds + " must be valid IDs.");
            } else if (token == SyntaxToken.CloseParen) {
                lexer.PutToken(token);
            } else {
                throw new SymplParseException(
                    "Import takes dotted names, then member vars.");
            }
            return names.ToArray();
        }

        private IdOrKeywordToken[] EnsureListOfIds(object[] list, 
                                                    bool allowKeywords,
                                                    string error) {
            foreach (var t in list) {
                IdOrKeywordToken id = t as IdOrKeywordToken;
                if (id == null || (!allowKeywords && id.IsKeywordToken)) {
                    throw new SymplParseException(error);
                }
            }
            return list.Select(t => (IdOrKeywordToken)t).ToArray();
        }

        // _parseDottedExpr gathers infix dotted member access expressions.  The
        // object expression can be anything and is passed in via expr.  Successive
        // member accesses must be dotted identifier expressions or member invokes --
        // a.b.(c 3).d.  The member invokes cannot have dotted expressions for the
        // member name such as a.(b.c 3).
        //
        private SymplDottedExpr ParseDottedExpr(Lexer lexer, SymplExpr objExpr) {
            Token token = lexer.GetToken();
            if (token != SyntaxToken.Dot) {
                throw new SymplParseException(
                    "Internal: parsing dotted expressions?");
            }
            List<SymplExpr> exprs = new List<SymplExpr>();
            token = lexer.GetToken();
            while (token is IdOrKeywordToken || token == SyntaxToken.Paren) {
                // Needs to be fun call or IDs
                SymplExpr expr;
                if (token is IdOrKeywordToken) {
                    // Keywords are ok as member names.
                    expr = new SymplIdExpr((IdOrKeywordToken)token);
                } else {
                    lexer.PutToken(token);
                    expr = ParseForm(lexer);
                    SymplFunCallExpr funCall = expr as SymplFunCallExpr;
                    if (funCall != null || !(funCall.Function is SymplIdExpr)) {
                        throw new SymplParseException(
                            "Dotted expressions must be identifiers or " +
                            "function calls with identiers as the function " +
                            "value -- " + expr.ToString());
                    }
                }
                exprs.Add(expr);
                token = lexer.GetToken();
                if (token != SyntaxToken.Dot) {
                    break;
                }
                token = lexer.GetToken();
            }
            lexer.PutToken(token);
            return new SymplDottedExpr(objExpr, exprs.ToArray());
        }

        // _parseSet parses a LHS expression and value expression.  All analysis on
        // the LHS is in etgen.py.
        //
        private SymplAssignExpr ParseSet(Lexer lexer) {
            if (lexer.GetToken() != KeywordToken.Set) {
                throw new SymplParseException("Internal error: parsing Set?");
            }
            var lhs = ParseExprAux(lexer);
            var val = ParseExprAux(lexer);
            if (lexer.GetToken() != SyntaxToken.CloseParen) {
                throw new SymplParseException(
                    "Expected close paren for Set expression.");
            }
            return new SymplAssignExpr(lhs, val);
        }

        // _parseLetStar parses (let* ((<var> <expr>)*) <body>).
        //
        private SymplLetStarExpr ParseLetStar(Lexer lexer) {
            if (lexer.GetToken() != KeywordToken.LetStar) {
                throw new SymplParseException("Internal error: parsing Let?");
            }
            var token = lexer.GetToken();
            if (token != SyntaxToken.Paren) {
                throw new SymplParseException(
                    "Let expression has no bindings?  Missing '('.");
            }
            // Get bindings
            List<LetBinding> bindings = new List<LetBinding>();
            token = lexer.GetToken();
            while (token == SyntaxToken.Paren) {
                var e = ParseExprAux(lexer);
                var id = e as SymplIdExpr;
                if (id == null || id.IdToken.IsKeywordToken) {
                    throw new SymplParseException(
                        "Let binding must be (<ID> <expr>) -- ");
                }
                var init = ParseExprAux(lexer);
                bindings.Add(new LetBinding(id.IdToken, init));
                token = lexer.GetToken();
                if (token != SyntaxToken.CloseParen) {
                    throw new SymplParseException(
                        "Let binding missing close paren -- ");
                }
                token = lexer.GetToken();
            }
            if (token != SyntaxToken.CloseParen) {
                throw new SymplParseException(
                    "Let bindings missing close paren.");
            }
            var body = ParseBody(lexer, "Unexpected EOF in Let.");
            return new SymplLetStarExpr(bindings.ToArray(), body);
        }

		// _parseBlock parses a block expression, a sequence of exprs to
		// execute in order, returning the last expression's value.
		//
        private SymplBlockExpr ParseBlock(Lexer lexer) {
            if (lexer.GetToken() != KeywordToken.Block) {
                throw new SymplParseException(
                    "Internal error: parsing Block?");
            }
            var body = ParseBody(lexer, "Unexpected EOF in Block.");
            return new SymplBlockExpr(body);
        }

		// first sub form must be expr resulting in callable, but if it is dotted expr,
		// then eval the first N-1 dotted exprs and use invoke member or get member
		// on last of dotted exprs so that the 2..N sub forms are the arguments to
		// the invoke member.  It's as if the call breaks into a block of a temp
		// assigned to the N-1 dotted exprs followed by an invoke member (or a get
		// member and call, which the runtime binder decides).  The non-dotted expr
		// simply evals to an object that better be callable with the supplied args,
		// which may be none.
		// 
        private SymplFunCallExpr ParseFunctionCall(Lexer lexer) {
            // First sub expr is callable object or invoke member expr.
            var fun = ParseExprAux(lexer);
            if (fun is SymplDottedExpr) {
                SymplDottedExpr dottedExpr = (SymplDottedExpr)fun;
                // Keywords ok as members.
                if (!(dottedExpr.Exprs.Last() is SymplIdExpr)) {
                    throw new SymplParseException(
                        "Function call with dotted expression for function must " + 
                        "end with ID Expr, not member invoke." +
                        dottedExpr.Exprs.Last().ToString());
                }
            }
            // Tail exprs are args.
            var args = ParseBody(lexer, "Unexpected EOF in arg list for " + 
                                        fun.ToString());
            return new SymplFunCallExpr(fun, args);

        }

		// This parses a quoted list, ID/keyword, or literal.
		//
        private SymplQuoteExpr ParseQuoteExpr(Lexer lexer) {
            var token = lexer.GetToken();
            if (token != SyntaxToken.Quote) {
                throw new SymplParseException("Internal: parsing Quote?.");
            }
            token = lexer.GetToken();
            object expr;
            if (token == SyntaxToken.Paren) {
                lexer.PutToken(token);
                expr = ParseList(lexer, "quoted list.");
            } else if (token is IdOrKeywordToken ||
                       token is LiteralToken) {
                expr = token;
            } else {
                throw new SymplParseException(
                   "Quoted expression can only be list, ID/Symbol, or literal.");
            }
            return new SymplQuoteExpr(expr);
        }

        private SymplEqExpr ParseEq (Lexer lexr) {
            var token = lexr.GetToken();
            if (token != KeywordToken.Eq) {
                throw new SymplParseException("Internal: parsing Eq?");
            }
            SymplExpr left, right;
            ParseBinaryRuntimeCall(lexr, out left, out right);
            return new SymplEqExpr(left, right);
        }
            
        private SymplConsExpr ParseCons (Lexer lexr) {
            var token = lexr.GetToken();
            if (token != KeywordToken.Cons) {
                throw new SymplParseException("Internal: parsing Cons?");
            }
            SymplExpr left, right;
            ParseBinaryRuntimeCall(lexr, out left, out right);
            return new SymplConsExpr(left, right);
        }

        // _parseBinaryRuntimeCall parses two exprs and a close paren, returning the
        // two exprs.
        //
        private void ParseBinaryRuntimeCall(Lexer lexr, out SymplExpr left,
                                              out SymplExpr right) {
            left = ParseExprAux(lexr);
            right = ParseExprAux(lexr);
            if (lexr.GetToken() != SyntaxToken.CloseParen) {
                throw new SymplParseException(
                    "Expected close paren for Eq call.");
            }
        }

        // _parseListCall parses a call to the List built-in keyword form that takes
        // any number of arguments.
        //
        private SymplListCallExpr ParseListCall (Lexer lexr) {
            Token token = lexr.GetToken();
            if (token != KeywordToken.List) {
                throw new SymplParseException("Internal: parsing List call?");
            }
            var args = ParseBody(lexr, "Unexpected EOF in arg list for call to List.");
            return new SymplListCallExpr(args);
        }

        private SymplIfExpr ParseIf (Lexer lexr) {
            var token = lexr.GetToken();
            if (token != KeywordToken.If) {
                throw new SymplParseException("Internal: parsing If?");
            }
            var args = ParseBody(lexr, "Unexpected EOF in If form.");
            int argslen = args.Length;
            if (argslen == 2) {
                return new SymplIfExpr(args[0], args[1], null);
            } else if (argslen == 3) {
                return new SymplIfExpr(args[0], args[1], args[2]);
            } else {
                throw new SymplParseException(
                    "IF must be (if <test> <consequent> [<alternative>]).");
            }
        }

		// This parses pure list and atom structure.  Atoms are IDs, strs, and nums.
        // _parseLoop parses a loop expression, a sequence of exprs to
        // execute in order, forever.  See Break for returning expression's value.
        //
        private SymplLoopExpr ParseLoop (Lexer lexr) {
            if (lexr.GetToken() != KeywordToken.Loop) {
                throw new SymplParseException("Internal error: parsing Loop?");
            }
            var body = ParseBody(lexr, "Unexpected EOF in Loop.");
            return new SymplLoopExpr(body);
        }

        // _parseBreak parses a Break expression, which has an optional value that
        // becomes a loop expression's value.
        //
        private SymplBreakExpr ParseBreak (Lexer lexr) {
            if (lexr.GetToken() != KeywordToken.Break) {
                throw new SymplParseException("Internal error: parsing Break?");
            }
            var token = lexr.GetToken();
            SymplExpr value;
            if (token == SyntaxToken.CloseParen) {
                value = null;
            } else {
                lexr.PutToken(token);
                value = ParseExprAux(lexr);
                token = lexr.GetToken();
                if (token != SyntaxToken.CloseParen) {
                    throw new SymplParseException(
                        "Break expression missing close paren.");
                }
            }
            return new SymplBreakExpr(value);
        }

        // Parse a New form for creating instances of types.  Second sub expr (one
        // after kwd New) evals to a type.
        //
        // Consider adding a new kwd form generic-type-args that could be the third
        // sub expr and take any number of sub exprs that eval to types.  These could
        // be used to specific concrete generic type instances.  Without this support
        // SymPL programmers need to open code this as the examples show.
        //
        private SymplNewExpr ParseNew(Lexer lexr) {
            Token token = lexr.GetToken();
            if (token != KeywordToken.New) {
                throw new SymplParseException("Internal: parsing New?");
            }
            var type = ParseExprAux(lexr);
            var args = ParseBody(lexr, "Unexpected EOF in arg list for call to New.");
            return new SymplNewExpr(type, args);
        }


        // This parses pure list and atom structure.  Atoms are IDs, strs, and nums.
		// Need quoted form of dotted exprs, quote, etc., if want to have macros one
		// day.  This is used for Import name parsing, Defun/Lambda params, and quoted
		// lists.
		//
        private SymplListExpr ParseList(Lexer lexer, string errStr) {
            var token = lexer.GetToken();
            if (token != SyntaxToken.Paren) {
                throw new SymplParseException(
                    "List expression must start with '('.");
            }
            token = lexer.GetToken();
            List<object> res = new List<object>();
            while (token != SyntaxToken.EOF && token != SyntaxToken.CloseParen) {
                lexer.PutToken(token);
                object elt;
                if (token == SyntaxToken.Paren) {
                    elt = ParseList(lexer, errStr);
                } else if (token is IdOrKeywordToken || token is LiteralToken) {
                    elt = token;
                    lexer.GetToken();
                } else if (token == SyntaxToken.Dot) {
                    throw new SymplParseException(
                        "Can't have dotted syntax in " + errStr);
                } else {
                    throw new SymplParseException(
                        "Unexpected token in list -- " + token.ToString());
                }
                if (elt == null) {
                    throw new SymplParseException(
                        "Internal: no next element in list?");
                }
                res.Add(elt);
                token = lexer.GetToken();
            }
            if (token == SyntaxToken.EOF) {
                throw new SymplParseException(
                    "Unexpected EOF encountered while parsing list.");
            }
            return new SymplListExpr(res.ToArray());

        }

        public SymplEltExpr ParseElt(Lexer lexr) {
            Token token = lexr.GetToken();
            if (token != KeywordToken.Elt) {
                throw new SymplParseException("Internal: parsing Elt?");
            }
            var obj = ParseExprAux(lexr);
            var indexes = ParseBody(lexr,
                                    "Unexpected EOF in arg list for call to Elt.");
            return new SymplEltExpr(obj, indexes);
        }

        // This parses a BinaryOp expression.
        //
        private SymplBinaryExpr ParseExprTreeBinaryOp(Lexer lexr) {
            KeywordToken keyword = lexr.GetToken() as KeywordToken;
            if (keyword == null) {
                throw new SymplParseException(
                                "Internal error: parsing Binary?");
            }

            SymplExpr left, right;
            ParseBinaryRuntimeCall(lexr, out left, out right);
            var op = GetOpType(keyword);
            return new SymplBinaryExpr(left, right, op);
        }

        // This parses a UnaryOp expression.
        //
        private SymplUnaryExpr ParseExprTreeUnaryOp(Lexer lexr) {
            KeywordToken keyword = lexr.GetToken() as KeywordToken;
            if (keyword == null) {
                throw new SymplParseException(
                              "Internal error: parsing Unary?");
            }

            var op = GetOpType(keyword);
            var operand = ParseExprAux(lexr);
            if (lexr.GetToken() != SyntaxToken.CloseParen) {
                throw new SymplParseException(
                              "Unary expression missing close paren.");
            }
            return new SymplUnaryExpr(operand, op);
        }

        // Get the ExpressionType for an operator
        private ExpressionType GetOpType(KeywordToken keyword) {
            if (keyword == KeywordToken.Add) {
                return ExpressionType.Add;
            }
            if (keyword == KeywordToken.Substract) {
                return ExpressionType.Subtract;
            }
            if (keyword == KeywordToken.Muliply) {
                return ExpressionType.Multiply;
            }
            if (keyword == KeywordToken.Divide) {
                return ExpressionType.Divide;
            }
            if (keyword == KeywordToken.Equal) {
                return ExpressionType.Equal;
            }
            if (keyword == KeywordToken.NotEqual) {
                return ExpressionType.NotEqual;
            }
            if (keyword == KeywordToken.GreaterThan) {
                return ExpressionType.GreaterThan;
            }
            if (keyword == KeywordToken.LessThan) {
                return ExpressionType.LessThan;
            }
            if (keyword == KeywordToken.And) {
                return ExpressionType.And;
            }
            if (keyword == KeywordToken.Or) {
                return ExpressionType.Or;
            }
            if (keyword == KeywordToken.Not) {
                return ExpressionType.Not;
            }
            throw new SymplParseException(
                            "Unrecognized keyword for operators");
        }
    }



//###################
// SymplExpr Classes
//###################


    public class SymplExpr {
    }

    // SymplIdExpr represents identifiers, but the IdToken can be a keyword
    // sometimes.  For example, in quoted lists, import expressions, and as
    // members of objects in dotted exprs.  Need to check for .IsKeywordToken
    // when it matters.
    //
    public class SymplIdExpr : SymplExpr {
        private IdOrKeywordToken _idToken;

        public IdOrKeywordToken IdToken {
            get { return _idToken; }
        }

        public SymplIdExpr(IdOrKeywordToken id) {
            _idToken = id;
        }
        public override string ToString() {
            return "<IdExpr " + _idToken.Name + ">";
        }
    }

    public class SymplListExpr : SymplExpr {
        private object[] _elements;

        // This is always a list of Tokens or SymplListExprs.
        public object[] Elements { get { return _elements; } }

        public SymplListExpr(object[] elements) {
            _elements = elements;
        }
        public override string  ToString() {
            return "<ListExpr " + _elements.ToString() + ">";
        }
    }

    public class SymplFunCallExpr : SymplExpr {
        private SymplExpr _fun;
        private SymplExpr[] _args;

        public SymplFunCallExpr(SymplExpr fun, SymplExpr[] args) {
            _fun = fun;
            _args = args;
        }
        
        public SymplExpr Function { get { return _fun; } }

        public SymplExpr[] Arguments { get { return _args; } }

        public override string  ToString() {
            return "<Funcall ( " + _fun.ToString() + " " + _args.ToString() + " )>";
        }
    }

    public class SymplDefunExpr : SymplExpr {
        private string _name;
        private IdOrKeywordToken[] _params;
        private SymplExpr[] _body;
        
        public SymplDefunExpr(string name, IdOrKeywordToken[] parms,
                               SymplExpr[] body) {
            _name = name;
            _params = parms;
            _body = body;
        }

        public string Name { get { return _name; }  }

        public IdOrKeywordToken[] Params { get { return _params; } }

        public SymplExpr[] Body { get { return _body; } }

        public override string  ToString() {
            return "<Defun " + _name + " (" + _body.ToString() + ") ...>";
        }
    }

    public class SymplLambdaExpr : SymplExpr {
        private IdOrKeywordToken[] _params;
        private SymplExpr[] _body;
        
        public SymplLambdaExpr(IdOrKeywordToken[] parms, SymplExpr[] body) {
            _params = parms;
            _body = body;
        }

        public IdOrKeywordToken[] Params { get { return _params; } }

        public SymplExpr[] Body { get { return _body; } }

        public override string  ToString() {
            return "<Lambda " + " (" + _params.ToString() + ") ...>";
        }
    }

    // Used to represent numbers and strings, but not Quote.
    public class SymplLiteralExpr : SymplExpr {
        private object _value;

        public object Value { get { return _value; } }

        public SymplLiteralExpr(object val) {
            _value = val;
        }
        public override string  ToString() {
            return "<LiteralExpr " + _value.ToString() +">";
        }
    }

    public class SymplDottedExpr : SymplExpr {
        private SymplExpr _obj;
        // exprs is always a list of SymplIdExprs or SymplFunCallExprs,
        // ending with a SymplIdExpr when used as SymplFunCallExpr.Function.
        private SymplExpr[] _exprs;

        public SymplExpr ObjectExpr { get { return _obj; } }
        public SymplExpr[] Exprs { get { return _exprs; } }

        public SymplDottedExpr(SymplExpr obj, SymplExpr[] exprs) {
            _obj = obj;
            _exprs = exprs;
        }

        public override string  ToString() {
            return "<DotExpr " + _obj.ToString() + "." + _exprs.ToString() + ">";
        }
    }

    public class SymplImportExpr : SymplExpr {
        private IdOrKeywordToken[] _nsOrModule;
        private IdOrKeywordToken[] _members;
        private IdOrKeywordToken[] _asNames;
        public SymplImportExpr(IdOrKeywordToken[] nsOrModule,
                                IdOrKeywordToken[] members,
                                IdOrKeywordToken[] asNames) {
            _nsOrModule = nsOrModule;
            _members = members;
            _asNames = asNames;
        }

        public IdOrKeywordToken[] NamespaceExpr { get { return _nsOrModule; } }
        public IdOrKeywordToken[] MemberNames { get { return _members; } }
        public IdOrKeywordToken[] Renames { get { return _asNames; } }

        public override string ToString() {
            return "<ImportExpr>";
        }
    }

    public class SymplAssignExpr : SymplExpr {
        private SymplExpr _lhs;
        private SymplExpr _value;

        public SymplAssignExpr(SymplExpr lhs, SymplExpr value) {
            _lhs = lhs;
            _value = value;
        }

        public SymplExpr Location { get { return _lhs; } }

        public SymplExpr Value { get { return _value; } }

        public override string  ToString() {
            return "<AssignExpr " + _lhs.ToString() + "=" + _value.ToString() + ">";
        }
    }
            
    public class SymplLetStarExpr : SymplExpr {
        private LetBinding[] _bindings;
        private SymplExpr[] _body ;

        public SymplLetStarExpr(LetBinding[] bindings,
                                 SymplExpr[] body) {
            _bindings = bindings;
            _body = body;
        }

        public LetBinding[] Bindings {
            get { return _bindings; }
        }

        public SymplExpr[] Body { get { return _body; } }

        public override string  ToString() {
            return "<Let* (" + _bindings.ToString() + ")" + _body.ToString() + ">";
        }
    }

    // Represents a binding defined in a LetStarExpr
    public class LetBinding {
        private IdOrKeywordToken _variable;
        private SymplExpr _value;

        public LetBinding(IdOrKeywordToken variable, SymplExpr value) {
            _variable = variable;
            _value = value;
        }

        public IdOrKeywordToken Variable { get { return _variable; } }
        public SymplExpr Value { get { return _value; } }
    }

    public class SymplBlockExpr : SymplExpr {
        private SymplExpr[] _body ;

        public SymplBlockExpr(SymplExpr[] body) {
            _body = body;
        }

        public SymplExpr[] Body { get { return _body; } }

        public override string  ToString() {
            return "<Block* (" + _body.ToString() + ">";
        }
    }

    public class SymplEltExpr : SymplExpr {
        private SymplExpr _obj;
        private SymplExpr[] _indexes;

        public SymplEltExpr(SymplExpr obj, SymplExpr[] indexes) {
            _obj = obj;
            _indexes = indexes;
        }

        public SymplExpr ObjectExpr { get { return _obj; } }
        public SymplExpr[] Indexes { get { return _indexes; } }

        public override string  ToString() {
            return "<EltExpr " + _obj.ToString() + "[" + _indexes.ToString() + "] >";
        }
    }
        

    public class SymplQuoteExpr : SymplExpr {
        // Expr must be SymplListExpr, SymplIdExpr, or SymplLIteralExpr
        private object _expr;

        public SymplQuoteExpr(object expr) {
            _expr = expr;
        }

        public object Expr { get { return _expr; } }

        public override string  ToString() {
            return "<QuoteExpr " + _expr.ToString() + ">";
        }
    }

  
    public class SymplEqExpr : SymplExpr {
        private SymplExpr _left;
        private SymplExpr _right;

        public SymplEqExpr (SymplExpr left, SymplExpr right) {
            _left = left;
            _right = right;
        }

        public SymplExpr Left { get {return _left;}}
        public SymplExpr Right { get {return _right;}}
    }

    public class SymplConsExpr : SymplExpr {
        private SymplExpr _left;
        private SymplExpr _right;

        public SymplConsExpr (SymplExpr left, SymplExpr right) {
            _left = left;
            _right = right;
        }

        public SymplExpr Left { get {return _left;}}
        public SymplExpr Right { get {return _right;}}
    }

    public class SymplListCallExpr : SymplExpr {
        private SymplExpr[] _elements;

        // This is always a list of Tokens or SymplListExprs.
        public SymplExpr[] Elements { get { return _elements; } }

        public SymplListCallExpr(SymplExpr[] elements) {
            _elements = elements;
        }
    }

    public class SymplIfExpr : SymplExpr {
        private SymplExpr _test;
        private SymplExpr _consequent;
        private SymplExpr _alternative;

        public SymplIfExpr (SymplExpr test, SymplExpr consequent,
                             SymplExpr alternative) {
            _test = test;
            _consequent = consequent;
            _alternative = alternative;
        }

        public SymplExpr Test {
            get { return _test;}
        }
        public SymplExpr Consequent {
            get { return _consequent;}
        }
        public SymplExpr Alternative {
            get { return _alternative;}
        }
    }

    public class SymplLoopExpr : SymplExpr {
        private SymplExpr[] _body;

        public SymplLoopExpr(SymplExpr[] body) {
            _body = body;
        }

        public SymplExpr[] Body {
            get { return _body; }
        }

        public override string ToString() {
            return "<Loop ...>";
        }
    }

    public class SymplBreakExpr : SymplExpr {
        private SymplExpr _value;

        public SymplBreakExpr (SymplExpr value) {
            // Can be null.
            _value = value;
        }

        public SymplExpr Value {
            get { return _value; }
        }

        public override string ToString() {
            return "<Break ...)>";
        }
    }

    public class SymplNewExpr : SymplExpr {
        private SymplExpr _type;
        private SymplExpr[] _arguments;

        public SymplExpr Type { get { return _type; } }
        public SymplExpr[] Arguments { get { return _arguments; } }

        public SymplNewExpr(SymplExpr type, SymplExpr[] arguments) {
            _type = type;
            _arguments = arguments;
        }
    }

    public class SymplBinaryExpr : SymplExpr {
        private SymplExpr _left;
        private SymplExpr _right;
        ExpressionType _operation;

        public SymplExpr Left { get { return _left; } }
        public SymplExpr Right { get { return _right; } }
        public ExpressionType Operation { get { return _operation; } }

        public SymplBinaryExpr(SymplExpr left, SymplExpr right, ExpressionType operation) {
            _left = left;
            _right = right;
            _operation = operation;
        }
    }

    public class SymplUnaryExpr : SymplExpr {
        private SymplExpr _operand;
        ExpressionType _operation;

        public SymplExpr Operand { get { return _operand; } }
        public ExpressionType Operation { get { return _operation; } }

        public SymplUnaryExpr(SymplExpr expression, ExpressionType operation) {
            _operand = expression;
            _operation = operation;
        }
    }

    public class SymplParseException : Exception {
        public SymplParseException(string msg) : base(msg) { }
	}


} //SymplSample


