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

namespace TestDirect3D
{
    class Program
    {
        static void Main(string[] args)
        {
            
            WinClass winClass = new WinClass(args[0]);
            try
            {
                winClass.Test_Checkpoint1();
                winClass.Test_Checkpoint2();
                winClass.Test_Checkpoint3();

                winClass.Test_Checkpoint4();
                winClass.Test_Checkpoint5();
                winClass.Test_Checkpoint6();

                winClass.Test_demo1();
                winClass.Test_demo2();
                winClass.Test_demo3();
                winClass.Test_demo4();
                winClass.Test_GravityDemo();

                winClass.Test_MeshDemo();
                winClass.Test_Tutorial();
                       
               
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
