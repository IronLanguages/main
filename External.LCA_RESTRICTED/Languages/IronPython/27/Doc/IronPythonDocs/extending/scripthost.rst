.. highlightlang:: c


.. hosting-scripthost:

**********
ScriptHost
**********

ScriptHost represents the host to the ScriptRuntime.  Hosts can derive from this type and overload behaviors by returning a custom PlatformAdaptationLayer.  Hosts can also handle callbacks for some events such as when engines get created.

The ScriptHost object lives in the same app domain as the ScriptRuntime in remote scenarios.

Derived types from ScriptHost can have arguments passed to them via ScriptRuntimeSetup's HostArguments property.  For example::

    class MyHost : ScriptHost {
       public MyHost(string foo, int bar) {}
    }
    setup = new ScriptRuntimeSetup()
    setup.HostType = typeof(MyHost)
    setup.HostArguments = new object[] { “some foo”, 123 }
    ScriptRuntime.CreateRemote(otherAppDomain, setup)

The DLR instantiates the ScriptHost when the DLR initializes a ScriptRuntime.  The host can get at the instance with ScriptRuntime.Host.

ScriptHost Overview::

    public class ScriptHost : MarshalByRefObject {
        public ScriptHost() { }
        public ScriptRuntime Runtime { get; }
        public virtual PlatformAdaptationLayer {get; }
        protected virtual void RuntimeAttached()
        protected virtual void EngineCreated(ScriptEngine engine)
    }


ScriptHost Members
==================

.. ctype:: ScriptHost()

    Creates a new ScriptHost.

.. cfunction:: ScriptRuntime Runtime { get; }
    
    This property returns the ScriptRuntime to which this ScriptHost is attached.

