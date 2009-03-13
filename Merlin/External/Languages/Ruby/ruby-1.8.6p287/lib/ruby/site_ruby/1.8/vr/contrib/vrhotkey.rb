###############################
#
# contrib/vrhotkey.rb
#
# These modules/classes are contributed by Yuya-san.
# Modified by nyasu <nyasu@osk.3web.ne.jp>
# Distributed at http://vruby.sourceforge.net/index.html
#
###############################


#====================================================================#
# WConst Module
module WConst
  WM_HOTKEY = 786
end

#====================================================================#
# VRHotKey Module
module VRHotKey

  #==================================================================#
  # Instance Methods

  def vrinit
    super
    self.addHandler(WConst::WM_HOTKEY, 'hotkey', MSGTYPE::ARGWINT, nil)
    self.addEvent(WConst::WM_HOTKEY)
  end

end

#====================================================================#
# End of source.
#====================================================================#
