###################################
#
# vrcontrol.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require VR_DIR+'sysmod'
=begin
= VisualuRuby(tmp) Standard Controls
This file prepares classes of standard controls, panels and menu modules.

<<< handlers.rd

=== Control Container
* ((<VRStdControlContainer>))

=== Standard Controls
* ((<VRButton>))
* ((<VRGroupbox>))
* ((<VRCheckbox>))
* ((<VRRadiobutton>))
* ((<VRStatic>))
* ((<VREdit>))
* ((<VRText>))
* ((<VRListbox>))
* ((<VRCombobox>))

=== Menu
* ((<VRMenu>))
* ((<VRMenuItem>))
* ((<VRMenuUseable>))

=== Panels
* ((<VRPanel>))
* ((<VRBitmapPanel>))
* ((<VRCanvasPanel>))
=end

###########################################
#  Messages and Styles
#

module WMsg
  BN_CLICKED  =       0
  BN_DBLCLICKED  =    5
  BM_GETCHECK =    0xf0
  BM_SETCHECK =    0xf1

  STN_CLICKED =       0
  STN_DBLCLICKED =    1

  LB_ADDSTRING =    0x0180
  LB_INSERTSTRING = 0x0181
  LB_DELETESTRING = 0x0182
  LB_SETSEL =       0x0185
  LB_SETCURSEL =    0x0186
  LB_GETSEL =       0x0187
  LB_GETCURSEL =    0x188
  LB_GETTEXT =      0x189
  LB_GETTEXTLEN =   0x18a
  LB_GETCOUNT   =   0x18b
  LB_SELECTSTRING = 0x18c
  LB_DIR        =   0x18d
  LB_FINDSTRING =   0x18f
  LB_GETSELCOUNT =  0x190
  LB_GETSELITEMS =  0x191
  LB_GETITEMDATA =  0x199
  LB_SETITEMDATA =  0x19a
  LBN_SELCHANGE=1
  LBN_DBLCLK=2
  
  CB_ADDSTRING    = 0x143
  CB_DELETESTRING = 0x144
  CB_DIR          = 0x145
  CB_GETCOUNT     = 0x146
  CB_GETCURSEL =    0x147
  CB_GETLBTEXT =    0x148
  CB_GETLBTEXTLEN = 0x149
  CB_INSERTSTRING = 0x14a
  CB_FINDSTRING   = 0x14c
  CB_SELECTSTRING = 0x14d
  CB_SETCURSEL    = 0x14e
  CB_GETITEMDATA  = 0x150
  CB_SETITEMDATA  = 0x151
  CB_SETEXTENDEDUI =0x155
  CBN_SELCHANGE    = 1
end

module WStyle
  SS_LEFT =           0
  SS_CENTER =         1
  SS_RIGHT =          2

  BS_PUSHBUTTON =     0
  BS_CHECKBOX =       2
  BS_AUTOCHECKBOX =   3
  BS_RADIOBUTTON =    4
  BS_3STATE =         5
  BS_GROUPBOX =       7
  BS_USERBUTTON =     8
  BS_AUTORADIOBUTTON= 9

  ES_MULTILINE   = 4
  ES_PASSWORD    = 0x20
  ES_AUTOVSCROLL = 0x40
  ES_AUTOHSCROLL = 0x80
  ES_READONLY    = 0x800
  ES_WANTRETURN  = 0x1000
  LBS_STANDARD =    0x00a00000 | 1
  LBS_MULTIPLESEL = 8

  CBS_STANDARD    = 0x00a00200 | 3
  CBS_AUTOSCROLL = 0x40
end

###############################################
# Standard Control
#

class VRStdControl < VRControl
  def _vr_cmdhandlers
    unless defined?(@_vr_cmdhandlers)
      @_vr_cmdhandlers={} 
    else
      @_vr_cmdhandlers
    end
  end

  def addCommandHandler(msg,handlername,handlertype,argparsestr)
    @_vr_cmdhandlers={} unless defined?(@_vr_cmdhandlers)
    @_vr_cmdhandlers[msg]=[] unless _vr_cmdhandlers[msg]
    @_vr_cmdhandlers[msg].push [handlername,handlertype,argparsestr]
  end

  def deleteCommandHandler(msg,handlername)
    return false unless @_vr_cmdhandlers[msg]
    @_vr_cmdhandlers.delete_if do |shandler|
      shandler[0] != (PREHANDLERSTR+handlername).intern
    end
  end
end


################################################
# Standard Control Container 
#
module VRStdControlContainer
=begin
== VRStdControlContainer
This module provides a message handler for WM_COMMAND, which calls
the defined methods in a parent window.
VRForm includes this module automatically loading "vrcontrol.rb".
=end

  include VRMessageHandler

  def stdcontrolcontainerinit
    addHandler(WMsg::WM_COMMAND,"wmcommand",MSGTYPE::ARGPASS,nil)
    addEvent WMsg::WM_COMMAND
  end
  
  def vrinit
    super
    stdcontrolcontainerinit
    addNoRelayMessages [WMsg::WM_COMMAND]
  end

  def self_wmcommand(msg)
    id=LOWORD(msg.wParam)
    ct=@controls[id]       # Activated Control
    if !ct then
      if !@_vr_menu then return end
      ct=@_vr_menus[id]
      if !ct then
        return # no handler
      end
    end
    
    messageid=HIWORD(msg.wParam)
    return unless ct._vr_cmdhandlers and ct._vr_cmdhandlers[messageid]

    ct._vr_cmdhandlers[messageid].each do |shandler|
      args=msgarg2handlerarg(shandler[1],msg,shandler[2])
      ct.__send__(shandler[0],*args) if ct.respond_to?(shandler[0])
      msg.retval = controlmsg_dispatching(ct,shandler[0],*args)
    end
  end

end

# add
module VRContainersSet
  include VRStdControlContainer
  INITIALIZERS.push :stdcontrolcontainerinit
end



##########################################
#  Standard Controls
#
#
#  Minimum definition for a Standard Control is
#
#> class VRSampleControl <VRStdControl
#>   WINCLASSINFO = ["ControlClass",additionalStyle,exStyle]
#>   def vrinit()
#>     super 
#>     addCommandHandler(message,eventname,type,argparsestr])
#>   end
#> end
#
# @_vr_cmdhandlers such as {
## message -> [eventname, argtype, unpackstr]
#   WMsg::BN_CLICKED=>["clicked",MSGTYPE::ARGNONE,nil],
#   WMsg::LBN_SELCHANGE=>["selchange",MSGTYPE::ARGNONE,nil]
#}
# ############################################################

class VRButton < VRStdControl
=begin
== VRButton
Button.
Button's caption can be set/got by caption/caption= method of SWin::Window.

=== Event handlers
Buttons invoke the following method of parent window.
--- ????_clicked
    This parent's method is fired when button is pushed.
--- ????_dblclicked
    This parent's method is fired when button is double clicked.
=end

  WINCLASSINFO =  ["BUTTON",WStyle::BS_PUSHBUTTON] 

  def vrinit
    super
    addCommandHandler(WMsg::BN_CLICKED,    "clicked",   MSGTYPE::ARGNONE,nil)
    addCommandHandler(WMsg::BN_DBLCLICKED, "dblclicked",MSGTYPE::ARGNONE,nil)
  end
end

class VRGroupbox < VRStdControl
=begin
== VRGroupbox
Groupbox.

This control can be a container of other window.
You need to extend this class by Container modules, VRStdControlContainer for Standard Controls and VRCommonControlContainer for Common Controls.
=end

  include VRParent
  WINCLASSINFO = ["BUTTON",WStyle::BS_GROUPBOX]
end

class VRCheckbox < VRButton
=begin
== VRCheckbox
Check box.
Parent class is ((<VRButton>))

=== Methods
Checkbox has the following methods.
--- checked?
    Return boolean value whether checkbox is checked or not.
--- check(v)
    Check checkbox as v(=true/false).
=end
  WINCLASSINFO = ["BUTTON",WStyle::BS_AUTOCHECKBOX]
  private
  
  def to_check_state(v)
    res=case v
    when true
      1
    when false,nil
      0
    else
      if v.respond_to?(:to_i)
        v.to_i != 0 ? 1 : 0
      else
        v ? 1 : 0
      end
    end
    return res
  end
  
  public
  
  def checked?
    c=sendMessage(WMsg::BM_GETCHECK,0,0)               #getcheck
    return c!=0
  end
  
  def check(v)
    sendMessage WMsg::BM_SETCHECK, to_check_state(v),0
  end
  
  alias :checked= :check
end

class VRRadiobutton < VRCheckbox
=begin
== VRRadiobutton
Radio Button.

Parent class is ((<VRCheckbox>))
=end
  WINCLASSINFO = ["BUTTON",WStyle::BS_AUTORADIOBUTTON]
end


class VRStatic < VRStdControl
=begin
== VRStatic
Static Control(text).

Text can be access as caption.

=== Event handlers
--- ????_clicked
    Fired when the control is clicked if the control has the style WMsg::STN_CLICKED
=end
  WINCLASSINFO = ["STATIC",0]
  def vrinit
    addCommandHandler(WMsg::STN_CLICKED,  "clicked",0,nil)
  end
end

class VREdit < VRStdControl
=begin
== VREdit
Edit Control.

=== Methods
--- text
    Gets Text in the control. Line break(\r\n) is translated by this method.
--- text=(str)
    Sets Text in the control. Line break is translated by this method.
--- getSel
    Returns [start_pos,end_pos] of the selected area.
--- setSel(st,en,noscroll=0)
    Sets the selected area specified by ((|st|)) as start point and ((|en|))
    as end point and Scrolls to show the area. When ((|noscroll|)) is 1, 
    it doesn't scroll.
--- setCaret(r)
    Sets cursor point at ((|r|))
--- replaceSel(newstr)
    Replaces text in the selected area with ((|newstr|)).
--- readonly=(b)
    Sets readonly flag as boolean ((|b|))
--- limit
    Gets the length limit of this Editbox.
--- limit=(lmt)
    Sets the length limit of this Editbox.
--- modified
    Returns true if the editbox text is modified by user.
--- modified=(f)
    Sets modified flag of the editbox.
--- cut
    Cuts the selected area into clipboard.
--- copy
    Copies the selected area into clipboard.
--- paste
    Paste the clipboard text into the editbox.
--- clear
    Clears selected area text.
    Use ((<cut>)) method to set the text into clipboard.
--- undo
    Undo the previous cut/copy/paste/clear action.
--- charFromPos(x,y)
    retrieves information about the character closest to a specified point
    in the client area.
    ((x)) is the horizontal coordinate. ((y)) is the vertical coordinate.
    the return value specifies character index by Fixnum.
    the character index starts from zero
=== Event handlers
Editboxes invoke the following method of parent window.
--- ????_changed
    This parent's method is fired when editbox text is modified.
=end

  WINCLASSINFO = ["EDIT",WStyle::ES_AUTOHSCROLL,WExStyle::WS_EX_CLIENTEDGE]

  def vrinit
    super
    addCommandHandler(0x300,"changed",MSGTYPE::ARGNONE,nil)
  end

  def getSel
    r=sendMessage(0xb0,0,0)
    return LOWORD(r),HIWORD(r)
  end

  def setCaret(r)
    setSel(r,r)
  end

  def setSel(st,en,noscroll=0)
    sendMessage(0xb1,st,en)
  end

  def replaceSel(newstr)
    sendMessage(0xc2,0,((newstr.empty?)? '' : newstr.to_s))
  end

  def readonly=(b)
    f= (b)? 1 : 0
    sendMessage 0xcf,f,0
  end

  def limit=(lmt)
    sendMessage 0xc5,lmt.to_i,0
  end
  def limit
    sendMessage 0xd5,0,0
  end

  def modified
    r=sendMessage 0xb8,0,0              #GETMODIFY
    if r==0 then false else true end
  end
  alias modified? :modified
  def modified=(f)
    r= if f then 1 else 0 end
    sendMessage 0xb9,r,0
  end

# change linebreak code
# caption,caption= methods is raw methods

  VR_REGEXPNEWLINE="\r\n"
  def text() self.caption.gsub(/#{VR_REGEXPNEWLINE}/,$/); end
  def text=(str) 
    self.caption = str.to_s.gsub(/#{VR_REGEXPNEWLINE}/,"\n").gsub(/\n/,VR_REGEXPNEWLINE)
  end


 # for clipboard and so on..
  def cut
    sendMessage 0x300,0,0  #CUT
  end
  def copy
    sendMessage 0x301,0,0  #COPY
  end
  def paste
    sendMessage 0x302,0,0  #PASTE
  end
  def clear
    sendMessage 0x303,0,0  #CLEAR
  end
  def undo
    sendMessage 0x304,0,0  #UNDO
  end
  
  def charFromPos(x,y)
    r=sendMessage 0x00D7,0,(y << 16) | x  #EM_CHARFROMPOS
    return LOWORD(r)
  end
  
end

class VRText < VREdit
=begin
== VRText
Mutiline Editbox.
Parent class is ((<VREdit>))

=== Methods
--- scrollTo(line.col)
    Scrolls ((|line|)) lines vertically and ((|col|)) columns horizontally.
--- visibleStartLine
    Returns line number of the first visible line in the editbox.
--- countLines
    Returns max line number.
--- getLineLengthOf(ln)
    Returns length of the ((|ln|))-th line.
--- getLineTextOf(ln,maxlen)
    Returns text of ((|ln|))-th line. If maxlen is specified, returned text is
    limited in its length.
--- scrollup
    Scrolls line up in the editbox.
--- scrolldown
    Scrolls line down in the editbox.
--- scrollupPage
    Scrolls page up in the editbox.
--- scrolldownPage
    Scrolls page down in the editbox.
--- scrolltocaret
    Scrolls the caret into view in the editbox.
--- char2line(ptr)
    Returns the index of the line that contains the specified character index.
--- line2char(ln)
    Returns the character index of a line.
--- getCurrentLine
    Returns the current line index.
--- charLineFromPos(x,y)
    retrieves information about the character closest to a specified point
    in the client area.
    ((x)) is the horizontal coordinate. ((y)) is the vertical coordinate.
    the return value specifies character index and line index by Array.
    first item means character index. second item means line index.
    the character index and line index start from zero
=end

  WINCLASSINFO = ["EDIT",
     WStyle::ES_MULTILINE|WStyle::ES_AUTOVSCROLL,WExStyle::WS_EX_CLIENTEDGE
    ]

private
  def c_scroll(r)
    sendMessage 0xb5,r.to_i,0
  end

public

  def countLines
    sendMessage 0x00BA,0,0
  end

  def scrollTo(line,col)
    sendMessage 0x00B6,0,MAKELPARAM(line,col)    #LINESCROLL
  end
  def visibleStartLine()
    sendMessage 0x00CE,0,0             #GETFIRSTVISIBLELINE
  end
  
  def getLineLengthOf(l)
    sendMessage 0xc1,line2char(l.to_i),0          #GETLINELENGTH
  end
  def getLineTextOf(l,textsize=getLineLengthOf(l)+1)
    r=[textsize," "*textsize].pack("Sa*")
    len=sendMessage 0xc4,l.to_i,r
    r[0,len]
  end

  def scrollup() c_scroll 0; end
  def scrolldown() c_scroll 1; end
  def scrollupPage() c_scroll 2; end
  def scrolldownPage() c_scroll 3; end
  
  def scrolltocaret
    sendMessage 0xb7,0,0               #SCROLLCARET
  end
  
  def char2line(ptr)
    sendMessage 0xc9,ptr.to_i,0        #LINEFROMCHAR
  end
  def line2char(l)
    sendMessage 0xbb,l.to_i,0          #LINEINDEX
  end
  def getCurrentLine
    char2line(-1)
  end
  
  def charLineFromPos(x,y)
    r=sendMessage 0x00D7,0,(y << 16) | x  #EM_CHARFROMPOS
    return [LOWORD(r),HIWORD(r)]
  end
  
end


class VRListbox < VRStdControl
=begin
== VRListbox
Listbox.
Listbox has multi strings to select.

=== Methods
--- addString(idx,str)
    Adds ((|str|)) String at the ((|idx|)).
    When ((|idx|)) is omitted, ((|str|)) is added at the bottom.
--- deleteString(idx)
    Deletes string at ((|idx|))
--- countStrings
    Returns the count of its strings.
--- clearStrings
    Deletes all strings.
--- eachString
    Yields each string.
--- setListStrings(strarray)
    Sets array of string ( ((|strarray|)) ) as the listbox strings.
--- selectedString
    Returns the selected index.
--- select(idx)
    Selects ((|idx|))-th string.
--- getTextOf(idx)
    Retrieves the text of ((|idx|))-th string.
--- setDir(fname,opt=0)
    Sets the filenames specified by ((|fname|)) to the listbox.
    ((|opt|)) specifies the attributes of the files to be added.
--- findString(findstr,start=0)
    Finds ((|findstr|)) in the listbox strings and returns the its index.
    ((|start|)) specifies the index to start finding.
--- getDataOf(idx)
    Retrieves the 32bit value associated with the ((|idx|))-th string.
--- setDataOf(idx,data)
    Associates the 32bit value with the ((|idx|))-th string.
=== Event handlers
--- ????_selchanged
    Fired when the selected string is changed.
=end

  WINCLASSINFO = ["LISTBOX",WStyle::LBS_STANDARD,WExStyle::WS_EX_CLIENTEDGE]

  def vrinit
    super
    addCommandHandler(WMsg::LBN_SELCHANGE, "selchange",MSGTYPE::ARGNONE,nil)
    set_liststrings
  end
  
 private
  def set_liststrings
    clearStrings
    
    return unless defined? @_vr_init_items
    0.upto(@_vr_init_items.size-1) do |idx|
      addString idx,@_vr_init_items[idx]
    end if @_vr_init_items
  end

 public
  def addString(*arg)
    if arg[0].is_a?(Integer) then
      sendMessage WMsg::LB_INSERTSTRING,arg[0],arg[1].to_s
    else
      sendMessage WMsg::LB_ADDSTRING,0,arg[0].to_s
    end
  end

  def deleteString(idx)
    sendMessage WMsg::LB_DELETESTRING,idx.to_i,0
  end
  def countStrings
    sendMessage WMsg::LB_GETCOUNT,0,0
  end

  def clearStrings
    c=getCount
    (1..c).each do |i|
      deleteString 0           #delete #0 c-times
    end
  end
  
  def eachString
    c=getCount
    0.upto(c-1) do |i|
      yield getString(i)
    end
  end
  
  def setListStrings(sarray)
    @_vr_init_items=sarray
    if hWnd>0 then set_liststrings end
  end

  def selectedString
    sendMessage(WMsg::LB_GETCURSEL,0,0)
  end

  def getTextOf(idx)
    len = sendMessage(WMsg::LB_GETTEXTLEN,idx.to_i,0) #LB_gettextlen
    raise "No such index(#{idx}) in the Listbox"if len<0
    str=" " * len
    sendMessage(WMsg::LB_GETTEXT,idx.to_i,str)          #LB_getText
    str
  end

  def select(idx)
    sendMessage(WMsg::LB_SETCURSEL,idx.to_i,0)
  end

  def setDir(fname,opt=0)
    sendMessage(WMsg::LB_DIR,opt.to_i,fname.to_s)
  end

  def findString(findstr,start=0)
    sendMessage(WMsg::LB_FINDSTRING,start.to_i,findstr.to_s)
  end

  def getDataOf(idx)
    sendMessage(WMsg::LB_GETITEMDATA,idx.to_i,0)
  end
  def setDataOf(idx,data)
    sendMessage(WMsg::LB_SETITEMDATA,idx.to_i,data.to_i)
  end

  def eachSelected
    count = sendMessage(WMsg::LB_GETSELCOUNT,0,0)
  
    if count<1 then
      yield selectedString
      return
    end

    buffer = '\0\0\0\0' * count
    sendMessage(WMsg::LB_GETSELITEMS,count,buffer)

    buffer.unpack("I*")[0,count].each do |i|
      yield i
    end
  end

end

class VRCombobox < VRStdControl
=begin
== VRCombobox
Combobox is like Listbox, but Only one string is selected.

=== Methods
--- addString(idx,str)
    Adds ((|str|)) String at the ((|idx|)). 
    When ((|idx|)) is omitted, ((|str|)) is added at the bottom.
--- deleteString(idx)
    Deletes string at ((|idx|))
--- countStrings
    Returns the count of its strings.
--- clearStrings
    Deletes all strings.
--- eachString
    Yields each string.
--- setListStrings(strarray)
    Sets array of string ( ((|strarray|)) ) as the combobox strings.
--- selectedString
    Returns the selected index.
--- select(idx)
    Selects ((|idx|))-th string.
--- getTextOf(idx)
    Retrieves the text of ((|idx|))-th string.
--- setDir(fname,opt=0)
    Sets the filenames specified by ((|fname|)) to the combobox.
    ((|opt|)) specifies the attributes of the files to be added.
--- findString(findstr,start=0)
    Finds ((|findstr|)) in the combobox strings and returns the its index.
    ((|start|)) specifies the index to start finding.
--- getDataOf(idx)
    Retrieves the 32bit value associated with the ((|idx|))-th string.
--- setDataOf(idx,data)
    Associates the 32bit value with the ((|idx|))-th string.
=== Event handlers
--- ????_selchanged
    Fired when the selected string is changed.
=end

  WINCLASSINFO = ["COMBOBOX",WStyle::CBS_STANDARD] 

  def vrinit
    super
    set_liststrings
    addCommandHandler(WMsg::CBN_SELCHANGE, "selchange",MSGTYPE::ARGNONE,nil)
  end

 private
  def set_liststrings
    clearStrings
    return unless defined? @_vr_init_items
    0.upto(@_vr_init_items.size-1) do |idx|
      addString idx,@_vr_init_items[idx]
    end if @_vr_init_items
  end

 public
  def addString(*arg)
    if arg[0].is_a?(Integer) then
      sendMessage WMsg::CB_INSERTSTRING,arg[0],arg[1].to_s
    else
      sendMessage WMsg::CB_ADDSTRING,0,arg[0].to_s
    end
  end


  def deleteString(idx)
      sendMessage WMsg::CB_DELETESTRING,idx,0
  end
  def countStrings
      sendMessage WMsg::CB_GETCOUNT,0,0
  end

  def clearStrings
    c=getCount
    (1..c).each do |i|
      deleteString 0           #delete #0 c-times
    end
  end
  
  def eachString
    c=getCount
    0.upto(c-1) do |i|
      yield getString(i)
    end
  end

  def setListStrings(sarray)
    @_vr_init_items=sarray
    if hWnd>0 then set_liststrings end
  end

  def selectedString
    sendMessage(WMsg::CB_GETCURSEL,0,0)
  end

  def getTextOf(idx)
    len = sendMessage(WMsg::CB_GETLBTEXTLEN,idx,0)   #CB_gettextlen
    raise "No such index(#{idx}) in the Combobox"if len<0
    str=" " * len
    sendMessage(WMsg::CB_GETLBTEXT,idx,str)          #CB_getText
    str
  end

  def select(idx)
    sendMessage(WMsg::CB_SETCURSEL,idx,0)
  end

  def setDir(fname,opt=0)
    sendMessage(WMsg::CB_DIR,opt.to_i,fname.to_s)
  end

  def findString(findstr,start=0)
    sendMessage(WMsg::CB_FINDSTRING,start.to_i,findstr.to_s)
  end

  def getDataOf(idx)
    sendMessage(WMsg::CB_GETITEMDATA,idx.to_i,0)
  end
  def setDataOf(idx,data)
    sendMessage(WMsg::CB_SETITEMDATA,idx.to_i,data.to_i)
  end
end

#####################################
#  Editable Combobox  
#  programmed by USHIWATARI-san
module WMsg
  CBN_EDITCHANGE = 5
end
module WStyle
  CBS_DROPDOWN    = 0x00a00200 | 2
end

class VREditCombobox < VRCombobox
=begin
== VREditCombobox
Editable Combobox.
This is a kind of Combobox where you can edit the text.

=== Methods
This has also VRCombobox's methods.
--- text
    Returns the current text.

=== Event handlers
--- changed
    Fired when the text is changed.
=end


  WINCLASSINFO = ["COMBOBOX",WStyle::CBS_DROPDOWN | WStyle::CBS_AUTOSCROLL ] 
  def vrinit
    super
    addCommandHandler(WMsg::CBN_EDITCHANGE, "changed",MSGTYPE::ARGNONE,nil)
  end

  def text
    self.caption
  end
  def text=(str) 
    self.caption = str.gsub(/([^\r])\n/,'\1'+VREdit::VR_REGEXPNEWLINE)
  end

end



#####################################
#  Menu
#
class VRMenu
=begin
== VRMenu
Menu.

=== Methods
--- append(caption,state)
    Adds a new menu item at the last with the caption of ((|caption|)) and 
    the state of ((|state|)). See ((<state=>)) about ((|state|)).
--- insert(ptr,caption,name,state)
    Inserts new item at ((|ptr|)).
--- delete(id)
    Deletes menu. ((|id|)) can be a instance of VRMenuItem or a menu-id integer.
--- count
    Counts the menu items.
--- set(sarr)
    Sets the menu according to the ((|sarr|)) which is a structured array 
    of strings such like
    (({  [ ["&File", [["&Open","open"],["E&xit","exit"]],  }))
    (({       ["&Edit",["&Copy","copy]]    }))
    (({    ] ]  }))
      Caption : TEXT
      Name : TEXT
      Menu : [Caption, Name|Menu]

    When a menu is clicked, ((|Name|))_clicked method is fired.

=end

  attr_reader :menu, :parent

  SEPARATOR=["sep","_vrmenusep",0x800]

  def initialize(menu,parent)
    @menu=menu
    @parent=parent
  end

  def append (*arg)
    m=VRMenuItem.new(@menu)
    id=@parent.newControlID
    if arg[2] then
      @menu.append arg[0],id,arg[2]
    else
      @menu.append arg[0],id
    end
    @parent.registerMenu(m,arg[1],id,arg[0])
    m
  end
  def insert (ptr,*arg)
    m=VRMenuItem.new(@menu)
    id=@parent.newControlID
    if arg.size>2 then
      @menu.insert arg[0],id,ptr,arg[2]
    else
      @menu.insert arg[0],id,ptr
    end
    @parent.registerMenu(m,arg[1],id,arg[0])
    m
  end
  def delete (id)
    if id.is_a?(VRMenuItem) then
      @menu.delete id.etc
    else
      @menu.delete id
    end
  end
  def count (*arg)
    @menu.count(*arg)
  end

  def set(sarr)
    sarr.each do |item|
      if item[1].is_a?(Array) then
        m=@parent.newMenu(true)
        m.set(item[1])
        @menu.append item[0],m.menu,item[2]
      elsif item[1].is_a?(VRMenu) then
        @menu.append item[0],item[1].menu,item[2]
      else
        append(*item)
      end
    end
    self
  end
end

VRMenuTemplate = Struct.new("VRMenuTemplate",:caption,:item,:state)
=begin
== VRMenuTemplate
If you don't like using Array for VRMenu#set, this will help you.

=== Methods
--- caption
--- caption=
    Represents the caption of the menu.
--- item
--- item=
    When ((|item|)) is String, ((|item|)) means the name of menu item.
    This name is used for the event handler name like ((|item|))_clicked.
    ((|item|)) can be also an instance of VRMenuTemplate. In this case ((|item|))
    represents its submenu.
=== example
  a=VRMenuTemplate.new
  a.caption,a.item = "Test1","test1"
  
  b=VRMenuTemplate.new("Test2","test2")
  c=VRMenuTemplate.new("Test3","test3",VRMenuItem::GRAYED)

  d1=VRMenuTemplate.new("Test4-1","test41")
  d2=VRMenuTemplate.new("Test4-2","test42")
  d3=VRMenuTemplate.new("Test4-3","test43")

  d = VRMenuTemplate.new
  d.caption , d.item = "Test4" , [d1,d2,d3]
=end

class VRMenuItem
=begin
== VRMenuItem
This is a wrapper of SWin::Menu
=== Methods
--- new(menu)
    ((|menu|)) must be the instance of SWin::Menu.
--- state
    Returns the state of the menu item.
--- state=
    Sets the state of the menu item.
    state means
    * 0 MF_ENABLED
    * 1 MF_GRAYED
    * 2 MF_DISABLED
    * 8 MF_CHECKED
--- checked?
    Returns true if the menu item is checked. 
--- checked=
    Sets true/false whether the menu item is checked or not.
--- modify(text)
    Modifies the text of the menu item.
=end
  attr_accessor :name, :etc

  GRAYED=1
  DISABLED=2
  CHECKED=8

  def initialize(menu)
    @menu=menu
  end
  def _vr_cmdhandlers
    {0=>[["clicked",MSGTYPE::ARGNONE,nil]],    # for menu
     1=>[["clicked",MSGTYPE::ARGNONE,nil]]}    # for key accelerator
  end

  def state= (st)
    @menu.setState(@etc,st)
  end
  def state
    @menu.getState(@etc)
  end
  def checked=(f)
    @menu.setChecked(@etc,f)
  end
  def checked?
    (self.state&8)==8
  end
  def modify(text)
    @menu.modify @etc,text
  end
end

module VRKeyAcceleratorUseable   # Thanks to Yukimisake-san.
  def menuTransTable(hsh) 
    tVirt = {"Ctrl" => 0x08,"Shift" => 0x04,"Alt" => 0x10}
    tVkey = {"F1"=>0x70,"F2"=>0x71,"F3"=>0x72,"F4"=>0x73,
             "F5"=>0x74,"F6"=>0x75,"F7"=>0x76,"F8"=>0x77,
             "F9"=>0x78,"F10"=>0x79,"F11"=>0x7a,"F12"=>0x7b,
             "Insert"=>0x2d,"Delete"=>0x2e,"PageUp"=>0x21,"PageDown"=>0x22,
             "End"=>0x23,"Home"=>0x24
    }
    r = []
    hsh.each{|k,v|
      fVirt = 1
      key = nil
      if txt = v[/\t.*/] then
        a = txt.strip!.split(/\+/)
        a[0,a.size-1].each{|ii|
            raise "Iregal Key" unless n = tVirt[ii]
            fVirt += n } if a.size > 1
        if (s = a[a.size-1]).size > 1 then
          unless key = tVkey[s] then raise "Iregal Key" end
        else
          key = (s.upcase)[0]
        end
      end
      if key then
        r << fVirt
        r << key
        r << k
      end
    }
    r
  end
end

module VRMenuUseable
=begin
== VRMenuUseable
Include this module to use menus.

=== Methods
--- newMenu(popup=false)
    Creates a new ((<VRMenu>)). If popup is true, the created menu is popup menu.
--- newPopupMenu
    Same as newMenu(true)
--- setMenu(menu)
    Sets the menu ((<VRMenu>)) to the window.
--- showPopup(popupmenu)
    Shows ((|popupmenu|)) at the cursor position
=end

  include VRKeyAcceleratorUseable

  SetForegroundWindow = Win32API.new("user32","SetForegroundWindow","I","I")

  def registerMenu(m,name,id,mcaption="")
    m.etc= id
    instance_eval("@"+name+"=m")
    @_vr_menus= {} unless defined?(@_vr_menus)
    @_vr_menucaptions= {} unless defined?(@_vr_menucaptions)
    @_vr_menus[id]= m
    @_vr_menucaptions[id] = mcaption
    m.name = name
    m
  end

  def newMenu(popup=false)
    if popup then
      menu=@screen.factory.newpopup
    else
      menu=@screen.factory.newmenu
    end
    VRMenu.new(menu,self)
  end

  def newPopupMenu    # contributed by Yuya-san
    return self.newMenu(true)
  end

  def setMenu(menu,keyacc=false)
    SetForegroundWindow.call(self.hWnd)
    if menu.is_a?(SWin::Menu) then  
      super menu
      return
    end

    @_vr_menu=menu
    @menu=menu
    super menu.menu
    if keyacc then
      tb = menuTransTable(@_vr_menucaptions)
      SWin::Application.setAccel(self,tb) 
    end
  end

  def showPopup(menu)
    SetForegroundWindow.call(self.hWnd)
    if menu.is_a?(SWin::Menu) then  
      m=menu
    else     # asumes it is VRMenu
      m=menu.menu
      @_vr_menu = menu
    end
    popupMenu m,*Cursor.get_screenposition
  end

  def setMenuByArray(arr)
    self.setMenu newMenu(arr)
  end

end


##############################
# Other Controls
#

module WStruct
  SCROLLINFO="lllllll"
end

class VRScrollbar < VRControl 
  WINCLASSINFO = ["SCROLLBAR",0]
  attr_accessor :smallstep, :longstep

  def vrinit
    setRange(0,100)
    self.pagesize=10
    self.position=50
    @smallstep, @longstep = 1,10
  end
  
  def setRange(min,max)
    sendMessage 0xe6,min.to_i,max.to_i
  end

  def position=(val)
    sendMessage 0xe0,val.to_i,1
  end
  def position
    sendMessage 0xe1,0,0
  end

  def pagesize=(p)
    si=[4*7,2,0,0,p.to_i,0,0].pack WStruct::SCROLLINFO
    sendMessage 0xe9,1,si  # SBM_SETSCROLLINFO
  end
  def pagesize
    si=[4*7,2,0,0,0,0,0].pack WStruct::SCROLLINFO
    sendMessage 0xea,1,si  # SBM_GETSCROLLINFO
    si.unpack(WStruct::SCROLLINFO)[4]
  end
end

class VRHScrollbar < VRScrollbar
  WINCLASSINFO = ["SCROLLBAR",0]
end
class VRVScrollbar < VRScrollbar
  WINCLASSINFO = ["SCROLLBAR",1]
end

module WMsg
  WM_HSCROLL=276
  WM_VSCROLL=277
end

module VRScrollbarContainer
  def scrollbarcontainerinit
    addHandler WMsg::WM_HSCROLL,"vrscroll",MSGTYPE::ARGINTINT,nil
    addHandler WMsg::WM_VSCROLL,"vrscroll",MSGTYPE::ARGINTINT,nil
    acceptEvents [ WMsg::WM_HSCROLL,WMsg::WM_VSCROLL]
    @_vr_scrollbars={}
  end
  def vrinit
    super
    scrollbarcontainerinit
  end

  def searchscrollbar(hwnd)
    @_vr_scrollbars=Hash.new unless defined? @_vr_scrollbars
    srl = @_vr_scrollbars[hwnd]   # false : to be ignored window, 0: form
    if srl==nil then
      @controls.each_value do |v| 
        if v.is_a?(SWin::Window) and v.hWnd==hwnd then
          @_vr_scrollbars[hwnd] = srl = v
          break
        end
      end # @controls.each_value
    end
    srl
  end

=begin
          elsif defined?(VRTrackbar) and v.is_a?(VRTrackbar) then
            @_vr_scrollbars[hwnd] = srl = v
          elsif defined?(VRUpdown) and v.is_a?(VRUpdown) then
            @_vr_scrollbars[hwnd] = srl = v
          else 
            @_vr_scrollbars[hwnd] = 0
            srl = self
=end

  def self_vrscroll(wparam,hwnd)
    srl = searchscrollbar(hwnd)
    return if srl.nil?

    if srl.is_a?(VRScrollbar) then
      code=LOWORD(wparam)
      if code==4 then
        pos=HIWORD(wparam)
      else
        pos  = srl.position
      end
      case code
      when 0 
        srl.sendMessage 224,pos-srl.smallstep,1
      when 1
        srl.sendMessage 224,pos+srl.smallstep,1
      when 2
        srl.sendMessage 224,pos-srl.longstep,1
      when 3
        srl.sendMessage 224,pos+srl.longstep,1
      when 4
        srl.sendMessage 224,pos,1
      end
    end
    controlmsg_dispatching(srl,"changed")
  end
end

# add
module VRContainersSet
  include VRScrollbarContainer
  INITIALIZERS.push :scrollbarcontainerinit
end


########################
#
# Control using VRPanel
#
class VRBitmapPanel < VRPanel
=begin
== VRBitmapPanel
Bitmap panel that can display static bitmap.

=== Methods
--- loadFile(filename)
    Loads ((|filename|)) as bitmap file.
--- createBitmap(info,bmp)
    Creates an bitmap from ((|info|)) and ((|bmp|)).
--- bmp
    The instance of displaying SWin::Bitmap.
=end
  attr_accessor :bmp

  include VRDrawable
  def vrinit
    super
    @bmp=nil
  end

  def loadFile(fname)
    @bmp=SWin::Bitmap.loadFile fname
    self.refresh
  end

  def createBitmap(info,bmp)
    @bmp=SWin::Bitmap.createBitmap info,bmp
    self.refresh
  end

  def self_paint
    drawBitmap @bmp if @bmp.is_a?(SWin::Bitmap)
  end
end


class VRCanvasPanel < VRPanel
=begin
== VRCanvasPanel
Bitmap Canvas panel that can display drawable bitmap.
=== Methods
--- createCanvas(w,h,color=0xffffff)
    Creates a new canvas dimensioned ( ((|w|)),((|h||)) ) and
    colored ((|color|)) on the background.
--- canvas
    Returns the instance of SWin::Canvas to draw it.
=end

  attr_reader :canvas 

  include VRDrawable
  
  def createCanvas(w,h,color=0xffffff)
    @canvas=@screen.factory.newcanvas(w,h)
    @canvas.setBrush(color)
    @canvas.setPen(color)
    @canvas.fillRect(0,0,w,h)
    @canvas.setPen(0x0)
  end

  def vrinit
    super
    @canvas=nil
  end

  def self_paint
    bitblt @canvas if @canvas
  end
end

if VR_COMPATIBILITY_LEVEL then
  require VR_DIR + 'compat/vrcontrol.rb'
end
