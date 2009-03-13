###################################
#
# vrtray.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2002-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

module VRTrayiconFeasible
=begin
== VRTrayiconFeasible
This modules is included to create/delete/modify tray icons.

=== Methods
--- create_trayicon(icon=DEFAULTICON,tiptext="",icon_id=0)
    Creates new trayicon in the tasktray.((|icon|)) is hIcon of the trayicon and
    the trayicon has tooltip text ((|tiptext|)).
    When argument ((|icon|)) is nil, icon will set as DEFAULTICON.
    To distinguish multiple trayicon, ((|icon_id|)) information is added to it.
--- delete_trayicon(icon_id=0)
    Deletes the trayicon specified by ((|icon_id|))
--- modify_trayicon(hicon,tiptext,iconid=0)
    Modifies the trayicon's icon/tiptext.

=== Event Handlers
--- self_traymousemove(icon_id)
    Fired when mouse cursor is moved over the trayicon.
--- self_traylbuttondown(icon_id)
    Fired when mouse left-button down on the trayicon.
--- self_traylbuttonup(icon_id)
    Fired when mouse left-button up on the trayicon.
--- self_trayrbuttondown(icon_id)
    Fired when mouse right-button down on the trayicon.
--- self_trayrbuttonup(icon_id)
    Fired when mouse right-button up on the trayicon.
=end


  include VRUserMessageUseable

  Shell_NotifyIcon=Win32API.new("shell32","Shell_NotifyIcon","IP","I")
  
  NOTIFYICONDATA_a = "IIIIII"
  DEFAULTICON = Win32API.new("user32","LoadIcon","II","I").call(0,32512)

  NIF_MESSAGE = 1
  NIF_ICON    = 2
  NIF_TIP     = 4
  NIM_ADD     = 0
  NIM_MODIFY  = 1
  NIM_DELETE  = 2

  def vrinit
    super
    trayiconfeasibleinit
  end
  
  def trayiconfeasibleinit
    @_vr_traynotify_msg = registerUserMessage(ReservedMsg::WM_VR_TRAYNOTIFY,"_vr_traynotify")
  end

  def create_trayicon(icon=DEFAULTICON,tiptext="",iconid=0)
    icon = DEFAULTICON unless icon
    tip = tiptext.to_s
    s = [4*6+64,
          self.hWnd,iconid,NIF_MESSAGE|NIF_ICON|NIF_TIP,
          @_vr_traynotify_msg,icon ].pack(NOTIFYICONDATA_a) << 
        tip << "\0"*(64-tip.length)
    Shell_NotifyIcon.call(NIM_ADD,s)
  end

  def delete_trayicon(iconid=0)
    s = [4*6+64,
          self.hWnd,iconid,0,0,0].pack(NOTIFYICONDATA_a) << "\0"*64
    Shell_NotifyIcon.call(NIM_DELETE,s)
  end

  def modify_trayicon(hicon,tiptext,iconid=0)
    flag=0
    if hicon then
      flag |= NIF_ICON
    end
    if tiptext then
      flag |= NIF_TIP
    end
    tip = tiptext.to_s
    s = [4*6+64, self.hWnd,iconid,flag,0,hicon.to_i].pack(NOTIFYICONDATA_a) << 
        tip << "\0"*(64-tip.length)
    Shell_NotifyIcon.call(NIM_MODIFY,s)
  end

  def self__vr_traynotify(wparam,lparam)
    case lparam
    when WMsg::WM_MOUSEMOVE
      selfmsg_dispatching("traymousemove",wparam)
    when WMsg::WM_LBUTTONDOWN
      selfmsg_dispatching("traylbuttondown",wparam)
    when WMsg::WM_LBUTTONUP
      selfmsg_dispatching("traylbuttonup",wparam)
    when WMsg::WM_RBUTTONDOWN
      selfmsg_dispatching("trayrbuttondown",wparam)
    when WMsg::WM_RBUTTONUP
      selfmsg_dispatching("trayrbuttonup",wparam)
    end
  end

end

module VRTasktraySensitive
=begin
== VRTasktraySensitive
This modules enables to sense the tasktray creation on restarting explorer.
The deleted tasktray icons can be created again using this module and writing 
in its event handler how to do it.

=== Event Handlers
--- self_taskbarcreated
    Fired when the explorer restarts and creates its tasktray.
=end

  RegisterWindowMessage = Win32API.new("user32","RegisterWindowMessage","P","L")
  
  def vrinit
    super
    tasktraysensitive
  end
  
  def tasktraysensitive   # coded by Katonbo-san
    # get message ID of 'TaskbarCreated' message
    id_taskbar_created = RegisterWindowMessage.call('TaskbarCreated')
    
    # prepare 'TaskbarCreated' message handler
    addHandler(id_taskbar_created, 'taskbarcreated', MSGTYPE::ARGNONE, nil)
    addEvent(id_taskbar_created)
  end
end



require 'vr/contrib/vrtrayballoon.rb'
