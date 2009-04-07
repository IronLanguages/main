###################################
#
# vrmgdlayout.rb 
#
# Programmed by  yukimi_sake <yukimi_sake@mbi.nifty.com>
# Copyright 2003-2004 Yukio Sakaue
#
###################################

require 'vr/vrmargin'
require 'vr/vrlayout2'

module VRMarginedFrameUseable
=begin
== VRMarginedFrameUseable
This is a extension of VRParent for enable to use WM_SIZE message handler.
=== Extended methods
--- setMarginedFrame(class,*args)
    Register class on VRParent and setups WM_SIZE message handler.
=end
  
  include VRMessageHandler
  def vrinit
    super
    vrmarginedframe_init
  end
  
  def vrmarginedframe_init
    addHandler(WMsg::WM_SIZE,  "vrmgresize",  MSGTYPE::ARGLINTINT,nil)
    acceptEvents [WMsg::WM_SIZE]
  end
  
  def setMarginedFrame(klass,*args)
    raise "Only one margined frame can set this window." if
                                            @__regsterd_vr_margined_frame
    if klass <= VRTwoPaneFrame
      @__regsterd_vr_margined_frame=klass.new(args[0],args[1]).setup(self)
    elsif klass <= VRGridLayoutFrame
      @__regsterd_vr_margined_frame=klass.new(args[0],args[1])
    else
      @__regsterd_vr_margined_frame=klass.new
    end
    
    def self.self_vrmgresize(w,h)
      @__regsterd_vr_margined_frame.move(0,0,w,h) if
                                                  @__regsterd_vr_margined_frame
    end
    sendMessage(WMsg::WM_SIZE, 0, MAKELPARAM(self.w, self.h))
    @__regsterd_vr_margined_frame
  end
 
  module VRMgdLayoutFrame
=begin
== VRMgdLayoutFrame
This is the extension of VRLayoutFrame.
All classes are extended VRMargin which enables client's spacing.
And following methods are available.
=== Extended method of instance
--- VRGridLayoutFrame#register(*cntl)
    Allowd Arrays as arguments
--- VRMgdFullsizeLayoutFrame#_vr_relayout
    Redefined for that all clients are received same 'move(x,y,w,h)' method.
=end
    
    include VRMargin
    def initialize()
      self.initMargin(0,0,0,0)
      @_vr_layoutclients=[]
      @_vr_lx,@_vr_ly,@_vr_lw,@_vr_lh = 0,0,10,10
    end
    def register(*cntl)
      @_vr_layoutclients.concat(cntl.flatten.map{|i|
        i.extend(VRMargin).initMargin(0,0,0,0)})
      _vr_relayout
    end
  end
    
  class VRMgdVertLayoutFrame < VRVertLayoutFrame 
    include VRMgdLayoutFrame
  end
  
  class VRMgdHorizLayoutFrame < VRHorizLayoutFrame
    include VRMgdLayoutFrame
  end

  class VRMgdFullsizeLayoutFrame < VRFullsizeLayoutFrame  
    include VRMgdLayoutFrame    
    def _vr_relayout
      return if @_vr_layoutclients.size==0
      @_vr_layoutclients.each_index do |i|
        @_vr_layoutclients[i].move @_vr_lx, @_vr_ly,@_vr_lw,@_vr_lh
      end
    end
  end

  class VRMgdGridLayoutFrame < VRGridLayoutFrame
    include VRMargin
    def initialize(xs,ys)
      self.initMargin(0,0,0,0)
      super
    end
    def register(*cntl)
      if cntl[0].is_a? Array
        cntl.each{|i| i[0].extend(VRMargin).initMargin(0,0,0,0); super(*i)}
      else
        cntl[0].extend(VRMargin).initMargin(0,0,0,0)
        super
      end
    end
  end
 
  module VRMgdTwoPaneFrame
=begin
== VRMgdTwoPaneFrame
This is the extension of VRPaneFrame.
All classes are extended VRMargin which enables client's spacing.
And extended class has following methods and constants.

=== Extended methods
---position
---position=
   Position of splitter.
   If it is a positive integer, it means the distance from the left or top edge.
   If negative, it calculates opposite side.
   If this value sets, 'ratio' will be reseted.
   'ratio' and 'position' are exclusive each other.
===R/W attributes
---bevel
   It is the form of the edge of a gap.You can draw it like below.
    bevel=[[0x000000,0xffffff],[0xffffff,0x000000]]
   First one is left/top feature,other is right/botm.
   Drawn line in an order from an outside.
   or
    bevel=VRSplitter::BevelGroove1
   see below constants.
---gap
   space between panes.
===Class constants
---BevelNone
---BevelGroove1
---BevelGroove2
---BevelRaise1
---BevelRaise2
---BevelSunken1
---BevelSunken2
=end
    include VRMargin
    
    BevelNone    = [[],[]]
    BevelGroove1 = [[0x666666,0xffffff],[0xffffff,0x666666]]
    BevelGroove2 = [[0x666666,0xffffff,0x666666,0xffffff],
                    [0xffffff,0x666666,0xffffff,0x666666]]
    BevelRaise1  = [[0xffffff],[0x666666]]
    BevelRaise2  = [[0xffffff,0xffffff],[0x666666,0x666666]]
    BevelSunken1 = [[0x666666],[0xffffff]]
    BevelSunken2 = [[0x666666,0x666666],[0xffffff,0xffffff]]
    
    attr_reader :ratio, :bevel, :position
    
    module VRTwoPaneFrame::VRTwoPaneFrameUsable
      alias :_vr_twopaneframesinit_org :_vr_twopaneframesinit
      
      def _vr_twopaneframesinit
        addHandler(WMsg::WM_PAINT,  "vrsepl2paint",  MSGTYPE::ARGNONE,nil)
        _vr_twopaneframesinit_org
        acceptEvents [WMsg::WM_PAINT]
      end
      
      def self_vrsepl2paint
        @_vr_twopaneframes.each do |f|
          f._vr_draw_bevel(self)
        end
      end
      
    end
    
    def initialize(pane1,pane2)
      def set_margin_to_panes(pn)
        if pn.is_a? Array
          pn.each{|i| i.extend(VRMargin).initMargin(0,0,0,0)}
        elsif pn
          pn.extend(VRMargin).initMargin(0,0,0,0)
        end
      end
      super
      self.initMargin(0,0,0,0)
      set_margin_to_panes(@pane1)
      set_margin_to_panes(@pane2)
      @bevel=BevelNone
    end
    
    def position=(pos)
      return unless pos.is_a?(Integer)
      @position=pos
      @ratio=nil
      self._vr_relayout if @_vr_lw && @_vr_lh
    end
    
    def ratio=(r)
      return unless r.is_a?(Float) && ((0.0 <= r) && (r <= 1.0))
      @ratio=r
      @position=nil
      self._vr_relayout if @_vr_lw && @_vr_lh
    end
    
    def bevel=(b)
      @bevel=b
      self._vr_relayout if @_vr_lw && @_vr_lh
    end
      
    def gap(); @separatorheight; end
    def gap=(h)
      @separatorheight=h
      self._vr_relayout if @_vr_lw && @_vr_lh
    end
  end
  
  class VRMgdVertTwoPaneFrame < VRVertTwoPaneFrame
    include VRMgdTwoPaneFrame
    attr_accessor :uLimit, :lLimit
    def initialize(p1,p2)
      super
      @uLimit = 0; @lLimit = 0
    end
    
    def splitterDragging(x,y)
      return if y<@separatorheight+@_vr_ly+@uLimit or
                                     y>@_vr_ly+@_vr_lh-@separatorheight-@lLimit
      if @ratio then
        @ratio=(y-@_vr_ly).to_f/@_vr_lh
      elsif @position < 0
        @position = y - @_vr_lh - @_vr_ly
      else
        @position = y - @_vr_ly
      end
      
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
    
    def _vr_relayout(ys =
      if @ratio then (@_vr_lh*@ratio).to_i
      elsif @position < 0
        @_vr_lh + @position
      else
        @position
      end )
      sh=(@separatorheight/2).to_i
      @_vr_lxs,@_vr_lxe = @_vr_lx,@_vr_lx+@_vr_lw
      @_vr_lys,@_vr_lye = @_vr_ly+ys-sh,@_vr_ly+ys+sh
      if @pane1.is_a? Array
        @pane1.each{|i| i.move(@_vr_lx,@_vr_ly,@_vr_lw,ys-sh)}
      elsif @pane1
        @pane1.move(@_vr_lx,@_vr_ly,@_vr_lw,ys-sh)
      end
      if @pane2.is_a? Array
        @pane2.each{|i| i.move(@_vr_lx,@_vr_lys+@separatorheight, @_vr_lw,
          @_vr_lh-ys-sh)}
      elsif @pane2
        @pane2.move(@_vr_lx,@_vr_lys+@separatorheight, @_vr_lw,@_vr_lh-ys-sh) 
      end
    end
    
    def _vr_draw_bevel(win)
      return unless @_vr_lxs
      win.dopaint do
        @bevel[0].each_with_index{|cl,idx|
          win.setPen(@bevel[0][idx],1)
          win.grMoveTo(@_vr_lxs+idx,@_vr_lye-idx-1)
          win.grLineTo(@_vr_lxs+idx,@_vr_lys+idx)
          win.grLineTo(@_vr_lxe-idx,@_vr_lys+idx)
          win.setPen(@bevel[1][idx],1)
          win.grMoveTo(@_vr_lxe-idx-1,@_vr_lys+idx)
          win.grLineTo(@_vr_lxe-idx-1,@_vr_lye-idx-1)
          win.grLineTo(@_vr_lxs+idx,@_vr_lye-idx-1)
        }
      end
    end
  end
  
  class VRMgdHorizTwoPaneFrame < VRHorizTwoPaneFrame
    include VRMgdTwoPaneFrame
    attr_accessor :rLimit, :lLimit
    def initialize(p1,p2)
      super
      @rLimit = 0; @lLimit = 0
    end
    def splitterDragging(x,y)
      return if x<@separatorheight+@_vr_lx+@lLimit or
                                      x>@_vr_lx+@_vr_lw-@separatorheight-@rLimit
      if @ratio then
        @ratio=(x-@_vr_lx).to_f/@_vr_lw
      elsif @position < 0
        @position = x - @_vr_lw - @_vr_lx
      else
        @position = x - @_vr_lx
      end
      case splitter_operation_type
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

    def _vr_relayout(xs =
      if @ratio then (@_vr_lw*@ratio).to_i
      elsif @position < 0
        @_vr_lw + @position
      else
        @position
      end)
      sh=(@separatorheight/2).to_i
      @_vr_lxs,@_vr_lxe = @_vr_lx+xs-sh,@_vr_lx+xs+sh
      @_vr_lys,@_vr_lye = @_vr_ly,@_vr_ly+@_vr_lh
      
      if @pane1.is_a? Array then
        @pane1.each{|i| i.move(@_vr_lx,@_vr_ly,xs-sh,@_vr_lh)}
      elsif @pane1
        @pane1.move(@_vr_lx,@_vr_ly,xs-sh,@_vr_lh)
      end
      if @pane2.is_a? Array
        @pane2.each{|i| i.move(@_vr_lxs+@separatorheight,@_vr_ly,
           @_vr_lw-xs-sh, @_vr_lh)}
      elsif @pane2
        @pane2.move(@_vr_lxs+@separatorheight,@_vr_ly, @_vr_lw-xs-sh, @_vr_lh)
      end
    end
    
    def _vr_draw_bevel(win)
      return unless @_vr_lxs
      win.dopaint do
        @bevel[0].each_with_index{|cl,idx|
          win.setPen(@bevel[0][idx],1)
          win.grMoveTo(@_vr_lxe-idx-1,@_vr_lys+idx+1)
          win.grLineTo(@_vr_lxs+idx,@_vr_lys+idx+1)
          win.grLineTo(@_vr_lxs+idx,@_vr_lye-idx)
          win.setPen(@bevel[1][idx],1)
          win.grMoveTo(@_vr_lxs+idx,@_vr_ly+@_vr_lh-idx-1)
          win.grLineTo(@_vr_lxe-idx-1,@_vr_lye-idx-1)
          win.grLineTo(@_vr_lxe-idx-1,@_vr_lys+idx)
        }
      end
    end
  end
end
