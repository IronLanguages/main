###################################
#
# vrcomctl.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'

=begin
= VisualuRuby(tmp) Common Controls.
This file prepares classes of common controls, panels and menu modules.

<<< handlers.rd

=end

require 'Win32API'

begin
  Win32API.new("comctl32","InitCommonControlsEx","P","").call [8,0x400].pack("LL")
  VR_OLDCOMCTL=false
rescue
#  puts "comctl too old to use rebar and so on"
  VR_OLDCOMCTL=true
end

module WMsg
  NFR_ANSI=1
  NFR_UNICODE=2
  NF_QUERY=3
  NF_REQUERY=4
  WM_NOTIFYFORMAT=85
end

module WStyle
  CCS_TOP              =  0x00000001
  CCS_NOMOVEY          =  0x00000002
  CCS_BOTTOM           =  0x00000003
  CCS_NORESIZE         =  0x00000004
end

module WStruct
  NMHDR="UUU"
end


##################
#
#  The following control windows send WM_NOTIFY message to their parent
#  This definition is for handling them.
#

module VRComCtlContainer
=begin
== VRComCtlContainer
This module provides a message handler for WM_NOTIFY, which calls
defined method in a parent window.
VRForm includes this module automatically loading "vrcomctl.rb".
=end

  include VRMessageHandler

  def comctlcontainerinit
    addHandler(WMsg::WM_NOTIFY,"wmnotify", MSGTYPE::ARGPASS, nil)
    addHandler(WMsg::WM_NOTIFYFORMAT,"wmnotifyformat",MSGTYPE::ARGINTINT,nil)
    addEvent WMsg::WM_NOTIFY
    addEvent WMsg::WM_NOTIFYFORMAT

    addNoRelayMessages [WMsg::WM_NOTIFY,WMsg::WM_NOTIFYFORMAT]
  end

  def vrinit
    super
    comctlcontainerinit
  end

  def self_wmnotifyformat(parent,command)
    SKIP_DEFAULTHANDLER[1]
  end

  def self_wmnotify(msg)

    orig_arg= @screen.application.cstruct2array(msg.lParam,WStruct::NMHDR)

    id=msg.wParam
    ct=@controls[id]       # Activated Control
    if !ct then return end # Ignore message passed through childwin 
    r=nil
    return unless ct.is_a?(VRNotifyControl) and # Added by yukimi_sake
                  ct._vr_ntfyhandlers and ct._vr_ntfyhandlers[orig_arg[2]]
    ct._vr_ntfyhandlers[orig_arg[2]].each do |shandler|
      args=msgarg2handlerarg(shandler[1],msg,shandler[2])
      if shandler[1] == MSGTYPE::ARGSTRUCT and args.size>0 and
          ct.respond_to?("_vr_notifyarg") then

        necessary_arg=ct._vr_notifyarg[orig_arg[2]]
        if necessary_arg then
          n=necessary_arg.size-1
          0.upto(n) do |idx|
            args[idx+3]=nil if necessary_arg[idx,1]=='F'
          end
        end
        args=args[3..-1].compact if args.size > 3
      end

      r = ct.__send__(shandler[0],*args) if ct.respond_to?(shandler[0])
      rr = controlmsg_dispatching(ct,shandler[0],*args)
      r = rr if ( rr.kind_of?( SKIP_DEFAULTHANDLER ) )
    end
    r
  end
end

module VRContainersSet
  include VRComCtlContainer
  INITIALIZERS.push :comctlcontainerinit
end


####################################
#
# Argument Utilities for Common Controls
#

class VRNotifyControl < VRControl
=begin
== VRNotifyControl
All common controls have these event handlers.
=== Event handlers
--- ????_clicked
    fired when the control is clicked with left button.
--- ????_hitreturn
    fired when the control has the input focus and the user press ((*ENTER*)) key.
--- ????_dblclicked
    fired when the control is double-clicked with left button.
--- ????_rclicked
    fired when the control is clicked with right button.
--- ????_rdblclicked
    fired when the control is double-clicked with rightbutton.
--- ????_gotfocus
    fired when the control has got the input focus
--- ????_lostfocus
    fired when the control has lost the input focus
=end

  attr_reader :_vr_notifyarg

  def _vr_ntfyhandlers
    unless defined?(@_vr_ntfyhandlers)
      @_vr_ntfyhandlers={} 
    else
      @_vr_ntfyhandlers
    end
  end


  def notifycontrolinit
    @_vr_notifyarg={}
    addNotifyHandler(0xfffffffe,"clicked",MSGTYPE::ARGNONE,nil)
    addNotifyHandler(0xfffffffd,"dblclicked",MSGTYPE::ARGNONE,nil)
    addNotifyHandler(0xfffffffc,"hitreturn",MSGTYPE::ARGNONE,nil)
    addNotifyHandler(0xfffffffb,"rclicked",MSGTYPE::ARGNONE,nil)
    addNotifyHandler(0xfffffffa,"rdblclicked",MSGTYPE::ARGNONE,nil)
    addNotifyHandler(0xfffffff9,"gotfocus",MSGTYPE::ARGNONE,nil)
    addNotifyHandler(0xfffffff8,"lostfocus",MSGTYPE::ARGNONE,nil)
  end
  def vrinit
    super
    notifycontrolinit
  end
  
  def addFilterArg(msg,filter)   # filter as str : 'T'=true, 'F'=false
    @_vr_notifyarg[msg]=filter
  end
  def deleteFilterArg(msg)
    @_vr_notifyarg[msg]=nil
  end
  
  def addNotifyHandler(msg,handlername,handlertype,argparsestr)
    @_vr_ntfyhandlers={} unless defined?(@_vr_ntfyhandlers)
    @_vr_ntfyhandlers[msg]=[] unless @_vr_ntfyhandlers[msg]
    @_vr_ntfyhandlers[msg].push [handlername,handlertype,argparsestr]
  end

  def deleteNotifyHandler(msg,handlername)
    return false unless @_vr_ntfyhandlers and @_vr_ntfyhandlers[msg]
    @_vr_ntfyhandlers.delete_if do |shandler|
      shandler[0] != (PREHANDLERSTR+handlername).intern
    end
  end
end


###################################
# Common Controls
#

# Listview

module WMsg
  LVM_GETBKCOLOR    = 0x1000      #GETBKCOLOR, LVM_FIRST
  LVM_SETBKCOLOR    = 0x1001

  LVM_SETIMAGELIST  = 0x1003
  LVM_GETITEMCOUNT  = 0x1004
  LVM_GETITEM       = 0x1005       #GETITEMA
  LVM_SETITEM       = 0x1006       #SETITEMA
  LVM_INSERTITEM    = 0x1007       #INSERTITEMA
  LVM_DELETEITEM    = 0x1008
  LVM_DELETEALLITEM = 0x1009

  LVM_GETNEXTITEM   = 0x1000 + 12
  LVM_GETITEMRECT   = 0x1000 + 14
  LVM_HITTEST       = 0x1000 + 18
  LVM_ENSUREVISIBLE = 0x1000 + 19

  LVM_GETCOLUMN     = 0x1000+25    #GETCOLUMNA
  LVM_SETCOLUMN     = 0x1000+26    #SETCOLUMNA
  LVM_INSERTCOLUMN  = 0x1000+27    #INSERTCOLUMNA
  LVM_DELETECOLUMN  = 0x1000+28    #DELETECOLUMNA
  LVM_GETCOLUMNWIDTH= 0x1000+29
  LVM_SETCOLUMNWIDTH= 0x1000+30

  LVM_SETITEMSTATE  = 0x1000+43
  LVM_GETITEMSTATE  = 0x1000+44
  LVM_GETITEMTEXT   = 0x1000+45    #GETITEMTEXTA
  LVM_SETITEMTEXT   = 0x1000+46    #SETITEMTEXTA
  LVM_SORTITEMS     = 0x1000+48
  LVM_GETSELECTED   = 0x1000+50    #LVM_GETSELECTEDCOUNT
  LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1000 + 54
  LVM_GETEXTENDEDLISTVIEWSTYLE = 0x1000 + 55
  LVM_SUBITEMHITTEST= 0x1000+57

  LVN_ITEMCHANGING  = 0xffffffff-99
  LVN_ITEMCHANGED   = LVN_ITEMCHANGING-1
  LVN_INSERTITEM    = LVN_ITEMCHANGING-2
  LVN_DELETEITEM    = LVN_ITEMCHANGING-3
  LVN_COLUMNCLICK   = LVN_ITEMCHANGING-8
  LVN_BEGINDRAG     = LVN_ITEMCHANGING-9
  LVN_BEGINRDRAG    = LVN_ITEMCHANGING-11
end
module WStyle
  LVS_ICON          =   0x0000
  LVS_REPORT        =   0x0001
  LVS_SMALLICON     =   0x0002
  LVS_LIST          =   0x0003
  LVS_SINGLESEL     =   4
  LVS_SHOWSELALWAYS =   8
end
module WExStyle
  LVS_EX_FULLROWSELECT=32
  LVS_EX_GRIDLINES=1
end

module WConst
  LVIS_FOCUSED      =  0x0001
  LVIS_SELECTED     =  0x0002
  LVIS_CUT          =  0x0004
  LVIS_DROPHILITED  =  0x0008
end
module WStruct
  LVITEM      = "UIIUUPIIL"
  LVCOLUMN    = "UIIPIIII"
  LVFIND      = "UPLPU"
  LVHITTEST   = "IIUI"
  NM_LISTVIEW= NMHDR+"IIUUULLL" #INT item,subitem,UINT nst,ost,chg,POINT,LParam
end

class VRListview < VRNotifyControl
=begin
== VRListview
Listview.
Some features has not implemented yet.
=== Method
--- setViewMode(mode)
    ((|mode|)) as viewmode. 0 for ICON, 1 for REPORT, 2 for SMALLICON and 
    3 for LIST
--- getViewMode
    Returns the view mode.
--- iconview
    Sets the view mode for Icon view.
--- reportview
    Sets the view mode for Report view.
--- smalliconview
    Sets the view mode for Small icon view.
--- listview
    Sets the view mode for listing view.
--- setBkColor(color)
--- bkcolor=(color)
    Sets the background color.
--- lvexstyle
--- lvexstyle=(style)
    Gets/Sets Listview extended style. 
    If you need style mask, use setListviewExStyle()
--- setListviewExStyle(style,mask=0xffffffff)
    Sets Listview extended style using style mask.

--- insertColumn(column,text,width=50,format=0,textsize=title.size)
    Inserts a new column specified by ((|text|)), ((|width|)) and ((|format|))
    into the index of ((|column|)), which is started from 0.
    ((|format|)) means 0 for left-padded, 1 for right-padded, 2 for centred.
--- deleteColumn(column)
    Deletes the column at the index of ((|column|))
--- clearColumns
    Deletes all columns.
--- countColumns
    Counts the number of columns.
--- addColumn(text,width=50,format=0,textsize=title.size)
    Adds a new column after the last column.
--- setImagelist(imagelist,itype=0)
    Set ((|imagelist|)) for displaying icons of items. ((|imagelist|)) must be
    a kind of SWin::Imagelist. ((|itype|)) specifies the type of Imagelist.
    itype : 0 for normal icon, 1 for small icon  (,2 for state icon).
--- setItemIconOf(hitem,img)
    Sets image ids in a imagelist of the item. 
    The imagelist is set by ((<setImagelist>)) method.
--- getItemIconOf(hitem)
    Returns images id of the item. 
--- getColumnWidthOf(column)
    Returns the width of the column whose index is ((|column|)).
--- setColumnWidthOf(column,width)
    Sets the width of the column.
--- getColumnTextOf(column)
    Returns the text of the column whose index is ((|column|)).
--- setColumnTextOf(column,text)
    Sets the text of the column.
--- getColumnFormatOf(column)
    Returns the format of the column whose index is ((|column|)).
    (see ((<insertColumn>)) about format)
--- setColumnFormatOf(column,format)
    Sets the format of the column.
--- insertItem(index,texts,lparam=0,textsize=128)
    Inserts a new item into the index of ((|index|)).((|texts|)) is an array of
    String which means [item_text, subitem_text, subitem2_text,...].
    ((|lparam|)) is a 32bit value concerned with the item.
    ((|textsize|)) is the limit length of the item texts.
--- addItem(texts,lparam=0,textsize=128)
    Adds a new item after the last item.
--- insertMultiItems(index,multitexts)
    Inserts new items into the index of ((|index|)).
    ((|multitexts|)) is an array which is the arguments of ((<insertItem>))
    excluding ((|index|))
--- deleteItem(idx)
    Deletes an item at the index of ((|idx|)).
--- clearItems
    Deletes all items.
--- countItems
    Counts the items.
--- hittest(x,y)
    Inspect the item index which is at the co-ordinate(((|x|)),((|y|)))
--- hittest2(x,y)
    Returns the item information which is at the co-ordinate(((|x|)),((|y|))).
    The information is the array [x,y,part,index,subitem]
--- getItemRect(idx)
    Returns an co-ordinates array of integers [left,top,right,bottom]
--- getItemStateOf(idx)
    Returns the item state at the index of ((|idx|)).
    The state is 1 for forcused, 2 for selected, 3 for cut and 4 fordrophilited.
--- setItemStateOf(idx,state)
    Sets the item state.
--- selectItem(idx,flag)
    Change selected state of the item at the index of ((|idx|)). 
    If ((|flag|)) is true, the item is selected and if it's false the item is 
    un-selected.
--- getNextItem(start,flags)
    Searches for an item described by ((|start|)) and ((|flags|)).
    ((|flags|)) is 
    * 1 : focused item after ((|start|))
    * 2 : selected item after ((|start|))
    * 4 : cut item after ((|start|))
    * 8 : drophilited item after ((|start|))
    * 0x100 : item above ((|start|))
    * 0x200 : item below ((|start|))
    * 0x400 : left item from ((|start|))
    * 0x800 : right item from ((|start|))
--- focusedItem
    Returns index of the focused item.
--- getItemTextOf(idx,subitem=0,textsize=128)
    Retrieves the sub-text of the item (index ((|idx|)) ).
    ((|subitem|)) specifies the subitem number.
--- setItemTextOf(idx,subitem,text,textsize=text.size)
    Sets the sub-text of the item.
--- getItemLParamOf(idx)
    Gets the lParam of the item.
--- setItemLParamOf(idx,lparam)
    Sets the lParam of the item.
--- selected?(idx)
    Returns true if the item is selected.
--- focused?(idx)
    Returns true if the item is focused.
--- eachSelectedItems
    Yields each selected item index.
--- countSelectedItems()
    Counts the selected items.
--- ensureVisible(i,partial=true)
    Ensures the item(idx) is visible by scrolling.

=== Event handlers
--- ????_itemchanged(idx,state)
    fired when the item state is changed. This also means that this is fired after changing selected items.
    ((|idx|)) is the index of the item and ((|state|)) is the new state.
--- ????_itemchanging(idx,state)
    fired when the item state is changing.
--- ????_columnclick(subitem)
    fired when the column is clicked. ((|subitem|)) is the column index.
--- ????_begindrag
--- ????_beginrdrag
=end

 public
  WINCLASSINFO = ["SysListView32",WStyle::LVS_REPORT,0x200]

  def listviewinit
    addNotifyHandler(WMsg::LVN_ITEMCHANGED,"itemchanged",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_LISTVIEW)
    addFilterArg WMsg::LVN_ITEMCHANGED,"TFFTFFFF"

    addNotifyHandler(WMsg::LVN_ITEMCHANGING,"itemchanging",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_LISTVIEW)
    addFilterArg WMsg::LVN_ITEMCHANGING,"TFFTFFFF"

    addNotifyHandler(WMsg::LVN_COLUMNCLICK,"columnclick",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_LISTVIEW)
    addFilterArg WMsg::LVN_COLUMNCLICK,"FTFFFFFF"

    addNotifyHandler(WMsg::LVN_BEGINDRAG,"begindrag",
                    MSGTYPE::ARGNONE,nil)

    addNotifyHandler(WMsg::LVN_BEGINRDRAG,"beginrdrag",
                    MSGTYPE::ARGNONE,nil)
  end
  def vrinit
    super
    listviewinit
  end

  def setViewMode(mode)
    self.style = (self.style & 0xfffffff8) + (mode&3)
  end
  def getViewMode
    self.style & 0xfffffff8
  end
  def iconview() setViewMode(0) end
  def reportview() setViewMode(1) end
  def smalliconview() setViewMode(2) end
  def listview() setViewMode(3) end
  

  def setBkColor(color)
    sendMessage WMsg::LVM_SETBKCOLOR,0,color.to_i
  end

  def bkcolor=(color)
    setBkColor(color)
  end

  def setListviewExStyle(sty,mask=0xffffffff)
    sendMessage WMsg::LVM_SETEXTENDEDLISTVIEWSTYLE,mask,sty
  end
  def lvexstyle=(sty)
    setListviewExStyle(sty)
  end
  def lvexstyle
    sendMessage WMsg::LVM_GETEXTENDEDLISTVIEWSTYLE,0,0
  end

  def insertColumn(column,title,width=50,style=0,textsize=title.size)
    lv=@screen.application.arg2cstructStr(WStruct::LVCOLUMN,
                   0xf,style.to_i,width.to_i,title.to_s,textsize,column,0,0)
    sendMessage WMsg::LVM_INSERTCOLUMN,column.to_i,lv
  end

  def deleteColumn(column)
    sendMessage(WMsg::LVM_DELETECOLUMN,column.to_i,0)
  end
  def clearColumns
    while sendMessage(WMsg::LVM_DELETECOLUMN,0,0)!=0 do; end
  end

  def countColumns
    r=0
    while getColumnTextOf(r) do r+=1;  end
    r
  end

  def addColumn(*args)
    insertColumn(30000,*args)   #30000 as a big number. I believe this is enough.
  end

  def getColumnWidthOf(column)
    sendMessage(WMsg::LVM_GETCOLUMNWIDTH,column.to_i,0)
  end

  def setColumnWidthOf(column,width)
    sendMessage(WMsg::LVM_SETCOLUMNWIDTH,column.to_i,width.to_i)
  end

  def setColumnTextOf(column,text)
    p=@screen.application.arg2cstructStr(WStruct::LVCOLUMN,
                     4,0,0,text,text.size,0)
    sendMessage WMsg::LVM_SETCOLUMN,column.to_i,p
  end
  def setColumnFormatOf(column,format)
    p=@screen.application.arg2cstructStr(WStruct::LVCOLUMN,
                     1,format,0,"",0,0)
    sendMessage WMsg::LVM_SETCOLUMN,column.to_i,p
  end
  def getColumnTextOf(column)
    p=@screen.application.arg2cstructStr(WStruct::LVCOLUMN,
                     4,0,0,"\0"*128,128,0)
    rv=sendMessage WMsg::LVM_GETCOLUMN,column.to_i,p
    r=@screen.application.unpack(p,WStruct::LVCOLUMN)[3]
    if rv!=0 then
      @screen.application.pointer2string(r)
    else
      nil
    end
  end
  def getColumnFormatOf(column)
    p=@screen.application.arg2cstructStr(WStruct::LVCOLUMN,
                     1,0,0,"",0,0)
    rv=sendMessage WMsg::LVM_GETCOLUMN,column.to_i,p
    if rv!=0 then
      @screen.application.unpack(p,WStruct::LVCOLUMN)[1]
    else
      nil
    end
  end

  def insertItem(index,texts,lparam=0,textsize=128)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          0xf,index,0,0,0,texts[0].to_s,textsize,0,lparam)
    item=sendMessage(WMsg::LVM_INSERTITEM,0,lvitem)

    1.upto(texts.size-1) do |subitem|
      setItemTextOf(item,subitem,texts[subitem],textsize)
    end
    item
  end

  def addItem(*args)
    insertItem(30000,*args)
  end

  def insertMultiItems(index,multitexts,textsize=128)
    n=multitexts.size
    0.upto(n-1) do |i|
      insertItem(index+i,*multitexts[i])
    end
  end

  def deleteItem(idx)
    sendMessage WMsg::LVM_DELETEITEM,idx.to_i,0
  end
  def clearItems
    sendMessage WMsg::LVM_DELETEALLITEM,0,0
  end
  def countItems
    sendMessage WMsg::LVM_GETITEMCOUNT,0,0
  end
  def hittest(x,y)
    lvhit=@screen.application.arg2cstructStr(WStruct::LVHITTEST,
                          x.to_i,y.to_i,0,0)
    sendMessage WMsg::LVM_HITTEST,0,lvhit
    @screen.application.unpack(lvhit,WStruct::LVHITTEST)[3]
  end
  def hittest2(x,y)
    lvhit=@screen.application.arg2cstructStr(WStruct::LVHITTEST,
                          x.to_i,y.to_i,0,0)
    sendMessage WMsg::LVM_SUBITEMHITTEST,0,lvhit
    @screen.application.unpack(lvhit,WStruct::LVHITTEST)
  end

  def getNextItem(start,flag)
    r=sendMessage WMsg::LVM_GETNEXTITEM,start,MAKELPARAM(flag,0)
    case(r)
    when -1
      0
    when 0
      nil
    else
      r
    end
  end

  def focusedItem
    getNextItem(0,1)
  end

  def setImagelist(imagelist,itype=0)
    raise "not Imagelist" unless imagelist.is_a?(SWin::Imagelist)
    self.properties["imagelist"]=imagelist
    sendMessage WMsg::LVM_SETIMAGELIST,itype,imagelist.himagelist
  end

  def setItemIconOf(idx,imageid)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          2,idx,0,0,0xffff,"",0,imageid.to_i,0) # 2=LVIF_IMAGE
    sendMessage WMsg::LVM_SETITEM,idx.to_i,lvitem
  end
  def getItemIconOf(idx)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          0x2,idx,0,0,0,"",0,0,0)
    sendMessage( WMsg::LVM_GETITEM,0,lvitem )
    @screen.application.unpack(lvitem,WStruct::LVITEM)[7]
  end

  def setItemStateOf(idx,state)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          0,0,0,state,0xffff,"",0,0,0)
    sendMessage WMsg::LVM_SETITEMSTATE,idx.to_i,lvitem
  end
  def getItemStateOf(idx)
    sendMessage WMsg::LVM_GETITEMSTATE,idx.to_i,0xffff
  end
  def selectItem(idx,flag)
    if flag then
      s = getItemStateOf(idx) | WConst::LVIS_SELECTED
    else
      s = getItemStateOf(idx) & (~ WConst::LVIS_SELECTED)
    end
    setItemStateOf(idx,s)
  end

  def setItemTextOf(idx,subitem,text,textsize=text.size)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          0,0,subitem,0,0,text.to_s,textsize,0,0)
    sendMessage WMsg::LVM_SETITEMTEXT,idx.to_i,lvitem
  end
  def getItemTextOf(idx,subitem=0,textsize=128)
    app=@screen.application
    lvitem=app.arg2cstructStr(WStruct::LVITEM,
                     0,0,subitem,0,0,"\0"*textsize,textsize,0,0)
    sendMessage( WMsg::LVM_GETITEMTEXT,idx,lvitem )
    app.pointer2string(app.unpack(lvitem,WStruct::LVITEM)[5])
  end

  def setItemLParamOf(idx,lparam)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          0x4,idx,0,0,0,"",0,0,lparam.to_i)
    sendMessage WMsg::LVM_SETITEM,0,lvitem
  end
  def getItemLParamOf(idx)
    lvitem=@screen.application.arg2cstructStr(WStruct::LVITEM,
                          0x4,idx,0,0,0,"",0,0,0)
    sendMessage( WMsg::LVM_GETITEM,0,lvitem )
    @screen.application.unpack(lvitem,WStruct::LVITEM)[8]
  end

  def selected?(idx) (getItemStateOf(idx)&1)>0 end
  def focused?(idx) (getItemStateOf(idx)&2)>0 end

  def eachSelectedItems
    n=countItems
    0.upto(n-1) do |i|
      if (getItemStateOf(i)&WConst::LVIS_SELECTED)>0 then
        yield i
      end
    end
  end

  def countSelectedItems()
    sendMessage WMsg::LVM_GETSELECTED,0,0
  end

  def getItemRect(item,code=0)
    prc = [code,0,0,0].pack("iiii")
    sendMessage WMsg::LVM_GETITEMRECT,item,prc
    prc.unpack("iiii")
  end

  def ensureVisible(i,partial=true)
    flag = if partial then 1 else 0 end
    sendMessage WMsg::LVM_ENSUREVISIBLE,i.to_i,flag
  end

end


#Treeview
module WMsg
  TVM_INSERTITEM    = 0x1100       #INSERTITEMA
  TVM_DELETEITEM    = 0x1100+1
  TVM_EXPAND	    = 0x1100+2
  TVM_GETCOUNT      = 0x1100+5
  TVM_SETIMAGELIST  = 0x1100+9
  TVM_GETNEXTITEM   = 0x1100+10
  TVM_SELECTITEM    = 0x1100+11
  TVM_GETINDENT     = 0x1100+6
  TVM_SETINDENT     = 0x1100+7
  TVM_GETITEM       = 0x1100+12    #GETITEMA
  TVM_SETITEM       = 0x1100+13    #SETITEMA
  TVM_HITTEST       = 0x1100+17
  TVM_SORTCHILDREN  = 0x1100+19

  TVN_START = 0xffffffff-399
  TVN_SELCHANGED    = TVN_START-2        #SELCHANGEDA
  TVN_ITEMEXPANDED  = TVN_START-6        #ITEMEXPANDEDA
  TVN_BEGINDRAG     = TVN_START-7        #BEGINDRAGA
  TVN_BEGINRDRAG    = TVN_START-8        #BEGINRDRAGA
  TVN_DELETEITEM    = TVN_START-9        #DELETEITEMA
  TVN_KEYDOWN       = TVN_START-12
end
module WStyle
  TVS_DEFAULT       =   0xf
end

module WConst
  TVI_ROOT          = 0xffff0000
  TVI_FIRST         = 0xffff0001
  TVI_LAST          = 0xffff0002
  TVI_SORT          = 0xffff0003
  
  TVGN_ROOT         = 0x0000
  TVGN_NEXT         = 0x0001
  TVGN_PARENT       = 0x0003
  TVGN_CHILD        = 0x0004
  TVGN_CARET        = 0x0009
end
module WStruct
  TVITEM="UUUUPIIIIL"
  TREEINSERTITEM="UU"+TVITEM
  TVHITTEST = LVHITTEST
  NM_TREEVIEW= NMHDR+"U"+TVITEM+TVITEM+"LL" #UINT action,TV_ITEM old, new,POINT
end


class VRTreeview < VRNotifyControl
=begin
== VRTreeview
Treeview.

=== Methods
((|hitem|)) is an Integer value identifying treeview item.

--- insertItem(hparent,hafter,text,lparam=0,textsize=text.size)
    Inserts a new item below ((|hparent|)) and after ((|hafter|)), the text is 
    ((|text|)) and lparam=((|lparam|)).
--- insertMultiItems(hparent,hafter,items)
    Inserts new items below ((|hparent|)) and after ((|hafter|)).
    ((|items|)) is a structured array of item array.
    [text,lparam, child item array as optional].
--- addItem(hparent,text,lparam=0,textsize=text.size)
    Adds a new item below ((|hparent|)) at the last.
--- addMultiItems(hparent,items)
    Same as insertMultiItems( ((|hparent|)),last,((|items|)) )
--- deleteItem(hitem)
    Deletes the item.
--- clearItems
    Deletes all items.
--- countItems
    Counts items.
--- selectItem(hitem)
    Selects the item.
--- expandItem(hitem,mode)
    Expand or Collapse item.
    ((|mode|)) means; 1: collapse, 2: expand, 3:toggle
--- indent
    Returns the indent width.
--- indent=
    Sets the indent width.
--- hittest(x,y)
    Inspect the item index which is at the co-ordinate(((|x|)),((|y|)))
--- hittest2(x,y)
    Returns the item information which is at the co-ordinate(((|x|)),((|y|))).
    The information is the array [x,y,part,index,subitem]
--- getNextItem(hitem,code)
    Searches for an item described by ((|start|)) and ((|code|)).
    ((|code|)) is the series of TVGN_???? in commctrl.h
--- topItem
    The top item in the listview. (not the visible top item)
--- root
    The virtual root item which is not visible and touched.
--- last
    The last item in the listview (not the visible last item)
--- selectedItem()
    Returns the selected item. aliased as selected
--- getParentOf(hitem)
    Returns the parent item of ((|hitem|)).
--- getChildOf(hitem)
    Returns the first child item of ((|hitem|)).
--- getNextSiblingOf(hitem)
    Returns the next sibling item of ((|hitem|)).
--- setImagelist(imagelist)
    Set ((|imagelist|)) for displaying icons of items. ((|imagelist|)) must be
    a kind of SWin::Imagelist.
--- setItemIconOf(hitem,img,simg)
    Sets image ids of the item. ((|img|)) for normal state image and ((|simg|))
    for selected state image. Both of them are Integers of id in ImageList and
    ((|img|)) and ((|simg|)) can be ignored by setting nil instead of integer.
    The imagelist is set by ((<setImagelist>)) method.
--- getItemIconOf(hitem)
    Returns images id of the item. 
--- setItemLParamOf(hitem,lparam)
    Sets lparam of the item.
--- getItemLParamOf(hitem)
    Returns lparam of the item.
--- setItemTextOf(hitem,text)
    Sets text of the item.
--- getItemTextOf(hitem,textsize=128)
    Returns text of the item.
--- setItemStateOf(hitem,state)
    Sets state of the item.
    * 1 : focused
    * 2 : selected
    * 4 : cut
    * 8 : drophilited
    *16 : bold
    *32 : expanded
--- getItemStateOf(hitem)
    Returns state of the item.

=== Event handlers
--- ????_selchanged(hitem,lparam)
    fired when selected item is changed to ((|hitem|)) 
    whose lparam is ((|lparam|)).
--- ????_itemexpanded(hitem,state,lparam)
    fired when the ((|hitem|)) item is expanded or closed.
--- ????_deleteitem(hitem,lparam)
    fired when the item is deleted.
--- ????_begindrag(hitem,state,lparam)
--- ????_beginrdrag(hitem,state,lparam)
=end

 private
  def nilnull(r) if r==0 then nil else r end end

 public
  WINCLASSINFO = ["SysTreeView32",WStyle::TVS_DEFAULT,0x200]

  def treeviewinit
# "F FFFFFFFFFF FTFFFFFFFT FF"
    addNotifyHandler(WMsg::TVN_SELCHANGED,"selchanged",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_TREEVIEW)
    addFilterArg WMsg::TVN_SELCHANGED,"FFFFFFFFFFFFTFFFFFFFTFF" #hitem,lparam
    addNotifyHandler(WMsg::TVN_ITEMEXPANDED,"itemexpanded",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_TREEVIEW)
    addFilterArg WMsg::TVN_ITEMEXPANDED,"FFFFFFFFFFFFTTFFFFFFTFF" #hitem,state,lparam
    addNotifyHandler(WMsg::TVN_DELETEITEM,"deleteitem",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_TREEVIEW)
    addFilterArg WMsg::TVN_DELETEITEM,"FFTFFFFFFFTFFFFFFFFFFFF"   #hitem,lparam
    addNotifyHandler(WMsg::TVN_BEGINDRAG,"begindrag",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_TREEVIEW)
    addFilterArg WMsg::TVN_BEGINDRAG,"FFFFFFFFFFFFTTFFFFFFTFF"    #hitem,state,lparam
    addNotifyHandler(WMsg::TVN_BEGINRDRAG,"beginrdrag",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_TREEVIEW)
    addFilterArg WMsg::TVN_BEGINRDRAG,"FFFFFFFFFFFFTTFFFFFFTFF"   #hitem,state,lparam
  end
  
  def vrinit
    super
    treeviewinit
  end

  def insertItem(hparent,hafter,text,lparam=0,textsize=text.size)
    ti=@screen.application.arg2cstructStr(WStruct::TREEINSERTITEM,hparent,hafter,
                        0x0f,0,0,0,text.to_s,textsize,0,0,0,lparam)
    sendMessage WMsg::TVM_INSERTITEM,0,ti
  end
  
  def addItem(hparent,*args)
    insertItem(hparent, WConst::TVI_LAST,*args)
  end
  
  def insertMultiItems(hparent,hafter,multitext)
    ha = if hafter then hafter else WConst::TVI_LAST end

    multitext.each do |item|
      if item.is_a?(Array)
        item[1] = 0 unless item[1] 
        h=insertItem(hparent,ha,item[0],item[1])
        if item.size>2 and item[2].is_a?(Array) then
          insertMultiItems(h,WConst::TVI_LAST,item[2])
        end
        ha=h
      else
        raise ArgumentError,"Arg2 illegal"
      end
    end
  end
  def addMultiItems(hparent,*args)
    insertMultiItems(hparent, WConst::TVI_LAST,*args)
  end

  def deleteItem(htreei)
    sendMessage WMsg::TVM_DELETEITEM,0,htreei
  end
  def clearItems
    deleteItem(WConst::TVGN_ROOT)
  end
  def countItems
    sendMessage WMsg::TVM_GETCOUNT,0,0
  end
  def selectItem(hitem)
    sendMessage WMsg::TVM_SELECTITEM,9,hitem
  end
  def expandItem(hitem,mode)
    sendMessage WMsg::TVM_EXPAND,mode.to_i,hitem
  end
  def indent
    sendMessage WMsg::TVM_GETINDENT,0,0
  end
  def indent=(idt)
    sendMessage WMsg::TVM_SETINDENT,idt.to_i,0
  end
  
  def hittest(x,y)
    tvhit=@screen.application.arg2cstructStr(WStruct::TVHITTEST,
                          x.to_i,y.to_i,0,0)
    sendMessage WMsg::TVM_HITTEST,0,tvhit
    @screen.application.unpack(tvhit,WStruct::TVHITTEST)[3]
  end
  def hittest2(x,y)
    tvhit=@screen.application.arg2cstructStr(WStruct::TVHITTEST,
                          x.to_i,y.to_i,0,0)
    sendMessage WMsg::TVM_HITTEST,0,tvhit
    @screen.application.unpack(tvhit,WStruct::TVHITTEST)
  end

  def getNextItem(hitem,code)
    sendMessage WMsg::TVM_GETNEXTITEM,code,hitem
  end
  def topItem() getNextItem(0,WConst::TVGN_ROOT)  end
  def root() WConst::TVI_ROOT end
  def last() WCONST::TVI_LAST end

  def selectedItem()      nilnull(getNextItem(0,WConst::TVGN_CARET)) end
  alias selected :selectedItem
  def getParentOf(hitem)      nilnull(getNextItem(hitem,WConst::TVGN_PARENT))end
  def getChildOf(hitem)       nilnull(getNextItem(hitem,WConst::TVGN_CHILD)) end
  def getNextSiblingOf(hitem) nilnull(getNextItem(hitem,WConst::TVGN_NEXT))  end

  def setImagelist(imagelist,itype=0)
    raise "not Imagelist" unless imagelist.is_a?(SWin::Imagelist)
    self.properties["imagelist"]=imagelist
    sendMessage WMsg::TVM_SETIMAGELIST,itype,imagelist.himagelist
  end

  def setItemIconOf(hitem,img,simg=nil)
    # TVIF_IMAGE=2, TVIF_SELECTEDIMAGE=0x20
    mask=0; image=0; simage=0
    if img  then mask |= 2;    image = img   end
    if simg then mask |= 0x20; simage = simg end

    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
             mask,hitem.to_i,0,0,"",0,image,simage,0,0)
    sendMessage WMsg::TVM_SETITEM,0,tvitem
  end
  def getItemIconOf(hitem)
    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
         0x22,hitem.to_i,0,0,"",0,0,0,0,0)  
    sendMessage WMsg::TVM_GETITEM,0,tvitem
    @screen.application.unpack(tvitem,WStruct::TVITEM)[6..7]
  end

  def setItemLParamOf(hitem,lparam)
    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
             4,hitem.to_i,0,0,"",0,0,0,0,lparam.to_i)       # 4=TVIF_PARAM
    sendMessage WMsg::TVM_SETITEM,0,tvitem
  end
  def getItemLParamOf(hitem)
    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
         4,hitem.to_i,0,0,"",0,0,0,0,0)       # 4=TVIF_PARAM
    sendMessage WMsg::TVM_GETITEM,0,tvitem
    @screen.application.unpack(tvitem,WStruct::TVITEM)[9]
  end
  def setItemTextOf(hitem,text)
    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
         1,hitem.to_i,0,0,text.to_s,text.size,0,0,0,0) # 1=TVIF_TEXT
    sendMessage WMsg::TVM_SETITEM,0,tvitem
  end
  def getItemTextOf(hitem,textsize=128)
    app=@screen.application
    tvitem=app.arg2cstructStr(WStruct::TVITEM,
         1,hitem.to_i,0,0,"\0"*textsize,textsize,0,0,0,0)   # 1=TVIF_TEXT
    sendMessage WMsg::TVM_GETITEM,0,tvitem
    app.pointer2string(app.unpack(tvitem,WStruct::TVITEM)[4])
  end
  def setItemStateOf(hitem,state)
    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
         8,hitem.to_i,state.to_i,0x00ff,"",0,0,0,0,0) # 8=TVIF_STATE
    sendMessage WMsg::TVM_SETITEM,0,tvitem
  end
  def getItemStateOf(hitem)
    tvitem=@screen.application.arg2cstructStr(WStruct::TVITEM,
         8,hitem.to_i,0,0x00ff,"",0,0,0,0,0)   # 8=TVIF_STATE
    sendMessage WMsg::TVM_GETITEM,0,tvitem
    @screen.application.unpack(tvitem,WStruct::TVITEM)[2]
  end
end

# Progressbar
module WMsg
  PBM_SETRANGE      = WM_USER+1
  PBM_SETPOS        = WM_USER+2
  PBM_DELTAPOS      = WM_USER+3
  PBM_SETSTEP       = WM_USER+4
  PBM_STEPIT        = WM_USER+5
end
module WStyle
  PBS_SMOOTH        = 1   # ?
end

class VRProgressbar < VRNotifyControl
=begin
== VRProgressbar
Progressbar.
=== Methods
--- setRange(minr,maxr)
    Sets the range from ((|minr|)) to ((|maxr|)).
--- position
    Returns the current position.
--- position=(pos)
    Sets the current position.
--- stepwidth=(st)
    Sets the step width for ((<step>)).
--- step
    Steps one step in position.
--- advance(n=1)
    Steps multi steps in position.
=end

  WINCLASSINFO = ["msctls_progress32",0]

  attr_reader :stepwidth, :minrange, :maxrange     # ooooo.....

  def progressbarinit
    @stepwidth=10
    @minrange=0
    @maxrange=100
  end
  def vrinit
    super
    progressbarinit
  end

  def setRange(minr,maxr)
    @minrange=minr
    @maxrange=maxr
    sendMessage WMsg::PBM_SETRANGE,0,MAKELPARAM(minr,maxr)
  end
  def position=(pos)
    sendMessage WMsg::PBM_SETPOS,pos.to_i,0
  end
  def position
    raise StandardError,"not implemented"
  end
  def advance(n=1)
    sendMessage WMsg::PBM_DELTAPOS,n.to_i,0
  end
  def stepwidth=(st)
    @stepwidth=st
    sendMessage WMsg::PBM_SETSTEP,st.to_i,0
  end
  def step
    sendMessage WMsg::PBM_STEPIT,0,0
  end
end


# Trackbar
module WMsg
  TBM_GETPOS        =  WM_USER + 0
  TBM_GETRANGEMIN   =  WM_USER + 1
  TBM_GETRANGEMAX   =  WM_USER + 2
  TBM_SETPOS        =  WM_USER + 5
  TBM_SETRANGEMIN   =  WM_USER + 7
  TBM_SETRANGEMAX   =  WM_USER + 8
  TBM_SETSEL        =  WM_USER + 10
  TBM_SETSELSTART   =  WM_USER + 11
  TBM_SETSELEND     =  WM_USER + 12
  TBM_GETSELSTART   =  WM_USER + 17
  TBM_GETSELEND     =  WM_USER + 18
  TBM_CLEARSEL      =  WM_USER + 19
  TBM_SETPAGESIZE   =  WM_USER + 21
  TBM_GETPAGESIZE   =  WM_USER + 22
  TBM_SETLINESIZE   =  WM_USER + 23
  TBM_GETLINESIZE   =  WM_USER + 24
end
module WStyle
  TBS_AUTOTICS = 0x001
  TBS_VERT     = 0x002
  TBS_HORZ     = 0x000
  TBS_LEFT     = 0x004
  TBS_BOTH     = 0x008
  TBS_ENABLESEL= 0x020  #ENABELSELRANGE
end

class VRTrackbar < VRNotifyControl
=begin
== VRTrackbar
Trackbar.
=== Methods
--- position
    Returns the position.
--- position=(pos)
    Sets the position.
--- linesize
    Returns the number of positions moved on by arrow keys.
--- linesize=(s)
    Sets the number of positions mvoed on by arrow keys.
--- pagesize
    Returns the number of positions moved on by [page up]/[pagedown] keys.
--- pagesize=(p)
    Sets the number of positions moved on by [page up]/[pagedown] keys.
--- rangeMin
    Returns minimum value of the trackbar.
--- rangeMin=(m)
    Sets minimum value of the trackbar.
--- rangeMax
    Returns maximum value of the trackbar.
--- rangeMax=(m)
    Sets maximum value of the trackbar.
--- selStart
    Returns the selection start of the trackbar.
--- selStart=(m)
    Sets the selection start of the trackbar.
--- selEnd
    Returns the selection end of the trackbar.
--- selEnd=(m)
    Sets the selection end of the trackbar.
--- clearSel
    Clears the selection.
=end

  WINCLASSINFO = ["msctls_trackbar32",0]

  def vrinit
    super
  end

  def position
    sendMessage WMsg::TBM_GETPOS,0,0
  end
  def position=(pos)
    sendMessage WMsg::TBM_SETPOS,1,pos.to_i
  end

  def linesize
    sendMessage WMsg::TBM_GETLINESIZE,0,0
  end
  def linesize=(s)
    sendMessage WMsg::TBM_SETLINESIZE,0,s.to_i
  end
  def pagesize
    sendMessage WMsg::TBM_GETPAGESIZE,0,0
  end
  def pagesize=(s)
    sendMessage WMsg::TBM_SETPAGESIZE,0,s.to_i
  end

  def rangeMin
    sendMessage WMsg::TBM_GETRANGEMIN,0,0
  end
  def rangeMin=(m)
    sendMessage WMsg::TBM_SETRANGEMIN,1,m.to_i
  end
  def rangeMax
    sendMessage WMsg::TBM_GETRANGEMAX,0,0
  end
  def rangeMax=(m)
    sendMessage WMsg::TBM_SETRANGEMAX,1,m.to_i
  end
  def selStart
    sendMessage WMsg::TBM_GETSELSTART,0,0
  end
  def selStart=(m)
    sendMessage WMsg::TBM_SETSELSTART,1,m.to_i
  end
  def selEnd
    sendMessage WMsg::TBM_GETSELEND,0,0
  end
  def selEnd=(m)
    sendMessage WMsg::TBM_SETSELEND,1,m.to_i
  end
  def clearSel
    sendMessage WMsg::TBM_CLEARSEL,1,0
  end
end

# updown control
module WMsg
  UDM_SETRANGE            = WM_USER+101
  UDM_GETRANGE            = WM_USER+102
  UDM_SETPOS              = WM_USER+103
  UDM_GETPOS              = WM_USER+104
  UDM_SETBUDDY            = WM_USER+105
  UDM_GETBUDDY            = WM_USER+106
  UDM_SETACCEL            = WM_USER+107
  UDM_GETACCEL            = WM_USER+108
  UDM_SETBASE             = WM_USER+109
  UDM_GETBASE             = WM_USER+110

  UDN_DELTAPOS            = 0x100000000-722
end
module WStyle
  UDS_ALIGNRIGHT  = 0x04
  UDS_ALIGNLEFT   = 0x08
  UDS_HORZ        = 0x40

# Thanks to Katonbo-san
  UDS_SETBUDDYINT  = 0x0002
  UDS_AUTOBUDDY    = 0x0010
  UDS_ARROWKEYS    = 0x0020
  UDS_NOTHOUSANDS  = 0x0080

  UDS_INTBUDDYRIGHT = UDS_SETBUDDYINT | UDS_AUTOBUDDY | UDS_ALIGNRIGHT
end

module WStruct
  NM_UPDOWN = NMHDR+"UU"
end
class VRUpdown < VRNotifyControl
=begin
== VRUpdown
Updown control.
===Methods
--- setRange(minr,maxr)
    Sets the range from ((|minr|)) to ((|maxr|)).
--- getRange
    Returns the range as an array [minr,maxr]
--- position
    Returns current position.
--- position=
    Sets current position.
--- base
    Radix.
--- base=(b)
    Sets the radix that is 10 or 16.
=== Event handlers
--- ????_deltapos(pos)
    Fired when the position is changing from ((|pos|)).
    Note that ((|pos|)) is a previous value. To obtain the current value,
    use buddy control which is a (edit) control made directry before 
    the updown control(using VREdit#changed or its parent handler).
=end
  WINCLASSINFO = ["msctls_updown32",0]

  def updowninit
    addNotifyHandler(WMsg::UDN_DELTAPOS,"deltapos",
                    MSGTYPE::ARGSTRUCT,WStruct::NM_UPDOWN)
    addFilterArg WMsg::UDN_DELTAPOS,"TF"
  end
  def vrinit
    super
    updowninit
  end

  def setRange(minr,maxr)
    sendMessage WMsg::UDM_SETRANGE,0,MAKELPARAM(maxr,minr)
  end
  def getRange
    r=sendMessage WMsg::UDM_GETRANGE,0,0
    return HIWORD(r),LOWORD(r)
  end

  def position=(p)
    sendMessage WMsg::UDM_SETPOS,0,MAKELPARAM(p.to_i,0)
  end
  def position
    sendMessage WMsg::UDM_GETPOS,0,0
  end
  
  def base=(b)
    sendMessage WMsg::UDM_SETBASE,b.to_i,0
  end
  def base
    sendMessage WMsg::UDM_GETBASE,0,0
  end
end

# Statusbar

module WMsg
  SB_SETTEXT       = WM_USER+1  #SETTEXT
  SB_GETTEXT       = WM_USER+2  #GETTEXT
  SB_GETTEXTLENGTH = WM_USER+3  #GETTEXTLENGTH
  SB_SETPARTS      = WM_USER+4
  SB_GETPARTS      = WM_USER+6
  SB_SETMINHEIGHT  = WM_USER+8
  SB_GETRECT       = WM_USER+10
end
module WConst
   SBT_OWNERDRAW          =0x1000
   SBT_NOBORDERS          =0x0100
   SBT_POPOUT             =0x0200
   SBT_RTLREADING         =0x0400
end

class VRStatusbar < VRNotifyControl
=begin
== VRStatusbar
Statusbar.
=== Methods
--- setparts(p,width=[-1])
    Devides the statusbar into ((|p|)) parts with the widths specified by 
    ((|width|)) which is an Integer array. If the width is -1, the right edge
    of the part is to be at the right edge of the statusbar.
--- parts
    Returns the number of parts.
--- getTextOf(idx)
    Returns the text of the parts.
--- setTextOf(idx,text,style=0)
    Sets the text of the parts whose index is ((|idx|)) to ((|text|))
--- getRectOf(idx)
    Returns the position and size of the parts as an array [x,y,w,h].
--- minheight=(minh)
    Sets the minimum height of the statusbar.
=end

  WINCLASSINFO = ["msctls_statusbar32",0]

  def getTextOf(idx)
    len = 1+LOWORD(sendMessage(WMsg::SB_GETTEXTLENGTH,idx,0))
    r="\0"*len
    sendMessage WMsg::SB_GETTEXT,idx,r
    r
  end
  def setTextOf(idx,text,style=0)
    sendMessage WMsg::SB_SETTEXT,(idx|(style&0xff00)),text.to_s
  end
  def parts
    sendMessage WMsg::SB_GETPARTS,0,0
  end
  def setparts(p,widths=[-1])
    if widths then
         raise(ArgumentError,"width illegal") unless widths.is_a?(Array)
    end
    r=@screen.application.arg2cstructStr("I"*p,*widths) 
    sendMessage WMsg::SB_SETPARTS,p.to_i,r
  end
  def getRectOf(idx)
    r = [0,0,0,0].pack("iiii")
    sendMessage WMsg::SB_GETRECT,idx.to_i,r
    r.unpack("iiii")
  end
  def minheight=(h)
    sendMessage WMsg::SB_SETMINHEIGHT,h.to_i,0
  end
end

module VRStatusbarDockable
=begin
== VRStatusbarDockable
This is a module to be added into VRForm for a statusbar that follows form resizing.

=== Method
--- addStatusbar(caption='',height=10,control=VRStatusbar)
    Adds a statusbar on a form. If you have another statusbar control, you may
    set it for ((|control|)).
=end

  def statusbardockableinit
    addHandler WMsg::WM_SIZE, "vr_statusbardock", MSGTYPE::ARGLINTINT,nil
    acceptEvents [WMsg::WM_SIZE]
  end

  def vrinit
    super
    statusbardockableinit
  end

  def addStatusbar(caption="",height=10,control=VRStatusbar)
    @_vr_statusbar=addControl control,"statusbar",caption,10,10,10,height
    if self.visible? then
      a=self.clientrect
      sendMessage WMsg::WM_SIZE,0,MAKELPARAM(a[2]-a[0],a[3]-a[1])
    end
    @statusbar = @_vr_statusbar 
  end

  def self_vr_statusbardock(w,h)
    if defined?(@_vr_statusbar) then
      s=@_vr_statusbar
      s.move 0, self.h - s.h, self.w, self.h - s.h
    end
  end

end


#Tabcontrol

module WMsg
  TCM_FIRST=0x1300
  TCM_GETIMAGELIST   =   (TCM_FIRST + 2)
  TCM_SETIMAGELIST   =   (TCM_FIRST + 3)
  TCM_GETITEMCOUNT   =   (TCM_FIRST + 4)
  TCM_GETITEM        =   (TCM_FIRST + 5)
  TCM_SETITEM        =   (TCM_FIRST + 6)
  TCM_INSERTITEM     =   (TCM_FIRST + 7)
  TCM_DELETEITEM     =   (TCM_FIRST + 8)
  TCM_DELETEALLITEMS =   (TCM_FIRST + 9)
  TCM_GETITEMRECT    =   (TCM_FIRST + 10)
  TCM_GETCURSEL      =   (TCM_FIRST + 11)
  TCM_SETCURSEL      =   (TCM_FIRST + 12)
  TCM_ADJUSTRECT     =   (TCM_FIRST + 40)
  TCM_SETITEMSIZE    =   (TCM_FIRST + 41)
  TCM_GETCURFOCUS    =   (TCM_FIRST + 47)
  TCM_SETCURFOCUS    =   (TCM_FIRST + 48)

  TCN_FIRST          =   0xffffffff-549  
  TCN_SELCHANGE      =   (TCN_FIRST - 1)
end
  
module WStyle
  TCS_BOTTOM           =  0x0002
  TCS_RIGHT            =  0x0002
  TCS_MULTISELECT      =  0x0004
  TCS_FLATBUTTONS      =  0x0008

  TCS_FORCEICONLEFT    =  0x0010
  TCS_FORCELABELLEFT   =  0x0020
  TCS_HOTTRACK         =  0x0040
  TCS_VERTICAL         =  0x0080
  TCS_TABS             =  0x0000
  TCS_BUTTONS          =  0x0100
  TCS_SINGLELINE       =  0x0000
  TCS_MULTILINE        =  0x0200
  TCS_RIGHTJUSTIFY     =  0x0000
  TCS_FIXEDWIDTH       =  0x0400
  TCS_RAGGEDRIGHT      =  0x0800
  TCS_FOCUSONBUTTONDOWN=  0x1000
  TCS_OWNERDRAWFIXED   =  0x2000
  TCS_TOOLTIPS         =  0x4000
  TCS_FOCUSNEVER       =  0x8000
end

module WStruct
  TC_ITEM = "UUUPUUU"       # Mask,rsrv1,rsrv2,Text,TextMax,iImage,lParam
end

class VRTabControl < VRNotifyControl
=begin
== VRTabControl
Tabs.
This class doesn't have a function to show/hide controls according to
the selected tab. For that function ((<VRTabbedPanel>)) class is provided below.
=== Methods
--- insertTab(index,text,textmax=text.size,lparam=0)
    Inserts a new tab named ((|text|)) with ((|lparam|)) at ((|index|))-th.
    ((|index|)) is ordinal number for tabs.
--- clearTabs
    Deletes all tabs.
--- deleteTab(idx)
    Deletes a tab at ((|index|))
--- countTabs
    Counts tabs in the control.
--- selectedTab
    Returns the selected tab's index.
--- selectTab(idx)
    Selects the tab at ((|idx|))
--- setImagelist(imagelist)
    Sets an imagelist for the tabs.
--- setTabSize(width,height)
    Sets each tab size.
--- getTabRect(i)
    Returns the client area of the tab at ((|idx|)) as  an array of [x,y,w,h].
--- adjustRect(x,y,w,h,flag=false)
    Adjusts a rectangle coodinates for tabcontrol's clientarea which is 
    excluded tab buttons area. ((|flag|)) means the direction of adjusting.
    adjustRect(0,0,10,10,false) returns a leftsided rectangle below the 
    tab buttons.
--- getTabTextOf(idx)
    Gets a title text of tab at ((|idx|)).
--- setTabTextOf(idx,text)
    Sets a title text of tab at ((|idx|)) as ((|text|)).
--- getTabImageOf(idx)
    Gets a image id in the imagelist for tab at((|idx|)).
--- setTabImageOf(idx,image)
    Sets a image id into ((|image|)) in the imagelist for tab at((|idx|)).
--- getTabLParamOf(idx)
    Gets lparam value of tab at((|idx|)).
--- setTabLParamOf(idx,lparam)
    Sets lparam value of tab at((|idx|)) as ((|lparam|)).

=== Event Handlers
--- ????_selchanged
    Fired when the selected tab changed. To get current tab id, use selectedTab
    method.
=end

  include VRParent

  WINCLASSINFO = ["SysTabControl32",0]  #TCS_TAB
  
  def tabcontrolinit
    addNotifyHandler WMsg::TCN_SELCHANGE,"selchanged",MSGTYPE::ARGNONE,nil
  end
  
  def vrinit
    super
    tabcontrolinit
  end
  
  def clearTabs
    sendMessage WMsg::TCM_DELETEALLITEMS,0,0
  end
  
  def deleteTab(i)
    sendMessage WMsg::TCM_DELETEITEM,i,0
  end
  
  def selectedTab
    sendMessage WMsg::TCM_GETCURSEL,0,0
  end
  
  def countTabs
    sendMessage WMsg::TCM_GETITEMCOUNT,0,0
  end
  
  def insertTab(idx,text,textmax=text.size,lparam=0)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x9,0,0,text,textmax,0,lparam)
          # Mask,rsrv1,rsrv2,Text,TextMax,iImage,lParam
    sendMessage WMsg::TCM_INSERTITEM,idx.to_i,tb
  end

  def selectTab(i)
    sendMessage WMsg::TCM_SETCURSEL,i,0
  end

  def setImagelist(imagelist)
    raise "not Imagelist" unless imagelist.is_a?(SWin::Imagelist)
    sendMessage WMsg::TCM_SETIMAGELIST,0,imagelist.himagelist
  end

  def setTabSize(width,height)
    sendMessage WMsg::TCM_SETITEMSIZE,0,MAKELPARAM(width,height)
  end

  def getTabRect(i)
    rect="\0"*16
    sendMessage WMsg::TCM_GETITEMRECT,i.to_i,rect
    return @screen.application.unpack(rect,"UUUU")
  end

  def adjustRect(x,y,w,h,flag=false)
    f = if flag then  1 else 0 end
    rect=@screen.application.arg2cstructStr("UUUU",x,y,w,h)
    sendMessage WMsg::TCM_ADJUSTRECT,f,rect
    return @screen.application.unpack(rect,"UUUU")
  end

# tab's properties = text,image,lparam

  def getTabTextOf(idx)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x1,0,0,"\0"*128,128,0,0)
    rv=sendMessage WMsg::TCM_GETITEM,idx.to_i,tb
    if rv!=0 then
      r=@screen.application.unpack(tb,WStruct::TC_ITEM)[3]
      @screen.application.pointer2string(r)
    else
      nil
    end
  end
  def getTabImageOf(idx)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x2,0,0," \0",1,0,0)
    rv=sendMessage WMsg::TCM_GETITEM,idx.to_i,tb
    r=@screen.application.unpack(tb,WStruct::TC_ITEM)[5]
  end
  def getTabLParamOf(idx)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x8,0,0," \0",1,0,0)
    rv=sendMessage WMsg::TCM_GETITEM,idx.to_i,tb
    r=@screen.application.unpack(tb,WStruct::TC_ITEM)[6]
  end

  def setTabTextOf(idx,text)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x1,0,0,text,text.length,0,0)
    sendMessage WMsg::TCM_SETITEM,idx.to_i,tb
    self.refresh
  end
  def setTabImageOf(idx,iImage)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x2,0,0," \0",1,iImage,0)
    sendMessage WMsg::TCM_SETITEM,idx.to_i,tb
    self.refresh
  end
  def setTabLParamOf(idx,lparam)
    tb=@screen.application.arg2cstructStr(WStruct::TC_ITEM,
            0x8,0,0," \0",1,0,lparam)
    sendMessage WMsg::TCM_SETITEM,idx.to_i,tb
  end
end

class VRTabbedPanel < VRTabControl
=begin
== VRTabbedPanel
This is a class utilizing VRTabControl.
On this control, each tab has a VRPanel and be shown/hidden automatically
according to the selected tab.

=== Class method
--- auto_panelresize(flag)
    Resize panels on the tabs automatically when this control resized.
=== Methods
--- setupPanels(arg-1,arg-2,arg-3,....)
    Creates tabs each titled ((|arg-n|)) if args are String.
    When ((|arg-n|)) is array of ((|[title, contorl-class, control-name]|)),
    Creates control and relates with the tab.
    Use like follows.
      class Panel1 < VRPanel
        include VRContainersSet
        ...
      end
      class Panel2 < VRPanel
        ...
      end
      ...
      setupPanels(["tab-1",Panel1,"panel1"],["tab-2",Panel2,"panel2"])
--- send_parent2(idx,controlname,eventname)
    Sets to send to its parent an event of control named ((|controlname|)) 
    on the panel at ((|idx|)).
=== Attribute(s)
--- panels
    An array that contains panels for each tab.
    panels[i] means a panel concerned with the tab at ((|i|)).
=== Event Handlers
Same as ((<VRTabControl>)).
VRTabbedPanel#selchanged is already defined, so you need to call super when you
override this method.
=end
  attr_reader :panels
  include VRMessageHandler
  include VRParent
  
  @@_auto_tabpanelresize = nil
  
  def self.auto_panelresize(flag)
    @@_auto_tabpanelresize = flag
  end
  
  def vrinit
    super
    if @@_auto_tabpanelresize
      addHandler(WMsg::WM_SIZE,  "_vr_tabpanelresize",  MSGTYPE::ARGLINTINT,nil)
      acceptEvents [WMsg::WM_SIZE]
    end
  end
  
  def setupPanels(*args)
    @panels=Array.new(args.size)
    0.upto(args.size-1) do |i|
      if args[i].is_a? Array
        insertTab i,args[i][0]
        c = args[i][1] ? args[i][1] : VRPanel
        s = args[i][2] ? args[i][2] : "panel#{i}"
        x,y,w,h = adjustRect(0,0,self.w,self.h,false)
        @panels[i] = addControl(c,s,s,x,y,w-x,h-y)
      else
        insertTab i,args[i]
        x,y,w,h = adjustRect(0,0,self.w,self.h,false)
        @panels[i] = addControl(VRPanel,"panel#{i}","panel#{i}",x,y,w-x,h-y)
        @panels[i].extend VRContainersSet
        @panels[i].containers_init
      end
      @panels[i].show 0
    end

    @_vr_prevpanel=0
    selectTab 0
  end

  def send_parent2(i,name,method)
    @panels[i].send_parent(name,method)
    send_parent "panel#{i}","#{name}_#{method}"
  end

  def selectTab(i)
    super
    selchanged
  end

  def selchanged
    raise "assigned no panels" if @panels.size<1
    @panels[@_vr_prevpanel].show(0)
    t=selectedTab
    @panels[t].show
    @_vr_prevpanel=t
    @panels[t].refresh
  end
  
  def self__vr_tabpanelresize(w,h)
    x,y,w,h = adjustRect(0,0,self.w,self.h,false).map {|x| x & 0x7fffffff}
    return if @panels.nil?
    @panels.each{|i| i.move(x,y,w-x,h-y)}
  end
end

unless VR_OLDCOMCTL then


module WMsg
  RB_INSERTBAND   = WM_USER +  1
  RB_DELETEBAND   = WM_USER +  2
  RB_GETBARINFO   = WM_USER +  3
  RB_SETBARINFO   = WM_USER +  4
  RB_GETBANDCOUNT = WM_USER + 12
  RB_GETROWCOUNT  = WM_USER + 13
  RB_GETROWHEIGHT = WM_USER + 14
  RB_SETBKCOLOR   = WM_USER + 19
  RB_GETBKCOLOR   = WM_USER + 20
  RB_SETTEXTCOLOR = WM_USER + 21
  RB_GETTEXTCOLOR = WM_USER + 22
  RB_SIZETORECT   = WM_USER + 23
  RB_GETBARHEIGHT = WM_USER + 27
  RB_GETBANDINFO  = WM_USER + 29
  RB_SHOWBAND     = WM_USER + 35
  RB_SETPALETTE   = WM_USER + 37
  RB_GETPALETTE   = WM_USER + 38
  RB_MOVEBAND     = WM_USER + 39

  RBN_LAYOUTCHANGED = 0x100000000-831-2
end


module WConst
  RBBIM_STYLE         = 0x00000001
  RBBIM_COLORS        = 0x00000002
  RBBIM_TEXT          = 0x00000004
  RBBIM_IMAGE         = 0x00000008
  RBBIM_CHILD         = 0x00000010
  RBBIM_CHILDSIZE     = 0x00000020
  RBBIM_SIZE          = 0x00000040
  RBBIM_BACKGROUND    = 0x00000080
end

class VRRebar < VRNotifyControl
=begin
== VRRebar
  Rebar control.
  If comctl32.dll on your system is too old, this is not available.
=== Methods
--- insertband(cntl,txt,minx=30,miny=cnt.h+2,band=-1)
    Creates a new band and set the control on it.
    ((|txt|)) is the caption of the band and minx/miny is the minimum size of
    the band.
    The control is created by rebar's addControl() but its event handling is on
    the parent window.
--- bkColor=(c)
--- bkColor
    Sets/Gets background color of rebar.
--- textColor=(c)
--- textColor
    Sets/Gets band caption color.
--- relayout(x=self.x, y=self.y, w=self.w, h=self.h)
    rearranges rebar's bands in the specified rectangle.
=end

  WINCLASSINFO = ["ReBarWindow32",0]

  def rebarinit
    sendMessage WMsg::RB_SETBARINFO,0,[12,0,0].pack("LLL")
    addNotifyHandler WMsg::RBN_LAYOUTCHANGED,"layoutchanged",MSGTYPE::ARGNONE,nil
  end

  def vrinit
    super
    rebarinit
  end

  def insertband(cnt,txt,minx=30,miny=cnt.h+2,band=-1)
    size = 4*14
    mask= WConst::RBBIM_TEXT | WConst::RBBIM_STYLE | WConst::RBBIM_CHILD | WConst::RBBIM_CHILDSIZE | WConst::RBBIM_SIZE
    style= 4 #RBBS_CHILDEDGE
    frcolor= 0
    bkcolor= 0
    text= txt
    cch= 0
    image= 0
    hwndChild= cnt.hWnd
    cxmin= minx
    cymin= miny
    cx= 100
    bkBmp= 0
    wid= 0
    tis = [size,mask,style,frcolor,bkcolor,text,cch,image,hwndChild,cxmin,cymin,cx,bkBmp,wid].pack("LLLLLP#{text.length}LLLLLLLL")
    sendMessage WMsg::RB_INSERTBAND,band,tis
  end

  def h
    sendMessage WMsg::RB_GETBARHEIGHT,0,0
  end

  def bkColor
    sendMessage WMsg::RB_GETBKCOLOR,0,0
  end
  def bkColor=(c)
    sendMessage WMsg::RB_SETBKCOLOR,0,c
  end

  def textColor
    sendMessage WMsg::RB_GETTEXTCOLOR,0,0
  end
  def textColor=(c)
    sendMessage WMsg::RB_SETTEXTCOLOR,0,c
  end

  def relayout(x=self.x,y=self.y,w=self.w,h=self.h)
    sendMessage WMsg::RB_SIZETORECT,0,[x,y,w,h].pack("LLLL")
  end

  include VRParent
  def newControlID
    @parent.newControlID
  end
  def registerControl(*arg)
    @parent.registerControl(*arg)
  end
end

end  # unlessVR_OLDCOMCTL


# contribute files
require VR_DIR+'contrib/toolbar'
require VR_DIR+'contrib/vrlistviewex'

if VR_COMPATIBILITY_LEVEL then
  require VR_DIR + 'compat/vrcomctl.rb'
end
