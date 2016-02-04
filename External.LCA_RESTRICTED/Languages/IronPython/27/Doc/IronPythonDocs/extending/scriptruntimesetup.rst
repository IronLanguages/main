.. highlightlang:: c


.. hosting-scriptruntimesetup:

******************
ScriptRuntimeSetup
******************

This class gives hosts full control over how a ScriptRuntime gets configured.  You can instantiate this class, fill in the setup information, and then instantiate a ScriptRuntime with the setup instance.  Once you pass the setup object to create a ScriptRuntime, attempts to modify its contents throws an exception.

There is also a static method as a helper to hosts for reading .NET application configuration.  Hosts that want to be able to use multiple DLR-hostable languages, allow users to change what languages are available, and not have to rebuild can use the DLR's default application configuration model.  See ReadConfiguration for the XML details.

You can also get these objects from ScriptRuntime.Setup.  These instances provide access to the configuration information used to create the ScriptRuntime.  These instances will be read-only and throws exceptions if you attempt to modify them.  Hosts may not have created a ScriptRuntimeSetup object and may not have configuration information without the Setup property.

ScriptRuntimeSetup Summary::

    public sealed class ScriptRuntimeSetup {
        public ScriptRuntimeSetup();
        public IList<LanguageSetup> LanguageSetups { get;  }
        public bool DebugMode { get; set; }
        public bool PrivateBinding { get; set; }
        public Type HostType { get; set; }
        public Dictionary<string, object> Options { get; }
        public object[] HostArguments {get; set;  }
        public static ScriptRuntimeSetup ReadConfiguration();
        public static ScriptRuntimeSetup ReadConfiguration(Stream configFileStream);
    }


ScriptRuntimeSetup Members
==========================

.. ctype:: ScriptRuntimeSetup();

    The constructor returns an empty ScriptRuntimeSetup object, with no languages preconfigured.

.. cfunction:: static ScriptRuntimeSetup ReadConfiguration();
.. cfunction:: static ScriptRuntimeSetup ReadConfiguration(Stream configFileStream);

    These methods read application configuration and return a ScriptRuntimeSetup initialized from the application configuration data.  Hosts can modify the result before using the ScriptRuntimeSetup object to instantiate a ScriptRuntime.

.. cfunction:: IList<LanguageSetup> LanguageSetups { get;  }

    This property returns a list of LanguageSetup objects, each describing one language the ScriptRuntime will allow.  When you instantiate the ScriptRuntime, it will ensure there is only one element in the list with a given LanguageSetup.TypeName value.

.. cfunction:: Type HostType { get; set; }

    This property gets and sets the ScriptHost type that the DLR should instantiate when it creates the ScriptRuntime.  The DLR instantiates the host type in the app domain where it creates the ScriptRuntime object.  See ScriptHost for more information.

.. cfunction:: object[] HostArguments {get; set;  }

    This property gets and sets an array of argument values that should be passed to the HostType's constructor.  The objects must be MBRO or serializable when creating a remote ScriptRuntime.
    
    Here's an example::
        
        class MyHost : ScriptHost {
           public MyHost(string foo, int bar);
        }
        setup = new ScriptRuntimeSetup()
        setup.HostType = typeof(MyHost)
        setup.HostArguments = new object[] { "some foo", 123 }
        ScriptRuntime.CreateRemote(otherAppDomain, setup)


.. cfunction:: Dictionary<string, object> Options { get; }

    This property returns a dictionary of global options for the ScriptRuntime.  There are two options explicit on the ScriptRuntimeSetup type, DebugMode and PrivateBinding.  The Options property is an flexibility point for adding options later.  Names are case-sensitive.
    
    There is one specially named global option, "SearchPaths".  If this value is present, languages should add these paths to their default search paths.  If your intent is to replace an engine's default paths, then you can use Engine.SetSearchPaths (perhaps on the ScriptHost.EngineCreated callback).
    
.. cfunction:: bool DebugMode { get; set; }

    This property controls whether the ScriptRuntime instance and engines compiles code for debuggability.

.. cfunction::  bool PrivateBinding { get; set; }

    This property controls whether the ScriptRuntime instance and engines will use reflection to access private members of types when binding object members in dynamic operations.  Setting this to true only works in app domains running in full trust.

Configuration Structure
=======================

These lines must be included in the .config file as the first element under the <configuration> element for the DLR's default reader to work::

    <configSections>
        <section name="microsoft.scripting" 
           type="Microsoft.Scripting.Hosting.Configuration.Section, Microsoft.Scripting, Version=1.0.0.5000, Culture=neutral, PublicKeyToken=31bf3856ad364e35" />
    </configSections>
    The structure of the configuration section is the following (with some notes below):
    <microsoft.scripting [debugMode="{bool}"]? 
                         [privateBinding="{bool}"]?>
        <languages>
          <!-- BasicMap with type attribute as key.  Inherits language 
               nodes, overwrites previous nodes based on key -->
          <language names="{semicolon-separated}" 
                    extensions="{semicolon-separated, optional-dot}" 
                    type="{assembly-qualified type name}" 
                    [displayName="{string}"]? />
        </languages>
    
        <options>
          <!-- AddRemoveClearMap with option as key.  If language
               Attribute is present, the key is option cross language.
           -->
          <set option="{string}" value="{string}" 
               [language="{langauge-name}"]? />
          <clear />
          <remove option="{string}" [language="{langauge-name}"]? />
        </options>
    
      </microsoft.scripting>

Attributes enclosed in  [...]? are optional.
{bool} is whatever Convert.ToBoolean(string) works for ("true", "False", "TRUE", "1", "0"). 

<languages> tag inherits content from parent .config files.  You cannot remove a language in a child .config file once it is defined in a parent .config file.  You can redefine a language if the value of the "type" attribute is the same as a defined in a parent .config file (last writer wins).  If the displayName attribute is missing, ReadConfiguration sets it to the first name in the names attribute.  If names is the empty string, then ReadConfiguration sets the display name to the type attribute.  The names and extensions attributes support semi-colon and comma as separators.

<options> tag inherits options from parent .config files.  You can set, remove, and clear options (removes them all).  The key in the options dictionary is a pair of option and language attributes.  Language attribute is optional.  If specified, the option applies to the language whose simple name is stated; otherwise, it applies to all languages.  <remove option="foo"/> removes the option from common options dictionary, not from all language dictionaries.  <remove option="foo" language="rb"/> removes the option from Ruby language options.

Default DLR Configuration
=========================

The default application configuration section for using the DLR languages we ship for the desktop is (of course, you need correct type names from your current version)::

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <configSections>
        <section name="microsoft.scripting" 
           type="Microsoft.Scripting.Hosting.Configuration.Section, Microsoft.Scripting, Version=1.0.0.5000, Culture=neutral, PublicKeyToken=31bf3856ad364e35" />
      </configSections>
    
      <microsoft.scripting>
        <languages>
          <language names="IronPython;Python;py" extensions=".py" 
                    displayName="IronPython v2.0" 
              type="IronPython.Runtime.PythonContext, IronPython, Version=2.0.0.5000, Culture=neutral, PublicKeyToken=31bf3856ad364e35" />
          <language names="IronRuby;Ruby;rb" extensions=".rb" 
                    displayName="IronRuby v1.0" 
                    type="IronRuby.Runtime.RubyContext, IronRuby, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" 
           />
    
          <!-- If for experimentation you want ToyScript ... -->
          <language names="ToyScript;ts" extensions=".ts" 
                    type="ToyScript.ToyLanguageContext, ToyScript, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" 
           />
        </languages>
      </microsoft.scripting>
    </configuration>
    
