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

import clr
import clrtype
import System
from System.Reflection import BindingFlags

class IProduct(object):
    __metaclass__ = clrtype.ClrInterface

    _clrnamespace = "IronPython.Samples.ClrType"   

    @property
    @clrtype.accepts()
    @clrtype.returns(str)
    def Name(self): raise RuntimeError("this should not get called")
      
    @property
    @clrtype.accepts()
    @clrtype.returns(float)
    def Cost(self): raise RuntimeError("this should not get called")
      
    @clrtype.accepts()
    @clrtype.returns(bool)
    def IsAvailable(self): raise RuntimeError("this should not get called")

class Product(IProduct):
    __metaclass__ = clrtype.ClrClass
    
    _clrnamespace = "IronPython.Samples.ClrType"   
    
    _clrfields = {
        "name":str,
        "cost":float,
        "_quantity":int
    }
      
    CLSCompliant = clrtype.attribute(System.CLSCompliantAttribute)
    clr.AddReference("System.Xml")
    XmlRoot = clrtype.attribute(System.Xml.Serialization.XmlRootAttribute)
    
    _clrclassattribs = [
        # Use System.Attribute subtype directly for custom attributes without arguments
        System.ObsoleteAttribute,
        # Use clrtype.attribute for custom attributes with arguments (either positional, named, or both)
        CLSCompliant(False),
        XmlRoot("product", Namespace="www.contoso.com")
    ]

    def __init__(self, name, cost, quantity):
        self.name = name
        self.cost = cost
        self._quantity = quantity

    # IProduct methods    
    def Name(self): return self.name
    def Cost(self): return self.cost
    def IsAvailable(self): return self.quantity != 0

    @property
    @clrtype.accepts()
    @clrtype.returns(int)
    def quantity(self): return self._quantity
    
    @quantity.setter
    @clrtype.accepts(int)
    @clrtype.returns()
    def quantity(self, value): self._quantity = value

    @clrtype.accepts(float)
    @clrtype.returns(float)
    def calc_total(self, discount = 0.0):
        return (self.cost - discount) * self.quantity

class NativeMethods(object):
    # Note that you could also the "ctypes" modules instead of pinvoke declarations
    __metaclass__ = clrtype.ClrClass

    from System.Runtime.InteropServices import DllImportAttribute, PreserveSigAttribute
    DllImport = clrtype.attribute(DllImportAttribute)
    PreserveSig = clrtype.attribute(PreserveSigAttribute)
    
    @staticmethod
    @DllImport("user32.dll")
    @PreserveSig()
    @clrtype.accepts(System.Char)
    @clrtype.returns(System.Boolean)
    def IsCharAlpha(c): raise RuntimeError("this should not get called")

    @staticmethod
    @DllImport("user32.dll")
    @PreserveSig()
    @clrtype.accepts(System.IntPtr, System.String, System.String, System.UInt32)
    @clrtype.returns(System.Int32)
    def MessageBox(hwnd, text, caption, type): raise RuntimeError("this should not get called")

def print_classattribs(p):
    t = clr.GetClrType(p.GetType())
    oa = t.GetCustomAttributes(System.ObsoleteAttribute, True)[0]
    clsc = t.GetCustomAttributes(System.CLSCompliantAttribute, True)[0]
    xmlr = t.GetCustomAttributes(System.Xml.Serialization.XmlRootAttribute, True)[0]
    print "Type attributes:"
    print "    Obsolete.Message:", oa.Message
    print "    CLSCompliant.IsCompliant:", clsc.IsCompliant
    print "    XmlRoot.ElementName:", xmlr.ElementName
    print "    XmlRoot.NameSpace:", xmlr.Namespace

def display_clr_type_info(p):
    print "CLR type name: %s" % p.GetType().FullName
    bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
    print "CLR fields: %s" % sorted([field.Name for field in p.GetType().GetFields(bf)])
    print "CLR properties: %s" % sorted([property.Name for property in p.GetType().GetProperties(bf)])
    introduced_methods = [method.Name for method in p.GetType().GetMethods() if method.DeclaringType == method.ReflectedType]
    print "CLR methods (introduced): %s" % sorted(introduced_methods)
    print_classattribs(p)
    print

def call_interface_members(p):
    # We use Reflection to simulate a call from a strongly-typed language
    print "Calling IProduct.Name and IProduct.IsAvailable as a strongly-typed members:"
    name = clr.GetClrType(IProduct).GetProperty("Name").GetGetMethod()
    print "Name:", name.Invoke(p, None)
    isAvailable = clr.GetClrType(IProduct).GetMethod("IsAvailable")
    print "IsAvailable:", isAvailable.Invoke(p, None)

def call_typed_method(p):
    print "Calling calc_total as a strongly-typed method using Reflection:"
    calc_total = p.GetType().GetMethod("calc_total")
    print "calc_total CLR signature:", calc_total
    args = System.Array[object]( (1.0,) )
    print "calc_total(1.0):", calc_total.Invoke(p, args)
    print

def call_pinvoke_method():
    print "Calling pinvoke methods:"
    print "IsCharAlpha('A'):", NativeMethods.IsCharAlpha('A')
    # Call statically-typed method from another .NET language (simulated using Reflection)
    isCharAlpha = clr.GetClrType(NativeMethods).GetMethod('IsCharAlpha')
    args = System.Array[object](('1'.Chars[0],))
    print "IsCharAlpha('1') from another language:", isCharAlpha.Invoke(None, args)
    # NativeMethods.MessageBox(System.IntPtr.Zero, "some text", "some caption", System.UInt32(0))
    print

def python_dynamism(p):
    # The object is still a Python object, and so allows setting new attributes
    p.sale_discount = 2.0

    # The class is also a Python class, and can also be modified
    print "Changing calc_total from Python"
    def new_calc_total(self, discount = 0.0, log = False):
        if log: print "Calling new_calc_total"
        return (self.cost - (discount + self.sale_discount)) * self.quantity
    Product.calc_total = new_calc_total
    call_typed_method(p)

p = Product("Widget", 10.0, 42)

display_clr_type_info(p)
call_interface_members(p)
call_typed_method(p)
call_pinvoke_method()
python_dynamism(p)
