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
using System.Diagnostics;
using MS.Internal.Mita.Foundation.Controls;
using MS.Internal.Mita.Foundation;
using MS.Internal.Mita.Foundation.Waiters;
using System.Threading;

namespace TestDirect3D
{
    class WinClass
    {
        public WinClass(string cmdPara)
        {
            this.CMD = cmdPara;
        }

        public void Test_Checkpoint1()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\checkpoints\\checkpoint1.py");
            
            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(3000);
            buttonClose.Click();    
        }

        public void Test_Checkpoint2()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\checkpoints\\checkpoint2.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(2000);
            buttonClose.Click();
        }

        public void Test_Checkpoint3()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\checkpoints\\checkpoint3.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(3000);
            buttonClose.Click();
        }

        public void Test_Checkpoint4()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\checkpoints\\checkpoint4.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(3000);
            buttonClose.Click();
        }


        public void Test_Checkpoint5()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\checkpoints\\checkpoint5.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(3000);
            buttonClose.Click();
        }

        public void Test_Checkpoint6()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\checkpoints\\checkpoint6.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(3000);
            buttonClose.Click();
        }

        public void Test_demo1()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\demo1.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();
        }

        public void Test_demo2()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\demo2.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();
        }


        public void Test_demo3()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\demo3.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();
        }

        public void Test_demo4()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\demo4.py");

            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();
        }

        public void Test_GravityDemo()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\GravityDemo.py");
            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();           
        }

        public void Test_MeshDemo()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\meshdemo.py");
            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();        
        }

        public void Test_Tutorial()
        {
            //checkpoint1
            UICondition uIcondition = UICondition.Create("@ControlType=Window and @Name='IronPython Direct3D'", new object[0]);
            WindowOpenedWaiter wait1 = new WindowOpenedWaiter(uIcondition);
            Process.Start(CMD, ".\\tutorial.py");
            wait1.Wait(20000);
            UIObject ui = UIObject.Root.Children.Find(uIcondition);
            UIObject uiClose = ui.FirstChild.Children.Find("Close");
            Button buttonClose = new Button(uiClose);
            Thread.Sleep(5000);
            buttonClose.Click();
        }

        private string CMD;
    }
}
