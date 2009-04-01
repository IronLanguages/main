###################################
#
# vrdialog.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

=begin
= VisualuRuby(tmp) Dialog boxes
=end


VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require VR_DIR+'vrcontrol'

class VRDialogTemplate
=begin
== VRDialogTemplate
Create Dialog template string for the argument of 
DialogBoxIndirectParam() Win32API.

=== Attributes
((|style|)),((|exstyle|)),((|caption|)),((|fontsize|)),((|fontname|)) are 
read/write accessible.
((|x|)),((|y|)),((|w|)),((|h|)) are read-only and access these attributes
with 'move' method.

=== Methods
--- move(x,y,w,h)
    Sets the dialog dimension and position. The position is relative from 
    parent window.
--- addDlgControl(ctype,caption,x,y,w,h,style=0)
    Adds a control on the dialog. ((|ctype|)) is the control-class such as 
    VRButton.
--- to_template
    Create a dialog template string and return it.
=end

  attr_accessor :style, :exstyle
  attr_reader :x, :y, :w, :h
  attr_accessor :caption, :fontsize, :fontname

  require 'Win32API'
  MultiByteToWideChar = 
     Win32API.new("kernel32","MultiByteToWideChar",["I","I","P","I","P","I"],"I")

 private
  def padding_dwordAlignment(str)
    a=str.length
    c = ( (a+3)&0xfffc ) - a  # Don't use String whose length is over 0xfffc :)
    str << " "*c
  end

  def mb2wc(str)
    r=" "*(str.length*2)
    l=MultiByteToWideChar.call(0,0,str,str.length,r,r.length)
    r[0,l*2]
  end

  def class2param(cls)
    if cls == VRButton then 
      [0xffff,0x80].pack("SS")
    elsif cls == VREdit || cls == VRText
      [0xffff,0x81].pack("SS")
    elsif cls == VRStatic
      [0xffff,0x82].pack("SS")
    elsif cls == VRListbox
      [0xffff,0x83].pack("SS")
    elsif cls == VRScrollbar
      [0xffff,0x84].pack("SS")
    elsif cls == VRCombobox
      [0xffff,0x85].pack("SS")
    else
      mb2wc(cls.Controltype[0]+"\0")
    end
  end

  def tmplateinit
    @_vr_dcontrols={}; @_vr_cid=0; 
    @style = 0x90c800c0
    @exstyle=0
    self.move 100,100,200,100
    @caption=""
    @fontname="system"
    @fontsize=10
  end

  def initialize
    tmplateinit
  end

 public 
  def move(x,y,w,h)
    @x,@y,@w,@h = x,y,w,h
  end
  
  def addDlgControl(ctype,caption,x,y,w,h,style=0)
    newid = @_vr_cid + $VRCONTROL_STARTID*2
    @_vr_dcontrols[newid] = 
            [ctype,"",caption,x,y,w,h,
             ctype.Controltype[1] | style | 0x50000000]  #WS_VISIBLECHILD
    @_vr_cid+=1
    return newid
  end
  
  
  def to_template
    tmp = 
      [
        @style,@exstyle,@_vr_dcontrols.size,@x,@y,@w,@h,0,0
      ] .pack("LISSSSSSS") + mb2wc(@caption+"\0")

    if (@style & 0x40)>0 then # DS_SETFONT
      tmp << [@fontsize].pack("S") << mb2wc(@fontname+"\0")
    end
    padding_dwordAlignment(tmp)

    @_vr_dcontrols.each do |iid,val|
      tmp << [val[7],0,val[3,4],iid].flatten.pack("IISSSSS")
      tmp << class2param(val[0])
      tmp << mb2wc(val[2]+"\0") << [0].pack("S")
      padding_dwordAlignment(tmp)
    end
    return tmp
  end
end


module WMsg
  WM_INITDIALOG = 0x110
end

class VRDialogComponent < SWin::Dialog
=begin
== VRDialogComponent
This class represents modal/modeless dialogs.
--- setButtonAs(button,dlgbuttonid)
    Set the button as dialog's functional button like IDOK, IDCANCEL and so on.
    ((|button|)) must be a VRButton and dlgbuttonid must be between IDOK and IDHELP
--- centering
    set the dialog at the center of the parent window.
=end

  include VRParent

  IDOK             = 1
  IDCANCEL         = 2
  IDABORT          = 3
  IDRETRY          = 4
  IDIGNORE         = 5
  IDYES            = 6
  IDNO             = 7
  IDCLOSE          = 8
  IDHELP = DLGMAX_ID  = 9

  attr_accessor :options  #hash

  def vrinit
    extend VRWinComponent::VRInitBlocker
  end

  def setscreen(scr)
    @screen=scr
  end
  def create(*arg)
    self.open(*arg)
  end

  def setButtonAs(control, newid) #contributed by Katonbo-san.
    if newid > DLGMAX_ID then
      raise "id[#{newid}] is too big"
    end
    id = control.etc
    @controls[newid] = @controls[id]
#    control.etc = newid
#    @controls.delete(id)
  end

  def centering(target = @parent) #contributed by Yuya-san./modified by nyasu.
    unless target.is_a?(SWin::Window) then
      target = @screen
    end
    x0,y0,w0,h0 = self.windowrect
    if target == @screen then
      x1=target.x; y1=target.y; w1=target.w; h1=target.h
    else
      x1,y1,w1,h1 = target.windowrect
    end
    x = x1 + w1 / 2 - w0 / 2
    y = y1 + h1 / 2 - h0 / 2
    self.move(x, y, w0, h0)
  end

  def initialize(*arg)
    @options={}
  end

  def self.new(screen,template)   # obsolete
    r=screen.factory.newdialog(template.to_template,self)
    r.parentinit(screen)
    r.options={}
    r.addEvent WMsg::WM_INITDIALOG 
    r.addEvent WMsg::WM_COMMAND
    return r
  end

  def self.new2(screen)   # obsolete
    template = VRDialogTemplate.new
    opt={}
    yield(opt,template)
    r=self.new(screen,template)
    r.options=opt if opt.is_a?(Hash)
    return r
  end

  module VRInitDialogHandler
    def vrinit
      if self.kind_of?(VRMessageHandler) then
        addHandler WMsg::WM_INITDIALOG,"initdialog",MSGTYPE::ARGINTINT,nil
      end
      super
    end

    def msghandler(msg)
      if msg.msg==WMsg::WM_INITDIALOG then
        self.vrinit
        self.construct if self.respond_to?(:construct)
	self.self_created if self.respond_to?(:self_created)
      end
      super
    end
  end

  def open(parent=@parent,modal=true)
    @parent=parent
    super parent,modal
  end
end


class VRModalDialog <VRDialogComponent
=begin
== VRModalDialog
This class represents a modal dialog.
When a modal dialog opens, the sequence is blocked until the dialog is closed.
(Process about the dialog is advancing.)
VRModalDialog#open method returns the value which is the argument 
at VRModalDialog#close(value). <- SWin::Dialog#close

=== Attributes
Hash of ((|options|)) set values as 'options["cancelbutton"]=canbtn'

=== Methods
--- close(value)
    See SWin::Dialog#close
=end

  include VRParent

end

class VRModelessDialog <VRDialogComponent
=begin
== VRModelessDialog
This class represents a modeless dialog.
VRModelessDialog dialog is resemble like VRForm except at creating.
The modeless dialogs have advantages enabling TAB_STOP control focusing.

=== Attributes
Hash of ((|options|)) set values as 'options["cancelbutton"]=canbtn'

=== Methods
--- open(parent=@parent)
    See SWin::Dialog#open.
--- close(value)
    See SWin::Dialog#close
=end

  include VRParent

  def open(parent=@parent,ignored_modal=false)
    super parent,false
  end
end



class VRScreen
=begin
== VRScreen
A method is added to VRScreen by loading this file.

=== Method
--- openModalDialog(parent,style=nil,mod=VRDialogComponent,template=PlaneDialogTemplate,options={})
    Creates and opens a modal dialog box. 
    This method is blocked until the dialog box closed. 
    GUI definition can be specified by ((|template|)) or mod#construct.
    When mod==nil, then this method use VRDialogComponent instead of nil.
    The return value is dialog's return value set by SWin::Dialog#close.
    ((|options|)) specifies the focused control, ok button, cancel button, etc.
--- openModelessDialog(parent,style=nil,mod=VRDialogComponent,template=PlaneDialogTemplate,options={})
    Creates and opens a modeless dialog box.
    GUI definition can be specified by ((|template|)) or mod#construct.
    When mod==nil, then this method use VRDialogComponent instead of nil.
    The return value is false and this method returns immediately.(non-blocking)
    ((|options|)) specifies the focused control, ok button, cancel button, etc.
    (see VRInputbox)

--- newdialog(parent,style=nil,mod=VRDialogComponent,template=PlaneDialogTemplate,options={})
    Creates a dialogbox whose parent is ((|parent|)), and returns it.
    To open that dialogbox, call "open" method. 
    This method is called by openModalDialog() and openModelessDialog() .
    ((|mod|)) may be a module or a class which is a descendant of 
    VRDialogComponent.
=end

  PlaneDialogTemplate = VRDialogTemplate.new.to_template

  def newdialog(parent,style=nil,mod=VRDialogComponent,*template_arg)
    template,options = *template_arg
    template = PlaneDialogTemplate unless template
    options = {} unless options
    if mod.is_a?(Class) and mod.ancestors.index(VRDialogComponent) then
      frm=@factory.newdialog(template,mod)
    elsif mod.is_a?(Class) then
      raise "#{mod.class} is not a descendant of VRDialogComponent"
    elsif mod.is_a?(Module) then
      frm=@factory.newdialog(template,VRDialogComponent)
      frm.extend VRParent
      frm.extend mod
    else
      raise ArgumentError,"a Class/Module of VRDialogComponent required"
    end
    frm.parentinit(self)
    frm.addEvent WMsg::WM_INITDIALOG
    frm.extend(VRStdControlContainer)
    frm.style=style if style
    frm.extend(VRDialogComponent::VRInitDialogHandler)
    frm.options.update(options)
    frm.instance_eval("@parent=parent")
    frm
  end

  def openModalDialog(parent,style=nil,mod=VRModalDialog,*template_arg)
    mod = VRModalDialog unless mod
    frm = newdialog(parent,style,mod,*template_arg)
    a = frm.open(parent,true)
  end

  def openModelessDialog(parent,style=nil,mod=VRModelessDialog,*template_arg)
    mod = VRModelessDialog unless mod
    frm = newdialog(parent,style,mod,*template_arg)
    frm.open parent,false
    @_vr_box.push frm
    frm
  end

  alias modalform :openModalDialog
  alias modelessform :openModelessDialog
end

module VRInputboxDialog
=begin
== VRInputboxDialog
Abstract module of Inputbox.
Ok button, Cancel button and input area are have to be added by addDlgControl.
After creating them, set the options "okbutton","cancelbutton","target" and
"default".
=end

  include VRParent
  include VRStdControlContainer

  def vrinit
    super
    target = @options["target"]
  end

  def msghandler(msg)
    if msg.msg == WMsg::WM_INITDIALOG then
      self.setItemTextOf(@options["target"],@options["default"].to_s)
    end

    if msg.msg == WMsg::WM_COMMAND then
      if msg.wParam==@options["okbutton"] || 
         msg.wParam==VRDialogComponent::IDOK then
        close self.getItemTextOf(@options["target"])
      elsif msg.wParam==@options["cancelbutton"]  ||
            msg.wParam==VRDialogComponent::IDCANCEL then
        close false
      end
    end
  end
end

class VRInputbox < VRDialogComponent
  include VRInputboxDialog
end
