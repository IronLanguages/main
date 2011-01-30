

class Lexer (object):
    def __init__ (self, reader):
        self.Reader = reader
        self._putToken = None

    def PutToken (self, token):
        debugprint("puttoken: " + str(token))
        if self._putToken is not None:
            error("Internal Error: putting token when there is one?")
        self._putToken = token

    _numChars = [chr(x) for x in xrange(ord('0'), ord('9') + 1)]

    ### GetToken is one of two main entry points to Lexer.
    ###
    def GetToken (self):
        token = self._getToken()
        debugprint("getoken: " + str(token))
        return token

    ### Returns any saved _putToken, else skips whitespace and returns next
    ### token from input stream.
    ###
    ### If returning token directly based on char, need to gobble char, but if
    ### calling helper function to read more, then they gobble as needed.
    ###
    ### Need to maintain src loc info and store in token.
    ###
    def _getToken (self):
        if self._putToken is not None:
            tmp = self._putToken
            self._putToken = None
            debugprint("returning put token" + str(tmp))
            return tmp
        self._skipWhitespace()
        ch = self._peekChar()
        debugprint("gettoken: peek char is " + 
                   ((ch is SyntaxToken.EOF and "EOF") or ch))
        if ch is SyntaxToken.EOF:
            return ch
        elif ch == '(':
            self._getChar()
            return SyntaxToken.Paren
        elif ch == ')':
            self._getChar()
            return SyntaxToken.CloseParen
        elif ch in self._numChars:
            return self._getNumber()
        elif ch == '"':
            return self._getString()
        elif ch == "'":
            self._getChar()
            return SyntaxToken.Quote
        elif ch == '-':
            return self._getIdOrNumber()
        elif self._startsId(ch):
            return self._getIdOrKeyword()
        elif ch == '.':
            self._getChar()
            return SyntaxToken.Dot
        raise Exception("Internal: couldn't get token? -- char[" + 
                        str(ch) + "]")

    ### _getIdOrNumber expects a hyphen as the current char, and returns an Id
    ### or number token.
    ###
    def _getIdOrNumber (self):
        ch = self._getChar()
        if ch is SyntaxToken.EOF or ch != '-':
            raise Exception("Internal: parsing ID or number without hyphen.")
        ch = self._peekChar()
        if ch in self._numChars:
            token = self._getNumber()
            token.Value = - token.Value
            return token
        elif not self._isIdTerminator(ch):
            return self._getIdOrKeyword("-")
        else:
            return self._makeIdOrKeywordToken("-")

    ### GetIdOrKeyword has first param to handle call from GetIdOrNumber where
    ### ID started with a hyphen (and looked like a number).  Didn't want to
    ### add hyphen to StartId test since it normally appears as the keyword
    ### minus.
    ###
    ### Must not call when the next char is EOF.
    ###
    def _getIdOrKeyword (self, first = None):
        ## Process first and set initial result.
        quotedId = False
        first = first or self._getChar()
        if first is SyntaxToken.EOF or not self._startsId(first):
            raise Exception("Internal: getting Id or keyword?")
        if first == '\\':
            quotedId = True
            res = self._getChar()
            if res is SyntaxToken.EOF:
                raise Exception("Unexpected EOF when getting Id.")
            if not self._startsId(first):
                raise Exception("Don't support quoted Ids that have " +
                                "non Id constituent characters.")
        else:
            res = first
        ## See if there's more chars to Id
        c = self._peekChar()
        while (c is not SyntaxToken.EOF) and (not self._isIdTerminator(c)):
            res = res + c
            self._getChar()
            c = self._peekChar()
        return self._makeIdOrKeywordToken(res, quotedId)

    ### Keep whatever casing we found in the source program so that when the
    ### IDs are used a member names and metadata on binders, then if some MO
    ### doesn't respect the IgnoreCase flag, there's an out for still binding.
    ###
    def _makeIdOrKeywordToken (self, name, quotedId = False):
        if not quotedId and KeywordToken.IsKeywordName(name):
            return KeywordToken.GetKeywordToken(name)
        else:
            if name.lower() == "let":
                print "\nWARNING: using 'let'?  You probaby meant let*.\n"
            return IdOrKeywordToken(name, False)
    
    ### Must not be called on SyntaxToken.EOF
    def _startsId (self, c):
        return c == '\\' or not self._isIdTerminator(c)

    ## Restrict macro syntax chars in case try to add macros later, but need to
    ## allow backquote in IDs to support .NET type names that come from
    ## reflection.  We can fix this later with more machinery around type names.
    _id_terminators = ['(', ')', '"', ";", ',', '@', "'", "."] #'`', 
    
    def _isIdTerminator (self, c):
        return (c in self._id_terminators) or (ord(c) < 33)


    ### _getNumber returns parsed integers as NumberTokens.  Need to update
    ### and use .NET's System.Double.Parse after scanning to non-constituent
    ### char.
    ###
    def _getNumber (self):
        ## Check integrity before loop to avoid incidentally returning zero.
        c = self._getChar()
        if c is SyntaxToken.EOF or c not in self._numChars:
            raise Exception("Internal: lexing number?")
        digit = ord(c) - ord('0')
        res = digit
        c = self._peekChar()
        while (c is not SyntaxToken.EOF and c in self._numChars):
            res = (res * 10) + (ord(c) - ord('0'))
            self._getChar()
            c = self._peekChar()
        return NumberToken(res)

    def _getString (self):
        c = self._getChar()
        if c is SyntaxToken.EOF or c != '"':
            raise Exception("Internal: parsing string?")
        res = ""
        escape = False
        c = self._peekChar()
        while True:
            if c is SyntaxToken.EOF:
                raise Exception("Hit EOF in string literal.")
            elif c == '\n' or c == '\r':
                raise Exception("Hit newline in string literal")
            elif c == '\\' and not escape:
                self._getChar()
                escape = True
            elif c == '"' and not escape:
                self._getChar()
                return StringToken(res)
            elif escape:
                escape = False
                self._getChar()
                if c == 'n':
                    res = res + "\n"
                elif c == 't':
                    res = res + "\t"
                elif c == 'r':
                    res = res = "\r"
                elif c == '"':
                    res = res + c
                elif c == '\\':
                    res = res + c
            else:
                self._getChar()
                res = res + c
            c = self._peekChar()

    _whitespaceChars = [' ', '\r', '\n', ';', '\t']

    def _skipWhitespace (self):
        ch = self._peekChar()
        while ch in self._whitespaceChars:
            debugprint("gobble: " + ch)
            if ch == ';':
                self._getChar()
                ch = self._peekChar()
                ## If newline seq is two chars, second gets eaten in outer loop.
                while ch != '\n' and ch != '\r':
                    if ch is SyntaxToken.EOF: return
                    self._getChar()
                    ch = self._peekChar()
            else:
                self._getChar()
                ch = self._peekChar()

    def _getChar (self):
        c = self.Reader.Read()
        if c == -1: return SyntaxToken.EOF
        #debugprint("_getChar: " + chr(c))
        return chr(c)

    def _peekChar (self):
        c = self.Reader.Peek()
        if c == -1: return SyntaxToken.EOF
        #debugprint("_peekChar: " + chr(c))
        return chr(c)


class Token (object):
    def __init__ (self, srcloc):
        self.SrcLoc = srcloc
    pass

class LiteralToken (Token):
    def __init__ (self, val, srcloc = (0, 0)):
        Token.__init__(self, srcloc)
        self.Value = val

class NumberToken (LiteralToken):
    def __init__ (self, num, srcloc = (0, 0)):
        LiteralToken.__init__(self, num, srcloc)

class StringToken (LiteralToken):
    def __init__ (self, string, srcloc = (0, 0)):
        LiteralToken.__init__(self, string, srcloc)

### IdOrKeywordToken represents identifier.  A subtype, KeywordToken, reps
### keywords.  The parser handles when keywords can be used like identifiers,
### for example, as .NET members when importing and renaming, literal kwd
### constants (nil, true, false), etc.  These are used also when parsing list
### literals before they get converted to runtime Symbol types by etgen.
###
class IdOrKeywordToken (Token):
    def __init__ (self, name, kwd, srcloc = (0, 0)):
        Token.__init__(self, srcloc)
        self.Name = name
        self.IsKeywordToken = kwd
    def __repr__ (self):
        if self.IsKeywordToken:
            return "<Kwd: " + self.Name + ">"
        else:
            return "<Id: " + self.Name + ">"


class KeywordToken (IdOrKeywordToken):
    def __init__ (self, name, srcloc = (0, 0)):
        IdOrKeywordToken.__init__(self, name, True, srcloc)
        self.Name = name
        KeywordToken._keywords[name.lower()] = self
    
    _keywords = {}
    
    @staticmethod
    def IsKeywordName (name):
        return name.lower() in KeywordToken._keywords
    
    @staticmethod
    def GetKeywordToken (name):
        return KeywordToken._keywords[name.lower()]

KeywordToken.Import = KeywordToken("Import")
KeywordToken.Defun = KeywordToken('Defun')
KeywordToken.Lambda = KeywordToken('Lambda')
KeywordToken.Defclass = KeywordToken('DefClass')
KeywordToken.Defmethod = KeywordToken('DefMethod')
KeywordToken.New = KeywordToken('New')
KeywordToken.Set = KeywordToken('Set')
KeywordToken.LetStar = KeywordToken('Let*')
KeywordToken.Block = KeywordToken('Block')
KeywordToken.Loop = KeywordToken('Loop')
KeywordToken.Break = KeywordToken('Break')
KeywordToken.Continue = KeywordToken('Continue')
KeywordToken.Return = KeywordToken('Return')
KeywordToken.List = KeywordToken('List')
KeywordToken.Cons = KeywordToken('Cons')
KeywordToken.Eq = KeywordToken('Eq')
KeywordToken.Elt = KeywordToken('Elt')
KeywordToken.Nil = KeywordToken('Nil')
KeywordToken.True = KeywordToken('True')
KeywordToken.False = KeywordToken('False')
KeywordToken.If = KeywordToken('If')
KeywordToken.Add = KeywordToken('+')
KeywordToken.Subtract = KeywordToken('-')
KeywordToken.Multiply = KeywordToken('*')
KeywordToken.Divide = KeywordToken('/')
KeywordToken.Equal = KeywordToken('=')
KeywordToken.NotEqual = KeywordToken('!=')
KeywordToken.GreaterThan = KeywordToken('>')
KeywordToken.LessThan = KeywordToken('<')
KeywordToken.And = KeywordToken('And')
KeywordToken.Or = KeywordToken('Or')
KeywordToken.Not = KeywordToken('Not')



class SyntaxToken (Token):
    def __init__ (self, name):
        self.Name = name
    def __str__ (self): return self.Name
    def __repr__ (self): return "<SyntaxToken " + self.Name + ">"
SyntaxToken.Paren = SyntaxToken('Paren')
SyntaxToken.CloseParen = SyntaxToken('CloseParen')
SyntaxToken.EOF = SyntaxToken('EOF')
SyntaxToken.Quote = SyntaxToken('Quote')
SyntaxToken.Dot = SyntaxToken('Dot')


##################
### Dev-time Utils
##################

_debug = False
def debugprint (*stuff):
    if _debug:
        for x in stuff:
            print x,
        print
_debug = False
