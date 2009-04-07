###################################
#
# vrmmedia.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
# Good Reference: vfw.h
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vrcontrol'
require 'Win32API'

=begin
= VisualuRuby(tmp) modules for MCI
=end


#if RUBY_VERSION<"1.4.4" then   # Quick Hacking..
#  require 'DLLManager'
#  DLLManager.load("msvfw32.dll")
#end

MCIWndRegisterClass = Win32API.new("msvfw32","MCIWndRegisterClass",[],"I")
MCIWndRegisterClass.call


module VRMediaViewContainer
=begin
== VRMediaViewContainer
This module provides a message handler for MCI messages.
VRForm includes this module automatically loading "vrmmedia.rb".
=end
  include VRMessageHandler

  def vrinit
    super
    mediaviewcontainerinit
  end

  def mediaviewcontainerinit
    addHandler(VRMediaView::MCIWNDM_NOTIFYMODE,
                       "mmnotifies",MSGTYPE::ARGPASS,nil)
    addHandler(VRMediaView::MCIWNDM_NOTIFYERROR,
                       "mmnotifies",MSGTYPE::ARGPASS,nil)
    addHandler(VRMediaView::MCIWNDM_NOTIFYSIZE,
                       "mmnotifies",MSGTYPE::ARGPASS,nil)
    addHandler(VRMediaView::MCIWNDM_NOTIFYMEDIA,
                       "mmnotifies",MSGTYPE::ARGPASS,nil)
    acceptEvents [ VRMediaView::MCIWNDM_NOTIFYMODE,
                   VRMediaView::MCIWNDM_NOTIFYSIZE,
                   VRMediaView::MCIWNDM_NOTIFYMEDIA,
                   VRMediaView::MCIWNDM_NOTIFYERROR  ]

    addNoRelayMessages [ VRMediaView::MCIWNDM_NOTIFYMODE,
                         VRMediaView::MCIWNDM_NOTIFYSIZE,
                         VRMediaView::MCIWNDM_NOTIFYMEDIA,
                         VRMediaView::MCIWNDM_NOTIFYERROR  ]
  end

  def self_mmnotifies(msg)
    ct=@_vr_mediaviewers_hwnd[msg.wParam]
    return unless ct
    messageid=msg.msg

    return unless ct._vr_mmhandlers and ct._vr_mmhandlers[messageid] 

    ct._vr_mmhandlers[messageid].each do |shandler|
      args=msgarg2handlerarg(shandler[1],msg,shandler[2])
      ct.__send__(shandler[0],*args) if ct.respond_to?(shandler[0])
      msg.retval = controlmsg_dispatching(ct,shandler[0],*args)
    end
  end

  def addControl(*args)
    @_vr_mediaviewers_hwnd={} unless defined?(@_vr_mediaviewers_hwnd)
    a=super
    @_vr_mediaviewers_hwnd[a.hWnd]=a if a.is_a?(VRMediaView)
    return a
  end
end

module VRContainersSet
  include VRMediaViewContainer
  INITIALIZERS.push :mediaviewcontainerinit
end


#####

module VRMediaViewModeNotifier
  include VRMediaViewContainer

=begin
== VRMediaViewModeNotifier
This module is to use another event handlers for mci.
These new handlers are not relayed to parent by VRMessageParentRelayer.
(Use setnotifier() as setnotifier("cname1_cname2))

=== Methods
--- setnotifier(cname)
    This enables ((<VRMediaView>))'s other event handlers such as cname_stopped,
    cname_playing,cname_paused and cname_open, named after 
    ((<VRMediaView>))#modestring.
=end

  def setnotifier(cname)
    instance_eval(
      "def "+cname+"_modechanged(n)\n" +
      "  fname='#{cname}_'+@#{cname}.modestring(n) \n" +
       " __send__(fname) if respond_to?(fname) \n" +
      "end\n"
    )
  end
end


class VRMediaView < VRControl
=begin
== VRMediaView
This is a control window to play multimedia files.

=== Methods
--- mediaopen(filename,flag=0)
    Open mediafile as filename.
--- mediaclose
    Close mediafile.
--- mode
    Return current mode number.
--- modestring(n)
    Return description for mode number #n
--- errorstring
    Return error description
--- play
--- pause
--- stop
--- eject
--- step(n=1)
--- seek(pos)
--- seekHome
--- seekEnd
--- playable?
--- ejectable?
--- window?
--- length
--- position
--- volume
--- volume=(vl)
--- speed
--- speed=
--- zoom
--- zoom=

=== Event Handlers
--- ????_onerror()
--- ????_modechanged(newmode)
--- ????_sizechanged()
--- ????_mediachanged(str)
=end


  attr_reader :_vr_mmhandlers

  MCI_CLOSE = 0x0804
  MCI_PLAY  = 0x0806
  MCI_SEEK  = 0x0807
  MCI_STOP  = 0x0808
  MCI_PAUSE = 0x0809
  MCI_STEP  = 0x080E

  MCIWNDM_GETPOSITION  = (WMsg::WM_USER + 102)    # A message
  MCIWNDM_GETLENGTH    = (WMsg::WM_USER + 104)
  MCIWNDM_GETMODE      = (WMsg::WM_USER + 106)    # A message
  MCIWNDM_EJECT        = (WMsg::WM_USER + 107)
  MCIWNDM_SETZOOM      = (WMsg::WM_USER + 108)
  MCIWNDM_GETZOOM      = (WMsg::WM_USER + 109)
  MCIWNDM_SETVOLUME    = (WMsg::WM_USER + 110)
  MCIWNDM_GETVOLUME    = (WMsg::WM_USER + 111)
  MCIWNDM_SETSPEED     = (WMsg::WM_USER + 112)
  MCIWNDM_GETSPEED     = (WMsg::WM_USER + 113)

  MCIWNDM_GETERROR     = (WMsg::WM_USER + 128)    # A message

  MCIWNDM_CAN_PLAY     = (WMsg::WM_USER + 144)
  MCIWNDM_CAN_WINDOW   = (WMsg::WM_USER + 145)
  MCIWNDM_CAN_EJECT    = (WMsg::WM_USER + 148)
  MCIWNDM_CAN_CONFIG   = (WMsg::WM_USER + 149)
  MCIWNDM_SETOWNER     = (WMsg::WM_USER + 152)
  MCIWNDM_OPEN         = (WMsg::WM_USER + 153)    # A message

  MCIWNDM_NOTIFYMODE   = (WMsg::WM_USER + 200)  # wparam = hwnd, lparam = mode
  MCIWNDM_NOTIFYSIZE   = (WMsg::WM_USER + 202)  # wparam = hwnd
  MCIWNDM_NOTIFYMEDIA  = (WMsg::WM_USER + 203)  # wparam = hwnd, lparam = fn
  MCIWNDM_NOTIFYERROR  = (WMsg::WM_USER + 205)  # wparam = hwnd, lparam = error

  MCIWNDF_NOPLAYBAR       = 0x0002
  MCIWNDF_NOAUTOSIZEMOVIE = 0x0004
  MCIWNDF_NOMENU          = 0x0008

  MCIWNDF_SIMPLE          = 0xe

  WINCLASSINFO = ["MCIWndClass",0x5500 | 0x400 | 0x880 ] 
                                  #notify:mode,error,size,media

  def addMMHandler(msg,handlername,handlertype,argparsestr)
    @_vr_mmhandlers={} unless defined? @_vr_mmhandlers
    @_vr_mmhandlers[msg]=[] unless @_vr_mmhandlers[msg]
    @_vr_mmhandlers[msg].push [handlername,handlertype,argparsestr]
  end

  def deleteMMHandler(msg,handlername)
    return false unless defined?(@_vr_mmhandlers) and @_vr_mmhandlers[msg]
    @_vr_mmhandlers.delete_if do |shandler|
      shandler[0] != (PREHANDLERSTR+handlername).intern
    end
  end

  def vrinit
    super
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYERROR,"onerror",MSGTYPE::ARGINT,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYMODE,"modechanged",MSGTYPE::ARGINT,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYSIZE, "sizechanged",MSGTYPE::ARGNONE,nil)
    addMMHandler(VRMediaView::MCIWNDM_NOTIFYMEDIA,"mediachanged",MSGTYPE::ARGSTRING,nil)
  end

  def mediaopen(filename,flag=0)
    sendMessage  MCIWNDM_OPEN,flag,filename
    sendMessage  MCIWNDM_SETOWNER,@parent.hWnd,0
  end

  def play()         sendMessage MCI_PLAY,0,0 end
  def pause()        sendMessage MCI_PAUSE,0,0 end
  def stop()         sendMessage MCI_STOP,0,0 end
  def mediaclose()   sendMessage MCI_CLOSE,0,0 end
  def eject()        sendMessage MCI_EJECT,0,0 end
  def step(n=1)      sendMessage MCI_STEP,0,n.to_i end
  def seek(pos)      sendMessage MCI_SEEK,0,pos.to_i end
  def seekHome()     sendMessage MCI_SEEK,0,-1 end
  def seekEnd()      sendMessage MCI_SEEK,0,-2 end

  def playable?()    sendMessage(MCIWNDM_CAN_PLAY,0,0)!=0 end
  def ejectable?()   sendMessage(MCIWNDM_CAN_EJECT,0,0)!=0 end
  def window?()      sendMessage(MCIWNDM_CAN_WINDOW,0,0)!=0 end

  def length()       sendMessage MCIWNDM_GETLENGTH,0,0 end
  def position()     sendMessage MCI_SEEK,0,0 end
  
  def volume=(vl)    sendMessage MCIWNDM_SETVOLUME,0,vl.to_i end
  def volume()       sendMessage MCIWNDM_GETVOLUME,0,0 end
  def speed=(sp)     sendMessage MCIWNDM_SETSPEED,0,sp.to_i end
  def speed()        sendMessage MCIWNDM_GETSPEED,0,0 end
  def zoom=(zm)      sendMessage MCIWNDM_SETZOOM,0,zm.to_i end
  def zoom()         sendMessage MCIWNDM_GETZOOM,0,0 end
  
  def mode
    p="\0                               "  #32bytes
    sendMessage MCIWNDM_GETMODE,31,p
    p
  end
  
  def modestring(n)
    case n
      when 524; "not_ready"
      when 525; "stopped"
      when 526; "playing"
      when 527; "recording"
      when 528; "seeking"
      when 529; "paused"
      when 530; "open"
      else;     "unknown_mode"
    end
  end
  
  def errorstring
    p="\0                               "*8  #256bytes
    sendMessage MCIWNDM_GETERROR,255,p
    p
  end
  
end

if VR_COMPATIBILITY_LEVEL then
  require VR_DIR + 'compat/vrmmedia.rb'
end
