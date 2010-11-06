/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;


using RowanTest.Common;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.CodeDom.Compiler;


namespace TestInternalDLR {
    class TestSilverlightProxies : BaseTest {

        public void TestDefaultParameterValueAttribute() {
            DefaultParameterValueAttribute dpva = new DefaultParameterValueAttribute(this);
            dpva = new DefaultParameterValueAttribute(null);
            Assert(dpva.GetType().IsSubclassOf(typeof(System.Attribute)));
        }


        public void TestReflection() {
            AreEqual(PortableExecutableKinds.ILOnly.ToString(), "ILOnly");
            AreEqual(ImageFileMachine.I386.ToString(), "I386");
        }


        public void TestComponentModel() {
            WarningException we = new WarningException("a message");
            we = new WarningException(null);
            
            AreEqual(EditorBrowsableState.Advanced.ToString(), "Advanced");

            EditorBrowsableAttribute eba = new EditorBrowsableAttribute(EditorBrowsableState.Advanced);
            Assert(eba.GetType().IsSubclassOf(typeof(System.Attribute)));
#if !SILVERLIGHT
            //SilverlightProxies.cs does not define the State property
            AreEqual(eba.State, EditorBrowsableState.Advanced);
#endif
        }

        public void TestGeneratedCodeAttributeTest() {
            GeneratedCodeAttribute gca = new GeneratedCodeAttribute("tool", "version");
            gca = new GeneratedCodeAttribute("tool", null);
            gca = new GeneratedCodeAttribute(null, "version");
            gca = new GeneratedCodeAttribute(null, null);
            Assert(gca.GetType().IsSubclassOf(typeof(System.Attribute)));
        }

        public void TestSystem() {
            SerializableAttribute sa = new SerializableAttribute();
            Assert(sa.GetType().IsSubclassOf(typeof(System.Attribute)));
            NonSerializedAttribute nsa = new NonSerializedAttribute();
            Assert(nsa.GetType().IsSubclassOf(typeof(System.Attribute)));
            
            //just make sure the definition is in the right place
            System.Runtime.Serialization.ISerializable iSerial = null;
            AreEqual(iSerial, null);

            AreEqual(StringSplitOptions.None.ToString("D"), "0");
            AreEqual(StringSplitOptions.RemoveEmptyEntries.ToString("D"), "1");

            AreEqual(ConsoleColor.Black.ToString("D"), "0");
            AreEqual(ConsoleColor.DarkBlue.ToString("D"), "1");
            AreEqual(ConsoleColor.DarkGreen.ToString("D"), "2");
            AreEqual(ConsoleColor.DarkCyan.ToString("D"), "3");
            AreEqual(ConsoleColor.DarkRed.ToString("D"), "4");
            AreEqual(ConsoleColor.DarkMagenta.ToString("D"), "5");
            AreEqual(ConsoleColor.DarkYellow.ToString("D"), "6");
            AreEqual(ConsoleColor.Gray.ToString("D"), "7");
            AreEqual(ConsoleColor.DarkGray.ToString("D"), "8");
            AreEqual(ConsoleColor.Blue.ToString("D"), "9");
            AreEqual(ConsoleColor.Green.ToString("D"), "10");
            AreEqual(ConsoleColor.Cyan.ToString("D"), "11");
            AreEqual(ConsoleColor.Red.ToString("D"), "12");
            AreEqual(ConsoleColor.Magenta.ToString("D"), "13");
            AreEqual(ConsoleColor.Yellow.ToString("D"), "14");
            AreEqual(ConsoleColor.White.ToString("D"), "15");
        }
    }
}
