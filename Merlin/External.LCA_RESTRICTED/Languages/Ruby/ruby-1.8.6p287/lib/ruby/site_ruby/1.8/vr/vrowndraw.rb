###################################
#
# vrowndraw.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vrcontrol'

module WMsg
  WM_DRAWITEM = 0x002B
end

module VROwnerDrawControlContainer
=begin
== VROwnerDrawControlContainer
This module provides a message handler for WM_DRAWITEM.
VRForm includes this module automatically loading "vrowndraw.rb".
=end

  include VRMessageHandler

  HANDLERNAME="ownerdraw"

  def ownerdrawinit
    addHandler(WMsg::WM_DRAWITEM,"vrwmdrawitem",
               MSGTYPE::ARGINTSTRUCT,"UUUUUUUUUUUU")
    addEvent WMsg::WM_DRAWITEM
    addNoRelayMessages [WMsg::WM_DRAWITEM]
  end
  
  def vrinit
    super
    ownerdrawinit
  end
  
  def self_vrwmdrawitem(wParam,args)
    id=LOWORD(wParam)
    ct=@controls[id]       # Activated Control

    return unless ct  # can't find?
    r=0
    ct.dopaint(args[6]) do
      if ct.respond_to?(HANDLERNAME)
        r=ct.__send__(HANDLERNAME,*args[2..11]) 
      end
    end
    SKIP_DEFAULTHANDLER[r]
  end
end

module VRContainersSet
  include VROwnerDrawControlContainer
end



class VROwnerDrawButton < VRButton
=begin
== VROwnerDrawButton
Owner draw button.
This is just a button but the system doesn't draw button faces.
It's necessary to draw button face manually.

=== Required methods
In these method, it doesn't need to call ((*dopaint*)).

--- drawpushed(left,top,right,bottom,state)
    Draws the pushed button face.
--- drawreleased(left,top,right,bottom,state)
    Draws the released button face.
--- drawfocused(left,top,right,bottom,state)
    Draws the focused button face. 
    This method is fired after drawpushed or drawreleased.
=end

  BS_OWNERDRAW = 0x0000000B
  WINCLASSINFO = ["BUTTON", BS_OWNERDRAW]

  def ownerdraw(iid,action,state,hwnd,hdc,left,top,right,bottom,data)
    self.opaque=false
    if (pushed = ((state & 1)>0) ) then
      drawpushed(left,top,right,bottom,state)
      @parent.controlmsg_dispatching(self,
          "drawpushed",left,top,right,bottom,state)
    else
      drawreleased(left,top,right,bottom,state)
      @parent.controlmsg_dispatching(self,
          "drawreleased",left,top,right,bottom,state)
    end
    
    if (state & 0x10)>0 then
      drawfocused(left,top,right,bottom,state)
      @parent.controlmsg_dispatching(self,
          "drawfocused",left,top,right,bottom,state)
    end
  end

  def drawpushed(left,top,right,bottom,state) end
  def drawreleased(left,top,right,bottom,state) end
  def drawfocused(left,top,right,bottom,state) end
end


