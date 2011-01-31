import clr

from System import Environment
if Environment.Version.Major<4:
    clr.use35 = True
else:
    clr.use35 = False

if clr.use35:
    clr.AddReference("Microsoft.Scripting.Core")
    import Microsoft.Scripting.Ast as Exprs
else:
    clr.AddReference("System.Core")
    import System.Linq.Expressions as Exprs

from System.Dynamic import (ExpandoObject, IDynamicMetaObjectProvider,
                             DynamicMetaObject, BindingRestrictions, CallInfo)
from System.Collections.Generic import IEnumerable

import runtime
import parser
import etgen

import System.Reflection as refl

from System.IO import StreamReader, Path, StringReader

import System

import thread #Used for locking binders canonicalization tables


class Sympl (object):
    def __init__ (self, assms = None):
        ## Host Globals, also used by reflection of assemblies.
        self.Globals = ExpandoObject()
        ## Set up assemblies reflection.
        object.__setattr__(
            self, "_assemblies",
            assms or [refl.Assembly.LoadWithPartialName("System"),
                      refl.Assembly.LoadWithPartialName("mscorlib")])
        self._addNamespacesAndTypes()
        ## Set up Symbols interning table.
        self.Symbols = dict()
        self.Symbols["nil"] = runtime.Symbol("nil")
        self.Symbols["true"] = runtime.Symbol("true")
        self.Symbols["false"] = runtime.Symbol("false")
        ## Set up binder canonicalization tables.
        ## Should have lock per table, but this is good enough for smaple.
        self._lock = thread.allocate_lock()
        self._getMemberBinders = dict()
        self._setMemberBinders = dict()
        self._invokeBinders = dict()
        self._invokeMemberBinders = dict()
        self._createInstanceBinders = dict()
        self._getIndexBinders = dict()
        self._setIndexBinders = dict()
        self._binaryOperationBinders = dict()
        self._unaryOperationBinders = dict()


    ### _addNamespacesAndTypes builds a tree of ExpandoObjects representing
    ### .NET namespaces, with TypeModel objects at the leaves.  Though Sympl is
    ### case-insensitive, we store the names as they appear in .NET reflection
    ### in case our globals object or a namespace object gets passed as an IDO
    ### to another language or library, where they may be looking for names
    ### case-sensitively using EO's default lookup.
    ###
    def _addNamespacesAndTypes (self):
        helpers = runtime.DynamicObjectHelpers
        for assm in self._assemblies:
            for typ in assm.GetExportedTypes():
                names = typ.FullName.split('.')
                table = self.Globals
                for ns in names[:-1]:
                    if helpers.HasMember(table, ns):
                        table = helpers.GetMember(table, ns)
                    else:
                        tmp = ExpandoObject()
                        helpers.SetMember(table, ns, tmp)
                        table = tmp
                helpers.SetMember(table, names[-1], runtime.TypeModel(typ))
    
    def __setattr__ (self, name, value):
        if name != "_assemblies":
            object.__setattr__(self, name, value)
        else: raise("Can't set 'Assemblies' after instantiating Sympl.")

    dbgmodule = None
    dbgASTs = None
    dbgascope = None
    dbgbody = None
    dbgmodfun = None
    
    ### ExecuteFile executes the file in a new module scope and stores the
    ### scope on Globals, using either the provided name, globalVar, or the
    ### file's base name.  This function returns the module scope.
    ###
    def ExecuteFile (self, filename, globalVar = None):
        moduleEO = self.CreateScope()
        self.dbgmodule = moduleEO
        self.ExecuteFileInScope(filename, moduleEO)
        globalVar = globalVar or Path.GetFileNameWithoutExtension(filename)
        runtime.DynamicObjectHelpers.SetMember(self.Globals, globalVar,
                                               moduleEO)
        return moduleEO
    
    ### ExecuteFileInScope executes the file in the given module scope.  This
    ### does NOT store the module scope on Globals.  This function returns
    ### nothing.
    ###
    def ExecuteFileInScope (self, filename, moduleEO):
        try:
            f = StreamReader(filename)
            runtime.DynamicObjectHelpers.SetMember(moduleEO, "__file__", 
                                                   Path.GetFullPath(filename))
            ASTs = parser.ParseFile(f)
            self.dbgASTs = ASTs
            scope = etgen.AnalysisScope(
                        None, #parent
                        filename,
                        self,
                        Exprs.Expression.Parameter(Sympl, "symplRuntime"),
                        Exprs.Expression.Parameter(ExpandoObject, "fileModule"))
            self.dbgascope = scope
            body = []
            self.dbgbody = body
            for e in ASTs:
                body.append(etgen.AnalyzeExpr(e, scope))
            ## Use ftype with void return so that lambda ignores body result.
            ftype = Exprs.Expression.GetActionType(System.Array[System.Type](
                                                      [Sympl, ExpandoObject]))
            ## Due to .NET 4.0 co/contra-variance, IPy's binding isn't picking
            ## the overload with just IEnumerable<Expr>, so pick it explicitly.
            body = Exprs.Expression.Block.Overloads[IEnumerable[Exprs.Expression]](body)
            modulefun = Exprs.Expression.Lambda(ftype, body, scope.RuntimeExpr,
                                                scope.ModuleExpr)
            dbgmodfun = modulefun
            modulefun.Compile()(self, moduleEO)
        finally:
            f.Close()
        
    def ExecuteExpr (self, expr_str, moduleEO):
        f = StringReader(expr_str)
        ASTs = parser.ParseExpr(f)
        self.dbgASTs = ASTs
        scope = etgen.AnalysisScope(
                    None, #parent
                    "__snippet__",
                    self,
                    Exprs.Expression.Parameter(Sympl, "symplRuntime"),
                    Exprs.Expression.Parameter(ExpandoObject, "fileModule"))
        self.dbgascope = scope
        body = [Exprs.Expression.Convert(etgen.AnalyzeExpr(ASTs, scope),
                                         object)]
        #body = [etgen.AnalyzeExpr(ASTs, scope)]
        self.dbgbody = body
        ftype = Exprs.Expression.GetFuncType(
                    System.Array[System.Type](
                        [Sympl, ExpandoObject, object]))
        ## Due to .NET 4.0 co/contra-variance, IPy's binding isn't picking
        ## the overload with just IEnumerable<Expr>, so pick it explicitly.
        body = Exprs.Expression.Block.Overloads[IEnumerable[Exprs.Expression]](body)
        fun = Exprs.Expression.Lambda(ftype, body, scope.RuntimeExpr,
                                      scope.ModuleExpr)
        dbgmodfun = fun
        return fun.Compile()(self, moduleEO)
    
    def CreateScope (self):
        return ExpandoObject()

    ### Symbol return the Symbol interned in this runtime if it is already
    ### there.  If not, this makes the Symbol and interns it.
    ###
    def MakeSymbol (self, name):
        downname = name.lower()
        if self.Symbols.has_key(downname):
            return self.Symbols[downname]
        else:
            s = runtime.Symbol(name)
            self.Symbols[downname] = s
            return s

    ##########################
    ### Canonicalizing Binders
    ##########################

    ## We need to canonicalize binders so that we can share L2 dynamic
    ## dispatch caching across common call sites.  Every call site with the
    ## same operation and same metadata on their binders should return the
    ## same rules whenever presented with the same kinds of inputs.  The
    ## DLR saves the L2 cache on the binder instance.  If one site somewhere
    ## produces a rule, another call site performing the same operation with
    ## the same metadata could get the L2 cached rule rather than computing
    ## it again.  For this to work, we need to place the same binder instance
    ## on those functionally equivalent call sites.

    def GetGetMemberBinder (self, name):
        ## Don't lower the name.  Sympl is case-preserving in the metadata
        ## in case some DynamicMetaObject ignores ignoreCase.  This makes
        ## some interop cases work, but the cost is that if a Sympl program
        ## spells ".foo" and ".Foo" at different sites, they won't share rules.
        with self._lock:
            if self._getMemberBinders.ContainsKey(name):
                return self._getMemberBinders[name]
            b = runtime.SymplGetMemberBinder(name)
            self._getMemberBinders[name] = b
        return b
    
    def GetSetMemberBinder (self, name):
        ## Don't lower the name.  Sympl is case-preserving in the metadata
        ## in case some DynamicMetaObject ignores ignoreCase.  This makes
        ## some interop cases work, but the cost is that if a Sympl program
        ## spells ".foo" and ".Foo" at different sites, they won't share rules.
        with self._lock:
            if self._setMemberBinders.ContainsKey(name):
                return self._setMemberBinders[name]
            b = runtime.SymplSetMemberBinder(name)
            self._setMemberBinders[name] = b
        return b

    def GetInvokeBinder (self, info):
        with self._lock:
            if self._invokeBinders.ContainsKey(info):
                return self._invokeBinders[info]
            b = runtime.SymplInvokeBinder(info)
            self._invokeBinders[info] = b
        return b
    
    def GetInvokeMemberBinder (self, info):
        with self._lock:
            if self._invokeMemberBinders.ContainsKey(info):
                return self._invokeMemberBinders[info]
            b = runtime.SymplInvokeMemberBinder(info.Name, info.Info)
            self._invokeMemberBinders[info] = b
        return b
        
    def GetCreateInstanceBinder (self, info):
        with self._lock:
            if self._createInstanceBinders.ContainsKey(info):
                return self._createInstanceBinders[info]
            b = runtime.SymplCreateInstanceBinder(info)
            self._createInstanceBinders[info] = b
        return b
    
    def GetGetIndexBinder (self, info):
        with self._lock:
            if self._getIndexBinders.ContainsKey(info):
                return self._getIndexBinders[info]
            b = runtime.SymplGetIndexBinder(info)
            self._getIndexBinders[info] = b
        return b
    
    def GetSetIndexBinder (self, info):
        with self._lock:
            if self._setIndexBinders.ContainsKey(info):
                return self._setIndexBinders[info]
            b = runtime.SymplSetIndexBinder(info)
            self._setIndexBinders[info] = b
        return b
    
    def GetBinaryOperationBinder (self, op):
        with self._lock:
            if self._binaryOperationBinders.ContainsKey(op):
                return self._binaryOperationBinders[op]
            b = runtime.SymplBinaryOperationBinder(op)
            self._binaryOperationBinders[op] = b
        return b
    
    def GetUnaryOperationBinder (self, op):
        with self._lock:
            if self._unaryOperationBinders.ContainsKey(op):
                return self._unaryOperationBinders[op]
            b = runtime.SymplUnaryOperationBinder(op)
            self._unaryOperationBinders[op] = b
        return b
  
    

##################
### Dev-time Utils
##################

_debug = False
def debugprint (*stuff):
    if _debug:
        for x in stuff:
            print x,
        print
    
