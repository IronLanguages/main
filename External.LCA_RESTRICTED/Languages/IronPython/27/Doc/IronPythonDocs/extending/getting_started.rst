.. highlightlang:: c
.. hosting-getting-started:
   :synopsis: IronPython Simple Hosting Overview

*******************
  Getting Started 
*******************

Introduction
============

The simplest way to get starded with embedding IronPython in your application is to
use the Python class defined in IronPython.Hosting.  This class includes functions
for creating IronPython script engines and accessing commonly used Python-specific
functionality.  Much of the functionality is exposed as .NET extension methods so that
it seamlessly extends the normal DLR hosting API surface.

Most of the functions in the Python class have overloads that operate on either a 
ScriptRuntime or a ScriptEngine.  These functions provide identical functionality
but both forms are provided for convenience for when you may be working with either
an engine or an entire runtime.

Samples
=======

Hello World
-----------

The simplest program to start off with is always Hello World.  Here is a simple
Hello World app that creates a Python engine from C#::

    using System;
    using IronPython.Hosting;

    public class HelloWorld {
        public static void Main(string[] args) {
            var engine = Python.CreateEngine();
            engine.CreateScriptSourceFromString("print 'hello world'").Execute();
        }
    }

Compile using:
  csc HelloWorld.cs /r:IronPython.dll /r:Microsoft.Scripting.dll
  
Here's the equivalent program in VB.NET::

    Imports System
    Imports IronPython.Hosting
    
    Public Class HelloWorld
        Public Shared Sub Main(ByVal args As String())
            Python.CreateEngine.CreateScriptSourceFromString("print 'hello world'").Execute
        End Sub
    End Class

Compile using:
    vbc HelloWorld.vb /r:IronPython.dll /r:Microsoft.Scripting.dll /r:Microsoft.Scripting.Core.dll


And again in in IronPython::

    import clr
    clr.AddReference('IronPython')
    from IronPython.Hosting import Python
    
    engine = Python.CreateEngine()
    engine.CreateScriptSourceFromString("print 'hello world'").Execute()


Calling Python from C#
----------------------
This sample demonstrates how to get functions and classes defined in Python and access them
from your .NET application.  

In order to get back the results of executed Python code we 
need to first create a ScriptScope in which we can execute the code.  We then execute the
code we're interested inside of that scope and fetch the values back out.  To get the values
back out we use the GetVariable method defined on ScriptScope.  There is both a generic
version of this method as well as a non-generic version - here we use the generic version which allows us to
perform conversions on the the result of the variable.  One of the most convenient conversions
for functions and classes is to convert these objects to delegates.  This allows us to
repeatedly call the same function or class with different arguments.


C# Example::

    using System;
    using IronPython.Hosting;

    public class CallingPython {
        public static void Main(string[] args) {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromString(
                "def adder(arg1, arg2):\n" +
                "   return arg1 + arg2\n"  +
                "\n" +
                "class MyClass(object):\n" +
                "   def __init__(self, value):\n" +
                "       self.value = value\n");
            source.Execute(scope);
            
            var adder = scope.GetVariable<Func<object, object, object>>("adder");
            Console.WriteLine(adder(2, 2));
            Console.WriteLine(adder(2.0, 2.5));

            var myClass = scope.GetVariable<Func<object, object>>("MyClass");
            var myInstance = myClass("hello");
            
            Console.WriteLine(engine.Operations.GetMember(myInstance, "value"));
        }
    }

Compile using:
  csc CallingPython.cs /r:IronPython.dll /r:Microsoft.Scripting.dll /r:Microsoft.Scripting.Core.dll

VB.NET Example::

    Imports System
    Imports IronPython.Hosting
    
    Public Class CallingPython    
        Public Shared Sub Main(ByVal args As String())
            Dim engine = Python.CreateEngine
            Dim scope = engine.CreateScope
            engine.CreateScriptSourceFromString( _
                "def adder(arg1, arg2):" & VbCrLf & _   
                "   return arg1 + arg2" & VbCrLf & VbCrLf & _
                "class MyClass(object):" & VbCrLf & _
                "   def __init__(self, value):" & VbCrLf & _
                "       self.value = value" & VbCrLf).Execute(scope)
            
            Dim variable As Func(Of Object, Object, Object) = scope.GetVariable(Of Func(Of Object, Object, Object))("adder")
            Console.WriteLine(variable.Invoke(2, 2))
            Console.WriteLine(variable.Invoke(2, 2.5))
            
            Dim obj2 As Object = scope.GetVariable(Of Func(Of Object, Object))("MyClass").Invoke("hello")
            Console.WriteLine(engine.Operations.GetMember(obj2, "value"))
        End Sub
    End Class
    
Compile using:
  vbc CallingPython.vb /r:IronPython.dll /r:Microsoft.Scripting.dll /r:Microsoft.Scripting.Core.dll


IronPython Example::

    import clr
    clr.AddReference('IronPython')
    from IronPython.Hosting import Python
    from System import Console
    
    engine = Python.CreateEngine()
    
    scope = engine.CreateScope()
    source = engine.CreateScriptSourceFromString(
        "def adder(arg1, arg2):\n"
        "   return arg1 + arg2\n" 
        "\n" 
        "class MyClass(object):\n" 
        "   def __init__(self, value):\n" 
        "       self.value = value\n")
    source.Execute(scope)
    
    adder = scope.adder
    Console.WriteLine(adder(2, 2))
    Console.WriteLine(adder(2.0, 2.5))
    
    myClass = scope.MyClass
    myInstance = myClass("hello")
    
    Console.WriteLine(engine.Operations.GetMember(myInstance, "value"))


Exposing Application Object Model
---------------------------------
Another common them in hosting your IronPython in your application is exposing your own
object model to users scripting your application.  To do this is the exact opposite of calling
Python from C#.  Instead of getting values from a ScriptScope you will simply inject the
object model into the ScriptScope.

C# Example::

    using System;
    using IronPython.Hosting;

    public class CallingDotNet {
        private static void Main(string[] args) {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            
            scope.SetVariable("my_object_model", new CallingDotNet());
            
            engine.CreateScriptSourceFromString("my_object_model.Foo(42)").Execute(scope);
        }
        
        public void Foo(int arg) {
            Console.WriteLine("You gave me a {0}", arg);
        }
    }

Compile using:
  csc CallingDotNet.cs /r:IronPython.dll /r:Microsoft.Scripting.dll /r:Microsoft.Scripting.Core.dll
    
VB.NET Example::

    Imports System
    Imports IronPython.Hosting
    
    Public Class CallingDotNet    
        Private Shared Sub Main(ByVal args As String())
            Dim engine As ScriptEngine = Python.CreateEngine
            Dim scope As ScriptScope = engine.CreateScope
            scope.SetVariable("my_object_model", New CallingDotNet)
            engine.CreateScriptSourceFromString("my_object_model.Foo(42)").Execute(scope)
        End Sub
        
        Public Sub Foo(ByVal arg As Integer)
            Console.WriteLine("You gave me a {0}", arg)
        End Sub
    End Class

Compile using:
  vbc CallingDotNet.vb /r:IronPython.dll /r:Microsoft.Scripting.dll /r:Microsoft.Scripting.Core.dll


IronPython Example::

    class CallingPython(object):
        def Foo(self, value):
            print 'You gave me', value
    
    import clr
    clr.AddReference('IronPython')
    from IronPython.Hosting import Python
    from System import Console
    
    engine = Python.CreateEngine()
    scope = engine.CreateScope()
    
    scope.SetVariable("my_object_model", CallingPython())
    engine.CreateScriptSourceFromString("my_object_model.Foo(42)").Execute(scope)


Reference
=========

The Python class defines the following functions:

.. cfunction:: ScriptRuntime CreateRuntime()

   Creates a new ScriptRuntime with the IronPython scipting engine pre-configured.

.. cfunction:: ScriptRuntime CreateRuntime(IDictionary<string, object> options)

   Creates a new ScriptRuntime with the IronPython scipting engine pre-configured and
   additional options.

.. cfunction:: ScriptRuntime CreateRuntime(AppDomain domain)

   Creates a new ScriptRuntime with the IronPython scripting engine pre-configured
   in the specified AppDomain.  The remote ScriptRuntime may  be manipulated from 
   the local domain but all code will run in the remote domain.

.. cfunction:: ScriptRuntime CreateRuntime(AppDomain domain, IDictionary<string, object> options)

   Creates a new ScriptRuntime with the IronPython scripting engine pre-configured
   in the specified AppDomain with additional options.  The remote ScriptRuntime may 
   be manipulated from the local domain but all code will run in the remote domain.

.. cfunction:: ScriptEngine CreateEngine()

   Creates a new ScriptRuntime and returns the ScriptEngine for IronPython. If
   the ScriptRuntime is requierd it can be acquired from the Runtime property
   on the engine.

.. cfunction:: ScriptEngine CreateEngine(IDictionary<string, object> options)

   Creates a new ScriptRuntime with the specified options and returns the 
   ScriptEngine for IronPython. If the ScriptRuntime is requierd it can be 
   acquired from the Runtime property on the engine.

.. cfunction:: ScriptEngine CreateEngine(AppDomain domain)

   Creates a new ScriptRuntime and returns the ScriptEngine for IronPython. If
   the ScriptRuntime is requierd it can be acquired from the Runtime property
   on the engine.

   The remote ScriptRuntime may be manipulated from the local domain but 
   all code will run in the remote domain.

.. cfunction:: ScriptEngine CreateEngine(AppDomain domain, IDictionary<string, object> options)

   Creates a new ScriptRuntime with the specified options and returns the 
   ScriptEngine for IronPython. If the ScriptRuntime is requierd it can be 
   acquired from the Runtime property on the engine.
    
   The remote ScriptRuntime may be manipulated from the local domain but 
   all code will run in the remote domain.

.. cfunction:: ScriptEngine GetEngine(ScriptRuntime runtime)

   Given a ScriptRuntime gets the ScriptEngine for IronPython.

.. cfunction:: ScriptScope GetSysModule(this ScriptRuntime runtime)
.. cfunction:: ScriptScope GetSysModule(this ScriptEngine engine)

   Gets a ScriptScope which is the Python sys module for the provided ScriptRuntime or ScriptEngine.

.. cfunction:: ScriptScope GetBuiltinModule(this ScriptRuntime runtime)
.. cfunction:: ScriptScope GetBuiltinModule(this ScriptEngine engine)

   Gets a ScriptScope which is the Python __builtin__ module for the provided ScriptRuntime or ScriptEngine.


.. cfunction:: ScriptScope GetClrModule(this ScriptRuntime runtime)
.. cfunction:: ScriptScope GetClrModule(this ScriptEngine engine)

   Gets a ScriptScope which is the Python clr module for the provided ScriptRuntime or ScriptEngine.


.. cfunction:: ScriptScope ImportModule(this ScriptRuntime runtime, string moduleName)
.. cfunction:: ScriptScope ImportModule(this ScriptEngine engine, string moduleName)

   Imports the Python module by the given name and returns its ScriptSCope.  If the 
   module does not exist an exception is raised.

.. cfunction:: void SetHostVariables(this ScriptRuntime runtime, string prefix, string executable, string version)
.. cfunction:: void SetHostVariables(this ScriptEngine engine, string prefix, string executable, string version)

   Sets sys.exec_prefix, sys.executable and sys.version and adds the prefix to sys.path

.. cfunction:: void SetTrace(this ScriptEngine engine, TracebackDelegate traceFunc)
.. cfunction:: void SetTrace(this ScriptRuntime runtime, TracebackDelegate traceFunc)

   Enables call tracing for the current thread in this ScriptEngine.  
    
   TracebackDelegate will be called back for each function entry, exit, exception, and line change.


.. cfunction:: void CallTracing(this ScriptRuntime runtime, object traceFunc, params object[] args)
.. cfunction:: void CallTracing(this ScriptEngine engine, object traceFunc, params object[] args)

   Provides nested level debugging support when SetTrace or SetProfile are used.
    
   This saves the current tracing information and then calls the provided object.

.. cfunction:: ScriptRuntimeSetup CreateRuntimeSetup(IDictionary<string, object> options)

   Creates a ScriptRuntimeSetup object which includes the Python script engine with the specified options.
    
   The ScriptRuntimeSetup object can then be additional configured and used to create a ScriptRuntime.

.. cfunction:: LanguageSetup CreateLanguageSetup(IDictionary<string, object> options)
    
   Creates a LanguageSetup object which includes the Python script engine with the specified options.
    
   The LanguageSetup object can be used with other LanguageSetup objects from other languages to
   configure a ScriptRuntimeSetup object.
