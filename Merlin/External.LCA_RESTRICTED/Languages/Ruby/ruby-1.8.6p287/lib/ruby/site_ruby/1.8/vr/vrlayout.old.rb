###################################
#
# vrlayout.rb (old version)
# Programmed by  nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2001 Nishikawa,Yasuhiro
#
# More information at http://www.threeweb.ad.jp/~nyasu/software/vrproject.html
# (in Japanese)
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

=begin
= VisualuRuby(tmp) Layout Managers
Layout Managers re-arrange child windows(controls)
when the parent window is resized.
=end


##############################################
#  base module for LayoutManagers
#
module VRLayoutManager
=begin
== VRLayoutManager
The base class for the layout managers.

=== Methods
--- rearrange
    Re-arranges child windows.
=end


  include VRMessageHandler

  def vrLayoutinit
    addHandler(WMsg::WM_SIZE, "vrlayoutresize",MSGTYPE::ARGLINTINT,nil) 
    acceptEvents [WMsg::WM_SIZE]
  end

  def vrinit
    super
    vrLayoutinit
  end
  
  def rearrange
    if self.visible? then
      a=self.clientrect
      sendMessage WMsg::WM_SIZE,0,MAKELPARAM(a[2]-a[0],a[3]-a[1])
    end
  end
  
  def self_vrlayoutresize(*arg)
    self_layout_arrange(*arg)
  end
end

##
##########################################


module VRVertLayoutManager
=begin
== VRVertLayoutManager
On window resizing, each controls on the window is re-arranged
to vertical arrangement. each control's width is window width and 
each height is one n-th of window's height.

=== Methods
--- addControl(type,name,caption,astyle)
    Create a new control and add on the window.
    The arguments are same as VRParent#addControl 
    without args ((|x|)),((|y|)),((|w|)),((|h|))
=end

  include VRLayoutManager
  
  def self_layout_arrange(width,fheight)
    return unless @_vr_c_order
    height=(@_vr_c_order.size>0)? (fheight.to_f / @_vr_c_order.size) : fheight
    
    @_vr_c_order.each_index do |i|
      @_vr_c_order[i].move( 0,i*height, width,height )
    end
  end

  VR_ADDCONTROL_FEWARGS=true

  def addControl(type,name,caption,style=0)
    @_vr_c_order=[] if !@_vr_c_order
    r=vr_addControlOriginal(type,name,caption,0,@controls.size*10,10,10,style)
    @_vr_c_order.push r
    return r
  end
end


module VRHorizLayoutManager
=begin
== VRHorizLayoutManager
On window resizing, each controls on the window is re-arranged
to horizontal arrangement. each control's height is window height and 
each width is one n-th of window's width.

=== Methods
--- addControl(type,name,caption,astyle)
    Create a new control and add on the window.
    The arguments are same as VRParent#addControl 
    without args ((|x|)),((|y|)),((|w|)),((|h|))
=end

  include VRLayoutManager
  
  def self_layout_arrange(fwidth,height)
    return unless @_vr_c_order
    width =(@_vr_c_order.size>0)? (fwidth.to_f / @_vr_c_order.size) : fwidth

    @_vr_c_order.each_index do |i|
      @_vr_c_order[i].move( i*width,0, width,height )
    end
  end

  VR_ADDCONTROL_FEWARGS=true

  def addControl(type,name,caption,style=0)
    @_vr_c_order=[] if !@_vr_c_order
    r=vr_addControlOriginal(type,name,caption,0,@controls.size*10,10,10,style)
    @_vr_c_order.push r
    return r
  end

end

module VRGridLayoutManager
=begin
== VRGridLayoutManager
On window resizing, each controls on the window is re-arranged
with the grid which devides the window height and width by the number
specified by ((<setDimension>)) method.

=== Methods
--- setDimension(x,y)
    Devides windows width by ((|x|)) and height by ((|y|)).

--- addControl(type,name,caption,x,y,w,h,astyle)
    Create a new control and add on the window.
    The arguments are same as VRParent#addControl but
    args ((|x|)),((|y|)),((|w|)),((|h|)) are under grid dimension.
=end

  include VRLayoutManager
  
  def setDimension(x,y)
    @_vr_xsize=x
    @_vr_ysize=y
    @_vr_controldim={}
  end

  def self_layout_arrange(width,height)
    return unless @controls

    @controls.each do |id,cntl|
        cntl.move(width.to_f  / @_vr_xsize*@_vr_controldim[id][0],
                  height.to_f / @_vr_ysize*@_vr_controldim[id][1],
                  width.to_f  / @_vr_xsize*@_vr_controldim[id][2],
                  height.to_f / @_vr_ysize*@_vr_controldim[id][3]  )
    end
  end

  def addControl(type,name,caption,x,y,w,h,style=0)
    if !@_vr_controldim then raise("addControl before setDimension") end
    if @controls.size != @_vr_controldim.size then
      raise "VRGridLayoutManager misses some controls"+
            "(mng#{@_vr_controldim.size} cntls#{@_vr_cid})."
    end

    gx=self.w/@_vr_xsize*x; gw=self.w/@_vr_xsize*w;
    gy=self.h/@_vr_ysize*y; gh=self.h/@_vr_ysize*h;
    r=vr_addControlOriginal(type,name,caption,gx,gy,gw,gh,style)
    @_vr_controldim[r.etc]= [x,y,w,h]
    return r
  end
end

module VRFullsizeLayoutManager
=begin
== VRFullsizeLayoutManager
   This is a LayoutManager for only one control, whose size is full size 
   on the window.

=== Methods
--- addControl(type,name,caption,astyle)
    You can call this method only once.
=end


  include VRVertLayoutManager

  VR_ADDCONTROL_FEWARGS=true

  def addControl(*arg)
    super
    def self.addControl(*arg)
      raise "addControl twice"
    end
  end
end
