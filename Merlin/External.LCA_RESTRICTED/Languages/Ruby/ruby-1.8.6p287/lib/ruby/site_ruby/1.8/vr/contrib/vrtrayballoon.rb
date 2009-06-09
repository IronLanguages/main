###############################
#
# contrib/vrtrayballoon.rb
#
# These modules/classes are contributed by Drew Willcoxon.
# Modified by nyasu <nyasu@osk.3web.ne.jp>
# The original is at http://cs.stanford.edu/people/adw/vrballoon.zip
#

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

module VRTrayiconFeasible
  +  NIF_INFO    = 16   # Shell32.dll version 5.0 (Win2000/Me and later)
  # Tooltip balloon icon flags
  NIIF_NONE    = 0
  NIIF_INFO    = 1
  NIIF_WARNING = 2
  NIIF_ERROR   = 3
  NIIF_USER    = 4   # WinXP SP2 and later
  NIIF_NOSOUND = 16  # Shell32.dll version 6.0 (WinXP/Vista)

  # Tooltip balloon messages
  NIN_BALLOONSHOW      = WMsg::WM_USER + 2
  NIN_BALLOONHIDE      = WMsg::WM_USER + 3
  NIN_BALLOONTIMEOUT   = WMsg::WM_USER + 4
  NIN_BALLOONUSERCLICK = WMsg::WM_USER + 5

  def modify_trayicon5(hicon,infotitle,infotext,infoicon=NIIF_NONE,
                       infotimeout=20000,iconid=0)
    flag = NIF_INFO
    if hicon then
      flag |= NIF_ICON
    end
    size = 4*10 + 128 + 256 + 64
    s = [size, self.hWnd, iconid, flag, 0, hicon.to_i].pack(NOTIFYICONDATA_a) <<
        ['', 0, 0, infotext.to_s[0, 256], infotimeout, infotitle.to_s[0, 64],
         infoicon].pack('a128IIa256Ia64I')
    Shell_NotifyIcon.call(NIM_MODIFY,s)
  end

  def self__vr_traynotify(wparam,lparam)
    case lparam
    when WMsg::WM_MOUSEMOVE
      selfmsg_dispatching("trayrbuttondown",wparam)
    when WMsg::WM_RBUTTONUP
      selfmsg_dispatching("trayrbuttonup",wparam)
    when NIN_BALLOONSHOW
      selfmsg_dispatching("trayballoonshow",wparam)
    when NIN_BALLOONHIDE
      selfmsg_dispatching("trayballoonhide",wparam)
    when NIN_BALLOONTIMEOUT
      selfmsg_dispatching("trayballoontimeout",wparam)
    when NIN_BALLOONUSERCLICK
      selfmsg_dispatching("trayballoonclicked",wparam)
    end
  end
end

