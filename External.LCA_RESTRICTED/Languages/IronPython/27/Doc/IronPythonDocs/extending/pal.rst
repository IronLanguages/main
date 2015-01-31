.. highlightlang:: c


.. hosting-pal:

*********************
PlatformAdaptionLayer
*********************

This class abstracts system operations used by the DLR that could possibly be platform specific.  Hosts can derive from this class and implement operations, such as opening a file.  For example, the Silverlight PAL could go to the server to fetch a file.

To use a custom PAL, you derive from this type and implement the members important to you.  You also need to derive a custom ScriptHost that returns the custom PAL instance.  Then when you create your ScriptRuntime, you explicitly create a ScriptRuntimeSetup and set the HostType property to your custome ScriptHost.

PlatformAdaptionLayer Summary::

    public class PlatformAdaptationLayer {
        public static readonly PlatformAdaptationLayer Default;

        public virtual Assembly LoadAssembly(string name);
        public virtual Assembly LoadAssemblyFromPath(string path);

        public virtual void TerminateScriptExecution(int exitCode);

        public StringComparer PathComparer { get; }
        public virtual bool FileExists(string path);
        public virtual bool DirectoryExists(string path);
        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share);
        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize);
        public virtual Stream OpenInputFileStream(string path);
        public virtual Stream OpenOutputFileStream(string path);
        public virtual string[] GetFiles(string path, string searchPattern);
        public virtual string GetFullPath(string path);
    }


PlatformAdaptionLayer Members
=============================

.. ctype:: PlatformAdaptionLayer

    Usually you do not create instances of the base PlatformAdaptionLayer class.  Instead you subclass it and provide your derived type via the ScriptHost for a ScriptRuntime.
    
