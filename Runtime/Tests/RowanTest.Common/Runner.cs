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
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace RowanTest.Common {
    
    /// <summary>
    /// Utility class designed to execute all "Test*" methods of all classes derived
    /// from RowanTest.Common.BaseTest in a given assembly.  Runner consists exclusively
    /// of static members/methods of which only one is publically accessible - RunAssembly.
    /// </summary>
    public static class Runner {

        //---------------------------------------------------------------------
        //--Members

        /// <summary>
        /// This prefix designates the names of test methods that will be invoked
        /// by this API.
        /// </summary>
        private const string _MagicPrefix = "Test";
        
        /// <summary>
        /// True if we're running under 64-bit CLR
        /// </summary>
        private static bool _Is64 = false;

        /// <summary>
        /// True if we're running under Silverlight
        /// </summary>
        private static bool _IsSilverlight = false;

        /// <summary>
        /// True if we're running under Visual Studio Orcas
        /// </summary>
        private static bool _IsOrcas;


        /// <summary>
        /// List of all failures that have occurred.
        /// </summary>
        private static List<string> _Failures;

        /// <summary>
        /// List of all disabled tests and the reason why.
        /// </summary>
        private static List<string> _DisabledTests;

#if !SILVERLIGHT
        /// <summary>
        /// Used to keep track of method execution time.
        /// </summary>
        private static Stopwatch _Stopwatch = null;
#endif

        //---------------------------------------------------------------------
        //--Methods

        /// <summary>
        /// Initialize static members that never need to be reset.
        /// </summary>
        static Runner() {
            if (System.IntPtr.Size == 8) _Is64 = true;

#if SILVERLIGHT
            _IsSilverlight = true;
#endif

            Type t = typeof(object).Assembly.GetType("System.DateTimeOffset", false);
            _IsOrcas = t != null;
        }
        
        /// <summary>
        /// Reset the Runner object.
        /// </summary>
        private static void Reset() {
            
            _Failures = new List<string>();
            _DisabledTests = new List<string>();
#if !SILVERLIGHT
            _Stopwatch = new Stopwatch();
#endif
        }


        /// <summary>
        /// The only method defined in this class intended to be called from other
        /// classes.  In a nutshell, this method:
        /// 1.  Searches the given assembly, a, for all classes derived from BaseTest
        /// 2.  Creates an instance of each BaseTest subclass using the default constructor
        /// 3.  Invokes all "Test*()" methods the subclass contains
        /// 4.  Exits the runtime with a non-zero exit code if any failures were
        ///     detected.
        /// </summary>
        /// <param name="a">Search for BaseTest subclasses in this assembly</param>
        public static void RunAssembly(Assembly a) {

            Reset();

            var allTestMethods = new List<MethodInfo>(
                from type in a.GetTypes()
                where type.IsSubclassOf(typeof(BaseTest))
                from method in type.GetMethods()
                where method.Name.StartsWith(_MagicPrefix) && !Runner.IsDisabled(method)
                select method
            );

            var runOne = allTestMethods.Find((m) => m.IsDefined(typeof(RunAttribute), false));
            if (runOne != null) {
                InvokeTestMethod(Activator.CreateInstance(runOne.DeclaringType), runOne);
                return;
            }
            
            var groupedByType =
                from method in allTestMethods
                group method by method.DeclaringType;

            //look at each type defined in the assembly
            foreach (var grouping in groupedByType) {
                //otherwise we create an instance of it to test
                System.Console.WriteLine("------------------------------------------------------------");
                System.Console.WriteLine("Running tests for " + grouping.Key.Name + ":");
                object o = Activator.CreateInstance(grouping.Key);
                
                //look for methods that match the "Test*" pattern
                foreach (MethodInfo methInfo in grouping) {
                    InvokeTestMethod(o, methInfo);
                }
                System.Console.WriteLine("");
            }
            //print out an executive level summary suitable for devs.
            //also, exit the runtime if failures were detected.
            PrintSummary();
        }

        /// <summary>
        /// Helper method used to invoke an individual test method.
        /// </summary>
        /// <param name="o">An object derived from BaseTest</param>
        /// <param name="methInfo">The "Test*" method</param>
        private static void InvokeTestMethod(object o, MethodInfo methInfo) {
            System.Console.WriteLine("\t" + methInfo.Name);

            //invoke the method, logging any errors that occur
            try {
                //if the execution time attribute is defined we must ensure the method
                //executes in a reasonable amount of time
                if (methInfo.IsDefined(typeof(MaxExecutionTimeAttribute), false)) {
                    InvokeTimedMethod(o, methInfo);
                } else {
                    methInfo.Invoke(o, new object[] { });
                }
            } catch (Exception ex) {
                System.Console.WriteLine(ex.ToString());
                System.Console.WriteLine("");
                _Failures.Add(methInfo.DeclaringType.Name + " (" + methInfo.Name + ")");
            }
        }

        /// <summary>
        /// Helper method used to ensure a method completes in a certain amount of time.
        /// </summary>
        /// <param name="o">An object derived from BaseTest</param>
        /// <param name="methInfo">The "Test*" method</param>
        private static void InvokeTimedMethod(object o, MethodInfo methInfo) {
            //determine the maximum amount of time this method has to complete - milliseconds
            long maxTime = 0;
            object[] caList = methInfo.GetCustomAttributes(false);
            for (int i = 0; i < caList.Length; i++) {
                if (caList[i].GetType() == typeof(MaxExecutionTimeAttribute)) {
                    maxTime = ((MaxExecutionTimeAttribute)caList[i]).Max;
                    break;
                }
            }

            //Invoke the method
#if !SILVERLIGHT
            _Stopwatch.Start();
#endif
            //let caller worry about any exceptions methInfo might throw...
            methInfo.Invoke(o, new object[] { });
#if !SILVERLIGHT
            _Stopwatch.Stop();


            //Verify it didn't take too long
            if (_Stopwatch.ElapsedMilliseconds > maxTime) {
                System.Console.WriteLine(methInfo.DeclaringType.Name + " (" + methInfo.Name + ") took " + _Stopwatch.ElapsedTicks + "ms to execute.");
                System.Console.WriteLine("Should have finished within " + maxTime + " milliseconds!");
                _Failures.Add(methInfo.DeclaringType.Name + " (" + methInfo.Name + "): timeout.");
            }
            _Stopwatch.Reset();
#endif
        }

        /// <summary>
        /// Checks to see if a test method has been disabled.
        /// </summary>
        /// <param name="methInfo">The "Test*()" method we're interested in</param>
        /// <returns>True if the test is disabled under the current PC</returns>
        private static bool IsDisabled(MethodInfo methInfo) {

            //look over all of the method's attributes until we find a DisabledAttribute
            object[] caList = methInfo.GetCustomAttributes(false);

            for (int i = 0; i < caList.Length; i++) {
                if (caList[i].GetType() != typeof(DisabledAttribute)) {
                    continue;
                }
                DisabledAttribute da = (DisabledAttribute)caList[i];
                
                //check if it's platform/CLR independent
                if ((da.Reason & DisabledReason.GeneralFailure) != 0 ||
                    (da.Reason & DisabledReason.SlowPerf) != 0 ||
                    (da.Reason & DisabledReason.TODO) != 0) {
                    _DisabledTests.Add(methInfo.DeclaringType.Name + " (" + methInfo.Name + ") " + da.Reason + ": " + da.Description);
                    return true;
                }

                //64-bit
                if ((da.Reason & DisabledReason.SixtyFourBitCLR) != 0 && _Is64) {
                    _DisabledTests.Add(methInfo.DeclaringType.Name + " (" + methInfo.Name + ") " + da.Reason + ": " + da.Description);
                    return true;
                }

                //Silverlight
                if ((da.Reason & DisabledReason.Silverlight) != 0 && _IsSilverlight) {
                    _DisabledTests.Add(methInfo.DeclaringType.Name + " (" + methInfo.Name + ") " + da.Reason + ": " + da.Description);
                    return true;
                }

                //Orcas
                if ((da.Reason & DisabledReason.Orcas) != 0 && _IsOrcas) {
                    _DisabledTests.Add(methInfo.DeclaringType.Name + " (" + methInfo.Name + ") " + da.Reason + ": " + da.Description);
                    return true;
                }
                
            }
            return false;
        }

        /// <summary>
        /// Prints a summary of the test run and exits if any failures occurred.
        /// </summary>
        private static void PrintSummary() {
            System.Console.WriteLine("------------------------------------------------------------");
            if (_DisabledTests.Count != 0) {
                System.Console.WriteLine("The following tests were disabled:");
                foreach (string disabled in _DisabledTests) {
                    System.Console.WriteLine("\t" + disabled);
                }
            }
            
            if (_Failures.Count != 0) {
                System.Console.WriteLine("The following tests failed:");
                foreach (string fail in _Failures) {
                    System.Console.WriteLine("\t" + fail);
                }
#if !SILVERLIGHT
                System.Environment.Exit(1);
#else
                throw new System.Exception("Runner Failure");
#endif
            } else {
                System.Console.WriteLine("Passed!");
            }
        }
    }
}
