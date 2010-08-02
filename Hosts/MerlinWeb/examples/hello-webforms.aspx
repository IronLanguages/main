<%@ Page Language="IronPython" CodeFile="hello-webforms.aspx.py" %>
<%----------------------------------------------------------------------------------
  Copyright (c) Microsoft Corporation. 

  This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
  copy of the license can be found in the License.html file at the root of this distribution. If 
  you cannot locate the  Apache License, Version 2.0, please send an email to 
  dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
  by the terms of the Apache License, Version 2.0.

  You must not remove this notice, or any other, from this software.
------------------------------------------------------------------------------------%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    	Enter your name:
        <asp:TextBox ID="TextBox1" runat="server">  
        </asp:TextBox>
        <asp:Button ID="Button1" runat="server" Text="Submit" OnClick="Button1_Click"/>
        <p>
            <asp:Label ID="Label1" runat="server" Text="Label">  
            </asp:Label>
        </p>
    </div>
    </form>
</body>
</html>
