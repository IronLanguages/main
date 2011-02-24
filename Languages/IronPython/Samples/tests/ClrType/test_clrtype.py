#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

from iptest.assert_util import *
from clrtype import ClrInterface, ClrClass, accepts, returns
from clr import GetClrType
import System

class ClrTypeInterface(object):
    __metaclass__ = ClrInterface
    @accepts(str)
    @returns(str)
    def InterfaceMethod(self, s): raise RuntimeError("this should not get called")

class ClrTypeClass(ClrTypeInterface):
    __metaclass__ = ClrClass
    def InterfaceMethod(self, s = ""): return  "ClrTypeClass-InterfaceMethod-" + s
    @accepts(str)
    @returns(str)
    def ClassMethod(self, s = ""): return  "ClrTypeClass-ClassMethod-" + s

ct = GetClrType(ClrTypeClass)

# Ensures that the CLR types have unique names
global namespace_count
namespace_count = 0

def get_unique_namespace():
    global namespace_count
    namespace_count += 1
    return "test_clrclass_namespace_" + str(namespace_count)

def for_interface_and_class(f):
    def test_interface_and_class():
        for metaclass in [ClrInterface, ClrClass]:
            f(metaclass)
    return test_interface_and_class

#------------------------------------------------------------------------------
# Namespaces

def test_no_namespace():
    AreEqual(ct.FullName, "ClrTypeClass")

@for_interface_and_class
def test_namespace(metaclass):
    n = get_unique_namespace()
    class T(object):
        __metaclass__ = metaclass
        _clrnamespace = n
    AreEqual(GetClrType(T).FullName, n + ".T")

#------------------------------------------------------------------------------
# Methods

types_and_values = [
    (int, 100), 
    (float, 100.5),
    (System.DateTime, System.DateTime.Now), # struct
    (System.DateTimeKind, System.DateTimeKind.Local), # enum
    (System.Action[str], System.Action[str](dir)), # delegate
    (System.IComparable, "hello"), # type collision
    (System.IComparable[str], "hello"), # generic type
    (System.Collections.Generic.List[str], System.Collections.Generic.List[str]()), # Collection
    (ClrTypeInterface, ClrTypeClass()),
    (ClrTypeClass, ClrTypeClass())
]

@for_interface_and_class
def test_argument_types(metaclass):
    for t, v in types_and_values:
        class T(object):
            __metaclass__ = metaclass
            _clrnamespace = get_unique_namespace()
            @accepts(t)
            @returns()
            def Foo(self, a):
                AreEqual(a, v)
        foo = GetClrType(T).GetMethod("Foo")
        
        p0 = foo.GetParameters()[0]
        AreEqual(p0.ParameterType, GetClrType(t))
        
        if metaclass is ClrClass:
            foo.Invoke(T(), System.Array[object]( [v] ))

@for_interface_and_class
def test_return_type(metaclass):
    for t, v in types_and_values:
        class T(object):
            __metaclass__ = metaclass
            _clrnamespace = get_unique_namespace()
            @accepts()
            @returns(t)
            def Foo(self): return v
        foo = GetClrType(T).GetMethod("Foo")
        
        AreEqual(foo.ReturnType, GetClrType(t))
        
        if metaclass is ClrClass:
            AreEqual(v, foo.Invoke(T(), None))

#--MAIN------------------------------------------------------------------------
run_test(__name__)

