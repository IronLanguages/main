###################################
#
# vrlayout.rb (using vrlayout2 version)
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require VR_DIR+'vrlayout2'

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
    @_vr_layoutframe=nil
    addHandler(WMsg::WM_SIZE, "vrlayoutresize",MSGTYPE::ARGLINTINT,nil) 
    acceptEvents [WMsg::WM_SIZE]
  end

  def vrinit
    super
    vrLayoutinit
  end
  
  def rearrange
    a=self.clientrect
    sendMessage WMsg::WM_SIZE,0,MAKELPARAM(a[2]-a[0],a[3]-a[1])
  end
  
  def self_vrlayoutresize(*arg)
    self_layout_arrange(*arg)
  end

  def self_layout_arrange(xw,yh)
    @_vr_layoutframe.move 0,0,xw,yh if @_vr_layoutframe
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

  VR_ADDCONTROL_FEWARGS=true

  def addControl(type,name,caption,style=0)
    @_vr_layoutframe = VRVertLayoutFrame.new unless @_vr_layoutframe
    r=vr_addControlOriginal(type,name,caption,0,@controls.size*10,10,10,style)
    @_vr_layoutframe.register(r)
    rearrange
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

  VR_ADDCONTROL_FEWARGS=true

  def addControl(type,name,caption,style=0)
    @_vr_layoutframe = VRHorizLayoutFrame.new unless @_vr_layoutframe
    r=vr_addControlOriginal(type,name,caption,0,@controls.size*10,10,10,style)
    @_vr_layoutframe.register(r)
    rearrange
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
    unless @_vr_layoutframe then
      @_vr_layoutframe = VRGridLayoutFrame.new(x,y)
    else
      @_vr_layoutframe.setDimension(x,y)
    end
  end
  def addControl(type,name,caption,x,y,w,h,style=0)
    @_vr_layoutframe = VRGridLayoutFrame.new unless  @_vr_layoutframe
    r=vr_addControlOriginal(type,name,caption,0,@controls.size*10,10,10,style)
    @_vr_layoutframe.register(r,x,y,w,h)
    rearrange
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
