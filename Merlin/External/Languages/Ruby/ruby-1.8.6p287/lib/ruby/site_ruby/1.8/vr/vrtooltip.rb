###################################
#
# vrtooltip.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2001-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

module WMsg
  TTM_ACTIVATE       = WM_USER + 1
  TTM_SETDELAYTIME   = WM_USER + 3
  TTM_ADDTOOL        = WM_USER + 4
  TTM_DELTOOL        = WM_USER + 5
  TTM_RELAYEVENT     = WM_USER + 7
  TTM_GETTOOLINFO    = WM_USER + 8
  TTM_SETTOOLINFO    = WM_USER + 9
  TTM_GETTEXT        = WM_USER + 11
  TTM_GETTOOLCOUNT   = WM_USER + 13
  TTM_TRACKACTIVATE  = WM_USER + 17
  TTM_TRACKPOSITION  = WM_USER + 18
  TTM_SETTIPBKCOLOR  = WM_USER + 19
  TTM_SETTIPTEXTCOLOR= WM_USER + 20
  TTM_GETDELAYTIME   = WM_USER + 21
  TTM_GETTIPBKCOLOR  = WM_USER + 22
  TTM_GETTIPTEXTCOLOR= WM_USER + 23
  TTM_SETMAXTIPWIDTH = WM_USER + 24
  TTM_GETMAXTIPWIDTH = WM_USER + 25

  TTN_NEEDTEXT   = 0x100000000-520 
  TTN_SHOW       = 0x100000000-521 
  TTN_POP        = 0x100000000-522 
end

module WConst
  TTF_IDISHWND   = 1
  TTF_CENTERTIP  = 2
  TTF_SUBCLASS   = 16
end

module WStruct
  TOOLINFO = "IILLllllLp"
end

class VRTooltip < SWin::Window
=begin
== VRTooltip
This is a class for Tooltip window.
Tooltip windows are created by VRForm.createTooltip()

=== Methods
--- addTool(cntl,text)
    Assigns a tooltip text for ((|cntl|)).
    Returns a tooltip identifier.
--- addToolArea([sx,sy,ex,ey],text,wnd=parent)
    Assigns a tooltip text for the area in the parent window.
    ((|sx|)) and ((|sy|)) indicate left-top corner,
     and ((|ex|)) and ((|ey|)) indicate right-bottom corner.
    Returns a tooltip identifier.
--- delTool(idf)
    Deletes a tool identified by ((|idf|)) returned by addTool or addToolArea
--- enumTool
    Yields all tool identifiers.
--- maxtipwidth
--- maxtipwidth=(mw)
    About the tooltip width. 
    If you are to use multiline tooltip, set this parameter.
--- autopopTime
--- autopopTime=
    Sets/Gets time length that tooltip remains visible.
--- initialTime
--- initialTime=
    Sets/Gets time length that the pointer must remain stationary.
--- reshowTime
--- reshowTime=
    Sets/Gets time length to reshow another tool's tooltip.

--- bkColor
--- bkColor=
    Sets/Gets background color of tooltip window

--- textColor
--- textColor=
    Sets/Gets text color of tooltip window
    
--- activate=(f)
    ((|f|)) is boolean. This activates/deactivates the tooltip.
=end

  class VRTooltipTool
    attr_reader :hwnd, :uid, :tool
    def initialize(hwnd,uid,tool)
      @hwnd,@uid,@tool = hwnd,uid,tool
    end
  end

private
  def initialize
    self.classname = "tooltips_class32"
    self.style=1
    self.exstyle= WExStyle::WS_EX_TOPMOST | WExStyle::WS_EX_TOOLWINDOW
    @_vr_tools={}
  end

  def classcheck(cls,obj)
    (eval("defined?(#{cls})") and obj.is_a?(eval(cls)))
  end

  def createTIPTOOLCore(wnd=@parent)
    size=4*10
    flag=WConst::TTF_SUBCLASS
    hwnd=wnd.hWnd
    uid= sx= sy= ex= ey= 0
    hinst=@screen.application.hInstance
    text=""
    [size,flag,hwnd,uid,sx,sy,ex,ey,hinst,text]
  end

  def addToolCore(ti)
    tis=ti.pack(WStruct::TOOLINFO)
    sendMessage WMsg::TTM_ADDTOOL,0,tis
  end

  def getTOOLINFOCore(hwnd,uid)
    ti=createTIPTOOLCore
    ti[2]=hwnd
    ti[3]=uid
    ti[9] = "\0\0\0\0" * (1024/4+1)   # /* Thanks, Wayne Vucenic */
    tis = ti.pack(WStruct::TOOLINFO)
    sendMessage WMsg::TTM_GETTOOLINFO,0,tis
    tis.unpack(WStruct::TOOLINFO)
  end

public

  def setparam(parent,screen)
    @parent,@screen = parent,screen unless defined? @screen
  end

  def activate=(f)
    i = if f then 1 else 0 end
    sendMessage WMsg::TTM_ACTIVATE,i,0
  end

  def maxtipwidth=(w)
    s=[w].pack("L")
    sendMessage WMsg::TTM_SETMAXTIPWIDTH,0,s
  end

  def maxtipwidth
    sendMessage WMsg::TTM_GETMAXTIPWIDTH,0,0
  end

  def autopopTime
    sendMessage WMsg::TTM_GETDELAYTIME,2,0
  end
  def initialTime
    sendMessage WMsg::TTM_GETDELAYTIME,3,0
  end
  def reshowTime
    sendMessage WMsg::TTM_GETDELAYTIME,1,0
  end
  def autopopTime=(msec)
    sendMessage WMsg::TTM_SETDELAYTIME,2,MAKELPARAM(msec,0)
  end
  def initialTime=(msec)
    sendMessage WMsg::TTM_SETDELAYTIME,3,MAKELPARAM(msec,0)
  end
  def reshowTime=(msec)
    sendMessage WMsg::TTM_SETDELAYTIME,1,MAKELPARAM(msec,0)
  end

  def bkColor
    sendMessage WMsg::TTM_GETTIPBKCOLOR,0,0
  end
  def bkColor=(c)
    sendMessage WMsg::TTM_SETTIPBKCOLOR,c.to_i,0
  end
  def textColor
    sendMessage WMsg::TTM_GETTIPTEXTCOLOR,0,0
  end
  def textColor=(c)
    sendMessage WMsg::TTM_SETTIPTEXTCOLOR,c.to_i,0
  end

  def addTool(cnt,text)
    if classcheck("VRStatic",cnt) then
      sx=cnt.x; sy=cnt.y; ex=sx+cnt.w; ey=sy+cnt.h
      rect = [sx,sy,ex,ey]
      return addToolArea(rect,text)
    end
    if classcheck("VRToolbar::VRToolbarButton",cnt) then
      rect = [0,0,0,0].pack("LLLL")
      cnt.toolbar.sendMessage WMsg::TB_GETITEMRECT,cnt.index,rect
      return addToolArea(rect.unpack("LLLL"),text,cnt.toolbar)
    end

    ti = createTIPTOOLCore
    ti[1] |= WConst::TTF_IDISHWND 
    ti[3] = cnt.hWnd
    ti[9] =text
    addToolCore(ti)
    @_vr_tools[ ti[2,2] ] = VRTooltipTool.new(@parent.hWnd,cnt.hWnd,cnt)
  end

  def addToolArea(rect,text,wnd=@parent)
    sx,sy,ex,ey = *rect
    ti = createTIPTOOLCore wnd
    ti[3] = @parent.newControlID
    ti[4]=sx ; ti[5]=sy; ti[6]=ex ; ti[7]=ey
    ti[9] =text
    addToolCore(ti)
    @_vr_tools[ ti[2,2] ] = VRTooltipTool.new(wnd.hWnd,ti[3],rect)
  end

  def delTool(ttt)
    raise "Requires VRTooltipTool (#{ttt.class})" unless ttt.is_a?(VRTooltipTool)
    ti=createTIPTOOLCore
    ti[2] = ttt.hwnd
    ti[3] = ttt.uid
    tts = ti.pack(WStruct::TOOLINFO)
    @_vr_tools.delete(ti[2,2])
    sendMessage WMsg::TTM_DELTOOL,0,tts
  end

  def enumTool
    r = sendMessage WMsg::TTM_GETTOOLCOUNT,0,0
    raise "Unknown error" if r!=@_vr_tools.size
    @_vr_tools.each do |key,val|
      yield val
    end
  end

  def getTextOf(ttt,maxlength=1024)  # there is no way to determine the length
    ti = createTIPTOOLCore
    buffer = "\0\0\0\0" * (maxlength/4+1) 
    ti[2] = ttt.hwnd
    ti[3] = ttt.uid
    ti[9] = buffer
    tis = ti.pack(WStruct::TOOLINFO)
    sendMessage WMsg::TTM_GETTEXT,0,tis
    buffer.gsub!(/\0.*/,'')
    buffer
  end

  def setTextOf(ttt,text)
    ti = getTOOLINFOCore(ttt.hwnd,ttt.uid)
    ti[9] = text
    tis = ti.pack(WStruct::TOOLINFO)
    sendMessage WMsg::TTM_SETTOOLINFO,0,tis
  end

end


class VRForm
  attr_reader :tooltip
  def createTooltip
    return tooltip if defined?(@tooltip) && @tooltip
    @tooltip = @screen.factory.newwindow(self,VRTooltip)
    @tooltip.setparam self,@screen
    @tooltip.create
    @tooltip.etc =  newControlID # error occurs if this line is before creation
    @tooltip.top(-1)
  end
end

