/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using MS.Internal.Mita.Foundation.Controls;
using MS.Internal.Mita.Foundation;
using MS.Internal.Mita.Foundation.Waiters;
using System.Diagnostics;

namespace TestPuzzle
{
    class Program
    {
        static void Main(string[] args)
        {
            UICondition uIcondition = UICondition.Create("@Name='Puzzle'", new Object[0]);
            WindowOpenedWaiter wait = new WindowOpenedWaiter(uIcondition);
            var ipy_proc = Process.Start(System.Environment.GetEnvironmentVariable("DLR_BIN") + "\\ipy.exe", " .\\puzzle.py"); 
            wait.Wait(30000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition); 
            WinClass winClass = new WinClass(ui);

            try {
                winClass.Test_about();
                winClass.Test_options();
                winClass.Test_load();
                
                //http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=25404
                // winClass.Test_play();
                //exit verification
                winClass.Test_create();
                //http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=25404
                //winClass.Test_play();
                winClass.Test_exit();


            } catch (Exception e) {
                Console.WriteLine(e);
            } finally {
                //http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=19693
                if (!ipy_proc.HasExited) {
                    ipy_proc.Kill();
                }
            }
        }
    }
}
