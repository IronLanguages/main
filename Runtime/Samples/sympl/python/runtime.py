
import clr

if clr.use35:
    clr.AddReference("Microsoft.Scripting")
    clr.AddReference("Microsoft.Dynamic")
    clr.AddReference("Microsoft.Scripting.Core")

    import Microsoft.Scripting.Ast as Exprs
    from Microsoft.Scripting.ComInterop import ComBinder
    from Microsoft.Scripting.Utils import (Action, Func)
else:
    clr.AddReference("System.Core")
    clr.AddReference("Microsoft.Dynamic")
    
    import System.Linq.Expressions as Exprs
    from Microsoft.Scripting.ComInterop import ComBinder
    from System import (Action, Func)

from System.Runtime.CompilerServices import CallSite
from System.Dynamic import (ExpandoObject, InvokeBinder, DynamicMetaObject,
                            GetMemberBinder, SetMemberBinder, CallInfo,
                            BindingRestrictions, IDynamicMetaObjectProvider,
                            InvokeMemberBinder, CreateInstanceBinder,
                            GetIndexBinder, SetIndexBinder, 
                            BinaryOperationBinder, UnaryOperationBinder)

from System import (MissingMemberException,
                    InvalidOperationException, Boolean, MissingMemberException,
                    Type, Array, Delegate, Void)

import System.Reflection as refl

from System.IO import Path, File



### RuntimeHelpers is a collection of functions that perform operations at
### runtime of Sympl code, such as performing an import or fetching a global
### variable's value (depending on global look up semantics).
###
class RuntimeHelpers (object):
    ### SymplImport takes the runtime and module as context for the import.
    ### It takes a list of names, what, that either identify a (possibly dotted
    ### sequence) of names to fetch from Globals or a file name to load.  Names
    ### is a list of names to fetch from the final object that what indicates
    ### and then set each name in module.  Renames is a list of names to add to
    ### module instead of names.  If names is empty, then the name set in
    ### module is the last name in what.  If renames is not empty, it must have
    ### the same cardinality as names.
    ###
    @staticmethod
    def SymplImport (runtime, module, what, names, renames):
        ## Get object or file scope.
        helpers = DynamicObjectHelpers
        if len(what) == 1:
            name = what[0]
            if helpers.HasMember(runtime.Globals, name):
                value = helpers.GetMember(runtime.Globals, name)
            else:
                f = DynamicObjectHelpers.GetMember(module, "__file__")
                f = Path.Combine(Path.GetDirectoryName(f), name + ".sympl")
                if File.Exists(f):
                    value = runtime.ExecuteFile(f)
                else:
                    raise Exception("Import: can't find name in globals " +
                                    "or as file to load -- " + name + ", " +
                                    f)
        else:
            ## What has more than one name, must be Globals access.
            value = runtime.Globals
            for name in what:
                value = helpers.GetMember(value, name)
        ## Assign variables in module.
        if len(names) == 0:
            setattr(module, what[-1], value)
        else:
            for n, m in zip(names, renames or names):
                setattr(module, m, getattr(value, n))
        return None

    @staticmethod
    def SymplEq (x, y):
        ## Not that Sympl has other immediate values, but could add more branches
        ## for doubles or types that might flow in from .NET interop.
        if type(x) is int and type(y) is int:
            return x == y
        else:
            return x is y

    ### Hack until dynamic expr invoking Cons type is fixed in Ipy.
    ###
    @staticmethod
    def MakeCons (x, y):
        return Cons(x, y)

    @staticmethod
    def GetConsElt (lst, i):
        return RuntimeHelpers._nthcdr(lst, i).First
        
    @staticmethod
    def SetConsElt (lst, i, value):
        lst = RuntimeHelpers._nthcdr(lst, i)
        lst.First = value
        return value

    @staticmethod
    def _nthcdr (lst, i):
        while i > 0 and lst is not None:
            lst = lst.Rest
            i = i - 1
        if i == 0 and lst is not None:
            return lst
        else:
            raise Exception("List doesn't have " + repr(i) + " elements.")


    ### Don't need this in C# because can create an Property MemberExpr.  This
    ### works in IPy because our TMMO.BindGetMember falls back to Python's
    ### MO to fetch the member.
    ###
    #@staticmethod
    #def GetTypeModelReflType (typModel):
    #    return typModel.ReflType



########################
### Helpers for code gen
########################

### RunHelpersInvokeBinder is the binder that let's me invoke members of
### runtimeHelpers as DynamicExprs.  In C#, we can create MethodCallExprs
### with the MethodInfos of my RuntimeHelpers members, so we don't need this.
###
class RunHelpersInvokeBinder (InvokeBinder):
    #@property doesn't work
    def get_CacheIdentity (self):
        return self

    def GetHashCode (self):
        ## Random sophmoric hash ...
        return 197 ^ super(RunHelpersInvokeBinder, self).GetHashCode()

    def Equals (self, obj):
        return (isinstance(obj, RunHelpersInvokeBinder) and
                super(RunHelpersInvokeBinder, self).Equals(obj))

    def FallbackInvoke(objMO, argMOs, errorSuggestionMO):
        ## Python handles the actual invoke in its callable MO
        ## When translated to C#, won't need DynExpr to call helper ... MCE.
        pass

### MakeSymplImportCall gets called from analysis code that generates
### Expression Trees for Sympl 'import' expressions.  Runtime and module
### are ParamExprs from the outer lambda wrapping a file's top-level exprs.
### What, names, and renames are lists (possibly empty) of IdTokens.
###
def MakeSymplImportCall (runtime, module, what, names, renames):
    if not isinstance(names, list):
        raise Exception("Internal: name is not list?")
    return Exprs.Expression.Dynamic(
            RunHelpersInvokeBinder(CallInfo(5)),
            object, #ret type
            Exprs.Expression.Constant(RuntimeHelpers.SymplImport),
            runtime, module,
            Exprs.Expression.Constant([x.Name for x in what]),
            Exprs.Expression.Constant([x.Name for x in names]),
            Exprs.Expression.Constant([x.Name for x in renames]))

def MakeSymplEqCall (left, right):
    return Exprs.Expression.Convert(
              Exprs.Expression.Dynamic(
                 RunHelpersInvokeBinder(CallInfo(2)),
                 object, #ret type
                 Exprs.Expression.Constant(RuntimeHelpers.SymplEq),
                 left, right),
              bool) #clr.GetClrType(Boolean))

def MakeSymplConsCall (left, right):
    return Exprs.Expression.Dynamic(
            RunHelpersInvokeBinder(CallInfo(2)),
            object, #ret type
            Exprs.Expression.Constant(RuntimeHelpers.MakeCons),
            left, right)

def MakeSymplListCall (args):
    return Exprs.Expression.Dynamic(
            RunHelpersInvokeBinder(CallInfo(len(args))),
            object, #ret type
            Exprs.Expression.Constant(Cons._List),
            *args)



###############################
### Helpers for runtime binding
###############################

### GetTargetArgsRestrictions generates the restrictions needed for the
### MO resulting from binding an operation.  This combines all existing
### restrictions and adds some for arg conversions.  targetInst indicates
### whether to restrict the target to an instance (for operations on type
### objects) or to a type (for operations on an instance of that type).
###
### NOTE, this function should only be used when the caller is converting
### arguments to the same types as these restrictions. See ConvertArguments.
###
def GetTargetArgsRestrictions (targetMO, argMOs, targetInst):
    ## Important to add existing restriction first because the
    ## DynamicMetaObjects (and possibly values) we're looking at depend
    ## on the pre-existing restrictions holding true.
    restrictions = targetMO.Restrictions.Merge(
                      BindingRestrictions.Combine(argMOs))
    if targetInst:
        restrictions = restrictions.Merge(
                          BindingRestrictions.GetInstanceRestriction(
                              targetMO.Expression,
                              targetMO.Value))
    else:
        restrictions = restrictions.Merge(
                          BindingRestrictions.GetTypeRestriction(
                              targetMO.Expression,
                              targetMO.LimitType))
        
    for a in argMOs:
        if a.HasValue and a.Value is None:
            r = BindingRestrictions.GetInstanceRestriction(a.Expression,
                                                           None)
        else:
            r = BindingRestrictions.GetTypeRestriction(a.Expression,
                                                       a.LimitType)
        restrictions = restrictions.Merge(r)
    return restrictions

### ParamsMatchArgs returns whether the args are assignable to the parameters.
### We specially check for our TypeModel that wraps .NET's RuntimeType, and
### elsewhere we detect the same situation to convert the TypeModel for calls.
###
### IsAssignableFrom works except for value args that need to pass to reftype
### params. We could detect that to be smarter and then explicitly StrongBox
### the args.
###
### Could check for a.HasValue and a.Value is None and
### ((paramtype is class or interface) or (paramtype is generic and
### nullable<t>)) to support passing nil anywhere.
###
### Consider checking p.IsByRef and returning false since that's not CLS.
###
def ParamsMatchArgs (params, args):
    for a,p in zip(args, params):
        if (p.ParameterType is clr.GetClrType(Type) and #get past py type wrapping
            type(a.Value) is TypeModel): #ok if no value, value = null
                continue
        if not p.ParameterType.IsAssignableFrom(a.LimitType):
            ## or p.IsByRef: punt for non CLS
            return False
    return True

### Returns a DynamicMetaObject with an expression that fishes the .NET
### RuntimeType object from the TypeModel MO.
###
def GetRuntimeTypeMoFromModel (typeMO):
    if type(typeMO) is not TypeModelMetaObject:
        raise Exception("Internal: Need TMMO to fish out ReflType.")
    return DynamicMetaObject(
               ## In C# can use Expression.Call on methodinfo.
               Exprs.Expression.Convert(
                   Exprs.Expression.Dynamic(
                       ## This call site doesn't share any L2 caching
                       ## since we don't call GetGetMemberBinder from Sympl.
                       ## We aren't plumbed to get the runtime instance here.
                       SymplGetMemberBinder("ReflType"),
                       object,
                       typeMO.Expression),
                   Type),
               #Exprs.Expression.Dynamic(
               #    RunHelpersInvokeBinder(CallInfo(1)),
               #    object,
               #    Exprs.Expression.Constant(
               #        RuntimeHelpers.GetTypeModelReflType),
               #    typeMO.Expression),
               typeMO.Restrictions.Merge(
                   BindingRestrictions.GetTypeRestriction(
                       typeMO.Expression, TypeModel))) #,
               ## Must supply a value to prevent binder FallbackXXX methods
               ## from infinitely looping if they do not check this MO for
               ## HasValue == false and call Defer.  After Sympl added Defer
               ## checks, we could verify, say, FallbackInvokeMember by no
               ## longer passing a value here.
               #typeMO.ReflType)

### Returns list of Convert exprs converting args to param types.  If an arg
### is a TypeModel, then we treat it special to perform the binding.  We need
### to map from our runtime model to .NET's RuntimeType object to match.
###
### To call this function, args and pinfos must be the same length, and param
### types must be assignable from args.
###
### NOTE, if using this function, then need to use GetTargetArgsRestrictions
### and make sure you're performing the same conversions as restrictions.
###
def ConvertArguments (argMOs, pinfos):
    res = []
    for p,a in zip(pinfos, argMOs):
        argExpr = a.Expression
        if type(a.Value) is TypeModel and p.ParameterType is clr.GetClrType(Type):
            argExpr = GetRuntimeTypeMoFromModel(a).Expression
        res.append(Exprs.Expression.Convert(argExpr, p.ParameterType))
    return res

###
### Note, callers must ensure the DynamicMetaObject that uses this expression
### has consistent restrictions for the conversion done on args and the target.
###
def GetIndexExpression (targetMO, indexMOs):
    indexExprs = [Exprs.Expression.Convert(x.Expression, x.LimitType)
                  for x in indexMOs]
    if type(targetMO.Value) is Cons:  #Don't look at LimitType to compare py type objs.
        ## In C# can use Expression.Call on methodinfo.
        return Exprs.Expression.Dynamic(
                  RunHelpersInvokeBinder(CallInfo(2)),
                  object,
                  Exprs.Expression.Constant(RuntimeHelpers.GetConsElt),
                  Exprs.Expression.Convert(targetMO.Expression,
                                           targetMO.LimitType),
                   indexExprs[0])
    elif targetMO.LimitType.IsArray:
        return Exprs.Expression.ArrayAccess(
                   Exprs.Expression.Convert(targetMO.Expression,
                                            targetMO.LimitType),
                   indexExprs)
    else:
        ## Check for Item indexer.
        props = targetMO.LimitType.GetProperties()
        props = [x for x in props if len(x.GetIndexParameters()) == len(indexMOs)]
        res = []
        for p in props:
            if ParamsMatchArgs(p.GetIndexParameters(), indexMOs):
                res.append(p)
        if len(res) == 0:
            return Exprs.Expression.Throw(
                      Exprs.Expression.New(
                          MissingMemberException.GetConstructor(
                              Array[Type]([str])),
                          Exprs.Expression.Constant(
                             "Can't find matching indexer property.")))
        return Exprs.Expression.MakeIndex(
                  Exprs.Expression.Convert(targetMO.Expression, 
                                           targetMO.LimitType),
                  res[0], indexExprs)

## CreateThrow takes arguments like fallback and bind methods, dynamic meta
## objects.  It also takes restrictions to constrain when the throw rule is
## good.  It takes an Exception type and arguments for the throw the resulting
## DynamicMetaObject represents.  It returns a DynamicMetaObject whose expr
## throws the exception, and ensures the expr's type is object to satisfy
## the CallSite return type constraint.
##
def CreateThrow (target, args, moreTests, exception, *exceptionArgs):
    argExprs = None
    argTypes = Type.EmptyTypes
    if exceptionArgs is not None:
        argExprs = []
        argTypes = []
        for o in exceptionArgs:
            e = Exprs.Expression.Constant(o)
            argExprs.append(e)
            argTypes.append(e.Type)
    constructor = clr.GetClrType(exception).GetConstructor(Array[Type](argTypes))
    if constructor is None:
        raise ArgumentException(
            "Type doesn't have constructor with a given signature")
    return DynamicMetaObject(
        Exprs.Expression.Throw(
            Exprs.Expression.New(constructor, argExprs),
            ## Force expression to be type object so that DLR CallSite code
            ## things only type object flows out of the CallSite.
            object),
        target.Restrictions.Merge(BindingRestrictions.Combine(args))
                           .Merge(moreTests))

### EnsureObjectResult wraps expr if necessary so that any binder or
### DynamicMetaObject result expression returns object.  This is required
### by CallSites.
###
def EnsureObjectResult (expr):
    if not expr.Type.IsValueType:
        return expr
    if expr.Type is clr.GetClrType(Void):
        return Exprs.Expression.Block(expr, Exprs.Expression.Default(object))
    else:
        return Exprs.Expression.Convert(expr, object)



##############################
### Type model IDynObj wrapper 
##############################

### TypeModel wraps System.Runtimetypes. When Sympl code encounters
### a type leaf node in Sympl.Globals and tries to invoke a member, wrapping
### the ReflectionTypes in TypeModels allows member access to get the type's
### members and not ReflectionType's members.
###
class TypeModel (object, IDynamicMetaObjectProvider):
    def __init__ (self, typ):
        ## Note, need to check for initialized members in GetMetaObject so
        ## that creating TypeModel's works without using our custom MO.
        self.ReflType = typ
    
    ### GetMetaObject needs to wrap the base IDO due to Python's objects all
    ### being IDOs.  While this GetMetaObject definition is on the stack IPy
    ### ensures Ipy uses its own MO for TypeModel instances.  However, when
    ### this function is NOT pending on the stack, IPy calls this GetMetaObject
    ### to get the MO.  TypeModelMetaObject needs to delegate to the Python IDO
    ### to dot into TypeModel instances for members, or capture all the
    ### TypeModel state in our MO.  We pass ReflType here so that in
    ### TypeModelMetaObject BindXXX methods, we do not have to access TypeModel
    ### instance members. If we did access TypeModel instance members from
    ### TypeModelMetaObject, then that code would call this GetMetaObject and
    ### use the BindGetMember below, which would fail to access ReflType since
    ### the BindGetMember blow looks for members on the type represented by
    ### ReflType.
    ###
    ### The C# implementation won't need to do this awkward workaround.
    ###
    def GetMetaObject (self, objParam):
        baseIdoMo = IDynamicMetaObjectProvider.GetMetaObject(self, objParam)
        if hasattr(self, "ReflType"):
            ## If came through once and initialized this slot, then return
            ## my MO for accessing the members of the type represented by
            ## ReflType.
            return TypeModelMetaObject(objParam, self, self.ReflType,
                                       baseIdoMo)
        return baseIdoMo


class TypeModelMetaObject (DynamicMetaObject):

    ### Constructor takes ParameterExpr to reference CallSite, a TypeModel
    ### that the new TypeModelMetaObject represents, and the base Python IDO MO
    ### handle for the TypeModel instance that Python uses.  We need to
    ### delegate to this MO explicitly to get Python to do binding for us on
    ### TypeModel instances when it is NOT Sympl code that is trying to use
    ### the TypeModel to dot into members on the type represented by the
    ### TypeModel instance.
    ###
    def __new__ (self, objParam, typModel, refltype, baseIdoMo):
        mo = super(TypeModelMetaObject, self).__new__(
                 TypeModelMetaObject, objParam, BindingRestrictions.Empty,
                 typModel)
        mo.TypModel = typModel
        mo.ReflType = refltype
        mo.ObjParamExpr = objParam
        mo.BaseIDOMO = baseIdoMo
        return mo

    def BindGetMember (self, binder):
        #debugprint("tmmo bindgetmember ...", binder.Name)
        flags = (refl.BindingFlags.IgnoreCase | refl.BindingFlags.Static |
                 refl.BindingFlags.Public)
        ## consider BindingFlags.Instance if want to return wrapper for
        ## inst members that is callable.
        members = self.ReflType.GetMember(binder.Name, flags)
        if len(members) == 1:
            return DynamicMetaObject(
                      ## We always access static members for type model
                      ## objects, so the first argument in MakeMemberAccess
                      ## should be null (no instance).
                      EnsureObjectResult(
                          Exprs.Expression.MakeMemberAccess(
                              None, members[0])),
                          ## Don't need restriction test for name since this
                          ## rule is only used where binder is used, which is
                          ## only used in sites with this binder.Name.
                   self.Restrictions.Merge(
                           BindingRestrictions.GetInstanceRestriction(
                             self.Expression,
                             self.Value)))
        else:
            ## Defer to IPy binding to access TypeModel instance members.  IPy
            ## will fallback to the binder as appropriate.
            ##return binder.FallbackGetMember(self)
            return self.BaseIDOMO.BindGetMember(binder)

    ### Because we don't ComboBind over several MOs and operations, and no one
    ### is falling back to this function with MOs that have no values, we
    ### don't need to check HasValue.  If we did check, and HasValue == False,
    ### then would defer to new InvokeMemberBinder.Defer().
    ###
    def BindInvokeMember (self, binder, args):
        debugprint("tmmo: bindinvokemember ...", binder.Name)
        flags = (refl.BindingFlags.IgnoreCase | refl.BindingFlags.Static |
                 refl.BindingFlags.Public)
        members = self.ReflType.GetMember(binder.Name, flags)
        if (len(members) == 1 and
            (isinstance(members[0], refl.PropertyInfo) or
             isinstance(members[0], refl.FieldInfo))):
            raise Exception("Haven't implemented invoking delegate values " +
                            "from properties or fields.")
            ## NOT TESTED, should check type for isinstance delegate
            #return DynamicMetaObject(
            #    Exprs.Expression.Dynamic(
            #        SymplInvokeBinder(CallInfo(len(args))),
            #        object,
            #        ([Exprs.MakeMemberAccess(self.Expression, mem)] +
            #            (x.Expression for x in args))))
            
            ## Don't test for eventinfos since we do nothing with them now.
        else:
            ## Get MethodInfos with right arg count.
            debugprint("tmmo bind invoke mem ... searching ...", len(members))
            mi_mems = [x for x in members if isinstance(x, refl.MethodInfo) and
                       len(x.GetParameters()) == len(args)]
            debugprint("methodinfo members with same arg count: ", len(mi_mems))
            debugprint(mi_mems)
            res = []
            for mem in mi_mems:
                if ParamsMatchArgs(mem.GetParameters(), args):
                    res.append(mem)
            if len(res) == 0:
                ## Sometimes when binding members on TypeModels the member
                ## is an intance member since the Type is an instance of Type.
                ## We fallback to the binder with the Type instance to see if
                ## it binds.  The SymplInvokeMemberBinder does handle this.
                refltypeMO = GetRuntimeTypeMoFromModel(self)
                return binder.FallbackInvokeMember(refltypeMO, args, None)
            ## True means generate an instance restriction on the MO.
            ## We are only looking at the members defined in this Type instance.
            restrictions = GetTargetArgsRestrictions(self, args, True)
            ## restrictions and conversion must be done consistently.
            callArgs = ConvertArguments(args, res[0].GetParameters())
            ## Fix expr to satisfy object type required by CallSite.
            return DynamicMetaObject(
                       EnsureObjectResult(Exprs.Expression.Call(res[0],
                                                                callArgs)),
                       restrictions)
            ## Could try just letting Expr.Call factory do the work, but if
            ## there is more than one applicable method using just
            ## assignablefrom, Expr.Call flames out.  It does not pick a "most
            ## applicable" method.
            
            ## Defer to IPy binding to invoke TypeModel instance members.  IPy
            ## will fallback to the binder as appropriate.
            ##return binder.FallbackInvokeMember(self)
            ##return self.BaseIDOMO.BindInvokeMember(binder, args)
    
    def BindCreateInstance (self, binder, args):
        ctors = self.ReflType.GetConstructors()
        ## Get constructors with right arg count.
        ctors = [x for x in ctors 
                   if len(x.GetParameters()) == len(args)]
        res = []
        for mem in ctors:
            if ParamsMatchArgs(mem.GetParameters(), args):
                res.append(mem)
        if len(res) == 0:
            refltypeMO = GetRuntimeTypeMoFromModel(self)
            return binder.FallbackCreateInstance(refltypeMO, args)
        ## True means generate an instance restriction on the MO.
        ## We only have a rule to create this exact type.
        restrictions = GetTargetArgsRestrictions(self, args, True)
        ## restrictions and conversion must be done consistently.
        callArgs = ConvertArguments(args, res[0].GetParameters())
        return DynamicMetaObject(
                   ## Creating an object, so don't need EnsureObjectResult.
                   Exprs.Expression.New(res[0], callArgs),
                   restrictions)

    ###
    ### Bindings I don't care about, so defer to Pythons IDO
    ###
    
    def BindConvert (self, binder):
        return self.BaseIDOMO.BindConvert(binder)
        
    def BindSetMember (self, binder, valueMO):
        return self.BaseIDOMO.BindSetMember(binder, valueMO)
        
    def BindDeleteMember (self, binder):
        return self.BaseIDOMO.BindDeleteMember(binder)

    def BindGetIndex (self, binder, indexes):
        return self.BaseIDOMO.BindGetIndex (binder, indexes)
        
    def BindSetIndex (self, binder, indexes, value):
        return self.BaseIDOMO.BindSetIndex (binder, indexes, value)
        
    def BindDeleteIndex (self, binder, indexes):
        return self.BaseIDOMO.BindDeleteIndex (binder, indexes)
        
    def BindInvoke (self, binder, args):
        return self.BaseIDOMO.BindInvoke (binder, args)
        
    def BindUnaryOperation (self, binder):
        return self.BaseIDOMO.BindUnaryOperation (binder)
        
    def BindBinaryOperation (self, binder, arg):
        return self.BaseIDOMO.BindBinaryOperation (binder, arg)



#######################################################
### Dynamic Helpers for HasMember, GetMember, SetMember
#######################################################

### DynamicObjectHelpers provides access to IDynObj members given names as
### data at runtime.  When the names are known at compile time (o.foo), then
### they get baked into specific sites with specific binders that encapsulate
### the name.  We need this in python because hasattr et al are case-sensitive.
###
### Currently Sympl only uses this on ExpandoObjects, but it works generally on
### IDOs.
###
class DynamicObjectHelpers (object):
    Sentinel = object()
    GetSites = {}
    SetSites = {}

    @staticmethod
    def HasMember (dynObj, name):
        if not isinstance(dynObj, IDynamicMetaObjectProvider):
            raise Exception("DynamicObjectHelpers only works on IDOs for now.")
        return (DynamicObjectHelpers.GetMember(dynObj, name) !=
               DynamicObjectHelpers.Sentinel)
        #Alternative impl used when EOs had bug and didn't call fallback ...
        #mo = dynObj.GetMetaObject(Exprs.Expression.Parameter(object, "bogus"))
        #for member in mo.GetDynamicMemberNames():
        #    if String.Equals(member, name,
        #                     String.StringComparison.OrdinalIgnoreCase):
        #        return True
        #return False

    @staticmethod
    def GetMember (dynObj, name):
        if not isinstance(dynObj, IDynamicMetaObjectProvider):
            raise Exception("DynamicObjectHelpers only works on IDOs for now.")
        ## Work around an IronPython 4.0 issue:
        ## http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22735
        if clr.use35:
			func = Func
        else:
		    func = clr.GetPythonType(Type.GetType("System.Func`3"))
        site = DynamicObjectHelpers.GetSites.get(name)
        if site is None:
            site = CallSite[func[CallSite, object, object]].Create(
				       DOHelpersGetMemberBinder(name))
            DynamicObjectHelpers.GetSites[name] = site
        return site.Target(site, dynObj)

    @staticmethod
    def GetMemberNames (dynObj):
        if not isinstance(dynObj, IDynamicMetaObjectProvider):
            raise Exception("DynamicObjectHelpers only works on IDOs for now.")
        return (dynObj.GetMetaObject(Exprs.Expression.Parameter(object, "bogus"))
                       .GetDynamicMemberNames())

    @staticmethod
    def SetMember(dynObj, name, value):
        if not isinstance(dynObj, IDynamicMetaObjectProvider):
            raise Exception("DynamicObjectHelpers only works on IDOs for now.")
        ## Work around an IronPython 4.0 issue:
        ## http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=22735
        if clr.use35:
			action = Action
        else:
		    action = clr.GetPythonType(Type.GetType("System.Action`3"))
        site = DynamicObjectHelpers.SetSites.get(name)
        if site is None:
            ## For general usage ExpandoObject type param could be object.
            site = CallSite[action[CallSite, ExpandoObject, object]].Create(
				       DOHelpersSetMemberBinder(name))
            DynamicObjectHelpers.SetSites[name] = site
        site.Target(site, dynObj, value)


class DOHelpersGetMemberBinder (GetMemberBinder):
    #def __init__ (self, name, ignoreCase):
    #    ## super(...) always works, even with multiple inheritance but
    #    ## GetMemberBinder.__init__(self, name, True) would work in this case.
    #    super(DOHelpersGetMemberBinder, self).__init__(name, ignoreCase)

    def __new__ (cls, name):
        return GetMemberBinder.__new__(cls, name, True)

    def FallbackGetMember(self, targetMO, errorSuggestionMO):
        ## Don't add my own type restriction, target adds them.
        return DynamicMetaObject(
                  Exprs.Expression.Constant(DynamicObjectHelpers.Sentinel),
                  targetMO.Restrictions)

    ## Don't need Equals override or GetHashCode because there is no more
    ## specific binding metadata in this binder than what the base methods
    ## already compare.
#    def GetHashCode (self):
#        pass
#
#    def Equals (self, obj):
#        return (isinstance(obj, DOHelpersGetMemberBinder) and
#                super(DOHelpersGetMemberBinder, self).Equals(obj))


class DOHelpersSetMemberBinder (SetMemberBinder):
    #def __init__ (self, name, ignoreCase):
    #     super(DOHelpersSetMemberBinder, self).__init__(name, ignoreCase)

    def __new__ (cls, name):
        return SetMemberBinder.__new__(cls, name, True)

    def FallbackSetMember(self, targetMO, valueMO, errorSuggestionMO):
        return (errorSuggestionMO or 
                 CreateThrow(
                     targetMO, None, BindingRestrictions.Empty,
                     MissingMemberException,
                     ## General msg: Sympl doesn't override IDOs to set members.
                     "If IDynObj doesn't support setting members, " + 
                     "DOHelpers can't do it for the IDO."))

    ## Don't need Equals override or GetHashCode because there is no more
    ## specific binding metadata in this binder than what the base methods
    ## already compare.
    #def GetHashCode (self):
    #    pass
    #    
    #def Equals (self, obj):
    #    return (isinstance(obj, DOHelpersSetMemberBinder) and
    #            super(DOHelpersSetMemberBinder, self).Equals(obj))



###########################
### General Runtime Binders
###########################

### SymplGetMemberBinder is used for general dotted expressions for fetching
### members.
###
class SymplGetMemberBinder (GetMemberBinder):
    #def __init__ (self, name, ignoreCase):
    #    ## super(...) always works, even with multiple inheritance but
    #    ## GetMemberBinder.__init__(self, name, True) would work in this case.
    #    super(DOHelpersGetMemberBinder, self).__init__(name, ignoreCase)

    def __new__ (cls, name):
        return GetMemberBinder.__new__(cls, name, True) # True = IgnoreCase

    def FallbackGetMember(self, targetMO, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the GetMember.
        if not targetMO.HasValue:
            return self.Defer(targetMO)
        ## Try COM binding first.
        isCom, com = ComBinder.TryBindGetMember(self, targetMO, True)
        if isCom:
            return com
        debugprint("symplgetmember ...", targetMO.Expression, self.Name)
        ## Find our own binding.
        flags = (refl.BindingFlags.IgnoreCase | refl.BindingFlags.Static |
                 refl.BindingFlags.Instance | refl.BindingFlags.Public)
        ## bindingflags.flattenhierarchy?  public and protected static members
        members = targetMO.LimitType.GetMember(self.Name, flags)
        if len(members) == 1:
            return DynamicMetaObject(
                       EnsureObjectResult(
                         Exprs.Expression.MakeMemberAccess(
                           Exprs.Expression.Convert(targetMO.Expression,
                                                    members[0].DeclaringType),
                           members[0])),
                          ## Don't need restriction test for name since this
                          ## rule is only used where binder is used, which is
                          ## only used in sites with this binder.Name.
                       BindingRestrictions.GetTypeRestriction(
                          targetMO.Expression, targetMO.LimitType))
        else:
            if errorSuggestionMO is not None:
                return errorSuggestionMO
            return CreateThrow(
                     targetMO, None, 
                     BindingRestrictions.GetTypeRestriction(
                          targetMO.Expression, targetMO.LimitType),
                     MissingMemberException,
                     "Object " + str(targetMO.Value) + 
                     " does not have member " + self.Name)


### SymplSetMemberBinder is used for general dotted expressions for setting
### members.
###
class SymplSetMemberBinder (SetMemberBinder):
    #def __init__ (self, name, ignoreCase):
    #    ## super(...) always works, even with multiple inheritance but
    #    ## GetMemberBinder.__init__(self, name, True) would work in this case.
    #    super(DOHelpersGetMemberBinder, self).__init__(name, ignoreCase)

    def __new__ (cls, name):
        return SetMemberBinder.__new__(cls, name, True) # True = IgnoreCase

    def FallbackSetMember(self, targetMO, valueMO, errorSuggestionMO):
        debugprint("symplsetmember fallback ...", targetMO.Expression, self.Name,
                   " ..name now expr..", valueMO.Expression)
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the SetMember.
        if not targetMO.HasValue:
            return self.Defer(targetMO)
        ## Try COM object first.
        isCom, com = ComBinder.TryBindSetMember(self, targetMO, valueMO)
        if isCom:
            return com
        ## Find our own binding.
        flags = (refl.BindingFlags.IgnoreCase | refl.BindingFlags.Static |
                 refl.BindingFlags.Instance | refl.BindingFlags.Public)
        members = targetMO.LimitType.GetMember(self.Name, flags)
        if len(members) == 1:
            mem = members[0]
            val = None
			## Should check for member domain type being Type and value being
			## TypeModel, similar to ConvertArguments, and building an
			## expression like GetRuntimeTypeMoFromModel.
            if mem.MemberType == refl.MemberTypes.Property:
                val = Exprs.Expression.Convert(valueMO.Expression,
                                               mem.PropertyType)
            elif mem.MemberType == refl.MemberTypes.Field:
                val = Exprs.Expression.Convert(valueMO.Expression,
                                               mem.FieldType)
            else:
                return (errorSuggestionMO or
                        CreateThrow(
                             targetMO, None, 
                             BindingRestrictions.GetTypeRestriction(
                                  targetMO.Expression, targetMO.LimitType),
                             InvalidOperationException,
                             "Sympl only support setting properties and " +
                             "fields at this time."))
            return DynamicMetaObject(
                       EnsureObjectResult(
                         Exprs.Expression.Assign(
                           Exprs.Expression.MakeMemberAccess(
                               Exprs.Expression.Convert(
                                   targetMO.Expression,
                                   members[0].DeclaringType),
                               members[0]),
                           valueMO.Expression)),
                          ## Don't need restriction test for name since this
                          ## rule is only used where binder is used, which is
                          ## only used in sites with this binder.Name.
                       BindingRestrictions.GetTypeRestriction(
                          targetMO.Expression, targetMO.LimitType))
        else:
            if errorSuggestionMO is not None:
                return errorSuggestionMO
            return CreateThrow(
                     targetMO, None, 
                     BindingRestrictions.GetTypeRestriction(
                          targetMO.Expression, targetMO.LimitType),
                     MissingMemberException,
                     "IDynObj member name conflict.")


### SymplInvokeMemberBinder is used for general dotted expressions in function
### calls for invoking members.
###
class SymplInvokeMemberBinder (InvokeMemberBinder):
    def __new__ (cls, name, callinfo):
        return InvokeMemberBinder.__new__(cls, name, True, callinfo)

    def FallbackInvokeMember (self, targetMO, argMOs, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the InvokeMember.
        if not targetMO.HasValue or not all(map(lambda x: x.HasValue, argMOs)):
            return self.Defer((targetMO,) + tuple(argMOs))
        ## Try COM object first.
        isCom, com = ComBinder.TryBindInvokeMember(self, targetMO, argMOs)
        if isCom:
            return com
        ## Find our own binding.
        flags = (refl.BindingFlags.IgnoreCase | refl.BindingFlags.Instance |
                 refl.BindingFlags.Public)
        members = targetMO.LimitType.GetMember(self.Name, flags)
        if (len(members) == 1 and
            (isinstance(members[0], refl.PropertyInfo) or
             isinstance(members[0], refl.FieldInfo))):
            raise Exception("Haven't implemented invoking delegate values " +
                            "from properties or fields.")
            ## NOT TESTED, should check type for isinstance delegate
            #return DynamicMetaObject(
            #    Exprs.Expression.Dynamic(
            #        SymplInvokeBinder(CallInfo(len(args))),
            #        object,
            #        ([Exprs.MakeMemberAccess(self.Expression, mem)] +
            #            (x.Expression for x in args))))
            
            ## Don't test for eventinfos since we do nothing with them now.
        else:
            ## Get MethodInfos with right arg count.
            debugprint("tmmo bind invoke mem ... searching ...", len(members))
            mi_mems = [x for x in members if isinstance(x, refl.MethodInfo) and
                       len(x.GetParameters()) == len(argMOs)]
            debugprint("methodinfo members with same arg count: ", len(mi_mems))
            debugprint(mi_mems)
            res = []
            for mem in mi_mems:
                if ParamsMatchArgs(mem.GetParameters(), argMOs):
                    res.append(mem)
            ## False below means generate a type restriction on the MO.
            ## We are looking at the members targetMO's Type.
            restrictions = GetTargetArgsRestrictions(targetMO, argMOs, False)
            ## See if we have a result and return an error MO.
            if len(res) == 0:
                return (errorSuggestionMO or
                         CreateThrow(
                           targetMO, argMOs, restrictions,
                           MissingMemberException,
                           "Cannot bind member invoke -- " + repr(argMOs)))
            ## restrictions and conversion must be done consistently.
            callArgs = ConvertArguments(argMOs, res[0].GetParameters())
            return DynamicMetaObject(
                       EnsureObjectResult(
                          Exprs.Expression.Call(
                              Exprs.Expression.Convert(targetMO.Expression,
                                                       targetMO.LimitType),
                              res[0], callArgs)),
                       restrictions)

    def FallbackInvoke (self, targetMO, argMOs, errorSuggestionMO):
        ## Just "defer" since we have code in SymplInvokeBinder that knows
        ## what to do, and typically this fallback is from a language like Python
        ## that passes a DynamicMetaObject with HasValue == false.
        return DynamicMetaObject(
                   Exprs.Expression.Dynamic(
                      ## This call site doesn't share any L2 caching
                      ## since we don't call GetInvokeBinder from Sympl.
                      ## We aren't plumbed to get the runtime instance here.
                      SymplInvokeBinder(CallInfo(len(argMOs))),
                       object, #ret type
                       [targetMO.Expression] +
                          [x.Expression for x in argMOs]),
                   ## No new restrictions since SymplInvokeBinder will handle it.
                   targetMO.Restrictions.Merge(
                       BindingRestrictions.Combine(argMOs)))

## This class is needed to canonicalize InvokeMemberBinders in Sympl.  See
## the comment above the GetXXXBinder methods at the end of the Sympl class.
##
class InvokeMemberBinderKey (object):
    def __init__ (self, name, info):
        self._name = name
        self._info = info
    
    def _getName (self): return self._name
    Name = property(_getName)

    def _getInfo (self): return self._info
    Info = property(_getInfo)
    
    def __eq__ (self, obj): #def Equals (self, obj):
        return ((obj is not None) and (obj.Name == self._name) and
                obj.Info.Equals(self._info))
    
    def __hash__ (self): #def GetHashCode (self):
        return 0x28000000 ^ self._name.GetHashCode() ^ self._info.GetHashCode()


### Used for calling runtime helpers, delegate values, and callable IDO (which
### really get handled by their MOs.
###
class SymplInvokeBinder (InvokeBinder):

    def FallbackInvoke (self, targetMO, argMOs, errorSuggestionMO):
        debugprint("symplinvokebinder fallback...", targetMO.Expression, "...",
                   [x.Expression for x in argMOs])
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the InvokeMember.
        if not targetMO.HasValue or not all(map(lambda x: x.HasValue, argMOs)):
            return self.Defer((targetMO,) + tuple(argMOs))
        ## Try COM object first.
        isCom, com = ComBinder.TryBindInvoke(self, targetMO, argMOs)
        if isCom:
            return com
        ## Find our own binding.
        if targetMO.LimitType.IsSubclassOf(Delegate):
            params = targetMO.LimitType.GetMethod("Invoke").GetParameters()
            if len(params) == len(argMOs):
                debugprint("returning rule")
                debugprint(" ... ", targetMO.LimitType, "...", targetMO.Expression.Type,
                           "...", targetMO.Value)
                ## Don't need to check if argument types match parameters.
                ## If they don't, users get an argument conversion error.
                expression = Exprs.Expression.Invoke(
                               Exprs.Expression.Convert(targetMO.Expression,
                                                        targetMO.LimitType),
                               ConvertArguments(argMOs, params))
                return DynamicMetaObject(
                           EnsureObjectResult(expression),
                           BindingRestrictions.GetTypeRestriction(
                                targetMO.Expression, targetMO.LimitType))
        return (errorSuggestionMO or
                 CreateThrow(
                     targetMO, argMOs, 
                     BindingRestrictions.GetTypeRestriction(
                        targetMO.Expression, targetMO.LimitType),
                     InvalidOperationException,
                     "Wrong number of arguments for function -- " +
                     str(targetMO.LimitType) + " got " + argMOs))


### Used to instantiate types or IDOs that can be instantiated (though their MOs
### really do the work.
###
class SymplCreateInstanceBinder (CreateInstanceBinder):
    def __new__ (cls, callinfo):
        return CreateInstanceBinder.__new__(cls, callinfo)

    def FallbackCreateInstance (self, targetMO, argMOs, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the CreateInstance.
        if not targetMO.HasValue or not all(map(lambda x: x.HasValue, argMOs)):
            return self.Defer((targetMO,) + tuple(argMOs))
        ## Make sure target actually contains a Type.
        if not (Type.IsAssignableFrom(Type, targetMO.LimitType)):
            return (errorSuggestionMO or
                     CreateThrow(
                       targetMO, argMOs, BindingRestrictions.Empty,
                       InvalidOperationException,
                       ("Type object must be used when creating instance -- " +
                        repr(targetMO))))
        ## Get constructors with right arg count.
        ctors = [x for x in targetMO.Value.GetConstructors() 
                   if len(x.GetParameters()) == len(argMOs)]
        ## Get ctors with param types that work for args.  This works
        ## for except for value args that need to pass to reftype params. 
        ## We could detect that to be smarter and then explicitly StrongBox
        ## the args.
        res = []
        for mem in ctors:
            if ParamsMatchArgs(mem.GetParameters(), argMOs):
                res.append(mem)
        ## True means generate an instance restriction on the MO.
        ## We are only looking at the members defined in this Type instance.
        restrictions = GetTargetArgsRestrictions(targetMO, argMOs, True)
        if len(res) == 0:
            return (errorSuggestionMO or
                     CreateThrow(
                       targetMO, argMOs, restrictions,
                       MissingMemberException,
                       "Can't bind create instance -- " + repr(targetMO)))
        ## restrictions and conversion must be done consistently.
        callArgs = ConvertArguments(argMOs, res[0].GetParameters())
        return DynamicMetaObject(
                   ## Creating an object, so don't need EnsureObjectResult.
                   Exprs.Expression.New(res[0], callArgs),
                   restrictions)


class SymplGetIndexBinder (GetIndexBinder):
    #def __new__ (cls, callinfo):
    #    return GetIndexBinder.__new__(cls, callinfo)

    def FallbackGetIndex (self, targetMO, argMOs, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the InvokeMember.
        if not targetMO.HasValue or not all(map(lambda x: x.HasValue, argMOs)):
            return self.Defer((targetMO,) + tuple(argMOs))
        ## Try COM object first.
        isCom, com = ComBinder.TryBindGetIndex(self, targetMO, argMOs)
        if isCom:
            return com
        ## Give a good error for Cons.
        if type(targetMO.Value) is Cons:
            if len(argMOs) != 1:
                return (errorSuggestionMO or
                        CreateThrow(
                             targetMO, argMOs, BindingRestrictions.Empty,
                             InvalidOperationException,
                             "Indexing Sympl list requires exactly one argument."))
        ## Find our own binding.
        ##
        ## Conversions created in GetIndexExpression must be consistent with
        ## restrictions made in GetTargetArgsRestrictions.
        return DynamicMetaObject(
                  EnsureObjectResult(GetIndexExpression(targetMO, argMOs)),
                  ## False means make type restriction on targetMO.LimitType
                  GetTargetArgsRestrictions(targetMO, argMOs, False))


class SymplSetIndexBinder (SetIndexBinder):
    #def __new__ (cls, callinfo):
    #    return SetIndexBinder.__new__(cls, callinfo)

    def FallbackSetIndex (self, targetMO, argMOs, valueMO, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the SetIndex.
        if (not targetMO.HasValue or not all(map(lambda x: x.HasValue, argMOs)) or
            not valueMO.HasValue):
            return self.Defer((targetMO,) + tuple(argMOs) + (valueMO,))
        ## Try COM object first.
        isCom, com = ComBinder.TryBindSetIndex(self, targetMO, argMOs, valueMO)
        if isCom:
            return com
        ## Find our own binding.  First setup value.
        valueExpr = valueMO.Expression
        if type(valueMO.Value) is TypeModel:  
            ## Don't use LimitType to compare py type objs, use the value.
            valueExpr = GetRuntimeTypeMoFromModel(valueMO).Expression
        ## Check Cons vs. normal
        if type(targetMO.Value) is Cons:  
            ## Don't use LimitType to compare py type objs, use the value.
            if len(argMOs) != 1:
                return (errorSuggestionMO or
                         CreateThrow(
                             targetMO, argMOs, BindingRestrictions.Empty,
                             InvalidOperationException,
                             "Indexing Sympl list requires exactly one argument."))
            setIndexExpr = (
                ## In C# can use Expression.Call on methodinfo.
                Exprs.Expression.Dynamic(
                    RunHelpersInvokeBinder(CallInfo(3)),
                    object,
                    Exprs.Expression.Constant(RuntimeHelpers.SetConsElt),
                    Exprs.Expression.Convert(targetMO.Expression,
                                             targetMO.LimitType),
                    Exprs.Expression.Convert(argMOs[0].Expression,
                                             argMOs[0].LimitType),
                    ## Calling Py runtime helper doesn't need the type
                    ## conversions, and it is unnecessarily boxing in python.
                    valueExpr))
        else:
            indexExpr = GetIndexExpression(targetMO, argMOs)
            setIndexExpr = EnsureObjectResult(
                               Exprs.Expression.Assign(indexExpr, valueExpr))
        ## False means make type restriction on targetMO.LimitType
        restrictions = GetTargetArgsRestrictions(targetMO, argMOs, False)
        return DynamicMetaObject(setIndexExpr, restrictions)
        

class SymplBinaryOperationBinder (BinaryOperationBinder):

    def FallbackBinaryOperation (self, leftMO, rightMO, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the SetIndex.
        if not leftMO.HasValue or not rightMO.HasValue:
            self.Defer(leftMO, rightMO)
        restrictions = (leftMO.Restrictions.Merge(rightMO.Restrictions)
            .Merge(BindingRestrictions.GetTypeRestriction(
                leftMO.Expression, leftMO.LimitType))
            .Merge(BindingRestrictions.GetTypeRestriction(
                rightMO.Expression, rightMO.LimitType)))
        return DynamicMetaObject(
            EnsureObjectResult(
              Exprs.Expression.MakeBinary(
                self.Operation,
                Exprs.Expression.Convert(leftMO.Expression, leftMO.LimitType),
                Exprs.Expression.Convert(rightMO.Expression, rightMO.LimitType))),
            restrictions)


### This is mostly for example and plumbing in case anyone adds a dynamic unary
### operator.  The only unary Op Sympl suports is logical Not, which it handles
### without a dynamic node since everything that is not nil or false is true.
###
class SymplUnaryOperationBinder (UnaryOperationBinder):

    def FallbackUnaryOperation (self, operandMO, errorSuggestionMO):
        ## Defer if any object has no value so that we evaulate their
        ## Expressions and nest a CallSite for the SetIndex.
        if not operandMO.HasValue:
            self.Defer(operandMO)
        return DynamicMetaObject(
            EnsureObjectResult(
              Exprs.Expression.MakeUnary(
                self.Operation,
                Exprs.Expression.Convert(operandMO.Expression,
                                         operandMO.LimitType),
                operandMO.LimitType)),
            operandMO.Restrictions.Merge(
                BindingRestrictions.GetTypeRestriction(
                                        operandMO.Expression,
                                        operandMO.LimitType)))



###########################
### Cons Cells and Symbols
###########################

class Symbol (object):
    def __init__ (self, name):
        self._name = name
        self._value = None
        self.Plist = None

    ### Need __repr__ to just print name, not <Symbol name>, when Symbols are
    ### inside list structures.
    ###
    def __repr__ (self):
        return self._name
        ## IPy doesn't bind repr for Py printing, and ToString for .NET
        ## printing.  Need to print here like we want for ToString.
        #return "<Symbol " + self.Name + ">"

    ### Need ToString when Sympl program passing Symbol to Console.WriteLine.
    ### Otherwise, it prints as internal IPy constructed type.
    ###
    def ToString (self):
        return self._name

    def _getName (self): return self._name
    def _setName (self, value):
        self._name = value
        return value
    Name = property(_getName, _setName)

    def _getValue (self): return self._value
    def _setValue (self, value):
        self._value = value
        return value
    Value = property(_getValue, _setValue)

    def _getPlist (self): return self._plist
    def _setPlist (self, value):
        self._plist = value
        return value
    PList = property(_getPlist, _setPlist)


class Cons (object):
    def __init__ (self, first, rest):
        self._first = first
        self._rest = rest

    ### NOTE: does not handle circularities!
    ###
    def __repr__ (self):
        head = self
        res = "("
        while head is not None:
            res = res + repr(head._first)
            if head._rest is None:
                head = None
            elif type(head._rest) is Cons:
                head = head._rest
                res = res + " "
            else:
                res = res + " . " + repr(head._rest)
                head = None
        return res + ")"

    def ToString (self):
        return self.__repr__()

    def _getFirst (self):
        return self._first

    def _setFirst (self, value):
        self._first = value
        return value

    First = property(_getFirst, _setFirst)

    def _getRest (self):
        return self._rest

    def _setRest (self, value):
        self._rest = value
        return value

    Rest = property(_getRest, _setRest)

    ### In C# this will be internal to the Sympl Runtime, called only by the
    ### code emitted when analyzing a keyword form invocation for List.
    ###
    @staticmethod
    def _List (*elements):
        if len(elements) == 0: return None
        head = Cons(elements[0], None)
        tail = head
        for elt in elements[1:]:
            tail.Rest = Cons(elt, None)
            tail = tail.Rest
        return head



##################
### Dev-time Utils
##################

_debug = False
def debugprint (*stuff):
    if _debug:
        for x in stuff:
            print x,
        print


