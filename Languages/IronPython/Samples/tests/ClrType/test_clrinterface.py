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

#------------------------------------------------------------------------------
# Base types

def test_base_interfaces_are_honored():
    class I(System.IComparable, System.IConvertible):
        __metaclass__ = ClrInterface
        _clrnamespace = get_unique_namespace()
        
    AssertContains(GetClrType(I).GetInterfaces(), System.IComparable)
    AssertContains(GetClrType(I).GetInterfaces(), System.IConvertible)

def test_clrtype_interface_can_inherit_clrtype_interface():
    class I(ClrTypeInterface):
        __metaclass__ = ClrInterface
        _clrnamespace = get_unique_namespace()
    AssertContains(GetClrType(I).GetInterfaces(), GetClrType(ClrTypeInterface))
    
def test_python_class_can_implement_clrtype_interface():
    class C(ClrTypeInterface): pass
    AssertContains(C().GetType().GetInterfaces(), ClrTypeInterface)

#--MAIN------------------------------------------------------------------------
run_test(__name__)

