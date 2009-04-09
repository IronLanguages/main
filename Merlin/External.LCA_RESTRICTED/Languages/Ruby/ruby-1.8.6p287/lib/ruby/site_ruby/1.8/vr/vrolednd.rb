###################################
#
# vrolednd.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2002-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'sysmod'
require VR_DIR+'dragdropformat'

module OleDnDConstants
  IDROPTARGET_NOTIFY_DRAGENTER=0
  IDROPTARGET_NOTIFY_DRAGOVER=1
  IDROPTARGET_NOTIFY_DRAGLEAVE=2
  IDROPTARGET_NOTIFY_DROP=3

  DROPEFFECT_NONE=0
  DROPEFFECT_COPY=1
  DROPEFFECT_MOVE=2
  DROPEFFECT_LINK=4
end


module VROleDropTarget
=begin
== VROleDropTarget
   A module for OLE drop target.
   Include this module to make a window as an ole drop target.
=== Instance Variables
--- vr_oledropmessage
    Message ID to notify OLE drag-and-drop matter.
=== Methods
--- start_oledroptarget(formats)
    Starts the window as OLE drop target.
    ((|formats|)) is an array of acceptable formats (CF_TEXT, and so on).

=== Event handlers
--- self_oledragenter(format,keystate)
    This notifies entering OLE drag on the window.
    This method must return ((|Effects|)). The default effect is DROPEFFECT_COPY.
--- self_oledragover(keystate)
    This notifies OLE dragging cursor is moving on the window.
    This method must return ((|Effects|)) or ((|nil|)). 
    ((|nil|)) means "same as before". The default effect is ((|nil|)).
--- self_oledragleave
    This notifies leaving OLE drag cursor.
--- self_oledrop(handle,format,keystate)
    This notifies the item is dropped. Global heap handle of ((|handle|)) 
    containing the contents of dropped object. 
    When the object is multi-formattedformat, it's in the first format that 
    appears in the argument array of start_oledroptarget() .
=end

  include ClipboardFormat
  include OleDnDConstants
  include VRUserMessageUseable

  IDROPTARGET_NOTIFY = "IIIIII"  #*ppt,dwEffect,grfKeyState,cfFormat,hMem,*data

  def vrinit
    super
    oledroptargetinit
  end

  def oledroptargetinit
    @vr_oledropmessage = registerUserMessage(ReservedMsg::WM_VR_OLEDND,"vr_olednd",0)
    addEvent @vr_oledropmessage
  end

  def start_oledroptarget(formats)
    self.dndTargetStart( @vr_oledropmessage ,formats )
  end

  def self_vr_olednd(wparam,lparam)
    notify = @screen.application.cstruct2array(lparam,IDROPTARGET_NOTIFY)
    @vr_olednd_notify=notify
    case wparam
    when IDROPTARGET_NOTIFY_DRAGENTER
        if notify[3]==0 then
          @screen.application.pokeMemory(lparam+4,0,4) # effect none
          return
        end
        r = selfmsg_dispatching("oledragenter",notify[3],notify[2]).to_i
        @screen.application.pokeMemory(lparam+4,r,4) # effect r
    when IDROPTARGET_NOTIFY_DRAGOVER
        r = selfmsg_dispatching("oledragover",notify[2]) 
        if r then
          r = r.to_i
          @screen.application.pokeMemory(lparam+4,r,4) # effect r
        end
    when IDROPTARGET_NOTIFY_DRAGLEAVE
        selfmsg_dispatching("oledragleave")
    when IDROPTARGET_NOTIFY_DROP
        return if notify[3]==0
#        selfmsg_dispatching("oledropraw",dataobj,notify[2])
        selfmsg_dispatching("oledrop",notify[4],notify[3],notify[2])
    end
    @vr_olednd_notify=nil
  end

  def self_oledragenter(format,keystate)
    DROPEFFECT_COPY
  end

  def self_oledragover(keystate)
    nil
  end

  def self_oledragleave
  end

  def self_oledrop(handle,format,keystate)
  end
end


module VROleDragSourceLow
=begin
== VROleDragSourceLow
   A module for OLE drag source. (low level)

=== Methods
--- start_oledragsource(formats,effect= DROPEFFECT_COPY|DROPEFFECT_MOVE)
    Starts OLE Drag Drop. The dragged object is in the formats of ((|formats|)).
    Acceptable effects is to be set for ((|effect|)).

=== Event handlers
--- self_getoledragitem(format)
    Fired when DropTarget requires the dragged data in the ((|format|))
    This method must return the Global heap handle containing the dragged object.
    If the handle is not available, return 0.
=end

  include ClipboardFormat
  include OleDnDConstants
  include VRUserMessageUseable
  
  def vrinit
    super
    oledragsourcelowinit
  end
  
  def oledragsourcelowinit
    @vr_oledragmessage = registerUserMessage(ReservedMsg::WM_VR_OLEDND,"vr_oledrag",0)
    addEvent @vr_oledragmessage
  end

  def set_dragobj_lparam(lparam,hMem)
    @screen.application.pokeMemory(lparam,hMem,4)
  end

  def start_oledragsource(formats,effect=0x3)
    dndSourceStart @vr_oledragmessage,formats,effect
  end

  def self_vr_oledrag(wparam,lparam)
    handle = selfmsg_dispatching("getoledragitem",wparam).to_i
    set_dragobj_lparam(lparam,handle)
  end

  def self_getoledragitem(format)
    0
  end

end

module VROleDragSource
  include VROleDragSourceLow

  def start_oledrag(objs,effect=0x3)
    @_vr_draghash={}
    formats = []
    objs.each do |o| 
      formats.push o.objectformat 
      @_vr_draghash[o.objectformat] = o
    end
    start_oledragsource(formats,effect)
  end

  def self_getoledragitem(format)
    return 0 unless @_vr_draghash

    ddobj = @_vr_draghash[format]

    if ddobj then ddobj.handle else 0 end
  end

end
