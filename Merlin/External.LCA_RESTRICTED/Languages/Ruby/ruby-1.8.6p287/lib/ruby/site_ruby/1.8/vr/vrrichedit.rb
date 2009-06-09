###################################
#
# vrrichedit.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2000-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vrcontrol'
require VR_DIR+'rscutil'
require 'Win32API'

module WMsg
  EM_EXGETSEL      = WM_USER+52
  EM_EXLINEFROMCHAR= WM_USER+54
  EM_EXSETSEL      = WM_USER+55
  EM_GETCHARFORMAT = WM_USER+58
  EM_GETEVENTMASK  = WM_USER+59
  EM_GETPARAFORMAT = WM_USER+61
  EM_SETBKGNDCOLOR = WM_USER+67
  EM_SETCHARFORMAT = WM_USER+68
  EM_SETEVENTMASK  = WM_USER+69
  EM_SETPARAFORMAT = WM_USER+71
  EM_FINDTEXTEX    = WM_USER+79
  EM_SETLANGOPTIONS= WM_USER+120
end


class VRRichedit < VRText
=begin
== VRRichedit
This class represents RichEdit Control.
The parent class is VRText and this also has the same methods and event handlers.

=== Overwritten Methods
These methods are overwritten to support over64k texts.
--- getSel
--- setSel(st,en,noscroll=0)
--- char2line(ptr)

=== Methods
--- setTextFont(fontface,height=280,area=SCF_SELECTION)
    Sets the text font. ((|area|)) parameter limits the changed area.
--- getTextFont(selarea=true)
    Gets the text font and its size of the area. 
    If selarea is true, the area is the selected area and the other case, 
    it means the default font.
--- setTextColor(col,area=SCF_SELECTION)
    Sets the color of the text in the area.
--- getTextColor(selarea=true)
    Gets the text color in the area which is same as ((<getTextFont>)).
--- setBold(flag=true,area=SCF_SELECTION)
    Sets or resets the text style into BOLD.
    When ((|flag|)) is true, the text is set to bold style.
--- setItalic(flag=true,area=SCF_SELECTION)
    Sets or resets the text style into ITALIC.
--- setUnderlined(flag=true,area=SCF_SELECTION)
    Sets or resets the text style into UNDERLINED.
--- setStriked(flag=true,area=SCF_SELECTION)
    Sets or resets the text style into STRIKE_OUT.
--- bold?(selarea=true)
    Inspects whether the text style in the area is bold or not.
    If ((|selarea|)) is true, the area is selected area.
--- italic?(selarea=true)
    Inspects whether the text style in the area is italic or not.
--- underlined?(selarea=true)
    Inspects whether the text style in the area is underlined or not.
--- striked?(selarea=true)
    Inspects whether the text style in the area is striked out or not.
--- setAlignment(align)
    Sets text alignment of current paragraph. ((|align|)) can be PFA_LEFT,
    PFA_RIGHT or PFA_CENTER of VRRichedit constansts.
--- bkcolor=(color)
    Sets the background color of the control.
--- selformat(area=SCF_SELECTION)
    Gets the text format in ((|area|)). The return value is an instance of 
    FontStruct defined in rscutil.rb.
--- selformat=(format)
    Set the text format in the selected area. ((|format|)) must be an instance
    of FontStruct.
--- charFromPos(x,y)
    retrieves information about the character closest to a specified point
    in the client area.
    ((x)) is the horizontal coordinate. ((y)) is the vertical coordinate.
    the return value specifies character index by Fixnum.
--- charLineFromPos(x,y)
    retrieves information about the character closest to a specified point
    in the client area.
    ((x)) is the horizontal coordinate. ((y)) is the vertical coordinate.
    the return value specifies character index and line index by Array.
    first item means character index. second item means line index.
=end

  CFM_BOLD      = 0x00000001
  CFM_ITALIC    = 0x00000002
  CFM_UNDERLINE = 0x00000004
  CFM_STRIKEOUT = 0x00000008
  CFM_PROTECTED = 0x00000010
  CFM_LINK      = 0x00000020
  CFM_SIZE      = 0x80000000
  CFM_COLOR     = 0x40000000
  CFM_FACE      = 0x20000000
  CFM_OFFSET    = 0x10000000
  CFM_CHARSET   = 0x08000000

  CFE_BOLD      = 0x0001
  CFE_ITALIC    = 0x0002
  CFE_UNDERLINE = 0x0004
  CFE_STRIKEOUT = 0x0008
  CFE_PROTECTED = 0x0010
  CFE_LINK      = 0x0020
  CFE_AUTOCOLOR = 0x40000000

  SCF_SELECTION = 0x0001
  SCF_WORD      = 0x0002
  SCF_DEFAULT   = 0x0000
  SCF_ALL       = 0x0004     # not valid with SCF_SELECTION or SCF_WORD
  SCF_USEUIRULES= 0x0008

  PFA_LEFT  = 0x0001
  PFA_RIGHT = 0x0002
  PFA_CENTER= 0x0003
  PFM_STARTINDENT = 0x00000001
  PFM_RIGHTINDENT = 0x00000002
  PFM_OFFSET      = 0x00000004
  PFM_ALIGNMENT   = 0x00000008
  PFM_TABSTOPS    = 0x00000010
  PFM_NUMBERING   = 0x00000020
  PFM_OFFSETINDENT= 0x80000000

  module EventMaskConsts
    ENM_CHANGE = 0x00000001
    ENM_UPDATE = 0x00000002
    ENM_SCROLL = 0x00000004
  end

  class EventMask < Flags
    CONSTMOD=EventMaskConsts
   private
    def integer_getter
      @win.sendMessage WMsg::EM_GETEVENTMASK,0,0
    end

    def integer_setter(f)
      @win.sendMessage WMsg::EM_SETEVENTMASK,0,f
    end
  end

  CHARFORMATSIZE=60
  MAX_TAB_STOPS=32
  DEFAULTTABS=[4,8,12,16,20,24,28,32,36,40,44,48,52,56,60,64,68,72,76]

 private
  loadlib = Win32API.new("kernel32","LoadLibrary",["P"],"I")

  libhandle = 0

=begin oop
#This causes GeneralProtectionViolation maybe for the late of RichEdit releasing.
  freelib = Win32API.new("kernel32","FreeLibrary",["I"],"")
  at_exit {
    freelib.call(libhandle)  
  }
=end

  def textlength
    sendMessage WMsg::WM_GETTEXTLENGTH,0,0
  end

  # ooooops !
  if (libhandle=loadlib.call("riched20"))!=0 then
    RICHVERSION=2
    def self.Controltype()
      ["RICHEDIT20A",WStyle::ES_MULTILINE|WStyle::WS_VSCROLL,0x200]#EX_CLIENTEDGE
    end
  elsif (libhandle=loadlib.call("riched32"))!=0 then
    RICHVERSION=1
    def self.Controltype()
      ["RICHEDIT",WStyle::ES_MULTILINE|WStyle::WS_VSCROLL,0x200] #EX_CLIENTEDGE
    end
  else
    raise "no richedit control found"
  end


 public

  def richeditinit
    sendMessage WMsg::EM_SETLANGOPTIONS,0,0
    eventmask.enm_change=true
  end

  def vrinit
    super
    richeditinit
  end

  def text
    len=textlength+1
    buffer = "\0"*len
    sendMessage WMsg::WM_GETTEXT,len,buffer
    buffer[0..-2].gsub(/\r\n/,$/)
  end

 # parameter order is not the same of the return value of getcharformat()
  def setcharformat(area,effects,col=0,mask=0xf800003f,
                   font="System",height=280,off=0,pf=2,charset=128)
    buffer = [CHARFORMATSIZE,mask,effects,
              height,off,col,charset,pf].pack("LLLLLLCC")
    buffer += font.to_s + "\0"
    buffer += "\0"*(CHARFORMATSIZE-buffer.length)
    sendMessage(WMsg::EM_SETCHARFORMAT,area,buffer)
  end

  def getcharformat(mask=0xf800003f,selectionarea=true)
    buffer = [CHARFORMATSIZE,0,0,0,0,0].pack("LLLLLL")
    buffer += "\0"* (CHARFORMATSIZE-buffer.length)
    f = (selectionarea)? 1 : 0
    sendMessage WMsg::EM_GETCHARFORMAT,f,buffer
    return buffer.unpack("LLLLLLCC") << buffer[26..-2].delete("\0")
  end

  def getparaformat
    size=4*7+4*MAX_TAB_STOPS
    buffer = ([size]+Array.new(39,0)).pack("LLLLLLSSL*")
    sendMessage WMsg::EM_GETPARAFORMAT,0,buffer
    return buffer.unpack("LLLLLLSSL*")
  end

  def setparaformat(mask=0x8000003f,numbering=false,startindent=0,rightindent=0, offset=0,align=PFA_LEFT,tabstops=DEFAULTTABS)
    fNumber= if numbering then 1 else 0 end
    size=4*7+4*tabstops.size
    tabcount = (tabstops.size>MAX_TAB_STOPS)? MAX_TAB_STOPS : tabstops.size
    buffer = [
      size,mask,fNumber,startindent,rightindent,offset,align,tabcount
    ].pack("LLLLLLSS")
    buffer += (tabstops + Array.new(32-tabcount,0)).pack("L*")
    sendMessage WMsg::EM_SETPARAFORMAT,0,buffer
  end

## ## ## ##
  def getSel
    charrange = [0,0].pack("LL")
    sendMessage WMsg::EM_EXGETSEL,0,charrange
    return charrange.unpack("LL")
  end
  def setSel(st,en,noscroll=0)
    charrange = [st,en].pack("LL")
    r=sendMessage WMsg::EM_EXSETSEL,0,charrange
    if(noscroll!=0 && noscroll) then
      scrolltocaret
    end
    return r
  end
  def char2line(pt)
    sendMessage WMsg::EM_EXLINEFROMCHAR,0,pt
  end

  def setTextFont(fontface,height=280,area=SCF_SELECTION)
    setcharformat(area,0,0,CFM_FACE|CFM_SIZE,fontface,height)
  end
  def getTextFont(selarea=true)
    r=getcharformat(CFM_FACE|CFM_SIZE,selarea)
    return r[3],r[8]
  end

  def setTextColor(col,area=SCF_SELECTION)
    setcharformat(area,0,col,CFM_COLOR)
  end
  def getTextColor(selarea=true)
    getcharformat(CFM_COLOR,selarea)[5]
  end

  def setBold(flag=true,area=SCF_SELECTION)
    f = (flag)? CFE_BOLD : 0
    setcharformat(area,f,0,CFM_BOLD)
  end

  def setItalic(flag=true,area=SCF_SELECTION)
    f = (flag)? CFE_ITALIC : 0
    setcharformat(area,f,0,CFM_ITALIC)
  end

  def setUnderlined(flag=true,area=SCF_SELECTION)
    f = (flag)? CFE_UNDERLINE : 0
    setcharformat(area,f,0,CFM_UNDERLINE)
  end

  def setStriked(flag=true,area=SCF_SELECTION)
    f = (flag)? CFE_STRIKEOUT : 0
    setcharformat(area,f,0,CFM_STRIKEOUT)
  end

  def bold?(selarea=true)
    r=getcharformat(CFM_BOLD,selarea)[2]
    if (r&CFE_BOLD)==0 then false else true end
  end
  def italic?(selarea=true)
    r=getcharformat(CFM_ITALIC,selarea)[2]
    if (r&CFE_ITALIC)==0 then false else true end
  end
  def underlined?(selarea=true)
    r=getcharformat(CFM_UNDERLINE,selarea)[2]
    if (r&CFE_UNDERLINE)==0 then false else true end
  end
  def striked?(selarea=true)
    r=getcharformat(CFM_STRIKEOUT,selarea)[2]
    if (r&CFE_STRIKEOUT)==0 then false else true end
  end

  def getAlignment
    return self.getparaformat[6]
  end

  def setAlignment(align)
    setparaformat(PFM_ALIGNMENT,false,0,0,0,align)
  end

  def selformat=(f)
    raise "format must be an instance of FontStruct" unless f.is_a?(FontStruct)
    effects = f.style*2 + if f.weight>400 then 1 else 0 end
    height = if f.height>0 then f.height else f.point*2 end
    offset=0
    setcharformat SCF_SELECTION,effects,f.color,0xf800003f,f.fontface,height,
                  offset,f.pitch_family,f.charset
    f
  end

  def selformat(option=SCF_SELECTION)
    r=getcharformat(option)
    weight = if (r[2] & 1)>0 then 600 else 300 end
    style = (r[2]/2) & 0xf  #mask 
    width = r[3]/2          # tekitou....
    point = r[3]/2
    FontStruct.new2([r[8],r[3],style,weight,width,0,0,r[7],r[6]],point,r[5])
  end

  def bkcolor=(col)
    if col then
      sendMessage WMsg::EM_SETBKGNDCOLOR,0,col.to_i
    else
      sendMessage WMsg::EM_SETBKGNDCOLOR,1,0
    end
  end

  def eventmask
    EventMask.new(self)
  end

  def charFromPos(x,y)   # Thanks to yas
    return sendMessage( 0x00D7,0,[x,y].pack('ll'))  #EM_CHARFROMPOS
  end

  def charLineFromPos(x,y)
    pos=charFromPos(x,y)
    ln=char2line(pos)
    return [pos,ln]
  end

end

