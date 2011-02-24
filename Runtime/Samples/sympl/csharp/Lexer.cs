using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SymplSample {

    public class Lexer {
        private Token _token = null;
        private TextReader _reader;
        private const char _eof = unchecked ((char)(-1));

        public Lexer(TextReader reader) {
            if (reader == null) {
                throw new ArgumentException("reader");
            }
            _reader = reader;
        }

        public void PutToken(Token token) {
            if (_token != null) {
                throw new InvalidOperationException(
                    "Internal Error: putting token when there is one?");
            } else {
                _token = token;
            }
        }

        // Returns any saved _putToken, else skips whitespace and returns next
        // token from input stream.
        //
        // If returning token directly based on char, need to gobble char, but
        // if calling helper function to read more, then they gobble as needed.
        //
        // TODO: maintain src loc info and store in token.
        //
        public Token GetToken() {
            if (_token != null) {
                Token tmp = _token;
                _token = null;
                return tmp;
            }
            SkipWhitespace();
            char ch = PeekChar();
            if (ch == _eof) {
                return SyntaxToken.EOF;
            } else if (ch == '(') {
                GetChar();
                return SyntaxToken.Paren;
            } else if (ch == ')') {
                GetChar();
                return SyntaxToken.CloseParen;
            } else if (IsNumChar(ch)) {
                return GetNumber();
            } else if (ch == '\"') {
                return GetString();
            } else if (ch == '\'') {
                GetChar();
                return SyntaxToken.Quote;
            } else if (ch == '-') {
                return GetIdOrNumber();
            } else if (StartsId(ch)) {
                return GetIdOrKeyword();
            } else if (ch == '.') {
                GetChar();
                return SyntaxToken.Dot;
            }
            throw new InvalidOperationException("Internal: couldn't get token?");
        }

        // _getIdOrNumber expects a hyphen as the current char, and returns an Id
        // or number token.
        //
        private Token GetIdOrNumber() {
            char ch = GetChar();
            if (ch == _eof || ch != '-') {
                throw new InvalidOperationException(
                    "Internal: parsing ID or number without hyphen.");
            }
            ch = PeekChar();
            if (IsNumChar(ch)) {
                NumberToken token = GetNumber();
                return new NumberToken(-(int)(token.Value));
            } else if (!IsIdTerminator(ch)) {
                return GetIdOrKeyword('-');
            } else {
                return MakeIdOrKeywordToken("-", false);
            }
        }

        // GetIdOrKeyword has first param to handle call from GetIdOrNumber where
        // ID started with a hyphen (and looked like a number).  Didn't want to
        // add hyphen to StartId test since it normally appears as the keyword
        // minus.  Usually the overload without the first param is called.
        //
        // Must not call when the next char is EOF.
        //
        private IdOrKeywordToken GetIdOrKeyword() {
            return GetIdOrKeyword(GetChar());
        }
        private IdOrKeywordToken GetIdOrKeyword(char first) {
            bool quotedId = false;
            if (first == _eof || !StartsId(first)) {
                throw new InvalidOperationException(
                    "Internal: getting Id or keyword?");
            }
            StringBuilder res = new StringBuilder();
            char c;
            if (first == '\\') {
                quotedId = true;
                c = GetChar();
                if (c == _eof) {
                    throw new InvalidOperationException(
                        "Unexpected EOF when getting Id.");
                }
                if (!StartsId(first)) {
                    throw new InvalidOperationException(
                        "Don't support quoted Ids that have non " + 
                        "Id constituent characters.");
                }
                res.Append(c);
            } else {
                res.Append(first);
            }
            // See if there's more chars to Id
            c = PeekChar();
            while (c != _eof && (!IsIdTerminator(c))) {
                res.Append(c);
                GetChar();
                c = PeekChar();
            }
            return MakeIdOrKeywordToken(res.ToString(), quotedId);
        }

        // Keep whatever casing we found in the source program so that when the
        // IDs are used a member names and metadata on binders, then if some MO
        // doesn't respect the IgnoreCase flag, there's an out for still binding.
        //
        private IdOrKeywordToken MakeIdOrKeywordToken(string name, bool quotedId) {
            if (!quotedId && KeywordToken.IsKeywordName(name)) {
                return KeywordToken.GetKeywordToken(name);
            } else {
                if (name.ToLower() == "let") {
                    System.Console.WriteLine();
                    System.Console.WriteLine(
                       "WARNING: using 'let'?  You probably meant let*.");
                    System.Console.WriteLine();
                }
                return new IdOrKeywordToken(name);
            }
        }

        // Must not be called on EOF
        //
        private bool StartsId(char c) {
            return c == '\\' || !IsIdTerminator(c);
        }

        // Restrict macro syntax chars in case try to add macros later, but need to
        // allow backquote in IDs to support .NET type names that come from
        // reflection.  We can fix this later with more machinery around type names.
        private static char[] _id_terminators = {'(', ')', '\"', ';', ',', /*'`',*/
                                                 '@', '\'', '.'};
        private static bool IsIdTerminator(char c) {
            return _id_terminators.Contains(c) || (c < (char)33);
        }

        // _getNumber returns parsed integers as NumberTokens.  Need to update
        // and use .NET's System.Double.Parse after scanning to non-constituent
        // char.
        //
        private NumberToken GetNumber() {
            // Check integrity before loop to avoid accidently returning zero.
            char c = GetChar();
            if (c == _eof || !IsNumChar((char)c)) {
                throw new InvalidOperationException("Internal: lexing number?");
            }
            int digit = c - '0';
            int res = digit;
            c = PeekChar();
            while (c != _eof && IsNumChar(c)) {
                res = res * 10 + (c - '0');
                GetChar();
                c = PeekChar();
            }
            return new NumberToken(res);
        }

        private StringToken GetString() {
            char c = GetChar();
            if (c == _eof || c != '\"') {
                throw new InvalidOperationException(
                    "Internal: parsing string?");
            }
            StringBuilder res = new StringBuilder();
            bool escape = false;
            c = PeekChar();
            while (true) {
                if (c == _eof) {
                    throw new InvalidOperationException(
                        "Hit EOF in string literal.");
                } else if (c == '\n' || c == '\r') {
                    throw new InvalidOperationException(
                        "Hit newline in string literal");
                } else if (c == '\\' && !escape) {
                    GetChar();
                    escape = true;
                } else if (c == '"' && !escape) {
                    GetChar();
                    return new StringToken(res.ToString());
                } else if (escape) {
                    escape = false;
                    GetChar();
                    switch (c) {
                        case 'n':
                            res.Append('\n');
                            break;
                        case 't':
                            res.Append('\t');
                            break;
                        case 'r':
                            res.Append('\r');
                            break;
                        case '\"':
                            res.Append('\"');
                            break;
                        case '\\':
                            res.Append('\\');
                            break;
                    }
                } else {
                    GetChar();
                    res.Append(c);
                }
                c = PeekChar();
            }
        } //GetString
        
        private static char[] _whitespaceChars = { ' ', '\r', '\n', ';', '\t' };

        private void SkipWhitespace() {
            char ch = PeekChar();
            while (_whitespaceChars.Contains((char)ch)) {
                if (ch == ';') {
                    do {
                        GetChar();
                        ch = PeekChar();
                        if (ch == _eof) return;
                    // If newline seq is two chars, second gets eaten in outer loop.
                    } while (ch != '\n' && (char)ch != '\r');
                } else {
                    GetChar();
                    ch = PeekChar();
                }
            }
        }

        private char GetChar() {
            return unchecked ((char)_reader.Read());
        }

        private char PeekChar() {
            return unchecked ((char)_reader.Peek());
        }

        private static bool IsNumChar(char c) {
            return c >= '0' && c <= '9';
        }
    } //Lexer


    public abstract class Token {
        // TODO: Add source location to token
    }

    internal class LiteralToken : Token {
        private object _value;

        public object Value {
            get { return _value; }
        }

        public LiteralToken(object val) {
            _value = val;
        }
    }

    internal class NumberToken : LiteralToken {
        public NumberToken(int val) 
            : base(val)
        {
        }
    }

    internal class StringToken : LiteralToken {
        public StringToken(string str)
            : base(str) {
        }
    }


    // IdOrKeywordToken represents identifier.  A subtype, KeywordToken, reps
    // keywords.  The parser handles when keywords can be used like identifiers,
    // for example, as .NET members when importing and renaming, literal kwd
    // constants (nil, true, false), etc.  These are used also when parsing list
    // literals before they get converted to runtime Symbol types by etgen.
    //
    public class IdOrKeywordToken : Token {
        private string _name;

        public IdOrKeywordToken(string id) {
            this._name = id;
        }

        public string Name { get { return _name; } }

        public virtual bool IsKeywordToken { get { return false; } }
    }


    internal class KeywordToken : IdOrKeywordToken {

        static KeywordToken() {
            InitializeKeywords();
        }

        private KeywordToken(string id) 
            : base(id) {
        }

        private static Dictionary<string, KeywordToken> _keywords =
            new Dictionary<string,KeywordToken>();

        private static void InitializeKeywords() {
            _keywords["import"] = Import;
            _keywords["defun"] = Defun;
            _keywords["lambda"] = Lambda;
            _keywords["defclass"] = Defclass;
            _keywords["defmethod"] = Defmethod;
            _keywords["new"] = New;
            _keywords["set"] = Set;
            _keywords["let*"] = LetStar;
            _keywords["block"] = Block;
            _keywords["loop"] = Loop;
            _keywords["break"] = Break;
            _keywords["continue"] = Continue;
            _keywords["return"] = Return;
            _keywords["cons"] = Cons;
            _keywords["eq"] = Eq;
            _keywords["list"] = List;
            _keywords["elt"] = Elt;
            _keywords["nil"] = Nil;
            _keywords["true"] = True;
            _keywords["if"] = If;
            _keywords["false"] = False;
            _keywords["+"] = Add;
            _keywords["-"] = Substract;
            _keywords["*"] = Muliply;
            _keywords["/"] = Divide;
            _keywords["="] = Equal;
            _keywords["!="] = NotEqual;
            _keywords[">"] = GreaterThan;
            _keywords["<"] = LessThan;
            _keywords["and"] = And;
            _keywords["or"] = Or;
            _keywords["not"] = Not;
        }

        public static KeywordToken Import = new KeywordToken("Import");
        public static KeywordToken Defun = new KeywordToken("Defun");
        public static KeywordToken Lambda = new KeywordToken("Lambda");
        public static KeywordToken Defclass = new KeywordToken("Defclass");
        public static KeywordToken Defmethod = new KeywordToken("Defmethod");
        public static KeywordToken New = new KeywordToken("New");
        public static KeywordToken Set = new KeywordToken("Set");
        public static KeywordToken LetStar = new KeywordToken("LetStar");
        public static KeywordToken Block = new KeywordToken("Block");
        public static KeywordToken Loop = new KeywordToken("Loop");
        public static KeywordToken Break = new KeywordToken("Break");
        public static KeywordToken Continue = new KeywordToken("Continue");
        public static KeywordToken Return = new KeywordToken("Return");
        public static KeywordToken List = new KeywordToken("List");
        public static KeywordToken Cons = new KeywordToken("Cons");
        public static KeywordToken Eq = new KeywordToken("Eq");
        public static KeywordToken Elt = new KeywordToken("Elt");
        public static KeywordToken Nil = new KeywordToken("Nil");
        public static KeywordToken True = new KeywordToken("True");
        public static KeywordToken If = new KeywordToken("If");
        public static KeywordToken False = new KeywordToken("False");
        public static KeywordToken Add = new KeywordToken("+");
        public static KeywordToken Substract = new KeywordToken("-");
        public static KeywordToken Muliply = new KeywordToken("*");
        public static KeywordToken Divide = new KeywordToken("/");
        public static KeywordToken Equal = new KeywordToken("=");
        public static KeywordToken NotEqual = new KeywordToken("!=");
        public static KeywordToken GreaterThan = new KeywordToken(">");
        public static KeywordToken LessThan = new KeywordToken("<");
        public static KeywordToken And = new KeywordToken("And");
        public static KeywordToken Or = new KeywordToken("Or");
        public static KeywordToken Not = new KeywordToken("Not");

        public override bool IsKeywordToken {
            get {return true;}
        }

        internal static KeywordToken GetKeywordToken(string name) {
            return _keywords[name.ToLower()];
        }

        internal static bool IsKeywordName(string id) {
            return _keywords.ContainsKey(id.ToLower());
        }
    } //KeywordToken


    // SyntaxTokenKind is used for debugging.  The parser does identity check on
    // SyntaxToken members.
    //
	internal enum SyntaxTokenKind {
        Paren,
        CloseParen,
        EOF,
        Quote,
        Dot,
    }

    internal class SyntaxToken : Token {
        private SyntaxTokenKind _kind;

        private SyntaxToken(SyntaxTokenKind kind) {
            _kind = kind;
        }

        public override string ToString() {
            return "<SyntaxToken " + _kind.ToString() + ">";
        }

        public static SyntaxToken Paren = new SyntaxToken(SyntaxTokenKind.Paren);
        public static SyntaxToken CloseParen = 
            new SyntaxToken(SyntaxTokenKind.CloseParen);
        public static SyntaxToken EOF = new SyntaxToken(SyntaxTokenKind.EOF);
        public static SyntaxToken Quote = new SyntaxToken(SyntaxTokenKind.Quote);
        public static SyntaxToken Dot = new SyntaxToken(SyntaxTokenKind.Dot);
    }
}
