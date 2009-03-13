###############################
#
# contrib/toolbar.rb
#
# These modules/classes are contributed by Yukimi_Sake-san.
# Modified by nyasu <nyasu@osk.3web.ne.jp>
# Distributed at http://vruby.sourceforge.net/index.html
#
###############################

module WMsg
# WM_USER                 = 0x400
  TB_ENABLEBUTTON         = WM_USER + 1
  TB_CHECKBUTTON          = WM_USER + 2
  TB_PRESSBUTTON          = WM_USER + 3
  TB_HIDEBUTTON           = WM_USER + 4
  TB_INDETERMINATE        = WM_USER + 5
  TB_ISBUTTONENABLED      = WM_USER + 9
  TB_ISBUTTONCHECKED      = WM_USER + 10
  TB_ISBUTTONPRESSED      = WM_USER + 11
  TB_ISBUTTONHIDDEN       = WM_USER + 12
  TB_ISBUTTONINDETERMINATE = WM_USER + 13
  TB_SETSTATE             = WM_USER + 17
  TB_GETSTATE             = WM_USER + 18
  TB_ADDBITMAP            = WM_USER + 19
  TB_ADDBUTTONS           = WM_USER + 20
  TB_INSERTBUTTON         = WM_USER + 21
  TB_DELETEBUTTON         = WM_USER + 22
  TB_GETBUTTON            = WM_USER + 23
  TB_BUTTONCOUNT          = WM_USER + 24
  TB_COMMANDTOINDEX       = WM_USER + 25
  TB_SAVERESTOREA         = WM_USER + 26
  TB_SAVERESTOREW         = WM_USER + 76
  TB_CUSTOMIZE            = WM_USER + 27
  TB_ADDSTRINGA           = WM_USER + 28
  TB_GETITEMRECT          = WM_USER + 29
  TB_BUTTONSTRUCTSIZE     = WM_USER + 30
  TB_SETBUTTONSIZE        = WM_USER + 31
  TB_SETBITMAPSIZE        = WM_USER + 32
  TB_AUTOSIZE             = WM_USER + 33
  TB_GETTOOLTIPS          = WM_USER + 35
  TB_SETTOOLTIPS          = WM_USER + 36
  TB_SETPARENT            = WM_USER + 37
  TB_SETROWS              = WM_USER + 39
  TB_GETROWS              = WM_USER + 40
  TB_GETBITMAPFLAGS       = WM_USER + 41
  TB_SETCMDID             = WM_USER + 42
  TB_CHANGEBITMAP         = WM_USER + 43
  TB_GETBITMAP            = WM_USER + 44
  TB_GETBUTTONTEXTA       = WM_USER + 45
  TB_REPLACEBITMAP        = WM_USER + 46
  TB_SETINDENT            = WM_USER + 47
  TB_SETIMAGELIST         = WM_USER + 48
  TB_GETIMAGELIST         = WM_USER + 49
  TB_LOADIMAGES           = WM_USER + 50
  TB_GETRECT              = WM_USER + 51 # wParam is the Cmd instead of index
  TB_SETHOTIMAGELIST      = WM_USER + 52
  TB_GETHOTIMAGELIST      = WM_USER + 53
  TB_SETDISABLEDIMAGELIST = WM_USER + 54
  TB_GETDISABLEDIMAGELIST = WM_USER + 55
  TB_SETSTYLE             = WM_USER + 56
  TB_GETSTYLE             = WM_USER + 57
  TB_GETBUTTONSIZE        = WM_USER + 58
  TB_SETBUTTONWIDTH       = WM_USER + 59
  TB_SETMAXTEXTROWS       = WM_USER + 60
  TB_GETTEXTROWS          = WM_USER + 61
  TB_GETBUTTONTEXTW       = WM_USER + 75
  TB_ADDSTRINGW           = WM_USER + 77

  TBBF_LARGE              = 1

  TB_GETBUTTONINFO      = WM_USER + 65
  TB_SETBUTTONINFO      = WM_USER + 66


  TBN_FIRST               = -700
  TBN_GETBUTTONINFO       = TBN_FIRST-0
  TBN_BEGINDRAG           = TBN_FIRST-1
  TBN_ENDDRAG             = TBN_FIRST-2
  TBN_BEGINADJUST         = TBN_FIRST-3
  TBN_ENDADJUST           = TBN_FIRST-4
  TBN_RESET               = TBN_FIRST-5
  TBN_QUERYINSERT         = TBN_FIRST-6
  TBN_QUERYDELETE         = TBN_FIRST-7
  TBN_TOOLBARCHANGE       = TBN_FIRST-8
  TBN_CUSTHELP            = TBN_FIRST-9
  TBN_GETBUTTONINFOW      = TBN_FIRST-20
end

module WConst
  TBSTATE_CHECKED       = 1
  TBSTATE_PRESSED       = 2
  TBSTATE_ENABLED       = 4
  TBSTATE_HIDDEN        = 8
  TBSTATE_INDETERMINATE = 16
  TBSTATE_WRAP          = 32

  TBSTYLE_BUTTON        = 0
  TBSTYLE_SEP           = 1
  TBSTYLE_CHECK         = 2
  TBSTYLE_GROUP         = 4
  TBSTYLE_CHECKGROUP    = (TBSTYLE_GROUP|TBSTYLE_CHECK)
  TBSTYLE_TOOLTIPS      = 256
  TBSTYLE_WRAPABLE      = 512
  TBSTYLE_ALTDRAG       = 1024
  TBSTYLE_FLAT          = 2048


  TBIF_IMAGE = 0x00000001
  TBIF_TEXT  = 0x00000002
  TBIF_STATE = 0x00000004
  TBIF_STYLE = 0x00000008
  TBIF_LPARAM = 0x00000010
  TBIF_COMMAND = 0x00000020
  TBIF_SIZE = 0x00000040
end

module WStruct
    TB_BUTTON = "IICCCCLI"
                  # int iBitmap;int idCommand; BYTE fsState; BYTE fsStyle;
                  # BYTE[2] fsReserved; DWORD dwData;int iString;
    TBNOTIFY = NMHDR+"I"+TB_BUTTON+"IP"
                  # NMHDR hdr; int iItem; TBBUTTON tbButton;
                  # int cchText; LPTSTR pszText;
    TBADDBITMAP="UU"
                  #HINSTANCE hInst; UINT nID;

end

module VRToolbarUseable
=begin
== VRToolbarUseable
  If you include this module in parent, you can solve the fault of a notify
  events at the time of including a VRComCtlContainer, since toolbar buttons
  are set to _vr_toolbar_buttons which is an original variable.
  When not including this module , toolbar buttons are set to _vr_contorols
  for back compatibility.
=end
  require 'vr/vrcontrol'
  include VRStdControlContainer
  attr_reader :_vr_toolbar_buttons
  alias self_wmcommand_org self_wmcommand

  def self_wmcommand(msg)
    if @_vr_toolbar_buttons then
      tbbid=LOWORD(msg.wParam)
      tbbmid=HIWORD(msg.wParam)
      c = @_vr_toolbar_buttons[tbbid]
      if c then
        c._vr_cmdhandlers[tbbmid].each{|shandler|
          args=msgarg2handlerarg(shandler[1],msg,shandler[2])
          c.__send__(shandler[0],*args) if c.respond_to?(shandler[0])
          msg.retval = controlmsg_dispatching(c,shandler[0],*args)
        } if c._vr_cmdhandlers and c._vr_cmdhandlers[tbbmid]
      end
    end
    self_wmcommand_org(msg)
  end

  def registerToolbarButton(c,name,id)
    @_vr_toolbar_buttons = {} unless @_vr_toolbar_buttons
    c.etc= id
    c.name=name
    @_vr_toolbar_buttons[id]=c
  end

end

class VRToolbar < VRNotifyControl
=begin
== VRToolbar
This class represents Toolbar.

=== Methods
--- insertButton(i,name,style=TBSTYLE_BUTTON)
    Inserts a button as ((|i|))-th button. ((|style|)) can be
    a constant in WConst such as TBSTYLE_BUTTON (default),
    TBSTYLE_SEP (separator), TBSTYLE_CHECK,...
--- addButton(style)
    Adds a button at last of the buttons.
--- deleteButton(i)
    Deletes a button at ((|i|))-th.
--- clearButtons
    Deletes all buttons.
--- countButtons
    Counts buttons.
--- setImagelist(imglist)
    Sets the imagelist for the toolbar. ((|imglist|)) must be an instance of
    SWin::Imagelist.
--- setParent(hwnd)
    Sets the window to nofify command messages.
--- autoSize
    Resizes toolbar.
--- indeterminateOf(i,bool=true)
    Sets the indeterminate state of the ((|i|))-th button.
--- commandToIndex(id)
    Retrieves the index number for the button whose id is ((|id|)).
    This id is used in WM_COMMAND messages.
--- enableButton(i,bool=true)
    Enables or disables the ((|i|))-th button.
--- getButtonStateOf(id)
    Gets the state of the button whose id is ((|id|)).
--- setButtonStateOf(i,state)
    Sets the state of the button whose id is ((|id|)).
--- setButtons(buttons)
    Sets the array of ((|button|)) to a tool bar at once.
    ((|button|)) must be an array of ((|[name,style]|))
--- enumButtons
    Yields all toolbar buttons which are instance of
    VRToolbar::VRToolbarButton.
=== Event Handler
--- ????_clicked
    Fired when the button clicked.

== VRToolbar::VRToolbarButton
This class is for each toolbar buttons. This resemble menus in using it.

=== Methods
--- state
    Returns the state of the button.
--- checked?
    Returns true if the button is checked/pushed.
=end

  include WConst

  class VRToolbarButton
    attr_accessor :name, :etc
    attr_reader :index, :toolbar

    def _vr_cmdhandlers
    {0=>[["clicked",MSGTYPE::ARGNONE,nil]]}
    end

    def initialize(i,toolbar)
      @index,@toolbar = i,toolbar
    end

    def state
      @toolbar.getButtonStateOf(@etc)
    end

    def checked?
      (state&1)==1
    end

  end

  def VRToolbar.Controltype() ["ToolbarWindow32",0] end

  def vrinit
    super
    sendMessage WMsg::TB_BUTTONSTRUCTSIZE ,20,0
    if defined?(VRTooltip) and defined?(@parent.tooltip) and @parent.tooltip then
      sendMessage WMsg::TB_SETTOOLTIPS,@parent.tooltip.hWnd,0
    end
  end

  def setButtonText(i,text)
    if text.length>0 then
      iid = indexToCommand(i)
      tbi = [4*8,WConst::TBIF_TEXT,iid,0,0,0,text,text.length].pack("llllilpl")
      sendMessage WMsg::TB_SETBUTTONINFO,i,tbi
    end
  end

  def insertButton(i,name,tbStyle=TBSTYLE_BUTTON)
    @_vr_tbbuttons = -1 unless defined?(@_vr_tbbuttons)
    unless tbStyle == TBSTYLE_SEP
      @_vr_tbbuttons += 1
      iBitmap = @_vr_tbbuttons
    else
      iBitmap = 0
    end
    id = @parent.newControlID
    tbb=@screen.application.arg2cstructStr(WStruct::TB_BUTTON,
                                           iBitmap, id, 4, tbStyle, 0, 0, 0, 0)
    sendMessage WMsg::TB_INSERTBUTTON,i,tbb
    i = commandToIndex(id)
    r=VRToolbarButton.new(i,self)
    if @parent.respond_to?(:registerToolbarButton)
      @parent.registerToolbarButton(r,name,id)
    else
      @parent.registerControl(r,name,id)
    end
    r
  end

  def addButton(name,tbStyle=TBSTYLE_BUTTON)
    insertButton(0xffff,name,tbStyle)
  end

  def deleteButton(i)
    sendMessage WMsg::TB_DELETEBUTTON,i,0
  end

  def countButtons
    r=sendMessage WMsg::TB_BUTTONCOUNT,0,0
  end

  def clearButtons
    (countButtons-1).downto(0) do |i|
    deleteButton(i)
    refresh
    end
  end

  def setParent(win)
    hwnd = if win.is_a?(SWin::Window) then win.hwnd else win end
    sendMessage WMsg::TB_SETPARENT,hwnd,0
  end

  def autoSize
    sendMessage WMsg::TB_AUTOSIZE ,0,0
    refresh
  end

  def indeterminateOf(i,bool=true)
    sendMessage WMsg::TB_INDETERMINATE,i,(if bool then -1 else 0 end)
  end

  def setImagelist(imagelist)
    raise "not Imagelist" unless imagelist.is_a?(SWin::Imagelist)
    self.properties["imagelist"]=imagelist
    sendMessage WMsg::TB_SETIMAGELIST,0,imagelist.himagelist
    refresh
  end

  def commandToIndex(id)
    sendMessage WMsg::TB_COMMANDTOINDEX,id,0
  end

  def indexToCommand(i)
    getButton(i)[1]
  end

  def checked?(i)
    (getButtonStateOf(i) & 1)==1
  end

  def setButtonStateOf(id,state)
    sendMessage WMsg::TB_SETSTATE,id,state
  end

  def getButtonStateOf(id)
    sendMessage WMsg::TB_GETSTATE,id,0
  end

  def enableButton(id,bool=true)
    sendMessage WMsg::TB_ENABLEBUTTON,id,(if bool then 1 else 0 end)
  end

  def getButton(i)
    tbb=@screen.application.arg2cstructStr(WStruct::TB_BUTTON,
                                            0,0,0,0,0,0,0,0)
    sendMessage WMsg::TB_GETBUTTON ,i ,tbb
    @screen.application.unpack(tbb,WStruct::TB_BUTTON)
  end

  def setButtons(a)
    a.each{|i| addButton(i[0], (i[1] ? i[1] : TBSTYLE_BUTTON))}
  end

  def enumButtons
    raise "Use VRToolbarUseable" unless parent.respond_to? :_vr_toolbar_buttons
    n = countButtons
    raise "unknown error" unless n == parent._vr_toolbar_buttons.size
    parent._vr_toolbar_buttons.each{|key,val|
      yield val
    }
  end

end
