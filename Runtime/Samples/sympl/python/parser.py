
import lexer

### Only needed for _getOpKind
import clr
if clr.use35:
    clr.AddReference("Microsoft.Scripting.Core")
    from Microsoft.Scripting.Ast import ExpressionType
else:
    clr.AddReference("System.Core")
    from System.Linq.Expressions import ExpressionType

class Parser (object):
    pass

### ParseFile returns a list of top-level expressions parsed in the
### StreamReader.
###
def ParseFile (reader):
    body = []
    lex = lexer.Lexer(reader)
    token = lex.GetToken()
    while token is not lexer.SyntaxToken.EOF:
        lex.PutToken(token)
        body.append(_parseExpr(lex))
        token = lex.GetToken()
    return body

### Parse returns a single expression parsed from the StreamReader.
###
def ParseExpr (reader):
    return _parseExpr(lexer.Lexer(reader))

### _parseExpr parses an expression from the Lexer passed in.
###
def _parseExpr (lexr):
    token = lexr.GetToken()
    debugprint("_parseExpr: token= " + str(token))
    res = None
    if token is lexer.SyntaxToken.EOF:
        raise Exception("Unexpected EOF encountered while parsing expression.")
    if token is lexer.SyntaxToken.Quote:
        lexr.PutToken(token)
        res = _parseQuoteExpr(lexr)
    elif token is lexer.SyntaxToken.Paren:
        lexr.PutToken(token)
        res = _parseForm(lexr)
    elif isinstance(token, lexer.IdOrKeywordToken):
        ## If we encounter literal kwd constants, they get turned into ID
        ## Exprs.  Code that accepts Id Exprs, needs to check if the token is
        ## kwd or not when it matters.
        if (token.IsKeywordToken and
            token not in [lexer.KeywordToken.Nil, lexer.KeywordToken.True,
                          lexer.KeywordToken.False]):
            raise Exception("Keyword cannot be an expression: " + 
                            token.Name)
        else:
            res = SymplIdExpr(token)
    elif isinstance(token, lexer.LiteralToken):
        res = SymplLiteralExpr(token.Value)
    ## Check for dotted expr.
    if res is not None:
        next = lexr.GetToken()
        lexr.PutToken(next)
        if next is lexer.SyntaxToken.Dot:
            return _parseDottedExpr(lexr, res)
        else:
            return res
    raise Exception("Unexpected token when expecting "+ 
                    "beginning of expression -- " + str(token) + " ... " +
                    repr([lexr.GetToken(), lexr.GetToken(),
                          lexr.GetToken(), lexr.GetToken(),
                          lexr.GetToken(), lexr.GetToken()]))

### _parseForm parses a parenthetic form.  If the first token after the paren
### is a keyword, then it something like defun, loop, if, try, etc.  If the
### first sub expr is another parenthetic form, then it must be an expression
### that returns a callable object.
###
def _parseForm (lexr):
    debugprint("IN parseform:")
    token = lexr.GetToken()
    if token is not lexer.SyntaxToken.Paren:
        raise Exception("List expression must start with '('.")
    token = lexr.GetToken()
    debugprint("first form token: " + str(token))
    if isinstance(token, lexer.IdOrKeywordToken):
        lexr.PutToken(token)
        debugprint("parseform: " + str(token))
        if token.IsKeywordToken:
            # Defun, Let, Set, Import, ...
            return _parseKeywordForm(lexr)
        else:
            return _parseFunctionCall(lexr)
    #elif token is lexer.SyntaxToken.Paren:
    #    lexr.PutToken(token)
    #    return _parseFunctionCall(lexr)
    else:
        lexr.PutToken(token)
        return _parseFunctionCall(lexr)
        
        ## What else could start a function call?  Any Expr?
        #raise Exception("Sympl form must have ID or keyword as first element." +
        #                "  Got " + str(token))

### _parseKeywordForm parses parenthetic built in forms such as defun, if, loop,
### etc.
###
def _parseKeywordForm (lexr):
    debugprint("IN parse kwd form:")
    name = lexr.GetToken()
    if not isinstance(name, lexer.KeywordToken):
        raise Exception("Internal error: parsing keyword form?")
    lexr.PutToken(name)
    if name is lexer.KeywordToken.Import:
        return _parseImport(lexr)
    elif name is lexer.KeywordToken.Defun:
        return _parseDefun(lexr)
    elif name is lexer.KeywordToken.Lambda:
        return _parseLambda(lexr)
    elif name is lexer.KeywordToken.Set:
        return _parseSet(lexr)
    elif name is lexer.KeywordToken.LetStar:
        return _parseLetStar(lexr)
    elif name is lexer.KeywordToken.Block:
        return _parseBlock(lexr)
    elif name is lexer.KeywordToken.Eq:
        return _parseEq(lexr)
    elif name is lexer.KeywordToken.Cons:
        return _parseCons(lexr)
    elif name is lexer.KeywordToken.List:
        return _parseListCall(lexr)
    elif name is lexer.KeywordToken.If:
        return _parseIf(lexr)
    elif name is lexer.KeywordToken.Loop:
        return _parseLoop(lexr)
    elif name is lexer.KeywordToken.Break:
        return _parseBreak(lexr)
    elif name is lexer.KeywordToken.New:
        return _parseNew(lexr)
    elif name is lexer.KeywordToken.Elt:
        return _parseElt(lexr)
    elif (name is lexer.KeywordToken.Add or name is lexer.KeywordToken.Subtract or
          name is lexer.KeywordToken.Multiply or name is lexer.KeywordToken.Divide or
          name is lexer.KeywordToken.Equal or name is lexer.KeywordToken.NotEqual or
          name is lexer.KeywordToken.GreaterThan or 
          name is lexer.KeywordToken.LessThan or
          name is lexer.KeywordToken.And or name is lexer.KeywordToken.Or):
        return _parseExprTreeBinaryOp(lexr)
    elif name is lexer.KeywordToken.Not:
        return _parseExprTreeUnaryOp(lexr)
    raise Exception("Internal: unrecognized keyword form?")

def _parseDefun (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.Defun:
        raise Exception("Internal: parsing Defun?")
    name = lexr.GetToken()
    if not isinstance(name, lexer.IdOrKeywordToken) or name.IsKeywordToken:
        raise Exception("Defun must have an ID for name -- " + str(token))
    params = _parseParams(lexr, "Defun")
    body = _parseBody(lexr, "Hit EOF in function body -- " + name.Name)
    return SymplDefunExpr(name, params, body)

def _parseLambda (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.Lambda:
        raise Exception("Internal: parsing Lambda?")
    params = _parseParams(lexr, "Lambda")
    body = _parseBody(lexr, "Hit EOF in function body -- ")
    return SymplLambdaExpr(params, body)

### _parseParams parses sequence of vars for Defuns and Lambdas, and always
### returns a list of IdTokens.
###
def _parseParams (lexr, definer):
    token = lexr.GetToken()
    if token is not lexer.SyntaxToken.Paren:
        raise Exception(definer + " must have param list following keyword.")
    lexr.PutToken(token)
    return _ensureListOfIds(_parseList(lexr, "param list.").Elements, False,
                            definer + " params must be valid IDs.")

### _parseBody parses sequence of expressions as for Defun, Let, etc., as well
### as args to fun call.  This always returns a list, even if empty.  It
### gobbles the close paren too.
###
def _parseBody (lexr, errmsg):
    body = []
    token = lexr.GetToken()
    while (token is not lexer.SyntaxToken.EOF and
           token is not lexer.SyntaxToken.CloseParen):
        lexr.PutToken(token)
        body.append(_parseExpr(lexr))
        token = lexr.GetToken()
    if token is lexer.SyntaxToken.EOF:
        raise Exception(errmsg)
    return body


### (import id[.id]*  [{id | (id [id]*)}  [{id | (id [id]*)}]]  )
### (import file-or-dotted-Ids name-or-list-of-members reanme-or-list-of)
###
def _parseImport (lexr):
    if lexr.GetToken() is not lexer.KeywordToken.Import:
        raise Exception("Internal error: parsing Import call?")
    ns_or_module = _parseImportNameOrModule(lexr)
    members = _parseImportNames(lexr, "member names", True)
    as_names = _parseImportNames(lexr, "renames", False)
    if lexr.GetToken() is not lexer.SyntaxToken.CloseParen:
        raise Exception("Expected close paren for Import call.")
    if (len(members) != len(as_names)) and (len(as_names) != 0):
        raise Exception("Import as-names must be same form as member names.")
    return SymplImportExpr(ns_or_module, members, as_names)

### Parses dotted namespaces or Sympl.Globals members to import.
###
def _parseImportNameOrModule (lexr):
    token = lexr.GetToken()
    if not isinstance(token, lexer.IdOrKeywordToken): # Keywords are ok here.
        raise Exception("Id must follow Import symbol.")
    dot = lexr.GetToken()
    if dot is lexer.SyntaxToken.Dot:
        lexr.PutToken(dot)
        tmp = _parseDottedExpr(lexr, SymplIdExpr(token))
        ns_or_module = []
        for e in [tmp.ObjectExpr] + tmp.Exprs:
            if not isinstance(e, SymplIdExpr): # Keywords are ok here too.
                raise Exception("Import targets must be dotted identifiers " +
                                "only -- " + str(e) + str(ns_or_module))
            ns_or_module.append(e.IdToken)
        token = lexr.GetToken()
    else:
        ns_or_module = [token]
        token = dot
    lexr.PutToken(token)
    return ns_or_module

### Parses list of member names to import from the object represented in the
### result of _parseImportNameOrModule, which will be a file module or object
### from Sympl.Globals.
###
def _parseImportNames (lexr, nameKinds, allowKeywords):
    token = lexr.GetToken()
    debugprint("IN parseimport: " + str(token))
    if (isinstance(token, lexer.IdOrKeywordToken) and 
        not token.IsKeywordToken):
        names = [token]
    elif token is lexer.SyntaxToken.Paren:
        lexr.PutToken(token)
        names = _parseList(lexr, "Import " + nameKinds + ".")
        names = _ensureListOfIds(names.Elements, allowKeywords,
                                   "Import " + nameKinds + " must be valid IDs.")
    elif token is lexer.SyntaxToken.CloseParen:
        names = []
        lexr.PutToken(token)
    else:
        raise Exception("Import takes dotted names, then member vars.")
    return names

def _ensureListOfIds (lst, allowKeywords, error_str):
    for elt in lst:
        if (not isinstance(elt, lexer.IdOrKeywordToken) or
            (not allowKeywords and elt.IsKeywordToken)):
            raise Exception(error_str)
    return lst

### _parseDottedExpr gathers infix dotted member access expressions.  The
### object expression can be anything and is passed in via expr.  Successive
### member accesses must be dotted identifier expressions or member invokes --
### a.b.(c 3).d.  The member invokes cannot have dotted expressions for the
### member name such as a.(b.c 3).
###
def _parseDottedExpr (lexr, expr):
    debugprint("IN parse dotted:")
    obj_expr = expr
    token = lexr.GetToken()
    debugprint("parse dotted: " + str(token))
    if token is not lexer.SyntaxToken.Dot:
        raise Exception("Internal error: parsing dotted expressions?")
    exprs = []
    token = lexr.GetToken()
    is_paren = token is lexer.SyntaxToken.Paren
    is_id = isinstance(token, lexer.IdOrKeywordToken) # Keywords ok as members.
    while (is_id or is_paren):
        ## Need to be fun call or IDs
        if is_id:
            expr = SymplIdExpr(token)
        else:
            lexr.PutToken(token)
            expr = _parseForm(lexr)
            if ((not isinstance(expr, SymplFunCallExpr)) or
                (not isinstance(expr.Function, SymplIdExpr))):
                raise Exception("Dotted expressions must be identifiers or " +
                                "function calls with identiers as the function " +
                                "value --" + str(expr))
        exprs.append(expr)
        token = lexr.GetToken()
        if token is not lexer.SyntaxToken.Dot:
            break
        token = lexr.GetToken()
        is_paren = token is lexer.SyntaxToken.Paren
        is_id = isinstance(token, lexer.IdOrKeywordToken)
    lexr.PutToken(token)
    return SymplDottedExpr(obj_expr, exprs)

### _parseSet parses a LHS expression and value expression.  All analysis on
### the LHS is in etgen.py.
###
def _parseSet (lexr):
    if lexr.GetToken() is not lexer.KeywordToken.Set:
        raise Exception("Internal error: parsing Set?")
    lhs = _parseExpr(lexr)
    val = _parseExpr(lexr)
    if lexr.GetToken() is not lexer.SyntaxToken.CloseParen:
        raise Exception("Expected close paren for Set expression.")
    return SymplAssignExpr(lhs, val)

### _parseLetStar parses (let* ((<var> <expr>)*) <body>).
###
def _parseLetStar (lexr):
    if lexr.GetToken() is not lexer.KeywordToken.LetStar:
        raise Exception("Internal error: parsing Let?")
    token = lexr.GetToken()
    if token is not lexer.SyntaxToken.Paren:
        raise Exception("Let expression has no bindings?  Missing '('.")
    ## Get bindings
    bindings = []
    token = lexr.GetToken()
    while token is lexer.SyntaxToken.Paren:
        var = _parseExpr(lexr)
        if not isinstance(var, SymplIdExpr) or var.IdToken.IsKeywordToken:
            raise Exception("Let* binding must be (<ID> <expr>) -- " +
                            str(var))
        init = _parseExpr(lexr)
        bindings.append((var.IdToken, init))
        token = lexr.GetToken()
        if token is not lexer.SyntaxToken.CloseParen:
            raise Exception("Let binding missing close paren -- " +
                            str(token))
        token = lexr.GetToken()
    if token is not lexer.SyntaxToken.CloseParen:
        raise Exception("Let* bindings missing close paren.")
    body = _parseBody(lexr, "Unexpected EOF in Let.")
    return SymplLetStarExpr(bindings, body)

### _parseBlock parses a block expression, a sequence of exprs to
### execute in order, returning the last expression's value.
###
def _parseBlock (lexr):
    if lexr.GetToken() is not lexer.KeywordToken.Block:
        raise Exception("Internal error: parsing Block?")
    body = _parseBody(lexr, "Unexpected EOF in Block.")
    return SymplBlockExpr(body)

### first sub form must be expr resulting in callable, but if it is dotted expr,
### then eval the first N-1 dotted exprs and use invoke member or get member
### on last of dotted exprs so that the 2..N sub forms are the arguments to
### the invoke member.  It's as if the call breaks into a block of a temp
### assigned to the N-1 dotted exprs followed by an invoke member (or a get
### member and call, which the runtime binder decides).  The non-dotted expr
### simply evals to an object that better be callable with the supplied args,
### which may be none.
### 
def _parseFunctionCall (lexr):  
    debugprint("IN parse fun call:")
    ## First sub expr is callable object or invoke member expr.
    fun = _parseExpr(lexr)
    if ((type(fun) is SymplDottedExpr) and
        (not isinstance(fun.Exprs[-1], SymplIdExpr))): #Keywords ok as members.
        raise Exception("Function call with dotted expression for function " +
                        "must end with ID Expr, not member invoke. " +
                        str(fun.Exprs[-1]))
    ## Tail exprs are args.
    args = _parseBody(lexr, "Unexpected EOF in arg list for " + str(fun))
    return SymplFunCallExpr(fun, args)

### This parses a quoted list, ID/keyword, or literal.
###
def _parseQuoteExpr (lexr):
    token = lexr.GetToken()
    if token is not lexer.SyntaxToken.Quote:
        raise Exception("Internal: parsing Quote?.")
    token = lexr.GetToken()
    if token is lexer.SyntaxToken.Paren:
        lexr.PutToken(token)
        expr = _parseList(lexr, "quoted list.")
    elif (isinstance(token, lexer.IdOrKeywordToken) or
          isinstance(token, lexer.LiteralToken)):
        expr = token
    else:
        raise Exception("Quoted expression can only be list, ID/Symbol, or " +
                        "literal.")
    return SymplQuoteExpr(expr)

def _parseEq (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.Eq:
        raise Exception("Internal: parsing Eq?")
    left, right = _parseBinaryRuntimeCall(lexr)
    return SymplEqExpr(left, right)
    
def _parseCons (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.Cons:
        raise Exception("Internal: parsing Cons?")
    left, right = _parseBinaryRuntimeCall(lexr)
    return SymplConsExpr(left, right)

### _parseBinaryRuntimeCall parses two exprs and a close paren, returning the
### two exprs.
###
def _parseBinaryRuntimeCall (lexr):
    left = _parseExpr(lexr)
    right = _parseExpr(lexr)
    if lexr.GetToken() is not lexer.SyntaxToken.CloseParen:
        raise Exception("Expected close paren for binary op or eq call.")
    return (left, right)

### _parseListCall parses a call to the List built-in keyword form that takes
### any number of arguments.
###
def _parseListCall (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.List:
        raise Exception("Internal: parsing List call?")
    args = _parseBody(lexr, "Unexpected EOF in arg list for call to List.")
    return SymplListCallExpr (args)

def _parseIf (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.If:
        raise Exception("Internal: parsing If?")
    args = _parseBody(lexr, "Unexpected EOF in If form.")
    argslen = len(args)
    if argslen == 2:
        return SymplIfExpr(args[0], args[1], None)
    elif argslen == 3:
        return SymplIfExpr(args[0], args[1], args[2])
    else:
        raise Exception("IF must be (if <test> <consequent> [<alternative>]).")
    
### _parseLoop parses a loop expression, a sequence of exprs to
### execute in order, forever.  See Break for returning expression's value.
###
def _parseLoop (lexr):
    if lexr.GetToken() is not lexer.KeywordToken.Loop:
        raise Exception("Internal error: parsing Loop?")
    body = _parseBody(lexr, "Unexpected EOF in Loop.")
    return SymplLoopExpr(body)

### _parseBreak parses a Break expression, which has an optional value that
### becomes a loop expression's value.
###
def _parseBreak (lexr):
    if lexr.GetToken() is not lexer.KeywordToken.Break:
        raise Exception("Internal error: parsing Break?")
    token = lexr.GetToken()
    if token == lexer.SyntaxToken.CloseParen:
        value = None
    else:
        lexr.PutToken(token)
        value = _parseExpr(lexr)
        token = lexr.GetToken()
        if token != lexer.SyntaxToken.CloseParen:
            raise Exception("Break expression missing close paren.")
    return SymplBreakExpr(value)

### Parse a New form for creating instances of types.  Second sub expr (one
### after kwd New) evals to a type.
###
### Consider adding a new kwd form generic-type-args that could be the third
### sub expr and take any number of sub exprs that eval to types.  These could
### be used to specific concrete generic type instances.  Without this support
### SymPL programmers need to open code this as the examples show.
###
def _parseNew (lexr):  
    debugprint("IN new call:")
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.New:
        raise Exception("Internal: parsing New?")
    typ = _parseExpr(lexr)
    args = _parseBody(lexr, "Unexpected EOF in arg list for New" + str(typ))
    return SymplNewExpr(typ, args)


def _parseElt (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.Elt:
        raise Exception("Internal: parsing Elt?")
    obj = _parseExpr(lexr)
    indexes = _parseBody(lexr, "Unexpected EOF in arg list for call to Elt.")
    return SymplEltExpr(obj, indexes)
  

### _parseExprTreeBinaryOp handles operators that map to ET node kinds, but it
### doesn't handle Eq.  We could fold that in here, but it is harder to do in C#.
###
def _parseExprTreeBinaryOp (lexr):
    token = lexr.GetToken()
    if (token is lexer.KeywordToken.Add or token is lexer.KeywordToken.Subtract or
        token is lexer.KeywordToken.Multiply or token is lexer.KeywordToken.Divide or
        token is lexer.KeywordToken.Equal or token is lexer.KeywordToken.NotEqual or
        token is lexer.KeywordToken.GreaterThan or 
        token is lexer.KeywordToken.LessThan or
        token is lexer.KeywordToken.And or token is lexer.KeywordToken.Or):
        pass
    else:
        raise Exception("Internal: parsing Binary?")
    left, right = _parseBinaryRuntimeCall(lexr)
    return SymplBinaryExpr(_getOpKind(token), left, right)

def _parseExprTreeUnaryOp (lexr):
    token = lexr.GetToken()
    if token is not lexer.KeywordToken.Not:
        raise Exception("Internal: unrecognized unary op")
    operand = _parseExpr(lexr)
    if lexr.GetToken() is not lexer.SyntaxToken.CloseParen:
        raise Exception("Expected close paren for unary op call.")
    return SymplUnaryExpr(_getOpKind(token), operand)

def _getOpKind (token):
    if token is lexer.KeywordToken.Add:
        return ExpressionType.Add
    if token is lexer.KeywordToken.Subtract:
        return ExpressionType.Subtract
    if token is lexer.KeywordToken.Multiply:
        return ExpressionType.Multiply
    if token is lexer.KeywordToken.Divide:
        return ExpressionType.Divide
    if token is lexer.KeywordToken.Equal:
        return ExpressionType.Equal
    if token is lexer.KeywordToken.NotEqual:
        return ExpressionType.NotEqual
    if token is lexer.KeywordToken.GreaterThan:
        return ExpressionType.GreaterThan
    if token is lexer.KeywordToken.LessThan:
        return ExpressionType.LessThan
    if token is lexer.KeywordToken.And:
        return ExpressionType.And
    if token is lexer.KeywordToken.Or:
        return ExpressionType.Or
    if token is lexer.KeywordToken.Not:
        return ExpressionType.Not
    raise Exception("Internal: can't map to ET node kind Op.")


### This parses pure list and atom structure.  Atoms are IDs, strs, and nums.
### Need quoted form of dotted exprs, quote, etc., if want to have macros one
### day.  This is used for Import name parsing, Defun/Lambda params, and quoted
### lists.
###
def _parseList (lexr, errStr):
    debugprint("IN parse list")
    token = lexr.GetToken()
    if token is not lexer.SyntaxToken.Paren:
        raise Exception("List expression must start with '('.")
    token = lexr.GetToken()
    res = []
    while ((token != lexer.SyntaxToken.EOF) and
           (token != lexer.SyntaxToken.CloseParen)):
        lexr.PutToken(token)
        elt = None
        if token is lexer.SyntaxToken.Paren:
            elt = _parseList(lexr, errStr)
        elif (isinstance(token, lexer.IdOrKeywordToken) or
              isinstance(token, lexer.LiteralToken)):
            elt = token
            lexr.GetToken()
        elif  token is lexer.SyntaxToken.Dot:
            raise Exception("Can't have dotted syntax in " + errStr)
        else:
            raise Exception("Unexpected token in list -- " + repr(token))
        if elt is None:
            raise Exception("Internal: no next element in list?")
        res.append(elt)
        token = lexr.GetToken()
    if token is lexer.SyntaxToken.EOF:
        raise Exception("Unexpected EOF encountered while parsing list.")
    return SymplListExpr(res)



#####################
### SymplExpr Classes
#####################

class SymplExpr (object):
    pass

### SymplIdExpr represents identifiers, but the IdToken can be a keyword
### sometimes.  For example, in quoted lists, import expressions, and as
### members of objects in dotted exprs.  Need to check for .IsKeywordToken
### when it matters.
###
class SymplIdExpr (SymplExpr):
    def __init__ (self, id):
        self.IdToken = id
    def __repr__ (self):
        return "<IdExpr " + self.IdToken.Name + ">"

class SymplListExpr (SymplExpr):
    def __init__ (self, subexprs):
        ## subexprs is always a list of tokens or SymplListExprs.
        self.Elements = subexprs
    def __repr__ (self):
        return "<ListExpr " + str(self.Elements) + ">"

class SymplFunCallExpr (SymplExpr):
    def __init__ (self, fun, args):
        self.Function = fun
        ## args is always a list.
        self.Arguments = args
    def __repr__ (self):
        return ("<Funcall ( " + repr(self.Function) + " " +
                repr(self.Arguments) + " )>")

class SymplDefunExpr (SymplExpr):
    def __init__ (self, name, params, body):
        self.Name = name
        ## params and body are always lists.
        self.Params = params
        self.Body = body
    def __repr__ (self):
        return ("<Defun " + str(self.Name) + " (" +
                repr(self.Params) + ") ...>")

class SymplLambdaExpr (SymplExpr):
    def __init__ (self, params, body):
        ## params and body are always lists.
        self.Params = params
        self.Body = body
    def __repr__ (self):
        return ("<Lambda " + " (" + repr(self.Params) + ") ...>")

### Used to represent numbers and strings, but not Quote.
class SymplLiteralExpr (SymplExpr):
    def __init__ (self, val):
        self.Value = val
    def __repr__ (self):
        return "<LiteralExpr " + str(self.Value) + ">"

class SymplDottedExpr (SymplExpr):
    def __init__ (self, obj, exprs):
        self.ObjectExpr = obj
        ## exprs is always a list of SymplIdExprs or SymplFunCallExprs,
        ## ending with a SymplIdExpr when used as SymplFunCallExpr.Function.
        self.Exprs = exprs
    def __repr__ (self):
        return ("<DotExpr " + str(self.ObjectExpr) + " . " + str(self.Exprs)
                + " >")

class SymplImportExpr (SymplExpr):
    def __init__ (self, ns_or_module, members, as_names):
        ## All properties are always lists.
        self.NamespaceExpr = ns_or_module
        self.MemberNames = members
        self.Renames = as_names

class SymplAssignExpr (SymplExpr):
    def __init__ (self, lhs, value):
        self.Location = lhs
        self.Value = value

class SymplLetStarExpr (SymplExpr):
    def __init__ (self, vars, body):
        ## bindings and body are always lists.
        self.Bindings = vars # List of tuples: (idtoken, expr)
        self.Body = body
    def __repr__ (self):
        return ("<Let* (" + repr(self.Bindings) + ") ...)>")

class SymplBlockExpr (SymplExpr):
    def __init__ (self, body):
        ## body is always a list of SymplExpr.
        self.Body = body
    def __repr__ (self):
        return ("<Block ...)>")

class SymplEltExpr (SymplExpr):
    def __init__ (self, obj, indexes):
        self.ObjectExpr = obj
        self.Indexes = indexes

class SymplQuoteExpr (SymplExpr):
    def __init__ (self, expr):
        ## Expr must be SymplListExpr, SymplIdExpr, or SymplLIteralExpr
        self.Expr = expr

class SymplEqExpr (SymplExpr):
    def __init__ (self, left, right):
        self.Left = left
        self.Right = right

class SymplConsExpr (SymplExpr):
    def __init__ (self, left, right):
        self.Left = left
        self.Right = right

class SymplListCallExpr (SymplExpr):
    def __init__ (self, args):
        self.Elements = args

class SymplIfExpr (SymplExpr):
    def __init__ (self, test, consequent, alternative):
        self.Test = test
        self.Consequent = consequent
        self.Alternative = alternative

class SymplLoopExpr (SymplExpr):
    def __init__ (self, body):
        ## body is always a list of SymplExpr.
        self.Body = body
    def __repr__ (self):
        return ("<Loop ...)>")

class SymplBreakExpr (SymplExpr):
    def __init__ (self, value):
        ## SymplExpr or None.
        self.Value = value
    def __repr__ (self):
        return ("<Break ...)>")

class SymplNewExpr (SymplExpr):
    def __init__ (self, fun, args):
        self.Typ = fun
        ## args is always a list.
        self.Arguments = args
    def __repr__ (self):
        return ("<New ( " + repr(self.Typ) + " " +
                repr(self.Arguments) + " )>")

class SymplBinaryExpr (SymplExpr):
    def __init__ (self, op, left, right):
        self.Op = op
        self.Left = left
        self.Right = right

class SymplUnaryExpr (SymplExpr):
    def __init__ (self, op, operand):
        self.Operand = operand
        self.Op = op



##################
### Dev-time Utils
##################

_debug = False
def debugprint (*stuff):
    if _debug:
        for x in stuff:
            print x,
        print
