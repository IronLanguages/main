unless defined?(VREdit) then
  raise "compatibility file must be loaded after its original one."
end


class VRMediaView
  def vr_e_compatibility
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYMODE,"modechange",MSGTYPE::ARGINT,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYSIZE, "sizechange",MSGTYPE::ARGNONE,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYMEDIA,"mediachange",MSGTYPE::ARGSTRING,nil)
  end

  alias vrinitnew vrinit
  undef vrinitnew
  def vrinit
    super
    vr_e_compatibility
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYERROR,"onerror",MSGTYPE::ARGINT,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYMODE,"modechanged",MSGTYPE::ARGINT,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYSIZE, "sizechanged",MSGTYPE::ARGNONE,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYMEDIA,"mediachanged",MSGTYPE::ARGSTRING,nil)
  end
end

