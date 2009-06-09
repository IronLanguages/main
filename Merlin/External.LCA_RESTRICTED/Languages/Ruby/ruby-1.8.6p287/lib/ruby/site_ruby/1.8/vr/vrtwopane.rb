###################################
#
# vrtwopane.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require VR_DIR+'vrhandler'
require 'Win32API'

=begin
= VisualuRuby(tmp) Multi-pane Window
=end


module VRTwoPane
=begin
== VRTwoPane
This module is a base module for VRVertTwoPane and VRHorizTwoPane.
The meanings of 'Upper' and 'Lower' depend on those modules.
=== Methods
--- addPanedControl(type,name,caption,astyle)
    Adds a new control. This can be called only twice. The first call adds
    an upper/left control and the next call adds lower/right control.
    The third call causes RuntimeError.

--- addControlUpper(type,name,caption,astyle)
    ((*obsolete*)).
    Adds new control on the window upperside or leftside.
    The arguments are the same of VRParent#addControl.
--- addControlLower(type,name,caption,astyle)
    ((*obsolete*)).
    Adds new control on the window downside or rightside.
    The arguments are the same of VRParent#addControl.
=end

#  include VRMouseFeasible
  include VRParent

  attr :ratio,1
  attr :separatorheight,1
  attr :pane_1
  attr :pane_2

  SPLITTER_MOVEWINDOW=0
  SPLITTER_DRAWLINE=1
  PatBlt = Win32API.new("gdi32","PatBlt","IIIIII","I")

  def twopaneinit
    @_vr_paned_splitter_movingmethod = SPLITTER_DRAWLINE
    @_vr_dragging=false
    @_vr_splitter_last
    @pane_1 = @pane_2 = nil
    @ratio=0.5
    @separatorheight=6
    @_vr_app=@screen.application
    @_vr_acmethod=self.method(:addControlUpper) unless defined? @_vr_acmethod
    addHandler WMsg::WM_LBUTTONUP,  "vrseplbuttonup",  MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_LBUTTONDOWN,"vrseplbuttondown",MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_SIZE,       "vrpaneresize",    MSGTYPE::ARGLINTINT,nil
    addHandler WMsg::WM_MOUSEMOVE,  "vrsepmousemove",  MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_KILLFOCUS,  "vrseplkillfocus",  MSGTYPE::ARGNONE,nil
    acceptEvents [
      WMsg::WM_SIZE,WMsg::WM_LBUTTONUP,WMsg::WM_LBUTTONDOWN,WMsg::WM_MOUSEMOVE,WMsg::WM_KILLFOCUS
     ]
  end
  def vrinit
    super
    twopaneinit
  end

  def addControlUpper(type,name,caption,style=0)
    @pane_1=addControl(type,name,caption,0,0,10,10,style)
    @_vr_acmethod=self.method(:addControlLower)
  end
  def addControlLower(type,name,caption,style=0)
    @pane_2=addControl(type,name,caption,0,20,10,10,style)
    @_vr_acmethod=self.method(:addControlIllegal)
  end
  def addControlIllegal(*arg)
    raise "added more than two child windows."
  end

  def addPanedControl(*args)
    @_vr_acmethod.call(*args)
  end

  def self_vrseplbuttondown(shift,x,y)
    setCapture
    @_vr_dragging=true
    @_vr_app.setCursor @_vr_dragcur
    x,y,@_vr_w,@_vr_h = self.clientrect   #attr w,h is the size when drag starts.
  end

  def self_vrseplbuttonup(shift,x,y)
    releaseCapture
    @_vr_dragging=false
    splitterDragEnd(x,y)
  end

  def self_vrsepmousemove(shift,x,y)
    @_vr_app.setCursor @_vr_dragcur
    return unless @_vr_dragging
    splitterDragging(x,y)
  end

  def self_vrseplkillfocus()
    if @_vr_dragging then
      releaseCapture
      @_vr_dragging=false
      @pane_1.refresh
      @pane_2.refresh
      @_vr_splitter_last = nil
    end
  end


  def self_vrpaneresize(w,h)
    return unless @pane_1 and @pane_2
    x,y,w,h = self.clientrect
    resizepane(x,y,w,h,@ratio)
  end

  def splitterDragEnd(*arg) end
  def splitterDragging(*arg) end
  def resizepane(*arg) end
end
module VRVertTwoPane
=begin
== VRVertTwoPane
This module is a kind of VRTwoPane to separate the window vertically 
and places the controls in the separeted area.
=end

  include VRTwoPane

  def vrinit
    super 
    @_vr_dragcur = @_vr_app::SysCursors::SizeNS()
  end

  def splitterDragEnd(x,y)
    sh=(@separatorheight/2).to_i
    @_vr_splitter_last=nil
    @pane_1.move 0,0,@_vr_w,y-sh
    @pane_2.move 0,y+sh,@_vr_w,@_vr_h-y-sh
    @ratio=y.to_f/@_vr_h
    refresh
  end

  def splitterDragging(x,y)
    sh=(@separatorheight/2).to_i
    return if y+sh>@_vr_h || y<0
    case(@_vr_paned_splitter_movingmethod)
    when SPLITTER_MOVEWINDOW
      @pane_1.move 0,0,@_vr_w,y-sh
      @pane_2.move 0,y+sh,@_vr_w,@_vr_h-y-sh
    when SPLITTER_DRAWLINE
      dopaint do |hdc|
        setBrush(RGB(0x255,0x255,0x255))
        if @_vr_splitter_last then
          PatBlt.call(hdc,*@_vr_splitter_last) 
        end
        current=[0,y,@_vr_w,@separatorheight,0x5a0049] # PATINVERT
        PatBlt.call(hdc,*current)
        @_vr_splitter_last = current
      end
    end
  end

  def resizepane(x,y,w,h,ratio)
    ys = (h*@ratio).to_i
    sh=(@separatorheight/2).to_i
    @pane_1.move 0,0,w,ys-sh
    @pane_2.move 0,ys+sh,w,h-ys-sh
  end
end

module VRHorizTwoPane
=begin
== VRHorizTwoPane
This module is a kind of VRTwoPane to separate the window horizontally 
and places the controls in the separeted area.
=end
  include VRTwoPane

  def vrinit
    super
    @_vr_dragcur = @_vr_app::SysCursors::SizeWE()
  end

  def splitterDragEnd(x,y)
    sh=(@separatorheight/2).to_i
    @_vr_splitter_last=nil
    @ratio=x.to_f/@_vr_w
    @pane_1.move 0,0,x-sh,@_vr_h
    @pane_2.move x+sh,0,@_vr_w-x-sh,@_vr_h
    refresh
  end

  def splitterDragging(x,y)
    sh=(@separatorheight/2).to_i
    return if x+sh>@_vr_w || x<0
    case(@_vr_paned_splitter_movingmethod)
    when SPLITTER_MOVEWINDOW
      @pane_1.move 0,0,x-sh,@_vr_h
      @pane_2.move x+sh,0,@_vr_w-x-sh,@_vr_h
    when SPLITTER_DRAWLINE
      dopaint do |hdc|
        setBrush(RGB(0x255,0x255,0x255))
        if @_vr_splitter_last then
          PatBlt.call(hdc,*@_vr_splitter_last) 
        end
        current=[x,0,@separatorheight,@_vr_h,0x5a0049] # PATINVERT
        PatBlt.call(hdc,*current)
        @_vr_splitter_last = current
      end
    end


  end

  def resizepane(x,y,w,h,ratio)
    xs = (w*ratio).to_i
    sh=(@separatorheight/2).to_i
    return if x+sh>w || x<0
    @pane_1.move 0,0,xs-sh,h
    @pane_2.move xs+sh,0,w-xs-sh,h
  end
end

