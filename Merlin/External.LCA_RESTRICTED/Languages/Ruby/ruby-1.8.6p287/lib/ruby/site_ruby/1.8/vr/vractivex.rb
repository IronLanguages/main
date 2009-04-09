###################################
#
# vractivex.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2003-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

require 'vr/vruby'
require 'Win32API'
require 'win32ole'

class VRActiveXControl < VRControl
  AtlAxWinInit=Win32API.new("atl.dll","AtlAxWinInit","","I")
  AtlAxWinInit.call

  WINCLASSINFO=["AtlAxWin",0]
  ACTIVEXCINFO=["",""] # PROGID,EVENTSINK

  alias :progId :caption
  alias :progId= :caption=
  def caption() "" end
  def caption=(arg) arg end

  def initialize
    self.progId = self.class::ACTIVEXCINFO[0]
    class <<self
      undef_method :progId=
    end
  end

  def vrinit
    super
	@_vr_win32ole = self.get_oleinterface
    if self.class::ACTIVEXCINFO[1] then
      @_vr_win32ole_event = 
        WIN32OLE_EVENT.new(@_vr_win32ole,self.class::ACTIVEXCINFO[1])
    end
  end

  def ole_interface() @_vr_win32ole; end
  def ole_events() @_vr_win32ole_event; end

  def add_oleeventhandler(evname,handlername=evname.downcase)
    ole_events.on_event(evname) do |*args|
      @parent.controlmsg_dispatching(self,handlername,*args)
    end
  end

  def _invoke(*args) @_vr_win32ole._invoke(*args) end
  def _setproperty(*args) @_vr_win32ole._setproperty(*args) end
  def _getproperty(*args) @_vr_win32ole._getproperty(*args) end

end
