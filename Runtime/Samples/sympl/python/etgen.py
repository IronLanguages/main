
import clr
if clr.use35:
    clr.AddReference("Microsoft.Scripting.Core")
    import Microsoft.Scripting.Ast as Exprs
else:
    clr.AddReference("System.Core")
    import System.Linq.Expressions as Exprs

from System.Collections.Generic import IEnumerable
from System.Dynamic import CallInfo

import System

import parser
import runtime
import lexer

### AnalyzeExpr performs semantic checkind and name binding on the expression.
### It returns an Expression.
###
#def AnalyzeExpr (expr, scope):
#    expr.Analyze(scope)
#    dict[type(expr)](scope)
#
#def AnalyzeImportExpr (self, scope):
#    pass
#parser.SymplImportExpr.Analyze = AnalyzeImportExpr
###
def AnalyzeExpr (expr, scope):
    exprtype = type(expr)
    debugprint("exprtype: ", exprtype)
    if exprtype is parser.SymplImportExpr:
        return AnalyzeImportExpr(expr, scope)
    elif exprtype is parser.SymplFunCallExpr:
        return AnalyzeFunCallExpr(expr, scope)
    elif exprtype is parser.SymplDefunExpr:
        return AnalyzeDefunExpr(expr, scope)
    elif exprtype is parser.SymplLambdaExpr:
        return AnalyzeLambdaExpr(expr, scope)
    elif exprtype is parser.SymplIdExpr:
        return AnalyzeIdExpr(expr, scope)
    elif exprtype is parser.SymplQuoteExpr:
        return AnalyzeQuoteExpr(expr, scope)
    elif exprtype is parser.SymplLiteralExpr:
        return Exprs.Expression.Constant(expr.Value)
    elif exprtype is parser.SymplAssignExpr:
        return AnalyzeAssignExpr(expr, scope)
    elif exprtype is parser.SymplLetStarExpr:
        return AnalyzeLetStarExpr(expr, scope)
    elif exprtype is parser.SymplBlockExpr:
        return AnalyzeBlockExpr(expr, scope)
    elif exprtype is parser.SymplEqExpr:
        return AnalyzeEqExpr(expr, scope)
    elif exprtype is parser.SymplConsExpr:
        return AnalyzeConsExpr(expr, scope)
    elif exprtype is parser.SymplListCallExpr:
        return AnalyzeListCallExpr(expr, scope)
    elif exprtype is parser.SymplIfExpr:
        return AnalyzeIfExpr(expr, scope)
    elif exprtype is parser.SymplDottedExpr:
        return AnalyzeDottedExpr(expr, scope)
    elif exprtype is parser.SymplLoopExpr:
        return AnalyzeLoopExpr(expr, scope)
    elif exprtype is parser.SymplBreakExpr:
        return AnalyzeBreakExpr(expr, scope)
    elif exprtype is parser.SymplEltExpr:
        return AnalyzeEltExpr(expr, scope)
    elif exprtype is parser.SymplNewExpr:
        return AnalyzeNewExpr(expr, scope)
    elif exprtype is parser.SymplBinaryExpr:
        return AnalyzeBinaryExpr(expr, scope)
    elif exprtype is parser.SymplUnaryExpr:
        return AnalyzeUnaryExpr(expr, scope)
    else:
        raise Exception("Internal: no expression to analyze -- " +
                        repr(expr))

### Returns a call to the import runtime helper function.
###
def AnalyzeImportExpr (expr, scope):
    debugprint("analyze import ...")
    if type(expr) is not parser.SymplImportExpr:
        raise Exception("Internal: need import expr to analyze.")
    if not scope.IsModule():
        raise Exception("Import expression must be a top level expression.")
    return runtime.MakeSymplImportCall(scope.RuntimeExpr, scope.ModuleExpr,
                                       expr.NamespaceExpr, expr.MemberNames,
                                       expr.Renames)

def AnalyzeDefunExpr (expr, scope):
    debugprint("analyze defun ...", expr.Name.Name)
    if type(expr) is not parser.SymplDefunExpr:
        raise Exception("Internal: need defun to analyze.")
    if not scope.IsModule():
        raise Exception("Use Defmethod or Lambda when not defining " +
                        "top-level function.")
    return Exprs.Expression.Dynamic(
               scope.GetRuntime().GetSetMemberBinder(expr.Name.Name), 
               object,
               [scope.ModuleExpr, 
                AnalyzeLambdaDef(expr, scope, "defun " + expr.Name.Name)])

def AnalyzeLambdaExpr (expr, scope):
    debugprint("analyze lambda ...")
    if type(expr) is not parser.SymplLambdaExpr:
        raise Exception("Internal: need lambda to analyze.")
    return AnalyzeLambdaDef(expr, scope, "lambda")

def AnalyzeLambdaDef (expr, scope, description):
    funscope = AnalysisScope(scope, description)
    funscope.IsLambda = True  # needed for return support.
    paramsInOrder = []
    for p in expr.Params:
        var = Exprs.Expression.Parameter(object, p.Name)
        paramsInOrder.append(var)
        funscope.Names[p.Name.lower()] = var
    ## No need to add fun name to module scope since recursive call just looks
    ## up global name late bound.  For lambdas,to get the effect of flet to
    ## support recursion, bind a variable to nil and then set it to a lambda.
    ## Then the lambda's body can refer to the let bound var in its def.
    body = []
    for e in expr.Body:
        body.append(AnalyzeExpr(e, funscope))
    return Exprs.Expression.Lambda(
               Exprs.Expression.GetFuncType(
                   System.Array[System.Type](
                       [object] * (len(expr.Params) + 1))),
               ## Due to .NET 4.0 co/contra-variance, IPy's binding isn't picking
               ## the overload with just IEnumerable<Expr>, so pick it explicitly.
               Exprs.Expression.Block.Overloads[IEnumerable[Exprs.Expression]](body),
               paramsInOrder)


### Returns a dynamic InvokeMember or Invoke expression, depending on the
### Function expression.
###
def AnalyzeFunCallExpr (expr, scope):
    debugprint("analyze function ...", expr.Function)
    if type(expr) is not parser.SymplFunCallExpr:
        raise Exception("Internal: need function call to analyze.")
    if type(expr.Function) is parser.SymplDottedExpr:
        if len(expr.Function.Exprs) > 1:
            objExpr = AnalyzeDottedExpr(
                          parser.SymplDottedExpr(expr.Function.ObjectExpr,
                                                 expr.Function.Exprs[:-1]),
                          scope)
        else:
            objExpr = AnalyzeExpr(expr.Function.ObjectExpr, scope)
        args = [AnalyzeExpr(a, scope) for a in expr.Arguments]
        return Exprs.Expression.Dynamic(
                   scope.GetRuntime().GetInvokeMemberBinder(
                      runtime.InvokeMemberBinderKey(
                         ## Last must be ID.
                         expr.Function.Exprs[-1].IdToken.Name,
                         CallInfo(len(args)))),
                   object,
                   [objExpr] + args)
    else:
        fun = AnalyzeExpr(expr.Function, scope)
        args = [AnalyzeExpr(a, scope) for a in expr.Arguments]
        ## Use DynExpr so that I don't always have to have a delegate to call,
        ## such as what happens with IPy interop.
        return Exprs.Expression.Dynamic(
                 scope.GetRuntime().GetInvokeBinder(CallInfo(len(args))),
                 object,
                 [fun] + args)

### Returns a chain of GetMember and InvokeMember dynamic expressions for
### the dotted expr.
###
def AnalyzeDottedExpr (expr, scope):
    debugprint("analyze dotted ...", expr.ObjectExpr)
    if type(expr) is not parser.SymplDottedExpr:
        raise Exception("Internal: need dotted expr to analyze.")
    curExpr = AnalyzeExpr(expr.ObjectExpr, scope)
    for e in expr.Exprs:
        if type(e) is parser.SymplIdExpr:
            tmp = Exprs.Expression.Dynamic(
                      scope.GetRuntime().GetGetMemberBinder(e.IdToken.Name),
                      object, [curExpr])
        elif type(e) is parser.SymplFunCallExpr:
            tmp = Exprs.Expression.Dynamic(
                      scope.GetRuntime().GetInvokeMemberBinder(
                          runtime.InvokeMemberBinderKey(
                              ## Dotted exprs must be simple invoke members,
                              ## a.b.(c ...), that is, function is identifier.
                              e.Function.IdToken.Name,
                              CallInfo(len(e.Arguments)))),
                      object,
                      [curExpr] + e.Arguments)
        else:
            raise Exception("Internal: dotted must be IDs or Funs.")
        curExpr = tmp
    return curExpr

### AnalyzeAssignExpr handles IDs, indexing, and member sets.  IDs are either
### lexical or dynamic exprs on the module scope (ExpandoObject).  Everything
### else is dynamic.
###
def AnalyzeAssignExpr (expr, scope):
    debugprint("analyze expr ...", expr.Location)
    loctype = type(expr.Location)
    if loctype is parser.SymplIdExpr:
        lhs = AnalyzeExpr(expr.Location, scope)
        val = AnalyzeExpr(expr.Value, scope)
        param = _findIdDef(expr.Location.IdToken.Name, scope)
        if param is not None:
            return Exprs.Expression.Assign(
                       lhs,
                       Exprs.Expression.Convert(val, param.Type))
        else:
            tmp = Exprs.Expression.Parameter(object, "assignTmpForRes")
            return Exprs.Expression.Block([tmp], [
                       Exprs.Expression.Assign(
                           tmp,
                           Exprs.Expression.Convert(val, object)),
                       Exprs.Expression.Dynamic(
                           scope.GetRuntime().GetSetMemberBinder(
                               expr.Location.IdToken.Name), 
                           object,
                           [scope.GetModuleExpr(), tmp]),
                       tmp])
    elif loctype is parser.SymplEltExpr:
        obj = AnalyzeExpr(expr.Location.ObjectExpr, scope)
        args = [AnalyzeExpr(x, scope) for x in expr.Location.Indexes]
        args.append(AnalyzeExpr(expr.Value, scope))
        return Exprs.Expression.Dynamic(
                   scope.GetRuntime().GetSetIndexBinder(
                              CallInfo(len(expr.Location.Indexes))), 
                   object,
                   [obj] + args)
    elif loctype is parser.SymplDottedExpr:
        ## For now, one dot only.  Later, pick oflast dotted member
        ## access (like AnalyzeFunctionCall), and use a temp and block.
        if len(expr.Location.Exprs) > 1:
            raise Exception("Don't support assigning with more than simple " +
                            "dotted expression, o.foo.")
        if not isinstance(expr.Location.Exprs[0], parser.SymplIdExpr):
            raise Exception("Only support unindexed field or property when " +
                            "assigning dotted expression location.")
        return Exprs.Expression.Dynamic(
                   scope.GetRuntime().GetSetMemberBinder(
                       expr.Location.Exprs[0].IdToken.Name),
                   object,
                   [AnalyzeExpr(expr.Location.ObjectExpr, scope),
                    AnalyzeExpr(expr.Value, scope)])

### Return an Expression for referencing the ID.  If we find the name in the
### scope chain, then we just return the stored ParamExpr.  Otherwise, the
### reference is a dynamic member lookup on the root scope, a module object.
###
def AnalyzeIdExpr (expr, scope):
    debugprint("analyze ID ...", expr.IdToken.Name)
    if type(expr) is not parser.SymplIdExpr:
        raise Exception("Internal: need ID Expr to analyze.")
    if expr.IdToken.IsKeywordToken:
        if expr.IdToken is parser.lexer.KeywordToken.Nil:
            return Exprs.Expression.Constant(None, object)
        elif expr.IdToken is parser.lexer.KeywordToken.True:
            return Exprs.Expression.Constant(True)
        elif expr.IdToken is parser.lexer.KeywordToken.False:
            return Exprs.Expression.Constant(False)
        else:
            raise Exception("Internal: unrecognized keyword literal constant.")
    else:
        param = _findIdDef(expr.IdToken.Name, scope)
        if param is not None:
            return param
        else:
            return Exprs.Expression.Dynamic(
               scope.GetRuntime().GetGetMemberBinder(expr.IdToken.Name), 
               object,
               scope.GetModuleExpr())

### _findIdDef returns the ParameterExpr for the name by searching the scopes,
### or it returns None.
###
def _findIdDef (name, scope):
    curscope = scope
    name = name.lower()
    while curscope is not None and not curscope.IsModule():
        if name in curscope.Names:
            return curscope.Names[name]
        else:
            curscope = curscope.Parent
    if curscope is None:
        raise Exception("Got bad AnalysisScope chain with no module at end.")
    return None

### AnalyzeLetStar returns a Block with vars, each initialized in the order
### they appear.  Each var's init expr can refer to vars initialized before it.
### The Block's body is the Let*'s body.
###
def AnalyzeLetStarExpr (expr, scope):
    debugprint("analyze let* ...")
    if type(expr) is not parser.SymplLetStarExpr:
        raise Exception("Internal: need Let* Expr to analyze.")
    letscope = AnalysisScope(scope, "let*")
    ## Analyze bindings.
    inits = []
    varsInOrder = []
    for b in expr.Bindings:
        ## Need richer logic for mvbind
        var = Exprs.Expression.Parameter(object, b[0].Name)
        varsInOrder.append(var)
        inits.append(Exprs.Expression.Assign(
                        var, 
                        Exprs.Expression.Convert(AnalyzeExpr(b[1], letscope),
                                                 var.Type)))
        ## Add var to scope after analyzing init value so that init value
        ## references to the same ID do not bind to his uninitialized var.
        letscope.Names[b[0].Name.lower()] = var
    body = []
    for e in expr.Body:
        body.append(AnalyzeExpr(e, letscope))
    ## Order of vars to BlockExpr don't matter semantically, but may as well
    ## keep them in the order the programmer specified in case they look at the
    ## Expr Trees in the debugger or for meta-programming.
    return Exprs.Expression.Block(object, varsInOrder, inits + body)

### AnalyzeBlockExpr returns a Block with the body exprs.
###
def AnalyzeBlockExpr (expr, scope):
    debugprint("analyze block ...")
    if type(expr) is not parser.SymplBlockExpr:
        raise Exception("Internal: need Block Expr to analyze.")
    body = []
    for e in expr.Body:
        body.append(AnalyzeExpr(e, scope))
    ## Due to .NET 4.0 co/contra-variance, IPy's binding isn't picking the overload
    ## with Type and IEnumerable<Expr>, so pick it explicitly.
    return Exprs.Expression.Block.Overloads[
              System.Type, IEnumerable[Exprs.Expression]](object, body)

### AnalyzeQuoteExpr converts a list, literal, or id expr to a runtime quoted
### literal and returns the Constant expression for it.
###
def AnalyzeQuoteExpr (expr, scope):
    debugprint("analyze quote ...")
    if type(expr) is not parser.SymplQuoteExpr:
        raise Exception("Internal: need Quote Expr to analyze.")
    return Exprs.Expression.Constant(
               MakeQuoteConstant(expr.Expr, scope.GetRuntime()))

def MakeQuoteConstant (expr, symplRuntime):
    if type(expr) is parser.SymplListExpr:
        exprs = []
        for e in expr.Elements:
            exprs.append(MakeQuoteConstant(e, symplRuntime))
        return runtime.Cons._List(*exprs)
    elif isinstance(expr, lexer.IdOrKeywordToken):
        return symplRuntime.MakeSymbol(expr.Name)
    elif isinstance(expr, lexer.LiteralToken):
        return expr.Value
    else:
        raise Exception("Internal: quoted list has -- " + repr(expr))


def AnalyzeEqExpr (expr, scope):
    debugprint("analyze eq ...")
    if type(expr) is not parser.SymplEqExpr:
        raise Exception("Internal: need eq expr to analyze.")
    return runtime.MakeSymplEqCall(AnalyzeExpr(expr.Left, scope),
                                   AnalyzeExpr(expr.Right, scope))
    
def AnalyzeConsExpr (expr, scope):
    debugprint("analyze cons ...")
    if type(expr) is not parser.SymplConsExpr:
        raise Exception("Internal: need cons expr to analyze.")
    return runtime.MakeSymplConsCall(AnalyzeExpr(expr.Left, scope),
                                     AnalyzeExpr(expr.Right, scope))
    
def AnalyzeListCallExpr (expr, scope):
    debugprint("analyze List call ...")
    if type(expr) is not parser.SymplListCallExpr:
        raise Exception("Internal: need import expr to analyze.")
    return runtime.MakeSymplListCall([AnalyzeExpr(x, scope)
                                      for x in expr.Elements])


def AnalyzeIfExpr (expr, scope):
    if type(expr) is not parser.SymplIfExpr:
        raise Exception("Internal: need IF expr to analyze.")
    if expr.Alternative is not None:
        alt = AnalyzeExpr(expr.Alternative, scope)
    else:
        alt = Exprs.Expression.Constant(False)
    return Exprs.Expression.Condition(
               WrapBooleanTest(AnalyzeExpr(expr.Test, scope)),
               Exprs.Expression.Convert(AnalyzeExpr(expr.Consequent, scope),
                                        object),
               Exprs.Expression.Convert(alt, object))
    
def WrapBooleanTest (expr):
    tmp = Exprs.Expression.Parameter(object, "testtmp")
    return Exprs.Expression.Block(
        [tmp],
        [Exprs.Expression.Assign(tmp, Exprs.Expression.Convert(expr, object)),
         Exprs.Expression.Condition(
             Exprs.Expression.TypeIs(tmp, bool), 
             Exprs.Expression.Convert(tmp, bool),
             Exprs.Expression.NotEqual(tmp, Exprs.Expression.Constant(None)))])


def AnalyzeLoopExpr (expr, scope):
    debugprint("analyze loop ...")
    if type(expr) is not parser.SymplLoopExpr:
        raise Exception("Internal: need loop to analyze.")
    loopscope = AnalysisScope(scope, "loop ")
    loopscope.IsLoop = True  # needed for break and continue
    loopscope.LoopBreak = Exprs.Expression.Label(object, "loop break")
    body = []
    for e in expr.Body:
        body.append(AnalyzeExpr(e, loopscope))
    ## Due to .NET 4.0 co/contra-variance, IPy's binding isn't picking the overload
    ## with Type and IEnumerable<Expr>, so pick it explicitly.
    return Exprs.Expression.Loop(Exprs.Expression.Block.Overloads
                                    [System.Type, IEnumerable[Exprs.Expression]]
                                    (object, body), 
                                  loopscope.LoopBreak)

def AnalyzeBreakExpr (expr, scope):
    debugprint("analyze break ..." + repr(expr.Value))
    if type(expr) is not parser.SymplBreakExpr:
        raise Exception("Internal: need break to analyze.")
    loopscope = _findFirstLoop(scope)
    if loopscope is None:
        raise Exception("Call to Break not inside loop.")
    if expr.Value is None:
        value = Exprs.Expression.Constant(None, object)
    else:
        ## Ok if value jumps to break label.
        value = AnalyzeExpr(expr.Value, scope)
    ## Need final type=object arg because the Goto is in a value returning
    ## position, and the Break factory doesn't set the GotoExpr.Type property
    ## to the type of the LoopBreak label target's type.
    return Exprs.Expression.Break(loopscope.LoopBreak, value, object)

### _findFirstLoop returns the first loop AnalysisScope or None.
###
def _findFirstLoop (scope):
    curscope = scope
    while curscope is not None:
        if curscope.IsLoop:
            return curscope
        else:
            curscope = curscope.Parent
    return None

def AnalyzeNewExpr (expr, scope):
    debugprint("analyze new ...", expr.Typ)
    if type(expr) is not parser.SymplNewExpr:
        raise Exception("Internal: need New call to analyze.")
    typ = AnalyzeExpr(expr.Typ, scope)
    args = [AnalyzeExpr(a, scope) for a in expr.Arguments]
    ## Use DynExpr since we don't know type until runtime.
    return Exprs.Expression.Dynamic(
             scope.GetRuntime().GetCreateInstanceBinder(CallInfo(len(args))),
             object,
             [typ] + args)

### AnalyzeEltExpr returns and Expression for accessing an element of an
### aggregate structure.  This also works for .NET objs with indexer Item
### properties.  We handle analyzing Elt for assignment in AnalyzeAssignExpr.
###
def AnalyzeEltExpr (expr, scope):
    debugprint("analyze elt ...", expr.ObjectExpr)
    if type(expr) is not parser.SymplEltExpr:
        raise Exception("Internal: need Elt call to analyze.")
    obj = AnalyzeExpr(expr.ObjectExpr, scope)
    args = [AnalyzeExpr(a, scope) for a in expr.Indexes]
    ## Use DynExpr since we don't know obj until runtime.
    return Exprs.Expression.Dynamic(
             scope.GetRuntime().GetGetIndexBinder(CallInfo(len(args))),
             object,
             [obj] + args)


def AnalyzeBinaryExpr (expr, scope):
    if type(expr) is not parser.SymplBinaryExpr:
        raise Exception("Internal: need binary op to analyze.")
    if expr.Op == Exprs.ExpressionType.And:
        ## (and x y) is (if x y)
        return AnalyzeIfExpr(parser.SymplIfExpr(expr.Left, expr.Right, None),
                             scope)
    elif expr.Op == Exprs.ExpressionType.Or:
        ## (or x y) is (let ((tmpx x))
        ##                (if tmpx tmpx (let ((tmp2 y)) (if tmp2 tmp2))))
        ##
        ## Build inner let for y first.
        ## Real impl needs to ensure unique ID in scope chain.
        tmp2 = lexer.IdOrKeywordToken("__tmpOrLetVar2", False) #False = not kwd
        tmpExpr2 = parser.SymplIdExpr(tmp2)
        bindings2 = [(tmp2, expr.Right)]
        ifExpr2 = parser.SymplIfExpr(tmpExpr2, tmpExpr2, None)
        letExpr2 = parser.SymplLetStarExpr(bindings2, [ifExpr2])
        ## Build outer let for x.
        tmp1 = lexer.IdOrKeywordToken("__tmpOrLetVar1", False) #False = not kwd
        tmpExpr1 = parser.SymplIdExpr(tmp1)
        bindings1 = [(tmp1, expr.Left)]
        ifExpr1 = parser.SymplIfExpr(tmpExpr1, tmpExpr1, letExpr2)
        return AnalyzeLetStarExpr(
                   parser.SymplLetStarExpr(bindings1, [ifExpr1]),
                   scope)
    else:
        return Exprs.Expression.Dynamic(
                   scope.GetRuntime().GetBinaryOperationBinder(expr.Op),
                   object,
                   AnalyzeExpr(expr.Left, scope),
                   AnalyzeExpr(expr.Right, scope))
        

def AnalyzeUnaryExpr (expr, scope):
    if type(expr) is not parser.SymplUnaryExpr:
        raise Exception("Internal: need Unary op to analyze.")
    if expr.Op == Exprs.ExpressionType.Not:
        ## Sympl has specific semantics for what is true vs. false and would
        ## use the OnesComplement node kind if Sympl had that.
        return Exprs.Expression.Not(WrapBooleanTest(AnalyzeExpr(expr.Operand,
                                                                scope)))
    else:
        ## Should never get here unless we add, say, unary minus.
        return Exprs.Expression.Dynamic(
                   scope.GetRuntime().GetUnaryOperationBinder(expr.Op),
                   object,
                   AnalyzeExpr(expr.Operand, scope))
    



### AnalysisScope holds identifier information so that we can do name binding
### during analysis.  It manages a map from names to ParameterExprs so ET
### definition locations and reference locations can alias the same variable.
###
### These chain from inner most BlockExprs, through LambdaExprs, to the root
### which models a file or top-level expression.  The root has non-None
### ModuleExpr and RuntimeExpr, which are ParameterExprs.
###
class AnalysisScope (object):
    def __init__ (self, parent, nam = "", runtime = None, runtimeParam = None,
                   moduleParam = None):
        self.ModuleExpr = moduleParam
        self.RuntimeExpr = runtimeParam
        ## Need runtime for interning Symbol constants at code gen time.
        self.Runtime = runtime
        self.Name = nam
        self.Parent = parent
        self.Names = {}
        ## Need IsLambda when support return to find tightest closing fun.
        self.IsLambda = False
        self.IsLoop = False
        self.LoopBreak = None
        self.LoopContinue = None
    
    def IsModule (self):
        return self.ModuleExpr is not None

    def GetModuleExpr (self):
        curscope = self
        while not curscope.IsModule():
            curscope = curscope.Parent
        return curscope.ModuleExpr

    def GetRuntime (self):
        curscope = self
        while curscope.Runtime is None:
            curscope = curscope.Parent
        return curscope.Runtime



##################
### Dev-time Utils
##################

_debug = False
def debugprint (*stuff):
    if _debug:
        for x in stuff:
            print x,
        print
