.. highlightlang:: c


.. hosting-languagesetup:

*************
LanguageSetup
*************

This class represents a language configuration for use in a ScriptRuntimeSetup when instantiating a ScriptRuntime.  Once you pass the setup object to create a ScriptRuntime, attempts to modify its contents throws an exception.

You can also get these objects from ScriptRuntime.Setup and ScriptEngine.Setup.  These instances provide access to the configuration information used to create the ScriptRuntime.  These instances will be read-only and throws exceptions if you attempt to modify them.  Hosts may not have created a ScriptRuntimeSetup object and may not have language setup information without the Setup properties.  

LanguageSetup Summary::

    public sealed class LanguageSetup {
        public LanguageSetup(string typeName, string displayName)
        public LanguageSetup(string typeName, string displayName, IEnumerable<string> names, IEnumerable<string> fileExtensions);
        public string TypeName { get; set; }
        public string DisplayName { get; set; }
        public IList<string> Names { get; }
        public IList<string> FileExtensions { get; }
        public Dictionary<string, object> Options { get; }

        public bool InterpretedMode { get; set; }
        public bool ExceptionDetail { get; set; }
        public bool PerfStats { get; set; }
        public T GetOption<T>(string name, T defaultValue)
    }

LanguageSetup Members
=====================

.. ctype:: LanguageSetup(string typeName, string displayName)
.. ctype:: LanguageSetup(string typeName, string displayName, IEnumerable<string> names, IEnumerable<string> fileExtensions);

    The minimal construction requires an assembly-qualified type name for the language and a display name.  You can set other properties after instantiating the setup object.

    These ensure typeName and displayName are not null or empty.  The collections can be empty but not null so that you can fill them in after instantiating this type.

.. cfunction:: string TypeName { get; set; }

    This property gets or sets the assembly-qualified type name of the language.  This is the type the DLR loads when, for example, it needs to execute files with the specified file extensions.

.. cfunction:: string DisplayName { get; set; }

    This property gets or sets a suitably descriptive name for displaying in UI or for debugging.  It often includes the version number in case different versions of the same language are configured.

.. cfunction:: IList<string> Names { get; }

    This property returns a list of names for the language.  These can be nicknames or simple names used programmatically (for example, language=python on a web page or in a user's options UI).

.. cfunction:: IList<string> FileExtensions { get; }

    This property gets the list of file extensions that map to this language in the ScriptRuntime.

.. cfunction:: bool InterpretedMode { get; set; }

    This property gets or sets whether the language engine interprets sources or compiles and executes them.  Not all languages respond to this option.
    
    This method pulls the value from Options in case it is set there via application .config instead of via the property setter.  It defaults to false.  If the host or reading .config set this option, then it will be in Options with the key "InterpretedMode".

.. cfunction:: bool ExceptionDetail { get; set; }

    This property gets or sets whether the language engine should print exception details (for example, a call stack) when it catches exceptions.  Not all languages respond to this option.
    
    This method pulls the value from Options in case it is set there via application .config instead of via the property setter.  It defaults to false.    If the host or reading .config set this option, then it will be in Options with the key "ExceptionDetail".
    
.. cfunction:: bool PerfStats { get; set; }

    This property gets or sets whether the language engine gathers performance statistics.  Not all languages respond to this option.  Typically the languages dump the information when the application shuts down.
    
    This method pulls the value from Options in case it is set there via application .config instead of via the property setter.  It defaults to false.  If the host or reading .config set this option, then it will be in Options with the key "ExceptionDetail".
    
.. cfunction:: Dictionary<string, object> Options { get; }

    This property returns the list dictionary of options for the language.  Option names are case-sensitive.  The list of valid options for a given language must be found in its documentation.

.. cfunction:: T GetOption<T>(string name, T defaultValue)

    This method looks up name in the Options dictionary and returns the value associated with name, converting it to type T.  If the name is not present, this method return defaultValue.
