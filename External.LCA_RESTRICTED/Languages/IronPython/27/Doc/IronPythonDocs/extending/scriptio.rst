.. highlightlang:: c


.. hosting-scriptio:

********
ScriptIO
********

This class let's you control input and output by default for dynamic code running via DLR hosting.  You can access the instance of this class from the IO property on ScriptRuntime.

ScriptIO Summary::

    public sealed class ScriptIO : MarshalByRefObject
        /// Used for binary IO.
        public Stream InputStream { get; }
        public Stream OutputStream { get; }
        public Stream ErrorStream { get; }

        /// Used for pure unicode IO.
        public TextReader InputReader { get; }
        public TextWriter OutputWriter { get; }
        public TextWriter ErrorWriter { get; }

        /// What encoding are the unicode reader/writers using.
        public Encoding InputEncoding { get; }
        public Encoding OutputEncoding { get; }
        public Encoding ErrorEncoding { get; }
    
        public void SetOutput(Stream stream, Encoding encoding);
        public void SetOutput(Stream stream, TextWriter writer);
    
        public void SetErrorOutput(Stream stream, Encoding encoding);
        public void SetErrorOutput(Stream stream, TextWriter writer);
    
        public void SetInput(Stream stream, Encoding encoding);
        public void SetInput(Stream stream, TextReader reader,  Encoding encoding);
    
        public void RedirectToConsole();
    }

ScriptIO Members
================

.. ctype:: ScriptIO
    
    ScriptIO instances cannot be created.  To get a ScriptIO access the IO property on a ScriptRuntime object.

.. cfunction:: Stream OutputStream { get; }

    This property returns the standard output stream for the ScriptRuntime.  This is a binary stream.  All code and engines should output binary data here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct output to a file or stream, then that language's output would go there as directed by the code.

.. cfunction:: Stream InputStream { get; }

    This property returns the standard input stream for the ScriptRuntime.  This is a binary stream.  All code and engines should read binary data from here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct input from a file or stream, then that language's input would come from there as directed by the code.

.. cfunction:: Stream ErrorStream { get; }

    This property returns the standard erroroutput stream for the ScriptRuntime.  This is a binary stream.  All code and engines should send error binary output here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct error output to a file or stream, then that language's error output would go there as directed by the code.

.. cfunction:: TextReader InputReader { get; }

    This property returns the standard input reader for the ScriptRuntime.  This is a unicode reader.  All code and engines should read text from here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct input from a file or stream, then that language's input would come from there as directed by the code.

.. cfunction:: TextWriter OutputWriter { get; }

    This property returns the standard output writer for the ScriptRuntime.  All code and engines should send text output here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct output to a file or stream, then that language's output would go there as directed by the code.

.. cfunction:: TextWriter ErrorWriter { get; }

    This property returns the standard error output writer for the ScriptRuntime.  All code and engines should send text error output here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct error output to a file or stream, then that language's output would go there as directed by the code.

.. cfunction:: Encoding InputEncoding { get; }

    This property returns the encoding used by the TextReader returned from InputReader.

.. cfunction:: Encoding OutputEncoding { get; }

    This property returns the encoding used by the TextWriters returned from the OutputWriter property.

.. cfunction:: Encoding ErrorEncoding { get; }

    This property returns the encoding used by the TextWriters returned from the  ErrorWriter property.

.. cfunction:: void SetOutput(Stream stream, Encoding encoding);
.. cfunction:: void SetOutput(Stream stream, TextWriter writer);

    This method sets the standard output stream for the ScriptRuntime.  All code and engines should send output to the specified stream for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct output to a file or stream, then that language's output would go there as directed by the code.

    The first method is useful if the host just captures binary stream output.  The second method is useful if the host captures unicode text and binary output.  Note, if you pass just a stream and an encoding, the this method creates a default StreamWriter, which writes a BOM on first usage.  To avoid this, you'll need to pass your own TextWriter.

    If any argument to these methods is null, they throw an ArgumentException.

.. cfunction:: void SetErrorOutput(Stream stream, Encoding encoding);
.. cfunction:: void SetErrorOutput(Stream stream, TextWriter writer);

    This method sets the standard error output stream for the ScriptRuntime.  All code and engines should send error output to the specified stream for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct error output to a file or stream, then that language's output would go there as directed by the code.
    
    The first method is useful if the host just captures binary stream output.  The second method is useful if the host captures unicode text and binary output.
    
    If any argument to these methods is null, they throw an ArgumentException.

.. cfunction:: void SetInput(Stream stream, Encoding encoding);
.. cfunction:: void SetInput(Stream stream, TextReader reader, Encoding encoding);

    This method sets the standard input stream for the ScriptRuntime.  All code and engines should read input here for this ScriptRuntime.  Of course, if a language has a mechanism to programmatically direct input from a file or stream, then that language's input would come from there as directed by the code.

.. cfunction:: void RedirectToConsole();
    
    This method makes all the standard IO for the ScriptRuntime go to System.Console.  Of course, if a language has a mechanism to programmatically direct output to a file or stream, then that language's output would go there as directed by the code.

