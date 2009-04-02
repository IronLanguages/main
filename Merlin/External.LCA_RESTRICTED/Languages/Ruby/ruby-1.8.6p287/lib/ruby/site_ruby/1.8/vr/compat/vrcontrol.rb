unless defined?(VREdit) then
  raise "compatibility file must be loaded after its original one."
end

class VREdit
  alias vrinitnew vrinit
  undef vrinitnew
  def vrinit
    super
    addCommandHandler(0x300,"change",MSGTYPE::ARGNONE,nil) 
    addCommandHandler(0x300,"changed",MSGTYPE::ARGNONE,nil)
  end
end

class VRListbox
  alias vrinitnew vrinit
  undef vrinitnew
  def vrinit
    super
    addCommandHandler(WMsg::LBN_SELCHANGE, "selchange",MSGTYPE::ARGNONE,nil)
    addCommandHandler(WMsg::LBN_SELCHANGE, "selchanged",MSGTYPE::ARGNONE,nil)
    set_liststrings
  end

  alias getCount      :countStrings
  alias selectedIndex :selectedString
  alias getString     :getTextOf
  alias itemString    :getTextOf
  alias setItemData   :setDataOf
  alias getItemData   :getDataOf
end

class VRCombobox
  alias vrinitnew vrinit
  undef vrinitnew
  def vrinit
    super
    set_liststrings
    addCommandHandler(WMsg::CBN_SELCHANGE, "selchange",MSGTYPE::ARGNONE,nil)
    addCommandHandler(WMsg::CBN_SELCHANGE, "selchanged",MSGTYPE::ARGNONE,nil)
  end

  alias getCount      :countStrings
  alias selectedIndex :selectedString
  alias getString     :getTextOf
  alias itemString    :getTextOf
  alias setItemData   :setDataOf
  alias getItemData   :getDataOf

end
