###################################
#
# vrmargin.rb
#
# Programmed by  yukimi_sake <yukimi_sake@mbi.nifty.com>
# Copyright 2003-2004 Yukio Sakaue
#
###################################

require 'vr/vruby'
require 'vr/vrhandler'

module VRMargin
=begin
== VRMargin
This is a module to space itself from parent window on resized.
Also useable for layout flames and two panes.
=== Methods
--- initMargin(left=nil,top=nil,right=nil,bottom=nil)
    Call this method first to use margin and set each margins.
    If the margins are set to nil,the control becomes original size.
--- setMargin(left=nil,top=nil,right=nil,bottom=nil)
    Set all margins
--- mgLeft
--- mgLeft=
    Get/Set left margin.
--- mgTop
--- mgTop=
    Get/Set top margin.
---mgRight
---mgRight=
    Get/Set right margin.
--- mgBottom
--- mgBottom=
    Get/Set bottom margin.
=end
 
  def initMargin(left=nil,top=nil,right=nil,bottom=nil)
    if respond_to?(:vrinit)
      extend MarginForWindow
      vrinit 
      @__org_x,@__org_y,@__org_w,@__org_h=x,y,w,h
      @_mg_x,@_mg_y,@_mg_w,@_mg_h = x,y,w,h
    else
      extend MarginForClass
      mgLeft = 0 unless left
      mgTop = 0 unless top
      mgRight = 0 unless right
      mgBottom = 0 unless bottom
    end
    @_mg_top=top
    @_mg_bottom=bottom
    @_mg_left=left
    @_mg_right=right
    self
  end
      
  def mgTop(); @_mg_top; end
  def mgBottom(); @_mg_bottom; end
  def mgLeft(); @_mg_left; end
  def mgRight(); @_mg_right;end
  
  def setMargin(left=nil,top=nil,right=nil,bottom=nil)
    @_mg_top=top
    @_mg_bottom=bottom
    @_mg_left=left
    @_mg_right=right
    self.move(@_mg_x,@_mg_y,@_mg_w,@_mg_h)if @_mg_x&&@_mg_y&&@_mg_w&&@_mg_h
  end
  
  def mgTop=(t)
    @_mg_top=t
    self.move(@_mg_x,@_mg_y,@_mg_w,@_mg_h)
  end
  def mgBottom=(b)
    @_mg_bottom=b
    self.move(@_mg_x,@_mg_y,@_mg_w,@_mg_h)
  end
  def mgLeft=(l)
    @_mg_left=l
    self.move(@_mg_x,@_mg_y,@_mg_w,@_mg_h)
  end
  def mgRight=(r)
    @_mg_right=r
    self.move(@_mg_x,@_mg_y,@_mg_w,@_mg_h)
  end
  
  module MarginForClass
    def move(x,y,w,h)
      return unless x || y || w || h
       @_mg_x,@_mg_y,@_mg_w,@_mg_h = x,y,w,h
      _x = x + @_mg_left
      _w = w - @_mg_right - @_mg_left
      _y = y + @_mg_top
      _h = h - @_mg_bottom - @_mg_top
      super(_x,_y,_w,_h)
    end
  end
  
  module MarginForWindow
    def move(x,y,w,h)
      @_mg_x,@_mg_y,@_mg_w,@_mg_h = x,y,w,h
      if @_mg_left then 
        if @_mg_right then
          _x = x + @_mg_left
          _w = w - @_mg_right - @_mg_left
        else
          _x =  x + @_mg_left
          _w = @__org_w
        end
      else 
        if @_mg_right then
          _x = w - @_mg_right + x - @__org_w
          _w = @__org_w
        else
          _x = @__org_x
          _w = @__org_w
        end
      end
      
      if @_mg_top then 
        if @_mg_bottom then
          _y = y + @_mg_top
          _h = h - @_mg_bottom - @_mg_top
        else
          _y = y + @_mg_top
          _h = @__org_h
        end
      else 
        if @_mg_bottom then
          _y = y+h-@__org_h-@_mg_bottom
          _h = @__org_h
        else
          _y = @__org_y
          _h = @__org_h
        end
      end
      super(_x,_y,_w,_h)
    end
  end
end
