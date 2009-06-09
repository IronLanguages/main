###################################
#
# vrhandler.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

=begin
= VisualuRuby(tmp) Additional Modules
=end


module VRMouseFeasible
  include VRMessageHandler

  SHIFT_LBUTTON=1
  SHIFT_RBUTTON=2
  SHIFT_SHIFT  =4
  SHIFT_CONTROL=8
  SHIFT_MBUTTON=16

=begin
== VRMouseFeasible
This is a module to receive mouse messages. 

=== Event Handlers
--- self_lbuttonup(shift,x,y)
    This method is fired when mouse left button is released at coord(x,y).
    Press SHIFT key to set 0x4 bit of argument "shift". 
    Its 0x8 bit is for CONTROL key 
--- self_lbuttondown(shift,x,y)
    This method is fired when mouse left button is pressed.
--- self_rbuttonup(shift,x,y)
    This method is fired when mouse right button is released.
--- self_rbuttondown(shift,x,y)
    This method is fired when mouse right button is pressed.
--- self_mousemove(shift,x,y)
    This method is fired when mouse cursor is moving on the window at coord(x,y).
=end

  def mousefeasibleinit
    addHandler WMsg::WM_LBUTTONUP,  "lbuttonup",  MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_LBUTTONDOWN,"lbuttondown",MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_RBUTTONUP,  "rbuttonup",  MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_RBUTTONDOWN,"rbuttondown",MSGTYPE::ARGINTSINTSINT,nil
    addHandler WMsg::WM_MOUSEMOVE,  "mousemove",  MSGTYPE::ARGINTSINTSINT,nil
    acceptEvents [WMsg::WM_LBUTTONUP,WMsg::WM_RBUTTONUP,
                  WMsg::WM_LBUTTONDOWN,WMsg::WM_RBUTTONDOWN,
                  WMsg::WM_MOUSEMOVE]
  end
  
  def vrinit
    super
    mousefeasibleinit
  end

=begin handlers
  def self_lbuttonup(*arg)   end
  def self_lbuttondown(*arg) end
  def self_rbuttonup(*arg)   end
  def self_rbuttondown(*arg) end
  def self_mousemove(*arg)   end
=end
end

module VRFocusSensitive
  include VRMessageHandler

=begin
== VRFocusSensitive
This is a module to sense getting/losing focus.
=== Event Handlers
--- self_gotfocus()
    This method is fired when the window get focus.
--- self_lostfocus()
    This method is fired when the window lose the focus.
=end

  def focussensitiveinit
    addHandler WMsg::WM_SETFOCUS,   "gotfocus",   MSGTYPE::ARGNONE,nil
    addHandler WMsg::WM_KILLFOCUS,  "lostfocus",  MSGTYPE::ARGNONE,nil
    acceptEvents [WMsg::WM_SETFOCUS,WMsg::WM_KILLFOCUS]
  end
  
  def vrinit
    super
    focussensitiveinit
  end
=begin handlers
  def gotfocus()  end
  def lostfocus() end
=end
end

module VRKeyFeasible
  include VRMessageHandler
  
=begin
== VRKeyFeasible
This is a module to sense keyboard input.

=== Event Handlers
--- self_char(keycode,keydata)
    This method is fired when WM_KEYUP and WM_KEYDOWN are translated into 
    keyboard input messages.
--- self_deadchar(keycode,keydata)
--- self_syschar(keycode,keydata)
--- self_sysdeadchar(keycode,keydata)
=end

  def keyfeasibleinit
    addHandler(0x102,"char",     MSGTYPE::ARGINTINT,nil)      # WM_CHAR
    addHandler(0x103,"deadchar", MSGTYPE::ARGINTINT,nil)      # WM_DEADCHAR
    addHandler(0x106,"syschar",  MSGTYPE::ARGINTINT,nil)      # WM_SYSCHAR
    addHandler(0x107,"sysdeadchar", MSGTYPE::ARGINTINT,nil)   # WM_SYSDEADCHAR
    acceptEvents [0x102,0x103,0x106,0x107]
  end
  def vrinit
    super
    keyfeasibleinit
  end

=begin handlers
  def self_char(*arg)        end
  def self_deadchar(*arg)    end
  def self_syschar(*arg)     end
  def self_sysdeadchar(*arg) end
=end
end

module VRDestroySensitive
=begin
== VRDestroySensitive
This is a module to sense window destruction.

=== Event Handlers
--- self_destroy
    This is fired when the window is destroying.
=end

  include VRMessageHandler

  def destroysensitiveinit
    addHandler WMsg::WM_DESTROY,"destroy",MSGTYPE::ARGNONE,nil
    acceptEvents [WMsg::WM_DESTROY]
  end
  
  def vrinit
    super
    destroysensitiveinit
  end
end

module VRClosingSensitive
=begin
== VRClosingSensitive
This is a module to sense window closing.
This module can be used for implement of CanClose method.

ex. 
  class MyForm < VRForm
    include VRClosingSensitive
    def self_close
      r = messageBox("Can Close?","close query",4) #4=MB_YESNO
  
      if r==7 # ID_NO then 
        SKIP_DEFAULTHANDLER
      end
    end
  end

=== Event Handlers
--- self_close
    This is fired when the window is closing.
=end

  include VRMessageHandler
  def closingsensitiveinit
    addHandler WMsg::WM_CLOSE,"close",MSGTYPE::ARGNONE,nil
    acceptEvents [WMsg::WM_CLOSE]
  end
  
  def vrinit
    super
    closingsensitiveinit
  end
end

require VR_DIR+'contrib/vrctlcolor'
