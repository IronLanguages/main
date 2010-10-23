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
    class WinClass
    {
        public WinClass(UIObject uiobject)
        {
            this.uImain = uiobject;
            this.CarNum = 0;
        }

        public void Test_ADDCarrierModulator()
        {
            UICondition uIconAddCa = UICondition.Create("@Name='Add Carrier'",new Object[0]);
            UICondition uIconAddMo = UICondition.Create("@Name='Add Modulator'", new Object[0]);
            Random rand = new Random(System.Environment.TickCount);
            UIObject uIOAddCa = this.uImain.Children.Find(uIconAddCa);
            UIObject uIOAddMo = this.uImain.Children.Find(uIconAddMo);
            uIOAddCa.Click();
            for (int i = 0; i < 25; i++)
            {
                int a = rand.Next(2);
                switch(a)
                {
                    case 0:
                        uIOAddCa.Click();
                        CarNum++;
                        break;
                    case 1:
                        uIOAddMo.Click();
                        break;
                }
            }
          
        }

        public void Test_RemoveSource()
        {
            UICondition uICoRem = UICondition.Create("@Name='Remove Source'", new Object[0]);
            UIObject uIRem = this.uImain.Children.Find(uICoRem);
            Button button = new Button(uIRem);
            while (this.CarNum >2)
            {
                button.Click();
                this.CarNum--;
            }
        }

        public void Test_Button_rightpane()
        {
            UICondition uICoUseRatio = UICondition.Create("@Name='Use ratio'", new Object[0]);
            UIObject uIOUseRatio = this.uImain.Children.Find(uICoUseRatio);
            CheckBox cbox = new CheckBox(uIOUseRatio);
            Thread.Sleep(400);
            cbox.Click();
            
        }

        public void Test_ComboBox()
        {
            UICondition uICoComboBox = UICondition.Create("@ControlType=ComboBox and @Name=':1'", new Object[0]);
            UIObject uIComboBox = this.uImain.Children.Find(uICoComboBox);
            ComboBox  combox = new ComboBox(uIComboBox);
            Thread.Sleep(400);
            ListBox lbox = new ListBox(combox.FirstChild);
            for (int i = 1; i < 3; i++)
            {
                combox.Expand();
                lbox.Children[i].Click();
                Thread.Sleep(400);
            }
        }

        public void Test_Scrollbar()
        {
            UICondition uICoScro = UICondition.Create("@Name='Amplitude'and @ControlType=ScrollBar", new Object[0]);
            UIObject uIScor= this.uImain.Children.Find(uICoScro);
            ScrollBar scrollBar = new ScrollBar(uIScor);
            Button button = new Button(scrollBar.Children.Find(UICondition.Create("@Name='Page left'",new Object[0]))); 
            for (int i = 1; i < 7; i++)
            {
                button.Click();
            }
        }

        public void Test_Piano(MouseButtons key)
        {
            UICondition uICoPiano = UICondition.Create("@Name='Volume'and @ControlType=Pane", new Object[0]);
            UIObject uIPiano = this.uImain.Children.Find(uICoPiano);
            
            for (int i = 10; i < 60; i=i+20)
            {
                Mouse.Instance.Click(key, uIPiano, i, 50, ModifierKeys.None);
                Thread.Sleep(2000);
                switch (i)
                {
                    case 70:
                        Mouse.Instance.Click(key, uIPiano, i + 10, 20, ModifierKeys.None);
                        Thread.Sleep(2000);
                        break;
                    case 130:
                        Mouse.Instance.Click(key, uIPiano, i + 10, 20, ModifierKeys.None);
                        Thread.Sleep(2000);
                        break;
                    case 190:
                        Mouse.Instance.Click(key, uIPiano, i + 10, 20, ModifierKeys.None);
                        Thread.Sleep(2000);
                        break;
                    case 250:
                        Mouse.Instance.Click(key, uIPiano, i + 10, 20, ModifierKeys.None);
                        Thread.Sleep(2000);
                        break;
                }
            }

        }

        public void Test_Stop()
        {
            UICondition uICoStop = UICondition.Create("@Name='Stop'", new Object[0]);
            UIObject uIStop = this.uImain.Children.Find(uICoStop);
            Button button = new Button(uIStop);
            button.Click();
        }

        public void Test_StopAll()
        {
            UICondition uICoStopAll = UICondition.Create("@Name='Stop All'", new Object[0]);
            UIObject uIStopAll = this.uImain.Children.Find(uICoStopAll);
            Button button = new Button(uIStopAll);
            button.Click();
        }

        public void Test_Close()
        {
            UIObject uIClose = this.uImain.Descendants.Find("Close");
            Button bu = new Button(uIClose);
            bu.Click();
        }

        private int CarNum;
        private UIObject uImain;

    }
}
