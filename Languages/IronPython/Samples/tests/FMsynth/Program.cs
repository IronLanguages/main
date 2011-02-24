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
using System.Threading;


namespace TestFMsynth
{
    class Program
    {
        static void Main(string[] args)
        {
            UICondition uIcondition = UICondition.Create("@Name='Frequency Modulation Synthesizer'", new Object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            //Process.Start(args[0], ".\\fmsynth.py");
           
            Process.Start(args[0], ".\\fmsynth.py");
            wait1.Wait(60000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);

            try
            {
                WinClass winClass = new WinClass(ui);
                winClass.Test_ADDCarrierModulator();
                winClass.Test_RemoveSource();
                winClass.Test_Button_rightpane();
                winClass.Test_ComboBox();
                winClass.Test_Scrollbar();
                winClass.Test_Piano(MouseButtons.PhysicalLeft);
                winClass.Test_Stop();
                winClass.Test_Piano(MouseButtons.PhysicalRight);
                winClass.Test_StopAll();
                winClass.Test_Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }



    }
}
