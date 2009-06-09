###################################
#
# vruby.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

=begin
= VisualuRuby(tmp)  Main modules and classes
=end


SWIN_REQUIRED = "030817" unless defined?(SWIN_REQUIRED)
VR_COMPATIBILITY_LEVEL=3 unless defined?(VR_COMPATIBILITY_LEVEL)

require 'swin'

if SWin::VERSION < SWIN_REQUIRED then
  raise StandardError,"\nswin.so (#{SWin::VERSION}) version too old. Need #{SWIN_REQUIRED} or later."
end

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'compat/rubycompat'
require VR_DIR+'winconst'
require VR_DIR+'rscutil'

module MSGTYPE
=begin
== MSGTYPE
This is a module to collect constants of Windows message's argument type.
These constants decide an argument list for message handler.
The point is how to use lParam and wParam which are Integers.
=== Constants
--- ARGNONE
    No use of wParam and lParam. The handler don't have arguments.
--- ARGINT
    lParam is the argument of handler.
    The handler is defined as a type of self_handler(lParam)
--- ARGSTRUCT
    lParam is a pointer to structured data. The handler's argument is decided
    by that structure.
--- ARGSTRING
    lParam is a char* pointer. 
    The handler is defined as a type of self_handler(String)
--- ARGWINT
    wParam is for the argument of handler.
    The handler is defined as a type of self_handler(wParam)
--- ARGLINT
    Same as ARGINT
--- ARGINTINT
    lParam and wParam are for the argument.
    The handler is defined as a type of self_handler(wParam,lParam)
--- ARGINTINTINT
    lParam and wParam are for the arguments, and lParam will be devided into 
    two integers of lo-word of lParam and hi-word of lParam.
    The handler is defined as a type of self_handler(wParam,LO_lParam,HI_lParam)
--- ARGLINTINT
    lParam is for the argument, and lParam will be devided into 
    two integers of lo-word of lParam and hi-word of lParam.
    The handler is defined as a type of self_handler(LO_lParam,HI_lParam)
--- ARGINTSTRUCT
    wParam and structured lParam is for the arguments.
    The handler is defined as a type of self_handler(wParam, ...*struct...)
--- ARGINTSINTSINT
    Almost same as ARGINTINTINT.
    But that two lParam integers are dealed as signed integers.
--- ARGPASS
    The argument is an instance of SWin::MSG.
    The handler is define as a type of self_handler(msg).
=end
    
  ARGNONE    = 0    #arg none 
  ARGINT     = 1    #arg lParam
  ARGSTRUCT  = 2    #arg lParam as struct
  ARGSTRING  = 3    #arg lParam as char*

  ARGWINT    = 8    #arg wParam
  ARGLINT    = 9    #arg lParam
  ARGINTINT  = 10   #arg wParam,lParam
  ARGINTINTINT=11   #arg wParam,LO(lParam),HI(lParam)
  ARGLINTINT = 12   #arg LO(lParam),HI(lParam)
  ARGINTSTRUCT=13   #arg wParam, lParam as struct

  ARGINTSINTSINT=14 #arg wParam,SignedLO(lParam),SignedHI(lParam)
  ARGPASS    = 16   #arg msg
end

$VRCONTROL_STARTID=1000

class VRWinComponent < SWin::Window
=begin
== VRWinComponent
Root of all window class.
=== Methods
--- hide
    Hides window (this calls SWin::Window#show(0)
--- winstyle
    Returns an instance of the style utilizing class
--- exwinstyle
    Returns an instance of the exstyle utilizing class
=end

  attr_reader :screen, :parent

  module VRInitBlocker
    def vrinit
      extend VRInitBlocker.dup
    end
  end

  def vrinit
    extend VRWinComponent::VRInitBlocker
  end

  def _init() self.vrinit(); end

  def setscreen(scr)
    @screen=scr
  end
  def create
    @created=true
    super
    self.vrinit
    _vr_call_created
    self
  end

  def _vr_call_created
    self_created if respond_to?("self_created")
  end

  def hide
    self.show 0
  end

  class Flags
    CONSTMOD=WStyle

    def initialize(win)
      @win=win
    end

    def method_missing(msd,*arg)
      msd=msd.to_s
      f_setter = (msd[-1..-1]=="=")
      flgname = if f_setter then msd[0..-2] else msd end.upcase
      flgname.chop! if flgname[-1..-1]=="?"
      mod = self.class::CONSTMOD
      raise "No such flags(#{flgname})" unless mod.const_defined?(flgname)
      flg=mod.const_get(flgname)
      if f_setter then
        setter(flg,arg[0])
      else
        getter(flg)
      end
    end

    def setter(flagint,value)
      f = integer_getter()
      if value then 
        f |= flagint
      else
        f &= ~flagint
      end
      integer_setter(f)
    end

    def getter(flagint)
      (integer_getter() & flagint)==flagint
    end

    def value() integer_getter; end
    def value=(f) integer_setter(f); end
  end

  class WinStyle < Flags
   private
    def integer_getter()
      @win.style
    end
    def integer_setter(f)
      @win.style=f
    end
  end

  class ExWinStyle < Flags
    CONSTMOD=WExStyle
   private
    def integer_getter
      @win.exstyle
    end
    def integer_setter(f)
      @win.exstyle=f
    end
  end


  def winstyle
    WinStyle.new(self)
  end

  def exwinstyle
    ExWinStyle.new(self)
  end
end

module VRMessageHandler
=begin
== VRMessageHandler
This is a module to handle windows messages. 
If you need to receive windows messages , this module is necessary.

=== Methods
--- acceptEvents(ev_array)
    Declares the windows messages to be handled. 
    ((|ev_array|)) is an array of the handled messages.
    This method calls SWin::addEvent for each message.
--- addHandler(msg,handlername,argtype,argparsestr)
    Defines the name and type of the message handler for ((|msg|)).
    The handlername is composed with ((|handlername|)) and the arguments
    are depend on ((|argtype|)) which is a constant in module MSGTYPE.
    When the message come with structured lParam, it is necessary to be defined
    ((|argparsestr|)) to devide it.
=end

  PREHANDLERSTR="self_"

  def msghandlerinit
    @_vr_handlers={} unless defined?(@_vr_handlers)
    self.hookwndproc unless self.hookedwndproc?

              # for VRMessageParentRelayer
    @_vr_msg_norelays=[] unless defined?(@_vr_msg_norelays)
  end

  def addNoRelayMessages(arg)  # for VRMessageParentRelayer
    @_vr_msg_norelays=[] unless defined?(@_vr_msg_norelays)
    @_vr_msg_norelays += arg if arg and  arg.size>0
  end

  def vrinit
    super
    msghandlerinit
  end

  def acceptEvents(ev_array)
    ev_array.each do |ev|
      addEvent ev
    end
  end

  def addHandler(msg,handlername,handlertype,argparsestr)
    @_vr_handlers={} unless defined?(@_vr_handlers)
    @_vr_handlers[msg]=[] unless @_vr_handlers[msg]
    @_vr_handlers[msg].push [
         (PREHANDLERSTR+handlername).intern,handlertype,argparsestr
       ]
  end

  def deleteHandler(msg,handlername)
    return false unless defined?(@_vr_handlers) and @_vr_handlers[msg]
    @_vr_handlers[msg].delete_if do |shandler|
      shandler[0] == (PREHANDLERSTR+handlername).intern
    end
  end

  class SKIP_DEFAULTHANDLER
    attr_accessor :retval
    def initialize(val)
      @retval=val
    end
    def self.[](val=0)
      self.new(val)
    end
  end

 private

  def msgarg2handlerarg(handlertype,msg,parsestr)
    case handlertype
    when MSGTYPE::ARGNONE
      []
    when MSGTYPE::ARGINT
      [msg.lParam]
    when MSGTYPE::ARGSTRUCT
      @screen.application.cstruct2array(msg.lParam,parsestr)
    when MSGTYPE::ARGSTRING
      [@screen.application.pointer2string(msg.lParam)]
    when MSGTYPE::ARGWINT
      [msg.wParam]
    when MSGTYPE::ARGLINT
      [msg.lParam]
    when MSGTYPE::ARGINTINT
      [msg.wParam,msg.lParam]
    when MSGTYPE::ARGINTINTINT
      [msg.wParam,LOWORD(msg.lParam),HIWORD(msg.lParam)]
    when MSGTYPE::ARGLINTINT
      [LOWORD(msg.lParam),HIWORD(msg.lParam)]
    when MSGTYPE::ARGINTSTRUCT
      [msg.wParam, @screen.application.cstruct2array(msg.lParam,parsestr)]
    when MSGTYPE::ARGINTSINTSINT
      [msg.wParam,SIGNEDWORD(LOWORD(msg.lParam)),SIGNEDWORD(HIWORD(msg.lParam))]

    when MSGTYPE::ARGPASS
      [msg]
    else
      raise "Unknown MSGTYPE for #{msg.msg}"
      false
    end
  end


  def msghandler(msg)
    r = nil
    if msg.hWnd==self.hWnd then # FormEvent
      if @_vr_handlers then   # error occurs if the class is not adaptable.
        handlers = @_vr_handlers[msg.msg]
        if handlers then
          handlers.each do |shandler|
            args=msgarg2handlerarg(shandler[1],msg,shandler[2])
            if respond_to?(shandler[0])
              r = __send__(shandler[0],*args) 
            end
          end
        end
      end
    else  # other's WM event
    end

    # fire default handler?
    if SKIP_DEFAULTHANDLER==r then
      1
    elsif  r.is_a?(SKIP_DEFAULTHANDLER) then
      msg.retval=r.retval
    else
      nil
    end
  end

  module VRArrayedComponent
    attr_reader :_vr_arrayednumber
    def _vr_arrayednumber=(num)
      if defined?(@_vr_arrayednumber) then
        raise "set array number twice"
      else
        @_vr_arrayednumber=num
      end
    end
  end

 public
  def controlmsg_dispatching(ct,methodname,*args)
    mthdname = "#{ct.name}_#{methodname}"
    if respond_to?(mthdname)
      if ct.is_a?(VRArrayedComponent) then
        __send__(mthdname,ct._vr_arrayednumber,*args)
      else
        __send__(mthdname,*args) 
      end
    else
      0
    end
  end

  def selfmsg_dispatching(methodname,*args)
    smethod="self_#{methodname}".intern
    if respond_to?(smethod) then
      __send__(smethod,*args) 
    else
      nil
    end
  end

end

module VRMessageParentRelayer
  include VRMessageHandler

  def messageparentrelayerinit
    @_vr_messageparentrelayer=true
  end


  def vrinit
    super
    messageparentrelayerinit
  end

  def controlmsg_dispatching(ct,methodname,*args)
    mthdname = "#{ct.name}_#{methodname}"
    @parent.controlmsg_dispatching(self,mthdname,*args)
  end

  def selfmsg_dispatching(methodname,*args)
    @parent.controlmsg_dispatching(self,methodname,*args)
  end

  def msghandler(msg)   # almost same as VRMessageHandler#msghandler
    if @_vr_msg_norelays.include?(msg.msg) then   # no relays
      return super
    end

    r = nil
    if msg.hWnd==self.hWnd then # FormEvent
      if @_vr_handlers then   # error occurs if the class is not adaptable.
        @_vr_handlers[msg.msg].each do |shandler|
          args=msgarg2handlerarg(shandler[1],msg,shandler[2])
          mthdname = "#{@name}_#{shandler[0]}"
          if @parent.respond_to?(mthdname)
            r = @parent.__send__(mthdname,*args) 
          end
        end
      end
    else  # other's WM event
    end

    # fire default handler?
    if SKIP_DEFAULTHANDLER==r then
      1
    elsif  r.is_a?(SKIP_DEFAULTHANDLER) then
      msg.retval=r.retval
    else
      nil
    end
  end
end


module VRParent
=begin
== VRParent
This module provides the features to be the parent of the child windows.

=== Constants
--- DEFAULT_FONT
    Each control created by addControl method is invoked setFont(font) method.
    Default font of child controls can be set with this constants in the class.

=== Method
--- addControl(ctype,name,caption, x=0,y=0,w=10,h=10, style=0)
    Adds a child window(control) of ((|ctype|)) named ((|name|)) 
    with ((|caption|)) at ( ((|x|)) , ((|y|)) ) whose width and height is
    ( ((|w|)) , ((|h|)) ).
    ((|ctype|)) is not an instance of the control-class but the class of 
    the child window.
    ((|style|)) is additional styles for the childwindow such as WMsg::WS_BORDER.
    You can set nil or "" for ((|name|)). "" is for nameless control, 
    and nil is for the control which is nothing to do with vruby's control 
    management.
--- addArrayedControl(index,ctype,name,caption,x=0,y=0,w=10,h=10,style=0)
    Adds an indexed child window(control) of ((|ctype|)) named ((|name|)).
--- countControls
    Returns the number of controls added on the window.
--- deleteControls(cntl)
    Deletes a control ((|cntl|)) as VRControl.
--- clearControls
    Deletes all controls on the window.
--- send_parent(cname,func)
    Sends to parent an event from control. ((|cname|)) is controlname and 
    ((|func|)) is event handler name.
=== Callback Methods(event handler)
--- construct
    You can add controls and set menues for the window in this method.

=== Event handlers
--- self_created
    Fired when all of child windows is created.
=end

  DEFAULT_FONT=nil

  attr_reader :screen

  def newControlID
    r=@_vr_cid + $VRCONTROL_STARTID
    @_vr_cid+=1
    return r
  end

  def registerControl(c,name,cid)
    c.etc= cid
    if name.is_a?(String) then
      if name.length>0 then
        atname = instance_eval("@" + name + " ||= nil")
        raise "Already used name '#{name}'" unless atname.nil?
        begin
          instance_eval("@"+name+"=c") if name
        rescue
        end
      end
      c.name = name
      @controls[cid]= c
    end
    c
  end

  def registerControlAsArrayed(num,c,name,cid)
    instance_eval("@#{name}=[] unless defined? @#{name}")
    if instance_eval("@#{name}[#{num}]")
      raise "Already used number #{num} for #{name}" 
    end
    begin
     instance_eval("@#{name}")[num]=c
    rescue
    end
    c.name=name
    c.extend VRMessageHandler::VRArrayedComponent
    c._vr_arrayednumber=num
    c
  end

  def _vr_call_created() # to move the timing of self_created
    construct
    super
  end 

  def parentinit(screen)
    @screen=screen
    @controls={}
    @_vr_cid=0
  end

  def createControl(type,name,caption, x=0,y=0,w=10,h=10, style=0)
    c=@screen.factory.newwindow(self,type)
#    c.extend type
    info = type.Controltype
    c.classname= info[0] if info[0]
    c.caption= caption
    c.style=WStyle::WS_VISIBLECHILD | info[1] | style
    c.exstyle = info[2] if info.size>2
    c.move x,y,w,h
    c
  end

  VR_ADDCONTROL_FEWARGS=false
  def addControl(type,name,caption, x=10,y=10,w=10,h=10, style=0)
    c = createControl(type,name,caption, x,y,w,h,style)
    cid=newControlID
    registerControl(c,name,cid) 
    c.parent=self
    c.setscreen(screen)
    c.parentinit(screen) if c.respond_to?("parentinit")
    c.create
    font = self.class::DEFAULT_FONT
    c.setFont(font) if font.is_a?(SWin::Font)
    c
  end

  def addArrayedControl(num,type,name,caption,x=0,y=0,w=10,h=10,style=0)
#p method(:addControl).arity
    if self.class::VR_ADDCONTROL_FEWARGS then
      c = addControl(type,"",caption,style)
    else
      c = addControl(type,"",caption,x,y,w,h,style)
    end
    registerControlAsArrayed(num,c,name,c.etc)
    c
  end

  alias vr_addControlOriginal addControl

  def countControls
    @controls.size
  end

  def deleteControl(cntl)
    if cntl.is_a?(VRMessageHandler::VRArrayedComponent)
      if @controls[cntl.etc]==instance_eval("@#{cntl.name}")[cntl._vr_arrayednumber] then
        instance_eval("@#{cntl.name}")[cntl._vr_arrayednumber]=nil
      end
    else
      if @controls[cntl.etc]==instance_eval("@#{cntl.name}") then
        instance_eval("@#{cntl.name}=nil")
      end
    end

    @controls.delete(cntl.etc)
    cntl.close if cntl.alive?
  end

  def clearControls
    @controls.each do |key,cntl|
      deleteControl(cntl)
    end
    @_vr_cid=0
  end

  def send_parent(cname,func)
    defname=cname+"_"+func
    funcname = self.name + "_" + defname
    evalstr=
      "def "+defname+"(*arg) " <<
        "if parent.respond_to?('"+funcname+"') then " <<
          "parent.__send__('"+funcname+"',*arg) " <<
        "end " <<
      "end"
    instance_eval(evalstr)
  end

# ###########################
#

  def construct
    # placeholder
  end

end

module WMsg
  WM_SETFONT = 0x0030
  WM_GETFONT = 0x0031
end
class VRControl < VRWinComponent
=begin
== VRControl
Base class for controls.

=== Methods
--- add_parentcall(funcname)
    Makes event handlers to be passed through to its parent window.
    For example, add_parentcall("btn1_clicked") provides a new event 
    ((| ????_btn1_clicked |)) on its parent window.
    (???? is the name of the control).
--- call_parenthandler(handlername,*arg)
    Calls event handler of parent window in ctrlname_handlername(*arg) style.
--- setFont(font,redraw=true)
    Sets drawing font to ((|font|)). Control will be re-drawn if redraw flag.

=end

  attr_reader :handlers
  attr_accessor :parent,:name


  WINCLASSINFO = [nil,0]

  def self.Controltype()
    self::WINCLASSINFO
  end

  def add_parentcall(func)
    funcname=@name+"_"+func
    evalstr=
      "def self_"+func+"(*arg) "+
        "if parent.respond_to?('"+funcname+"') then "+
          "parent.__send__('"+funcname+"',*arg) "+
        "end "+
      "end"
    instance_eval(evalstr)
  end

  def setFont(font,redraw=true)
    if self.dopaint then
      super font
    end
    if font.is_a?(SWin::Font) then
      self.properties["font"]=font
      sendMessage WMsg::WM_SETFONT,font.hfont, ( (redraw)? 1 : 0 )
    else
      raise "#{font} is not a font."
    end
  end

  def call_parenthandler(handlername,*arg)
    @parent.controlmsg_dispatching(self,handlername,*arg)
  end
end

module VRCommonDialog
=begin
== VRCommonDialog
Additional module for common dialogs.
You can omit the first argument of the method of SWin::CommonDialog
using this module.

=== Methods
--- openFilenameDialog(*arg)
--- saveFilenameDialog(*arg)
--- chooseColorDialog(*arg)
--- chooseFontDialog(*arg)
--- selectDirectory(*arg)
=end

  def openFilenameDialog(*arg)
    SWin::CommonDialog::openFilename(self, *arg)
  end
  def saveFilenameDialog(*arg)
    SWin::CommonDialog::saveFilename(self, *arg)
  end
  def chooseColorDialog(*arg)
    SWin::CommonDialog::chooseColor(self, *arg)
  end

  DEFAULTFONT = [["System",19,0,300,135,0,0,2,128],135,0]
  def chooseFontDialog(defaultvalue=nil)
    sarg = if defaultvalue.is_a?(FontStruct) then
             sarg = defaultvalue.spec
           elsif defaultvalue
             sarg = defaultvalue
           else
             sarg = DEFAULTFONT
           end
    r=SWin::CommonDialog::chooseFont(self, sarg)
    if r then
      FontStruct.new2(r)
    else
      r
    end
  end

  def selectDirectory(*arg)
    SWin::CommonDialog::selectDirectory self,*arg
  end
end


class VRForm < VRWinComponent
=begin
== VRForm
This class is for top-level window and have the features of 
((<VRMessageHandler>)), ((<VRParent>)) and ((<VRCommonDialog>))

=== Method
--- create
    Creates the window.
=end

  attr_reader :handlers
  VR_WINCLASS = nil

  def self.winclass() nil end

  include VRMessageHandler
  include VRParent
  include VRCommonDialog

#  def create
#    super
#    self.vrinit
#    self
#  end


  def forminit(screen,parent)
    @_vr_handlers={}
    parentinit(screen)
    @parent=parent
  end

end

class VRPanel < VRControl
=begin
== VRPanel
This means only child window that is useable like control.
=end

  include VRParent
  WINCLASSINFO = [nil,0]
end

module VRContainersSet
  INITIALIZERS=[]
  def containers_init
    INITIALIZERS.each do |mtd|
      self.__send__(mtd)
    end
  end
end


module VRDrawable
=begin
== VRDrawable
This module is for handling WM_PAINT message to paint the window.

=== Event handler
--- self_paint
    Fired when the window is required to re-paint by the system.
    You can use GDI's methods of SWin::Window in this method.
=end

  include VRMessageHandler
  def drawableinit
    addHandler(WMsg::WM_PAINT,"paint",MSGTYPE::ARGNONE,nil)
    addEvent WMsg::WM_PAINT
  end
  def vrinit
    super
    drawableinit
  end
end



module VRResizeSensitive
=begin
== VRResizeSensitive
This module is for capturing window resizing.

=== Event handler
--- self_resize(w,h)
    Fired when the window is resized. 
    The new width and height is ((|w|)) and ((|h|)).
=end

  include VRMessageHandler
  
  def resizeableinit
    acceptEvents [ WMsg::WM_SIZE ]
    addHandler(WMsg::WM_SIZE, "resize",MSGTYPE::ARGLINTINT,nil) 
  end
  def vrinit
    super
    resizeableinit
  end
end

VRResizeable = VRResizeSensitive

module VRUserMessageUseable
  module ReservedMsg
    WM_VR_OLEDND      = 1
    WM_VR_TRAYNOTIFY  = 2
  end

=begin
== VRUserMessageUseable
This module is for using user-defined Windows-system messages.

--- registerUserMessage(messageid,eventname,offset=0x100)
    Registers an user-defined message whose eventname is ((|eventname|)).
    On your Windows system, this message is assigned for 
    (WM_APP + messageid + offset).
    Argument ((|offset|)) is only for vruby system use. Don't use it.

=== EventHandler
--- self_<userdefinedname>(wparam,lparam)
    Fired when the user-defined message is sent.
=end

  include VRMessageHandler
  
  def usermessageuseableinit
    @_vr_usermessages={}
  end
  def vrinit
    super
    usermessageuseableinit
  end

  def registerUserMessage(messageid,eventname,offset=0x100)
    msg = WMsg::WM_APP+messageid+offset
    addEvent msg
    addHandler(msg, eventname,MSGTYPE::ARGINTINT,nil) 
    @_vr_usermessages[eventname]=msg
  end

  def userMessage(eventname,wparam=0,lparam=0)
    msg = @_vr_usermessages[eventname]
    raise "No such an usermessage (#{eventname}" unless msg
    postMessage msg,wparam.to_i,lparam.to_i
  end

end

class VRScreen
=begin
== VRScreen
This class expresses the desktop screen.
Currently only ((<VRLocalScreen>)) defined in vruby, the instance of this 
class, is available.

=== Class Method
--- new(app,factory)
    ((|app|)) as SWin::Application and ((|factory|)) as SWin::Factory.

=== Methods
--- newform(parent=nil,style=nil,mod=VRForm)
    Creates and initializes the top-level window which is the instance of
    ((<VRForm>)) or its descendant.
    The parent window is specified by ((|parent|)) and parent==nil means
    that it has no parent window.
    The window style is specified by ((|style|)) which can be ((|nil|)) for
    default style.
    ((|mod|)) can be a module which is to be added to ((<VRForm>)) or a class
    which is a descendant of ((<VRForm>)).
--- showForm(mod,x,y,w,h)
    Creates and shows the new top-level window using ((<newform>)).
    The arguments ((|x|)),((|y|)),((|w|)),((|h|)) is omittable.
    ((|mod|)) is the argument for ((<newform>))'s argument. 
--- addIdleproc(f)
    Adds a idling process executed while message loop runs.
    ((|f|)) is an instance of Proc class.
--- messageloop(wflag=false)
    Get into the system message loop. You need to call this method to
    process windows messages.
    While wflag is true, messageloop waits a message by WaitMessage() API.
    This waiting will prevent other threads' processes and can suppress CPU load
    average.
    If wflag==false and your ruby's version is so high that Thread has 'list' 
    class-method, WaitMessage() API is used automatically by the case.
--- idling_messageloop
    ((*obsolete*))
    Almost same as ((<messageloop>)). The difference is that this method
    yields when the messageloop is in idle state. You need to use this 
    method in iterator's style.
--- width
    Width of the desktop.
--- height
    Height of the desktop.
--- newFormClass(name,brush=nil,style=nil,icon=nil,cursor=nil)
    Register a new window class to the system.
    This method returns a form class with specified icon, color, cursor
    and default window style.
=end

  attr_reader :screen,:application,:factory,:desktop



  attr_accessor :idle_sleep_timer

  WINDOWCHECK_INTERVAL=3

  def initialize(frame,factory)
    @application=frame
    @factory=factory
    @desktop=@application.getDesktop
    @idle_sleep_timer = 0.01
    @_vr_box=[]    # pushed windows for avoiding destruction by GC
  end

  def newform(parent=nil,style=nil,mod=VRForm)   # create top-level window
    if mod.is_a?(Class) then
      if mod.ancestors.index(VRForm) then
        frm=@factory.newwindow(parent,mod)
      else
        raise "#{mod} is not a window class"
      end
    elsif  mod.is_a?(Module) then
      frm=@factory.newwindow(parent,VRForm)
      frm.extend mod
    else
      raise ArgumentError,"required a child class of VRForm or a Module extending VRForm"
    end
    frm.style= style if style
    frm.classname = frm.class.winclass if frm.class.winclass
    frm.extend VRContainersSet
    frm.forminit(self,parent)
    frm
  end

  def showForm(formmodule,*rect)
    if rect.is_a?(Array) and rect.size>3 then
      x,y,w,h = *rect
    end

    frm=newform(nil,nil,formmodule)
    frm.move x,y,w,h if x
    frm.create.show
    @_vr_box.push frm
    frm
  end

  def start(*args)
    showForm(*args)
    messageloop
  end

  def addIdleproc(f)
    @idleprocs=[] unless defined?(@idleprocs)
    @idleprocs.push f
  end

  def messageloop(waitflag=false)
    @idleprocs=[] unless defined?(@idleprocs)

=begin commented out for activex support
    cth =Thread.new do 
      while true do
        @_vr_box.reject! do |w| (! w.alive?);  end
        sleep WINDOWCHECK_INTERVAL
      end
    end
=end

    @application.messageloop do
      n=@idleprocs.shift
      if n then
        Thread.new do
          n.call
        end
      else
        if waitflag then
          @application.waitmessage
        else
#          Thread.pass
           sleep(@idle_sleep_timer)
        end
      end
    end
  end

  def idling_messageloop # obsolete
    @application.messageloop do |q|
      yield q
    end
  end

  def newFormClass(name,brush=nil,style=nil,icon=nil,cursor=nil)
    hicon = case(icon) 
            when Integer 
              icon 
            when SWin::Icon 
              icon.hicon 
            else
              nil
            end

    sw = factory.registerWinClass(name.to_s,brush,style,hicon,cursor)
    raise "register class failed" unless sw
    a = Class.new(VRForm)
    a.class_eval(<<EEOOFF)
      VR_WINCLASS="#{sw}"
      def self.winclass() VR_WINCLASS end
EEOOFF
    a
  end

  def width
    @desktop.w
  end
  def height
    @desktop.h
  end
  alias w :width
  alias h :height
  def x() @desktop.x; end
  def y() @desktop.y; end

end

VRLocalScreen=
  VRScreen.new(SWin::Application,
               SWin::LWFactory.new(SWin::Application.hInstance))

# contributed files
require VR_DIR+'contrib/vrwincomponent'

