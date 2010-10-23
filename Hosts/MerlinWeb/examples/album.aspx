<%@ Page Language="IronPython" Inherits="Microsoft.Scripting.AspNet.UI.ScriptPage" EnableViewState="false" %>
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
<head id="Head1" runat="server">
    <title>Album</title>
</head>
<body bgcolor="GhostWhite">
    <form id="form1" runat="server">
    <div>
        <asp:MultiView ID="AlbumMultiView" runat="server">

            <asp:View ID="FolderView" runat="server">

            <span id="FolderViewParentLinkSpan" runat="server" visible="false">
<a href='<%= ParentLink.Link %>'><img alt='<%= ParentLink.Alt %>' src='<%= ParentLink.Src %>'
    width='<%= ParentLink.Width %>' height='<%= ParentLink.Height %>' style='border:0' /></a>
            </span>

            <asp:Repeater ID="ThumbnailList" runat="server">
                <ItemTemplate>

<a href='<%# Link %>'><img alt='<%# Alt %>' src='<%# Src %>'
    width='<%# Width %>' height='<%# Height %>' style='border:0' /></a>

                </ItemTemplate>
            </asp:Repeater>
            </asp:View>

            <asp:View ID="PageView" runat="server">

<table border="0"><tr>
<td align="center" width="<%= ThumbnailSize+8 %>">
    <span id="PreviousLinkSpan" runat="server" visible="false">
    <a href='<%= PreviousLink.Link %>'>
    Previous Picture<br />
    <img alt='<%= PreviousLink.Alt %>' src='<%= PreviousLink.Src %>'
        width='<%= PreviousLink.Width %>' height='<%= PreviousLink.Height %>' style='border:0' /></a>
    </span>
    <span id="NoPreviousLinkSpan" runat="server" visible="false">
    No Previous Picture
    </span>
</td>
<td align="center" width="<%= ThumbnailSize+8 %>">
    <a href='<%= ParentLink.Link %>'>
    Up to Folder View<br />
    <img alt='<%= ParentLink.Alt %>' src='<%= ParentLink.Src %>'
        width='<%= ParentLink.Width %>' height='<%= ParentLink.Height %>' style='border:0' /></a>
</td>
<td align="center" width="<%= ThumbnailSize+8 %>">
    <span id="NextLinkSpan" runat="server" visible="false">
    <a href='<%= NextLink.Link %>'>
    Next Picture<br />
    <img alt='<%= NextLink.Alt %>' src='<%= NextLink.Src %>'
        width='<%= NextLink.Width %>' height='<%= NextLink.Height %>' style='border:0' /></a>
    </span>
    <span id="NoNextLinkSpan" runat="server" visible="false">
    No Next Picture
    </span>
</td>
</tr></table>
<p>
    <span id="PictureLinkSpan" runat="server" visible="false">
    <a href='<%= PictureLink.Link %>'><img alt='<%= PictureLink.Alt %>' src='<%= PictureLink.Src %>'
        width='<%= PictureLink.Width %>' height='<%= PictureLink.Height %>' style='border:0' /></a>
    </span>
</p>

            </asp:View>

        </asp:MultiView>
    </div>
    </form>
</body>
</html>
