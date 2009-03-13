###################################
#
# sysmod.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

require 'Win32API'

=begin
= sysmod.rb
Win32 Utilities
=end

module MEMCOPY  #Thanks, ruby-chan :)
=begin
== MEMCOPY
This is for copying string and memory each other.

=== Class Methods
--- MEMCOPY::Str2Mem(dst,src,len)
    Copies src (String) into memory at address dst (Integer).
--- MEMCOPY::Mem2Str(dst,src,len)
    Copies contents from address dst (Integer) into src (String).
=end
  begin
    ## Windows2000
    Pmemcpy1 = Win32API.new("NTDLL", "memcpy", ['P', 'L', 'I'], 'L')
    Pmemcpy2 = Win32API.new("NTDLL", "memcpy", ['L', 'P', 'I'], 'L')
  rescue
    ## Windows95/98
    Pmemcpy1 = Win32API.new("CRTDLL", "memcpy", ['P', 'L', 'I'], 'L')
    Pmemcpy2 = Win32API.new("CRTDLL", "memcpy", ['L', 'P', 'I'], 'L')
  end

  def MEMCOPY::Str2Mem(dst,src,len)
    Pmemcpy2.call(dst,src,len)
  end
  def MEMCOPY::Mem2Str(dst,src,len=dst.length)
    Pmemcpy1.call(dst,src,len)
  end
end

module GAtom
=begin
== GAtom
This is for utilizing Global Atoms.

--- GAtom::Add(text)
    Create a new global atom whose name is ((|text|)).
--- GAtom::Delete(atm)
    Delete ((|atm|)) of global atom.
--- GAtom::GetName(atm)
    Returns name of ((|atm|)) of global atom.
=end

  GlobalAddAtom = Win32API.new("kernel32","GlobalAddAtom",["P"],"I")
  GlobalDeleteAtom=Win32API.new("kernel32","GlobalDeleteAtom",["I"],"I")
  GlobalGetAtomName=Win32API.new("kernel32","GlobalGetAtomName",["I","P","I"],"I")

  def GAtom::Add(text) (0xffff & GlobalAddAtom.call(text.to_s)) end
  def GAtom::Delete(atm) GlobalDeleteAtom.call(atm) end

  def GAtom::GetName(atm)
    buffer=" "*256
    r = GlobalGetAtomName.call(atm,buffer,256)
    buffer[0,r]
  end
end

module GMEM
=begin
== GMEM
This is for utilizing Global memories.

=== Class Methods
--- GMEM::AllocStr(mode,text)
    Allocates a global memory and set its contents as ((|text|)).
--- GMEM::Set(gmem,text,len)
    Sets the contents of global memory gmem as ((|text|)).
--- GMEM::Get(gmem)
    Gets the contents of gmem.

--- GMEM::Alloc(size)
    Allocates a global memory.
--- GMEM::Lock(gmem)
    Locks gmem to access it.
--- GMEM::Unlock(gmem)
    Unlocks gmem.
--- GMEM::Free(gmem)
    Frees gmem.
--- GMEM::Size(gmem)
    Returns the size of gmem.
=end

  #GlobalAlloc(method,size)
  GlobalAlloc = Win32API.new("kernel32","GlobalAlloc",["I","I"],"I")

  GlobalLock = Win32API.new("kernel32","GlobalLock",["I"],"I")
  GlobalUnlock = Win32API.new("kernel32","GlobalUnlock",["I"],"")
  GlobalFree = Win32API.new("kernel32","GlobalFree",["I"],"I")
  GlobalSize = Win32API.new("kernel32","GlobalSize",["I"],"I")
  GlobalFlags= Win32API.new("kernel32","GlobalFlags",["I"],"I")

  def GMEM::Alloc(*arg)  GlobalAlloc.call(*arg) end
  def GMEM::Lock(*arg)   GlobalLock.call(*arg) end
  def GMEM::Unlock(*arg) GlobalUnlock.call(*arg) end
  def GMEM::Free(*arg)   GlobalFree.call(*arg) end
  def GMEM::Size(*arg)   GlobalSize.call(*arg) end

  def GMEM::AllocStr(mode,text)
    mem = GlobalAlloc.call(mode,text.length+1)
    Set(mem,text,text.size+1)
    mem
  end

  def GMEM::Set(hglb,text,siz=nil)
    len= if siz then siz.to_i else [size,Size(hglb)].min end
    lp = GlobalLock.call(hglb)
    MEMCOPY::Str2Mem(lp,text.to_s,len)
    GlobalUnlock.call(hglb)
  end
  def GMEM::Get(hglb)
    lp = GlobalLock.call(hglb)
    len = GlobalSize.call(hglb)
#p format "%x %d, %x",hglb, lp, GlobalFlags.call(hglb)
    raise "Memory not accessible" if len==0
    str = " " * (len+1)
    MEMCOPY::Mem2Str(str,lp,len)
    GlobalUnlock.call(hglb)
    str
  end
end

module SMSG
=begin
== SMSG
This is for Windows Messaging.

=== Class Methods
--- sendMessage(hwnd,uMsg,wParam,lParam)
    Calls SendMessage(). see Windows SDK document.
    ((|lParam|)) can be both type of Integer and String
--- postMessage(hwnd,uMsg,wParam,lParam)
    Calls PostMessage(). see Windows SDK document.
    ((|lParam|)) can be both type of Integer and String
=end

  SendMessage = Win32API.new("user32","SendMessage",["I","I","I","I"],"I")
  PostMessage = Win32API.new("user32","PostMessage",["I","I","I","I"],"I")
  SendMessage2 = Win32API.new("user32","SendMessage",["I","I","I","P"],"I")
  PostMessage2 = Win32API.new("user32","PostMessage",["I","I","I","P"],"I")

  def SMSG.sendMessage(*arg)
    if arg[3].is_a?(Integer) then
      SendMessage.call(*arg)
    else
      SendMessage2.call(*arg)
    end
  end

  def SMSG.postMessage(*arg)
    if arg[3].is_a?(Integer) then
      PostMessage.call(*arg)
    else
      PostMessage2.call(*arg)
    end
  end
end

module Cursor
=begin
== Cursor
This is for System cursor handling.

=== Class Methods
--- get_screenposition
    Returns x,y in the screen coordinate.
--- set_screenposition(x,y)
    Sets cursor position into (x,y) in the screen coordinate.
=end

  GetCursorPos = Win32API.new("user32","GetCursorPos","P","I")
  SetCursorPos = Win32API.new("user32","SetCursorPos","II","I")
  POINTSTRUCT="ll"

  def self.get_screenposition
    r=[0,0].pack(POINTSTRUCT)
    GetCursorPos.call(r)
    r.unpack(POINTSTRUCT)
  end
  
  def self.set_screenposition(x,y)
    SetCursorPos.call(x.to_i,y.to_i)
  end
end

module LastError
=begin
== LastError
This is for handling LastError.

=== Class Methods
--- set(ecode)
    Sets lasterror code as ((|ecode|)).
--- code
--- get
    Gets last error code.
--- code2msg(ecode,msgarg=0)
    Get the error message of ((|ecode|)). 
    ((|msgarg|)) is optional argument for message formatting.
--- message(arg=0)
    Returns last error message.
=end

  GetLastError = Win32API.new('kernel32','GetLastError','V','L')
  SetLastError = Win32API.new('kernel32','SetLastError','L','V')
  FormatMessage = Win32API.new('kernel32','FormatMessageA','LPLLPLP','L')
  FORMAT_MESSAGE_FROM_SYSTEM = 4096

  def self.set(ecode)
    SetLastError.call(ecode)
  end

  def self.code
    GetLastError.call()
  end
  def self.get() self.code end

  def self.code2msg(scode,msg=0)
    buffer = " "*(2048)
    len = FormatMessage.call FORMAT_MESSAGE_FROM_SYSTEM,0,scode,0,
                             buffer,buffer.size,msg
    buffer[0,len]
  end

  def self.message(msg=0)
    self.code2msg(self.code,msg)
  end
end

module ClipboardFormat
  CF_TEXT    =  1 
  CF_OEMTEXT =  7
  CF_HDROP   = 15
end
