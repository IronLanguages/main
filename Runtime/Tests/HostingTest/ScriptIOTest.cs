using System;
using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;


namespace HostingTest    {

    /// <summary>
    /// This is a test class for ScriptIOTest and is intended
    /// to contain all ScriptIOTest Unit Tests
    ///
    ///</summary>
    [TestFixture]
    public partial class ScriptIOTest : HAPITestBase
    {

        /// <summary>
        /// Constructor sets up specific code for the tests for
        /// this class.
        /// </summary>
        public ScriptIOTest(){
           
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetOutput_FirstParameterIsNull() {
            _runTime.IO.SetOutput((Stream)null, Encoding.ASCII);
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetOutput_SecondParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
            _runTime.IO.SetOutput((Stream)stream, (Encoding)null);
        }


        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetOutput_TextWriterFirstParameterIsNull() {

            string file = TestHelpers.CreateTempFile("Test!");
            StreamWriter streamOut = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.ReadWrite),
                                                        Encoding.ASCII, 128);
            _runTime.IO.SetOutput((Stream)null, streamOut);
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetOutput_TextWriterSecondParameterIsNull() {
            
            string file = TestHelpers.CreateTempFile("Test!");
            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
            _runTime.IO.SetOutput((Stream)stream, (StreamWriter)null);
        }

        /// <summary>
        /// Test    : Set stream to a valid object; set writer to a valid object
        /// Result  : The output stream receives the proper values in proper format
        /// </summary>
        [Ignore]// bug #466678 - this issue doesn't happen in ruby
        [Test]
        public void SetOutput_TextWriterValidateOutputCloseStreamAndVerifyEngineStability() {

            string testFile = Path.GetTempFileName();
            string expectedOutput = "test-" + DateTime.Now.Millisecond;
            FileStream stream = new FileStream(testFile, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter streamWriter = new StreamWriter(stream);

            _runTime.IO.SetOutput((Stream)stream, streamWriter);

            // Get a scope and execute some simple code
            _testEng.Execute("print \'" + expectedOutput + "\'");

            // Release the stream
            stream.Close();

            // This seems to pass
            ValidateAttachedStreamOutput(testFile, expectedOutput);

            string newOutput = "test-" + DateTime.Now.Millisecond;

            stream = new FileStream(testFile, FileMode.Create, FileAccess.ReadWrite);
            _runTime.IO.SetOutput((Stream)stream, streamWriter);

            _testEng.Execute("print \'" + newOutput + "\'");


            // Validate that output stream has not been updated - no side effects
            ValidateAttachedStreamOutput(testFile, expectedOutput);

        }

        /// <summary>
        /// Test    : ‘stream’ is set explicitly; output is captured in ‘stream’ then ‘stream’ is closed when still in use by the engine
        /// Result  : No output appears. No side effect – app runs normally without exceptions
        /// </summary>
        [Test]
        public void SetOutput_TextWriterValidateOutput() {
            string expectedOutput = "hello";
            string file = TestHelpers.CreateTempFile("Test!");

            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter streamWriter = new StreamWriter(stream);

            _runTime.IO.SetOutput((Stream)stream, streamWriter);

            ScriptScope scope = _testEng.CreateScope();
            ScriptSource source = scope.Engine.CreateScriptSourceFromString("print \"" + expectedOutput + "\"", Microsoft.Scripting.SourceCodeKind.Statements);
            source.Execute(scope);
            stream.Close();

            ValidateAttachedStreamOutput(file, expectedOutput);
        }


        /// <summary>
        /// Tests    : Set stream to a valid object; set encoding to a known value
        /// Expected : The output stream receives the proper values in proper format 
        /// </summary>
        [Test]
        public void SetOutput_ValidateOutput() {
            string testFile = Path.GetTempFileName();
           
            string expectedOutput = "test-" + DateTime.Now.ToUniversalTime();
            FileStream stream = new FileStream(testFile, FileMode.Create, FileAccess.ReadWrite);

            _runTime.IO.SetOutput((Stream)stream, Encoding.ASCII);

            ScriptScope scope = _testEng.CreateScope();
            ScriptSource source = _testEng.CreateScriptSourceFromString("print \'" + expectedOutput + "\'", Microsoft.Scripting.SourceCodeKind.Statements);
            source.Execute(scope);

            _runTime.IO.OutputStream.Close();

            ValidateAttachedStreamOutput(testFile, expectedOutput);
        }

        /// <summary>
        /// Tests    : ‘stream’ is set explicitly; output is captured in ‘stream’ then ‘stream’ 
        ///            is closed when still in use by the engine	
        /// Expected : ObjectDisposed exception is thrown since the stream was closed
        /// </summary>
        [Test]
        [ExpectedException( typeof(ObjectDisposedException))]
        public void SetOutput_ValidateOutputCloseStreamAndVerifyEngineStability() {

            string testFile = Path.GetTempFileName();
            FileStream stream = new FileStream(testFile, FileMode.Create, FileAccess.ReadWrite);
            _runTime.IO.SetOutput((Stream)stream, Encoding.ASCII);

            ScriptScope scope = _testEng.CreateScope();
            ScriptSource source = _testEng.CreateScriptSourceFromString("print 'something'", Microsoft.Scripting.SourceCodeKind.Statements);

            source.Execute(scope);   
            stream.Close();

            ValidateAttachedStreamOutput(testFile, "something");
            //this should throw
            source.Execute(scope);
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetInput_FirstParameterIsNull() {
            _runTime.IO.SetInput((Stream)null, Encoding.ASCII);
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetInput_SecondParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
            _runTime.IO.SetInput((Stream)stream, (Encoding)null);
        }


        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetInput_TextReaderFirstParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            StreamReader stream = new StreamReader(file);
            _runTime.IO.SetInput((Stream)null, stream, Encoding.ASCII);
        }


        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetInput_TextReaderSecondParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
            _runTime.IO.SetInput((Stream)stream, (TextReader)null, Encoding.ASCII);


        }



        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetErrorOutput_FirstParameterIsNull() {
            _runTime.IO.SetErrorOutput((Stream)null, Encoding.ASCII);
        
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetErrorOutput_SecondParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);

            _runTime.IO.SetOutput((Stream)stream, (Encoding)null);
            _runTime.IO.SetErrorOutput((Stream)stream, (Encoding)null);
        }

        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetErrorOutput_TextWriterFirstParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            StreamWriter streamOut = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.ReadWrite),
                                                        Encoding.ASCII, 128);

            _runTime.IO.SetErrorOutput((Stream)null, streamOut);
        }


        [Negative()]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetErrorOutput_TextWriterSecondParameterIsNull() {
            string file = TestHelpers.CreateTempFile("Test!");
            FileStream stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);

            _runTime.IO.SetErrorOutput((Stream)stream, (StreamWriter)null);
        }

        /// <summary>
        /// Test     : Invoke the method
        /// Results  : Errors, input and output is redirected to console overriding any pervious setting
        /// </summary>
        [Ignore]//This is easy to do as a manual test. Consider that option too
        [Test]
        public void RedirectToConsole_Invoke() {
            _runTime.IO.RedirectToConsole();

            // TODO : Finish Validation part of this test
            Assert.Inconclusive("This is easy to do as a manual test. Consider that option too");
        }
    }
}
