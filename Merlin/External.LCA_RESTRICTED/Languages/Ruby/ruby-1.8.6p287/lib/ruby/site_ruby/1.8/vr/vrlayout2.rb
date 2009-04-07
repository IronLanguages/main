###################################
#
# vrlayout2.rb 
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2000-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

require 'Win32API'

class VRLayoutFrame
=begin
== VRLayoutFrame
   This is a base class for layout frames.

=== Methods
--- register(*controls)
    Registers controls for LayoutFrame
--- move(x,y,w,h)
    Resizes the layout frame size.
    This re-calculate the registered controls coordinates.
=end

  def initialize()
    @_vr_layoutclients=[]
    @_vr_lx,@_vr_ly,@_vr_lw,@_vr_lh = 0,0,10,10
  end

  def register(*cntl)
    @_vr_layoutclients.concat cntl
    _vr_relayout
  end

  def move(x,y,w,h)
    @_vr_lx,@_vr_ly,@_vr_lw,@_vr_lh = x,y,w,h
    _vr_relayout
  end

  def _vr_relayout
  end
end


class VRVertLayoutFrame < VRLayoutFrame
=begin
== VRVertLayoutFrame
   This is a frame that lays out its controls vertically.
   Use ((|register|)) and ((|move|)) method of VRLayoutFrame
=end

  def _vr_relayout
    return if @_vr_layoutclients.size==0
    height = @_vr_lh.to_f / @_vr_layoutclients.size
    @_vr_layoutclients.each_index do |i|
      @_vr_layoutclients[i].move @_vr_lx, @_vr_ly+height*i,@_vr_lw,height
    end
  end
end

class VRHorizLayoutFrame < VRLayoutFrame
=begin
== VRHorizLayoutFrame
   This is a frame that lays out its controls horizontally.
   Use ((|register|)) and ((|move|)) method of VRLayoutFrame
=end
  def _vr_relayout
    return if @_vr_layoutclients.size==0
    width = @_vr_lw.to_f / @_vr_layoutclients.size
    @_vr_layoutclients.each_index do |i|
      @_vr_layoutclients[i].move @_vr_lx+width*i, @_vr_ly,width,@_vr_lh
    end
  end
end

class VRFullsizeLayoutFrame < VRVertLayoutFrame
=begin
== VRFullsizeLayoutFrame
   This is a frame that lays out its one control full-sized.
   Use ((|register|)) and ((|move|)) method of VRLayoutFrame
   ((|register|)) method can be called once and accept only one control.
=end

  def register(re)
    super
    def self.register
      raise "register twice"
    end
  end
end

class VRGridLayoutFrame <VRLayoutFrame
  def initialize(xs,ys)
    super()
    @_vr_xsize=xs
    @_vr_ysize=ys
    @_vr_layoutclients = Array.new
  end
  def setDimention(xs,ys)
    @_vr_xsize=xs
    @_vr_ysize=ys
  end

  def register(cntl,x,y,w,h)
    @_vr_layoutclients.push [cntl,x,y,w,h]
    _vr_relayout
  end

  def _vr_relayout
    @_vr_layoutclients.each do |c|
      c[0].move( @_vr_lw * c[1] / @_vr_xsize,
                 @_vr_lh * c[2] / @_vr_ysize,
                 @_vr_lw * c[3] / @_vr_xsize,
                 @_vr_lh * c[4] / @_vr_ysize)
    end
  end
end

class VRTwoPaneFrame
=begin
== VRTwoPaneFrame
   This is a base class for twopane frames.

=== Methods
--- register(*controls)
--- move(x,y,w,h)
    same as ((<VRLayoutFrame>))
--- setup(parent)
    Call this method to combine the frame to the window.
--- window_parent
    Find the first actual window object from its parent or ancestors.
=end
  SPLITTER_MOVEWINDOW=0
  SPLITTER_DRAWLINE=1
  @@_vr_paned_splitter_movingmethod = SPLITTER_DRAWLINE
  
  def splitter_operation_type
     @@_vr_paned_splitter_movingmethod
  end
  
  def splitter_operation_type=(tp)
     @@_vr_paned_splitter_movingmethod=tp
  end

  attr_accessor :ratio,:separatorheight


  Cursor="Size"

  PatBlt = Win32API.new("gdi32","PatBlt","IIIIII","I")

  module VRTwoPaneFrameUsable
    attr_accessor :_vr_twopaneframes

    def _vr_twopaneframesinit
      return if defined?(@_vr_twopaneframes)  # no init twice
      @_vr_twopaneframes = Array.new
      @_vr_current_tpframe=nil
      addHandler(
        WMsg::WM_LBUTTONUP,  "vrsepl2dragend",  MSGTYPE::ARGNONE,nil)
      addHandler(
        WMsg::WM_LBUTTONDOWN,"vrsepl2buttondown",MSGTYPE::ARGINTSINTSINT,nil)
      addHandler(
        WMsg::WM_MOUSEMOVE,  "vrsepl2mousemove",  MSGTYPE::ARGINTSINTSINT,nil)
      addHandler(
        WMsg::WM_KILLFOCUS,  "vrsepl2dragend",  MSGTYPE::ARGNONE,nil)
      acceptEvents [
        WMsg::WM_LBUTTONUP,WMsg::WM_LBUTTONDOWN,WMsg::WM_MOUSEMOVE,
        WMsg::WM_KILLFOCUS
       ]
    end

    def self_vrsepl2buttondown(s,x,y)
      @_vr_twopaneframes.each do |f|
        if f.hittest(x,y)
          f.setDragCur
          f.dragstart 
          @_vr_current_tpframe=f
        end
      end
    end

    def self_vrsepl2dragend
      @_vr_current_tpframe=nil
      @_vr_twopaneframes.each do |f|
        f.dragend
      end
      refresh
    end

    def self_vrsepl2mousemove(s,x,y)
      @_vr_twopaneframes.each do |f|
        if f.hittest(x,y)
          f.setDragCur
        end
      end
      if @_vr_current_tpframe then
        @_vr_current_tpframe.splitterDragging(x,y)
      end
    end
  end

  def window_parent
    a=self
    while(! a.is_a?(SWin::Window) ) do
      a = a.instance_eval("@parent")
    end
    return a
  end

  def initialize(pane1,pane2)
    @pane1,@pane2 = pane1,pane2 
    @_vr_dragging=false
    @ratio=0.5
    @separatorheight=4
    @_vr_splitter_last=nil
  end

  def hittest(x,y)
    return unless @_vr_lxs
    if (@_vr_lxs < x) and (x  < @_vr_lxe) and
       (@_vr_lys < y) and (y  < @_vr_lye) 
    then
      true
    else
      false
    end
  end

  def setDragCur
    @_vr_app.setCursor @_vr_dragcur if @_vr_app
  end

  def dragstart
    @parent.setCapture
    @_vr_dragging=true
  end

  def dragend
    return unless @_vr_dragging
    @parent.releaseCapture
    @_vr_dragging=false
    splitterDragEnd
  end

  def setup(parent)
    raise "setup twice" if defined? @parent
    @parent = parent
    @_vr_app = parent.instance_eval("@screen.application") # parent.application
    @_vr_dragcur = eval("@_vr_app::SysCursors::#{self.class::Cursor}()")
    parent.extend VRTwoPaneFrameUsable
    parent._vr_twopaneframesinit
    parent._vr_twopaneframes.push self
    self
  end

  def move(x,y,w,h)
    @_vr_lx,@_vr_ly,@_vr_lw,@_vr_lh = x,y,w,h
    self._vr_relayout
  end

  def splitterDragging(x,y) end
  def splitterDragEnd(x,y) end
  def _vr_relayout() raise "using base class #{self.class}" end
end

class VRVertTwoPaneFrame < VRTwoPaneFrame
  Cursor = "SizeNS"
  def splitterDragEnd
    @_vr_splitter_last=nil
    _vr_relayout
  end
  def splitterDragging(x,y)
    return if y<@separatorheight+@_vr_ly or y>@_vr_ly+@_vr_lh-@separatorheight
    @ratio=(y-@_vr_ly).to_f/@_vr_lh

    case(splitter_operation_type)
    when SPLITTER_MOVEWINDOW
        _vr_relayout(y-@_vr_ly)
    when SPLITTER_DRAWLINE
      w = window_parent
      w.dopaint do |hdc|
        w.setBrush(RGB(0x255,0x255,0x255))
        if @_vr_splitter_last then
          VRTwoPaneFrame::PatBlt.call(hdc,*@_vr_splitter_last) 
        end
        current=[@_vr_lx,y,@_vr_lw,@separatorheight,0x5a0049] # PATINVERT
        VRTwoPaneFrame::PatBlt.call(hdc,*current)
        @_vr_splitter_last = current
      end
    end
  end

  def _vr_relayout( ys = (@_vr_lh*@ratio).to_i)
    sh=(@separatorheight/2).to_i
    @_vr_lxs,@_vr_lxe = @_vr_lx,@_vr_lx+@_vr_lw
    @_vr_lys,@_vr_lye = @_vr_ly+ys-sh,@_vr_ly+ys+sh

    @pane1.move @_vr_lx,@_vr_ly,  @_vr_lw,ys-sh
    @pane2.move @_vr_lx,@_vr_lys+@separatorheight, @_vr_lw,@_vr_lh-ys-sh
  end
end

class VRHorizTwoPaneFrame < VRTwoPaneFrame
  Cursor = "SizeWE"
  def splitterDragEnd
    @_vr_splitter_last=nil
    _vr_relayout
  end
  def splitterDragging(x,y)
    return if x<@separatorheight+@_vr_lx or x>@_vr_lx+@_vr_lw-@separatorheight
    @ratio=(x-@_vr_lx).to_f/@_vr_lw

    case(splitter_operation_type)
    when SPLITTER_MOVEWINDOW
        _vr_relayout(x-@_vr_lx)
    when SPLITTER_DRAWLINE
      w = window_parent
      w.dopaint do |hdc|
        w.setBrush(RGB(0x255,0x255,0x255))
        if @_vr_splitter_last then
          VRTwoPaneFrame::PatBlt.call(hdc,*@_vr_splitter_last) 
        end
        current=[x,@_vr_ly,@separatorheight,@_vr_lh,0x5a0049] # PATINVERT
        VRTwoPaneFrame::PatBlt.call(hdc,*current)
        @_vr_splitter_last = current
      end
    end


  end

  def _vr_relayout( xs = (@_vr_lw*@ratio).to_i)
    sh=(@separatorheight/2).to_i
    @_vr_lxs,@_vr_lxe = @_vr_lx+xs-sh,@_vr_lx+xs+sh
    @_vr_lys,@_vr_lye = @_vr_ly,@_vr_ly+@_vr_lh

    @pane1.move @_vr_lx,@_vr_ly,  xs-sh,@_vr_lh
    @pane2.move @_vr_lxs+@separatorheight,@_vr_ly, @_vr_lw-xs-sh,@_vr_lh
  end
end
