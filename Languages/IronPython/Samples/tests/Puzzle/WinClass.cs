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
using System.Windows.Automation;
using System.Threading;
using System.Windows.Forms;

class WinClass:Window
{
    public WinClass(UIObject ui):base(ui)
    {
    }

    public void Test_about()
    {
        UICondition uICondition = UICondition.Create("@Name='about'", new Object[0]);
        UIObject uIObject = this.Descendants.Find(uICondition);
        Window obj = new Window(uIObject);
        UICondition uICondition2 = UICondition.Create("@Name='Tile Size'", new Object[0]);
        obj.Click();
        Thread.Sleep(300);
        UIObject result = this.Children.Find(uICondition2);
    }

    public void Test_options()
    {
        UICondition uICondition = UICondition.Create("@Name='options'", new Object[0]);
        UIObject uIObject = this.Descendants.Find(uICondition);
        Window obj = new Window(uIObject);
        UICondition uI2 = UICondition.Create("@Name='Tile Size'",new Object[0]);
        UICondition uI50 = UICondition.Create("@Name='50%'",new Object[0]);
        UICondition uI100 = UICondition.Create("@Name='100%'", new Object[0]);
        UICondition uI75 = UICondition.Create("@Name='75%'", new Object[0]);
        obj.Click();
        UIObject uITileSize = this.Descendants.Find(uI2);
        UIObject uIo75 = this.Descendants.Find(uI75);
        UIObject uIo100 = this.Descendants.Find(uI100);
        Window obj75 = new Window(uIo75);
        obj75.Click();
        Thread.Sleep(200);
        Window obj100 = new Window(uIo100);
        obj100.Click();
        UIObject uIo50 = this.Descendants.Find(uI50);
        Thread.Sleep(200);
        Window obj50= new Window(uIo50);
        obj50.Click();
        Thread.Sleep(200);
        //UICondition uICache = UICondition.Create("@Name='Allow caching'", new Object[0]);
        //UIObject uIoCache = this.Descendants.Find(uICache);
        //Window objCache = new Window(uIoCache);
        //objCache.Click();
        //Thread.Sleep(200);
        UICondition uIClsCa = UICondition.Create("@Name='Clear Cache'", new Object[0]);
        UIObject uIoClsCa = this.Descendants.Find(uIClsCa);
        Window objClsCa = new Window(uIoClsCa);
        objClsCa.Click();
        Thread.Sleep(200);
    }




    public void Test_load()
    {
        UICondition uICondition = UICondition.Create("@Name='load'", new Object[0]);
        UIObject uIObject = this.Descendants.Find(uICondition);
        Window obj = new Window(uIObject);
        obj.Click();

        UICondition uIButton1 = UICondition.Create("@Name='Seattle (default game)\n(327, 714)\nAerial - Zoom Level 11 - 3x3'", new Object[0]);
        UIObject uIoButton1 = this.Descendants.Find(uIButton1);
        Window objButton1 = new Window(uIoButton1);
        objButton1.Click();
        Thread.Sleep(100);

        UICondition uIButton2 = UICondition.Create("@Name='New York\n(1205, 1538)\nRoad - Zoom Level 12 - 3x3'", new Object[0]);
        UIObject uIoButton2 = this.Descendants.Find(uIButton2);
        Window objButton2 = new Window(uIoButton2);
        objButton2.Click();
        Thread.Sleep(100);

        UICondition uIButton3 = UICondition.Create("@Name='World\n(0, 0)\nHybrid - Zoom Level 2 - 4x4'", new Object[0]);
        UIObject uIoButton3 = this.Descendants.Find(uIButton3);
        Window objButton3 = new Window(uIoButton3);
        objButton3.Click();
        Thread.Sleep(100);

        UICondition uIButton4 = UICondition.Create("@Name='North America\n(2, 5)\nAerial - Zoom Level 4 - 3x3'", new Object[0]);
        UIObject uIoButton4 = this.Descendants.Find(uIButton4);
        Window objButton4 = new Window(uIoButton4);
        objButton4.Click();
        Thread.Sleep(100);

        UICondition uILoad = UICondition.Create("@Name='Load Puzzle'", new Object[0]);
        UIObject uIoLoad = this.Descendants.Find(uILoad);
        Window objLoad = new Window(uIoLoad);
        objLoad.Click();
        
    }

    public void Test_play()
    {
        UICondition uIToStart = UICondition.Create("@Name='Shuffle\nto\nStart'", new Object[0]);
        for (int i = 0; i < 1000; i++)
        {
            if (this.Descendants.Contains(uIToStart))
            {
                break;
            }
            Thread.Sleep(100);
        }
        UIObject uIoStart = this.Descendants.Find(uIToStart);
        Window objStart = new Window(uIoStart);
        objStart.Click();
        play_puzzle();
    }

    public void play_puzzle()
    {
        int i;
        UICollection<UIObject> UICoPane;
       
        UICondition uIpane = UICondition.Create("@ControlType=Pane", new Object[0]);
        UIObject uIopane = this.Children.Find(uIpane);
        //UICondition uIpane = UICondition.Create("@ControlType=ControlType.Pane", new Object[0]);
        UICoPane = uIopane.Children.FindMultiple(uIpane);
        int Dim = UICoPane.Count; 
        //foreach (UIObject i in UICoPane)
        //{
        //    Window objpuzzle = new Window(i);
        //    objpuzzle.Click();
        //}
        for(int aa = 0; aa < 40; aa++)
        {
            Random rand = new Random(Environment.TickCount);
            i = rand.Next(Dim);
            //Console.Write(i.ToString()+" : ");
            //Console.WriteLine(UICoPane.Count.ToString()+" : "+aa.ToString());
            Window objpuzzle = new Window(UICoPane[i]);
            objpuzzle.Click();
            Thread.Sleep(200);       
        }
    }

    public void Test_exit()
    {
        UICondition uICexit = UICondition.Create("@Name='exit'", new Object[0]);
        UIObject uIoExit = this.Descendants.Find(uICexit);
        Window objexit = new Window(uIoExit);

        //http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=27142
        //UIProperty u = UIProperty.Get("ClassName"); 
        //UICondition uIcondition2 = UICondition.Create(u,"#32770");
        //WindowOpenedWaiter wait2 = new WindowOpenedWaiter(uIcondition2);
        //objexit.Click();
        
        //wait2.Wait(5000);

        //UIObject ui2 = this.Children.Find(uIcondition2);
        //UIObject uiyes = ui2.Children.Find(UICondition.Create("@Name='Yes'", new Object[0]));
        //Window winyes = new Window(uiyes);
        //Thread.Sleep(1000);
        //winyes.Click();       
    }
    
    public void Test_create()
    {
        UICondition uICcreate = UICondition.Create("@Name='create'", new Object[0]);
        UIObject uIoCreate = this.Descendants.Find(uICcreate);
        Window objcreate = new Window(uIoCreate);
        objcreate.Click();
      

        UICondition uI3x3 = UICondition.Create("@Name='3x3'", new Object[0]);
        UIObject uIo3x3 = this.Descendants.Find(uI3x3);

        UICondition uI4x4 = UICondition.Create("@Name='4x4'", new Object[0]);
        UIObject uIo4x4 = this.Descendants.Find(uI4x4);

        MS.Internal.Mita.Foundation.Controls.RadioButton radioButton3 = new MS.Internal.Mita.Foundation.Controls.RadioButton(uIo3x3);
        MS.Internal.Mita.Foundation.Controls.RadioButton radioButton4 = new MS.Internal.Mita.Foundation.Controls.RadioButton(uIo4x4);
        if (radioButton3.IsSelected)
        {
            radioButton4.Select();
        }

        //http://ironpython.codeplex.com/WorkItem/View.aspx?WorkItemId=27142
        //UICondition uICcreate2 = UICondition.Create("@Name='Create'", new Object[0]);
        //UIObject uIoCreate2 = this.Descendants.Find(uICcreate2);
        //Window objcreate2 = new Window(uIoCreate2);

        //UIProperty u = UIProperty.Get("ClassName");
        //UICondition uIcondition2 = UICondition.Create(u, "#32770");
        //WindowOpenedWaiter wait2 = new WindowOpenedWaiter(uIcondition2);
        //objcreate2.Click();
        //wait2.Wait(5000);

        //UIObject ui2 = this.Children.Find(uIcondition2);
        //UIObject uiyes = ui2.Children.Find(UICondition.Create("@Name='Yes'", new Object[0]));
        //Window winyes = new Window(uiyes);
        //Thread.Sleep(300);
        //winyes.Click();  
    }
}

