###################################
#
# vrddrop.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require VR_DIR+'sysmod'
require VR_DIR+'dragdropformat'
require 'Win32API'


module VRDropFileTarget
=begin
== VRDropFileTarget
This module prepares the feature that accepts file dropping.

=== Event handler(s)
--- self_dropfiles(files)
    Argument ((|files|)) is the array of dropped filenames.
=end

  include VRMessageHandler

  DragAcceptFiles = Win32API.new("shell32","DragAcceptFiles",["I","I"],"")
  DragFinish      = Win32API.new("shell32","DragFinish",["I"],"")
  
  def filedropinit
    addHandler(WMsg::WM_DROPFILES,"vrdropfiles",MSGTYPE::ARGWINT,nil)
    addEvent WMsg::WM_DROPFILES
    addNoRelayMessages [WMsg::WM_DROPFILES]
    DragAcceptFiles.call(self.hWnd,1)
  end
  
  def vrinit
    super
    filedropinit
  end

  def self_vrdropfiles(handle)
    r = DragDropFiles.get(handle).files
    DragFinish.call(handle)
    selfmsg_dispatching("dropfiles",r)
#    self_dropfiles(r) if self.respond_to?("self_dropfiles")
  end
end


module VRDragDropSource
=begin
== VRDragDropSource
  This module is a base module for dragdrop source. Note that this module uses
  message posting to realize drag and drop instead of using OLE Drag&Drop.
  To use this module, see VRDragFileSource module.
  This module needs a callback function createDropItem, which is called 
  when the user releases a mouse button to drop.

=== Methods
--- dragDetect()
    Determines by the user's input whether the user is doing dragging or not.
--- dragStart()
    Starts dragging.
--- createDropItem()
    This is a callback method to calculate the dropping item. This needs to
    return three values message,wParam and lParam to send to the dropped window.
    The dropped window will receive the message by Windows messaging service.

=== Attributes
--- dropToplevel
    While this is true, the message is sent to the toplevel window under the 
    cursor, and in the other case the message is sent to the window just 
    under the cursor.

=end

  include VRMessageHandler

  DragDetect = Win32API.new("user32","DragDetect","IP","I")
  GetCursorPos = Win32API.new("user32","GetCursorPos","P","I")
  WindowFromPoint = Win32API.new("user32","WindowFromPoint","II","I")
  GetParent=Win32API.new("user32","GetParent","I","I")
  
  def dragdropsourceinit
    @_vr_dragging=false
    @_vr_droptotoplevel=true;
    @_vr_pointbuffer=[0,0].pack("II")
    unless @_vr_dragging_cursor then
      @_vr_dragging_cursor=@screen.application::SysCursors.Cross 
    end
    addHandler(WMsg::WM_LBUTTONUP,"_vrdsrclbuttonup",MSGTYPE::ARGINTINT,nil)
    acceptEvents [WMsg::WM_LBUTTONUP]
  end

  def vrinit
    super
    dragdropsourceinit
  end

  def dropToplevel() @_vr_droptotoplevel; end
  def dropToplevel=(f) @_vr_droptotoplevel=f;end

  def dragDetect
    DragDetect.call(self.hWnd,@_vr_pointbuffer)!=0
  end

  def dragStart()
    GetCursorPos.call(@_vr_pointbuffer)
    if DragDetect.call(self.hWnd,@_vr_pointbuffer)!=0 then
      @_vr_dragging=true
      @screen.application.setCursor @_vr_dragging_cursor
      setCapture
    end
  end

  def createDropItem()
    # msg,wParam,lParam
    return nil,nil,nil
  end

  def self__vrdsrclbuttonup(shift,xy)
    if @_vr_dragging then
      @screen.application.setCursor @screen.application::SysCursors.Arrow
      GetCursorPos.call(@_vr_pointbuffer)
      handle=WindowFromPoint.call(*@_vr_pointbuffer.unpack("II"))
      releaseCapture

      if @_vr_droptotoplevel then
        while handle!=0 do   # search the top level window
          droptarget=handle; handle=GetParent.call(handle)
        end
      else
        droptarget=handle;
      end

      msg,wParam,lParam = self.createDropItem
      SMSG::postMessage droptarget,msg,wParam,lParam
    end
    @_vr_dragging=false
  end
end

module VRDragFileSource
  include VRDragDropSource
=begin
== VRDragFileSource
This module prepares the feature to start dragging files.
This is the result of quick-hacking. Are you so kind that teach me 
the structure of "internal structure describing the dropped files?"

=== Event handler(s)
--- dragStart(files)
    Starts file dragging with ((|files|)) that is an Array including 
    the filenames to be dragged.
=end

  def dragStart(paths)
    @_vr_dragpaths=paths
    super()
  end

  def createDropItem
    hDrop = DragDropFiles.set(@_vr_dragpaths).handle
    return WMsg::WM_DROPFILES, hDrop, 0
  end
end

=begin VRDragFileSource sample 
require 'vr/vrcontrol'
require 'vr/vrhandler'
require 'vr/vrddrop'

class MyForm < VRForm
  include VRDragFileSource
  include VRMouseFeasible

  def self_lbuttondown(shift,x,y)
    if dragDetect then
      dragStart ['c:\autoexec.bat','c:\config.sys']
    end
  end
end

VRLocalScreen.start(MyForm)
=end

