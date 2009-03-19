###################################
#
# vrclipboard.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2000-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

module VRClipboardObserver
=begin
== VRClipboardObserver
module for clipboard observing.

=== Handlers
--- self_drawclipboard
    Fired when clipboard is changed.
=end

  include VRMessageHandler

  SetClipboardViewer = Win32API.new('user32','SetClipboardViewer','L','L')
  ChangeClipboardChain = Win32API.new('user32','ChangeClipboardChain','LL','I')
  WM_CHANGECBCHAIN = 781
  WM_DRAWCLIPBOARD = 776

  def clipboardobserverinit
    @cbchainnext = SetClipboardViewer.call(self.hWnd)
    addHandler WM_DRAWCLIPBOARD,"drawclipboard",MSGTYPE::ARGNONE,nil
    addHandler WM_CHANGECBCHAIN,"vrchangecbchain",MSGTYPE::ARGINTINT,nil
    addHandler WMsg::WM_DESTROY,"vrcbdestroy",MSGTYPE::ARGNONE,nil
    acceptEvents [WM_DRAWCLIPBOARD,WM_CHANGECBCHAIN,WMsg::WM_DESTROY]
  end

  def vrinit
    super
    clipboardobserverinit
  end

  def self_vrchangecbchain(hwndremove,hwndnext)
    @cbchainnext=hwndnext if hwndremove == @cbchainnext
  end

  def self_vrcbdestroy
    ChangeClipboardChain.call self.hWnd,@cbchainnext
  end
end

