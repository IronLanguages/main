###############################
#
# contrib/vrwincomponent.rb
#
# These modules/classes are contributed by Yuya-san.
# Modified by nyasu <nyasu@osk.3web.ne.jp>
# Distributed at http://vruby.sourceforge.net/index.html
#
###############################

#====================================================================#
# VRWinComponent Class
class VRWinComponent

  #==================================================================#
  # Instance Methods

  def maximizebox
    return self.winstyle.getter(WStyle::WS_MAXIMIZEBOX)
  end

  def maximizebox=(bool)
    self.winstyle.setter(WStyle::WS_MAXIMIZEBOX, bool)
  end

  def minimizebox
    return self.winstyle.getter(WStyle::WS_MINIMIZEBOX)
  end

  def minimizebox=(bool)
    self.winstyle.setter(WStyle::WS_MINIMIZEBOX, bool)
  end

  def sizebox
    return self.winstyle.getter(WStyle::WS_SIZEBOX)
  end

  def sizebox=(bool)
    self.winstyle.setter(WStyle::WS_SIZEBOX, bool)
  end

  def tabstop
    return self.winstyle.getter(WStyle::WS_TABSTOP)
  end

  def tabstop=(bool)
    self.winstyle.setter(WStyle::WS_TABSTOP, bool)
  end

end

#====================================================================#
# End of source.
#====================================================================#
