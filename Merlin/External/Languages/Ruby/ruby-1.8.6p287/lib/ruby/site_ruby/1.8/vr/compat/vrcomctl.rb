class VRUpdown < VRNotifyControl
  alias vrinitnew vrinit
  undef vrinitnew
  def vrinit
    super
    addNotifyHandler(WMsg::UDN_DELTAPOS,"deltapos",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_UPDOWN)
#    addNotifyHandler(WMsg::UDN_DELTAPOS,"changed",
#                    MSGTYPE::ARGSTRUCT,WStruct::NM_UPDOWN)
    addFilterArg WMsg::UDN_DELTAPOS,"TF"
  end
end
