###################################
#
# clipboard.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2000-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'sysmod'
require 'Win32API'

class Clipboard
=begin
== Clipboard
A class for handling clipboard.

=== Class Method
--- open(hwnd)
    opens the clipboard and returns the instance.
    When it is used in iterator style, clipboard is automatically closed
    and returns whether it succeed to close clipboard or not.

=== Methods
--- close
    Closes clipboard.
--- enum_formats
    Enums the clipboard formats to get from clipboard.
--- getData(format)
    Gets data in the ((|format|)) from clipboard.
--- setData(format,data)
    Sets data into the clipboard. (clipboard is cleared before set)
--- getText
    same as getData(VR_CF_TEXT)
--- setText(string)
    same as setData(VR_CF_TEXT, ((|string|)) )
=end

  include ClipboardFormat
  GetClipboardData = Win32API.new('user32','GetClipboardData','I','L')
  SetClipboardData = Win32API.new('user32','SetClipboardData','IL','L')
  EnumClipboardFormats = Win32API.new('user32','EnumClipboardFormats','I','I')
  OpenClipboard = Win32API.new('user32','OpenClipboard','L','I')
  CloseClipboard = Win32API.new('user32','CloseClipboard','V','I')
  EmptyClipboard = Win32API.new('user32','EmptyClipboard','V','I')

#
  VR_CF_TEXT = CF_OEMTEXT # modify if you like to use CF_TEXT than CF_OEMTEXT

  def self.open(*args)
    r = self.new(*args)
    if iterator?
      yield r
      r.close
    else
      return r
    end
  end

  def initialize(hwnd)
    @hwnd = hwnd
    @opened = (OpenClipboard.call(hwnd)!=0)
    raise "fail to open clipboard" unless defined?(@opened)
  end

  def close
    @opened = (CloseClipboard.call() == 0)
  end

  def getData(uformat)
    raise "Clipboard not opened" unless defined?(@opened)
    gmem = GetClipboardData.call(uformat.to_i)
    raise "GetData failed" if gmem==0
    GMEM::Get(gmem) if gmem!=0
  end

  def setData(uformat,data)
    raise "Clipboard not opened" unless defined? @opened
    EmptyClipboard.call
    gmem = GMEM::AllocStr(66,data)
    SetClipboardData.call(uformat,gmem)
#    GMEM::Free(gmem)
  end

  def getText
    r=getData(VR_CF_TEXT)
    r.split(/\0/,0)[0]
  end

  def setText(str)
    setData VR_CF_TEXT,str.to_s
  end

  def enum_formats
    r=0
    while true do
      r=EnumClipboardFormats.call(r)
      break if r==0
      yield r 
    end
  end
end


