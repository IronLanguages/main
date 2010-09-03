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
from test_clrtype import get_unique_namespace, ClrTypeInterface, ClrTypeClass

ct = GetClrType(ClrTypeClass)

#------------------------------------------------------------------------------
# Base types

@disabled("clrtype.py currently generates an intermediate class")
def test_base_type_is_honored():
    AreEqual(ct.BaseType, GetClrType(object))

def test_can_implement_clrtype_interface():
    class C(ClrTypeInterface): pass
    AssertContains(GetClrType(C).GetInterfaces(), ClrTypeInterface)
    
def test_class_can_inherit_from_clrtype_class():
    class C(ClrTypeClass): pass

def test_clrtype_class_can_inherit_from_clrtype_class():
    class C(ClrTypeClass):
        __metaclass__ = ClrClass

#------------------------------------------------------------------------------
# super

def test_can_call_interface_method_of_base_clrtype_class():
    class C(ClrTypeClass):
        def InterfaceMethod(self, str = ""): return super(C, self).InterfaceMethod()
    AreEqual(C().InterfaceMethod(), ClrTypeClass().InterfaceMethod())

def test_can_call_method_of_base_clrtype_class():
    class C(ClrTypeClass):
        def ClassMethod(self, str = ""): return super(C, self).ClassMethod()
    AreEqual(C().ClassMethod(), ClrTypeClass().ClassMethod())

def test_can_call_object_method_of_base_clrtype_class():
    class C(ClrTypeClass):
        def ToString(self): return super(C, self).ToString()
    AssertContains(C().ToString(), "IronPython.NewTypes.")

def test_can_call_object_method_of_base_base_clrtype_class():
    class C(ClrTypeClass):
        def ToString(self): return super(ClrTypeClass, self).ToString()
    AssertContains(C().ToString(), "IronPython.NewTypes.")

#------------------------------------------------------------------------------
# isinstance

def test_isinstance():
    Assert(isinstance(ClrTypeClass(), ClrTypeClass))
    
def test_isinstance_for_interface():
    Assert(isinstance(ClrTypeClass(), ClrTypeInterface))
    
#--MAIN------------------------------------------------------------------------
run_test(__name__)

