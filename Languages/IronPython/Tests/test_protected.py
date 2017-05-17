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

import sys
from iptest.assert_util import *
skiptest("win32")

load_iron_python_test()
from IronPythonTest import *

# properties w/ differening access
def test_base():
    # TODO: revisit this
    # can't access protected methods directly in Silverlight
    # (need to create a derived class)
    #if not is_silverlight:
    #    a = BaseClass()
    #    AreEqual(a.Area, 0)
    #    a.Area = 16
    #    AreEqual(a.Area, 16)
        
    class WrapBaseClass(BaseClass): pass
    a = WrapBaseClass()
    AreEqual(a.Area, 0)
    a.Area = 16
    AreEqual(a.Area, 16)


def test_derived():
    class MyBaseClass(BaseClass):
        def MySetArea(self, size):
            self.Area = size

    a = MyBaseClass()
    AreEqual(a.Area, 0)

    a.MySetArea(16)
    AreEqual(a.Area, 16)

    a.Area = 36
    AreEqual(a.Area, 36)

    # protected fields
    AreEqual(a.foo, 0)
    a.foo = 7
    AreEqual(a.foo, 7)

def test_super_protected():
    class x(object): pass
    
    clone = super(x, x()).MemberwiseClone()
    AreEqual(type(clone), x)

def test_override():
    # overriding methods

    # can't access protected methods directly
    a = Inherited()
    
    # they are present...
    Assert('ProtectedMethod' in dir(a))
    Assert('ProtectedProperty' in dir(a))
    Assert(hasattr(a, 'ProtectedMethod'))
    
    # hasattr returns false if the getter raises...
    Assert(not hasattr(a, 'ProtectedProperty'))
    AssertErrorWithMessage(TypeError, "cannot access protected member ProtectedProperty without a python subclass of Inherited", lambda : a.ProtectedProperty)
    
    class WrapInherited(Inherited): pass
    a = WrapInherited()
    AreEqual(a.ProtectedMethod(), 'Inherited.ProtectedMethod')
    AreEqual(a.ProtectedProperty, 'Inherited.Protected')

    class MyInherited(Inherited):
        def ProtectedMethod(self):
            return "MyInherited"
        def ProtectedMethod(self):
            return "MyInherited Override"
        def ProtectedPropertyGetter(self):
            return "MyInherited.Protected"
        ProtectedProperty = property(ProtectedPropertyGetter)

    a = MyInherited()
    
    AreEqual(a.ProtectedMethod(), 'MyInherited Override')
    AreEqual(a.CallProtected(), 'MyInherited Override')
    AreEqual(a.ProtectedProperty, "MyInherited.Protected")
    AreEqual(a.CallProtectedProp(), "MyInherited.Protected")

def test_events():
    
    # can't access protected methods directly
    a = Events()
    
    # they are present...
    Assert('OnProtectedEvent' in dir(a))
    Assert('OnExplicitProtectedEvent' in dir(a))
    Assert(hasattr(a, 'OnProtectedEvent'))
    Assert(hasattr(a, 'OnExplicitProtectedEvent'))
    
    # they should not be present
    Assert('add_OnProtectedEvent' not in dir(a))
    Assert('remove_OnProtectedEvent' not in dir(a))
    Assert('add_OnExplicitProtectedEvent' not in dir(a))
    Assert('remove_OnExplicitProtectedEvent' not in dir(a))

    # should not be present as its private
    Assert('ExplicitProtectedEvent' not in dir(a))
    
    def OuterEventHandler(source, args):
        global called
        called = True

    global called
    # Testing accessing protected Events fails.  
    # TODO: Currently adding non-protected events do not generate errors due to lack of context checking
    called = False
    #AssertErrorWithMessage(TypeError, "Cannot add handler to a private event.", lambda : a.OnProtectedEvent += OuterEventHandler)
    a.OnProtectedEvent += OuterEventHandler
    a.FireProtectedTest()
    a.OnProtectedEvent -= OuterEventHandler
    #AssertErrorWithMessage(TypeError, "Cannot remove handler to a private event.", lambda : a.OnProtectedEvent -= OuterEventHandler)
    #AreEqual(called, False) # indicates that event fired and set value which should not be allowed
    
    called = False
    #AssertErrorWithMessage(TypeError, "Cannot add handler to a private event.", lambda : a.OnExplicitProtectedEvent += OuterEventHandler)
    a.OnExplicitProtectedEvent += OuterEventHandler
    a.FireProtectedTest()
    a.OnExplicitProtectedEvent -= OuterEventHandler
    #AssertErrorWithMessage(TypeError, "Cannot remove handler to a private event.", lambda : a.OnExplicitProtectedEvent -= OuterEventHandler)
    #AreEqual(called, False)
    
    
    class MyInheritedEvents(Events):
        called3 = False
        called4 = False
        
        def __init__(self):
            self.called1 = False
            self.called2 = False
            
        def InnerEventHandler1(self, source, args):
            self.called1 = True
            
        def InnerEventHandler2(self, source, args):
            self.called2 = True
            
        def RegisterEventsInstance(self):
            self.OnProtectedEvent += OuterEventHandler
            self.OnProtectedEvent += self.InnerEventHandler1
            self.OnExplicitProtectedEvent += self.InnerEventHandler2
            
        def UnregisterEventsInstance(self):
            self.OnProtectedEvent -= self.InnerEventHandler1
            self.OnExplicitProtectedEvent -= self.InnerEventHandler2

        @classmethod
        def InnerEventHandler3(cls, source, args):
            cls.called3 = True
            
        @classmethod
        def InnerEventHandler4(cls, source, args):
            cls.called4 = True
            
        @classmethod        
        def RegisterEventsStatic(cls, events):
            events.OnProtectedEvent += OuterEventHandler
            events.OnProtectedEvent += cls.InnerEventHandler3
            events.OnExplicitProtectedEvent += cls.InnerEventHandler4
            
        @classmethod
        def UnregisterEventsStatic(cls, events):
            events.OnProtectedEvent -= OuterEventHandler
            events.OnProtectedEvent -= cls.InnerEventHandler3
            events.OnExplicitProtectedEvent -= cls.InnerEventHandler4
    
    # validate instance methods work
    b = MyInheritedEvents()
    called = b.called1 = b.called2 = False
    b.RegisterEventsInstance()
    b.FireProtectedTest()
    AreEqual(called, True)
    AreEqual(b.called1, True)
    AreEqual(b.called2, True)

    # validate theat static methods work
    c = MyInheritedEvents()
    called = MyInheritedEvents.called3 = MyInheritedEvents.called4 = False
    MyInheritedEvents.RegisterEventsStatic(c)
    c.FireProtectedTest()
    MyInheritedEvents.UnregisterEventsStatic(c)
    AreEqual(called, True)
    AreEqual(MyInheritedEvents.called3, True)
    AreEqual(MyInheritedEvents.called4, True)

    class WrapEvents(Events): 
        @classmethod        
        def RegisterEventsStatic(cls, events):
            events.OnProtectedEvent += OuterEventHandler
        @classmethod
        def UnregisterEventsStatic(cls, events):
            events.OnProtectedEvent -= OuterEventHandler
    
    # baseline empty test 
    d = Events()
    called = False
    d.FireProtectedTest()
    AreEqual(called, False)
        
    # use wrapevents to bypass protection
    called = False
    WrapEvents.RegisterEventsStatic(d)
    d.FireProtectedTest()
    WrapEvents.UnregisterEventsStatic(d)
    AreEqual(called, True)


run_test(__name__)
