###################################
#
# winconst.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################


#######################################
#   Win Constants and utils
#
module WMsg
  WM_NULL           = 0x0000
  WM_CREATE         = 0x0001
  WM_DESTROY        = 0x0002
  WM_MOVE           = 0x0003
  WM_SIZE           = 0x0005
  WM_ACTIVATE       = 0x0006
  WM_SETFOCUS       = 0x0007
  WM_KILLFOCUS      = 0x0008
  WM_ENABLE         = 0x000A
  WM_SETREDRAW      = 0x000B
  WM_SETTEXT        = 0x000C
  WM_GETTEXT        = 0x000D
  WM_GETTEXTLENGTH  = 0x000E
  WM_PAINT          = 0x000F
  WM_CLOSE          = 0x0010
  WM_QUERYENDSESSION= 0x0011
  WM_QUIT           = 0x0012
  WM_QUERYOPEN      = 0x0013
  WM_ERASEBKGND     = 0x0014
  WM_SYSCOLORCHANGE = 0x0015
  WM_ENDSESSION     = 0x0016
  WM_SHOWWINDOW     = 0x0018
  WM_WININICHANGE   = 0x001A
  WM_SETTINGCHANGE  = WM_WININICHANGE
  WM_DEVMODECHANGE  = 0x001B
  WM_ACTIVATEAPP    = 0x001C
  WM_FONTCHANGE     = 0x001D
  WM_TIMECHANGE     = 0x001E
  WM_CANCELMODE     = 0x001F
  WM_SETCURSOR      = 0x0020
  WM_MOUSEACTIVATE  = 0x0021
  WM_CHILDACTIVATE  = 0x0022
  WM_QUEUESYNC      = 0x0023
  WM_GETMINMAXINFO  = 0x0024
  WM_NOTIFY         = 0x004E
  WM_KEYDOWN        = 0x0100
  WM_KEYUP          = 0x0101
  WM_CHAR           = 0x0102
  WM_COMMAND        = 0x0111
#
  WM_MOUSEFIRST     = 0x0200
  WM_MOUSEMOVE      = 0x0200
  WM_LBUTTONDOWN    = 0x0201
  WM_LBUTTONUP      = 0x0202
  WM_LBUTTONDBLCLK  = 0x0203
  WM_RBUTTONDOWN    = 0x0204
  WM_RBUTTONUP      = 0x0205
  WM_RBUTTONDBLCLK  = 0x0206
  WM_MBUTTONDOWN    = 0x0207
  WM_MBUTTONUP      = 0x0208
  WM_MBUTTONDBLCLK  = 0x0209
  WM_MOUSEWHEEL     = 0x020A
  WM_MOUSELAST      = 0x020A
  WM_DROPFILES      = 0x0233
#
  WM_CUT            = 0x0300
  WM_COPY           = 0x0301
  WM_PASTE          = 0x0302
  WM_CLEAR          = 0x0303
  WM_UNDO           = 0x0304

  WM_HOTKEY         = 0x0312
  WM_USER           = 0x0400
  WM_APP            = 0x8000
end

module WStyle
  WS_OVERLAPPED     = 0
  WS_TABSTOP        = 0x00010000
  WS_GROUP          = 0x00020000
  WS_THICKFRAME     = 0x00040000
  WS_SYSMENU        = 0x00080000
  WS_HSCROLL        = 0x00100000
  WS_VSCROLL        = 0x00200000
  WS_DLGFRAME       = 0x00400000
  WS_BORDER         = 0x00800000
  WS_CAPTION        = 0x00c00000
  WS_MAXIMIZE       = 0x01000000
  WS_CLIPCHILDREN   = 0x02000000
  WS_CLIPSIBLINGS   = 0x04000000
  WS_DISABLED       = 0x08000000
  WS_VISIBLE        = 0x10000000
  WS_MINIMIZE       = 0x20000000
  WS_CHILD          = 0x40000000
  WS_POPUP          = 0x80000000
  WS_VISIBLECHILD   = 0x50000000  # for util

   #followings are contributed by Yuya-san.
  WS_ICONIC           = 0x20000000
  WS_CHILDWINDOW      = 0x40000000
  WS_MAXIMIZEBOX      = 0x10000
  WS_MINIMIZEBOX      = 0x20000
  WS_OVERLAPPEDWINDOW = 0xCF0000
  WS_POPUPWINDOW      = 0x80880000
  WS_SIZEBOX          = 0x40000
  WS_TILED            = 0
  WS_TILEDWINDOW      = 0xCF0000
end

module WExStyle
  WS_EX_TOPMOST     = 0x00000008
  WS_EX_TRANSPARENT = 0x00000020
  WS_EX_MDICHILD    = 0x00000040
  WS_EX_TOOLWINDOW  = 0x00000080
  WS_EX_CLIENTEDGE  = 0x00000200
  WS_EX_APPWINDOW   = 0x00040000
end

=begin
== Global Functions
These are utility functions instead of macros.
=== Functions
--- LOWORD(lParam)
    Returns low-word of lParam
--- HIWORD(lParam)
    Returns hi-word of lParam
--- MAKELPARAM(w1,w2)
    Returns the DWORD from 2 words of w1,w2.
--- SIGNEDWORD(word)
    changes unsigned WORD into signed WORD.
--- RGB(r,g,b)
    returns color code from r,g,b.
=end

def LOWORD(lParam)
  return (lParam & 0xffff)
end
def HIWORD(lParam)
  return ( (lParam>>16) & 0xffff)
end
def MAKELPARAM(w1,w2)
  return (w2<<16) | w1
end

def SIGNEDWORD(word)
  if word>0x8000 then word-0x10000 else word end
end


def RGB(r,g,b)
  return r+(g<<8)+(b<<16)
end

