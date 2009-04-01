###############################
#
# contrib/vrlistviewex.rb
#
# These modules/classes are contributed by Yuya-san.
# Modified by nyasu <nyasu@osk.3web.ne.jp>
# Distributed at http://vruby.sourceforge.net/index.html
#
###############################

#====================================================================#
# VRListview Class
class VRListview

  #==================================================================#
  # Private Instance Methods

  def exstyle_getter(style)
    return (self.lvexstyle & style) == style
  end
  private :exstyle_getter

  def exstyle_setter(style, bool)
    if bool
      self.lvexstyle |= style
    else
      self.lvexstyle &= 0xFFFFFFFF - style
    end
  end
  private :exstyle_setter

  #==================================================================#
  # Instance Methods

  def row_select
    return exstyle_getter(WExStyle::LVS_EX_FULLROWSELECT)
  end

  def row_select=(bool)
    exstyle_setter(WExStyle::LVS_EX_FULLROWSELECT, bool)
  end

  def grid_lines
    return exstyle_getter(WExStyle::LVS_EX_GRIDLINES)
  end

  def grid_lines=(bool)
    exstyle_setter(WExStyle::LVS_EX_GRIDLINES, bool)
  end

  def hide_selection
    return !self.winstyle.getter(WStyle::LVS_SHOWSELALWAYS)
  end

  def hide_selection=(bool)
    self.winstyle.setter(WStyle::LVS_SHOWSELALWAYS, !bool)
  end

  def extended_select
    return !self.winstyle.getter(WStyle::LVS_SINGLESEL)
  end

  def extended_select=(bool)
    self.winstyle.setter(WStyle::LVS_SINGLESEL, !bool)
  end

end

#====================================================================#
# End of source.
#====================================================================#
