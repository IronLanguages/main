###################################
#
# vrdde.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

=begin
= VisualuRuby(tmp) modules for DDE
This file provides modules for DDE conversation.
<<< handlers.rd
=end


# DDE_ADVISE not tested.

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require VR_DIR+'sysmod'

module DDElParam
  PackDDElParam=Win32API.new("user32","PackDDElParam",["I","I","I"],"L")
  UnpackDDElParam=Win32API.new("user32","UnpackDDElParam",["I","L","P","P"],"I")
  FreeDDElParam=Win32API.new("user32","FreeDDElParam",["I","L"],"I")
  ReuseDDElParam=Win32API.new("user32","ReuseDDElParam","IIIII","L")

  def packDDElParam(msg,low,high)
    PackDDElParam.call(msg,low,high)
  end
  def unpackDDElParam(msg,lparam)
    a="        "; b="        "
    UnpackDDElParam.call(msg,lparam,a,b)
    [a.unpack("I")[0],b.unpack("I")[0]]
  end

  def freeDDElParam(msg,lparam)
    FreeDDElParam.call(msg,lparam)
  end

  def reuseDDElParam(lParam, uMsgIn, uMsgOut, uLow, uHigh) #by yukimi_sake
    ReuseDDElParam.call(lParam, uMsgIn, uMsgOut, uLow, uHigh)
  end
end

module VRDdeConversation
=begin
== VRDdeConversation
Utilities for DDE conversation.

=== Methods
--- sendDDEAck(shwnd,aItem,retcode=0,ack=true,busy=false)
    Sends DDE_ACK message to shwnd.

== VRDdeConversation::DDEAckFlags
This is a capsule of return code,ack flag, busy flag for DDE_ACK message
=== Attributes
--- retcode
    8bit value.
--- ack
    DDE request is accepted or not.
--- busy
    busy or not.
=end

  WM_DDE_INITIATE = 0x3e0
  WM_DDE_TERMINATE= 0x3e1
  WM_DDE_ADVISE   = 0x3e2
  WM_DDE_UNADVISE = 0x3e3
  WM_DDE_ACK      = 0x3e4
  WM_DDE_DATA     = 0x3e5
  WM_DDE_REQUEST  = 0x3e6
  WM_DDE_POKE     = 0x3e7
  WM_DDE_EXECUTE  = 0x3e8

  class DDEAckFlags
#    attr_accessor :ack
#    attr_accessor :busy
#    attr_accessor :retcode
    def ack() @_vr_ack; end
    def busy() @_vr_busy; end
    def retcode() @_vr_retcode; end

    def initialize(ack=true,busy=false,retcode=0)
      @_vr_ack,@_vr_busy,@_vr_retcode = ack,busy,retcode
    end
  end

  def sendDDEAck(shwnd,aItem,retcode=0,ack=true,busy=false)
    r = retcode & 0xff
    r |= 0x8000 if ack 
    r |= 0x4000 if busy 
    lparam = packDDElParam(WM_DDE_ACK,r,aItem)
    SMSG::PostMessage.call shwnd,WM_DDE_ACK, self.hWnd,lparam
  end
end

module VRDdeServer
=begin
== VRDdeServer
This module prepares fundamental functions for DDE server.

=== Methods
--- addDDEAppTopic(appname,topic)
    Adds acceptable pair of application name and topic for DDE.
--- delDDEAppTopic(appname,topic)
    Deletes acceptable pair of application name and topic.

=== Event handlers
--- ????_ddeinitiate(shwnd,appname,topic)
    Fired when DDE client whose hwnd is ((|shwnd|)) connects the 
    DDE server.
--- ???_ddeterminate(shwnd)
    Fired when DDE client whose hwnd is ((|shwnd|)) disconnects.
=end

  include VRMessageHandler
  include VRDdeConversation
  include DDElParam

  def ddeserverinit
    acceptEvents([WM_DDE_INITIATE,WM_DDE_TERMINATE])
    addHandler WM_DDE_INITIATE,"_ddeInitiate",MSGTYPE::ARGINTINTINT,nil
    addHandler WM_DDE_TERMINATE,"_ddeTerminate",MSGTYPE::ARGWINT,nil
    @_vr_clients={} #{client's hwnd=>[client's hwnd,appname,topic]
    @_vr_ddeacceptable=[] # [appname,topic]
  end
  def vrinit
    super
    ddeserverinit
  end

  def self__ddeInitiate(shwnd,aApp,aTopic)
    p=[GAtom::GetName(aApp),GAtom::GetName(aTopic)]
    return unless @_vr_ddeacceptable.index(p)

    @_vr_clients[shwnd] = [shwnd] + p
    r=nil
    r=selfmsg_dispatching("ddeinitiate",shwnd,p[0],p[1])
    SMSG::SendMessage.call shwnd,WM_DDE_ACK,self.hWnd,MAKELPARAM(aApp,aTopic)
  end

  def self__ddeTerminate(shwnd)
    selfmsg_dispatching("ddeterminate",shwnd)
    SMSG::PostMessage.call shwnd,WM_DDE_TERMINATE,self.hWnd,0
    @_vr_clients.delete(shwnd)
  end

  def addDDEAppTopic(appname,topic)
    @_vr_ddeacceptable.push([appname,topic])
  end
  def delDDEAppTopic(appname,topic)
    @_vr_ddeacceptable.delete([appname,topic])
  end

  def self_ddeinitiate(*args) end
  def self_ddeterminate(*arg) end
end

module VRDdeExecuteServer
=begin
== VRDdeExecuteServer
This module provides a feature of DDE_Execute server.
((<VRDdeServer>)) is included.

=== Event handler
--- self_ddeexecute(command,shwnd,appname,topic)
    Fired when the client whose name is ((|shwnd|)) sends DDE_EXECUTE 
    to the server.The command string is ((|command|)).
    If the return value is a kind of VRDdeConversation::((<DDEAckFlags>)),
    DDE_ACK message that will be sent is according to this return value.
=end

  include VRDdeServer
  
  EXECUTEMETHOD="ddeexecute"

  def ddeexecuteserverinit
    addEvent WM_DDE_EXECUTE
    addHandler WM_DDE_EXECUTE,"_ddeexecuteinternal",MSGTYPE::ARGINTINT,nil
  end
  def vrinit
    super
    ddeexecuteserverinit
  end
  
  def self__ddeexecuteinternal(shwnd,hcmd)
    cl=@_vr_clients[shwnd]
    raise "unknown dde client (not initiated)" unless cl
    cmd=GMEM::Get(hcmd).unpack("A*")[0]
    ret=nil
    ret=selfmsg_dispatching(EXECUTEMETHOD,cmd,*cl)
    freeDDElParam(WM_DDE_EXECUTE,hcmd) #by yukimi_sake
    if ret.is_a?(DDEAckFlags) then
      sendDDEAck shwnd,hcmd,(ret.retcode || 0),ret.ack,ret.busy
    else
      sendDDEAck shwnd,hcmd
    end
  end
end

module VRDdeRequestServer #by yukimi_sake
=begin
== VRDdeRequestServer
This module provides a feature of DDE_REQUEST server.
((<VRDdeServer>)) is included.

=== Event handler
--- self_dderequest(item,shwnd,app,topic)
    Fired when the client whose name is ((|shwnd|)) sends DDE_REQUEST 
    to the server.The item string is ((|item|)).
    Retuern value(as String) is sended to shwnd which is handle of client,
    using WM_DDE_DATA message.
    If the return value is a kind of VRDdeConversation::((<DDEAckFlags>)),
    DDE_ACK message that will be sent is according to this return value.
=end

  include VRDdeServer
  
  EXECUTEMETHOD="dderequest"
  GMEM_DDESHARE=0x2000
  
  def sendDDEData(shwnd,lp,data)
    raise "Data must be a String" unless data.is_a? String
    dDEDATA=[0xb000,ClipboardFormat::CF_TEXT,data+"\0"].pack("Ssa*")
    hData=GMEM::AllocStr(GMEM_DDESHARE,dDEDATA)
    lParam=reuseDDElParam lp,WM_DDE_REQUEST,WM_DDE_DATA,hData,HIWORD(lp)
    SMSG::PostMessage.call shwnd, WM_DDE_DATA, self.hWnd, lParam
  end
  
  def dderequestserverinit
    addEvent WM_DDE_REQUEST
    addHandler WM_DDE_REQUEST,"_dderequestinternal",MSGTYPE::ARGINTINT,nil
  end
  
  def vrinit
    super
    dderequestserverinit
  end
  
  def self__dderequestinternal(shwnd,lparam)
   cl=@_vr_clients[shwnd]
    raise "unknown dde client (not initiated)" unless cl
    cformat,aitem = unpackDDElParam(WM_DDE_REQUEST,lparam)
    raise "not supported this format" unless cformat == 1
    item = GAtom::GetName(aitem)
    ret=nil
    ret=selfmsg_dispatching(EXECUTEMETHOD,item,*cl)
    if ret.is_a?(DDEAckFlags) then
      freeDDElParam(WM_DDE_REQEST,lparam)
      sendDDEAck shwnd,aitem,(ret.retcode || 0),ret.ack,ret.busy
    else
      sendDDEData shwnd,lparam,ret
    end
  end
  def self_dderequest(item,shwnd,app,topic) end
end

module VRDdePokeServer #by yukimi_sake
=begin
== VRDdePokeServer
This module provides a feature of DDE_POKE server.
((<VRDdeServer>)) is included.

=== Event handler
--- self_ddepoke(item,data,cfmt,shwnd,app,topic)
    Fired when the client whose name is ((|shwnd|)) sends DDE_POKE 
    to the server.The item string is ((|item|)).The data is ((|data|)).
    If you want to treat data other than String, carry out according to
    clipboard format ((|cfmt|)) in the handler.
    If the return value is a kind of VRDdeConversation::((<DDEAckFlags>)),
    DDE_ACK message that will be sent is according to this return value.
=end
  include VRDdeServer
  
  EXECUTEMETHOD="ddepoke"
  
  def ddepokeserverinit
    addEvent WM_DDE_POKE
    addHandler WM_DDE_POKE,"_ddepokeinternal",MSGTYPE::ARGINTINT,nil
  end
  
  def vrinit
    super
    ddepokeserverinit
  end
  
  def self__ddepokeinternal(shwnd,lparam)
    cl=@_vr_clients[shwnd]
    raise "unknown dde client (not initiated)" unless cl
    hdata,aitem = unpackDDElParam(WM_DDE_POKE,lparam)
    item = GAtom::GetName(aitem)
    datastr = GMEM::Get(hdata)
    flag,cfmt,data = datastr.unpack("SsA*")
    if (flag&0x2000) > 0 then  # fRelease is asserted
      GMEM::Free(hdata)
    end
    ret=nil
    ret=selfmsg_dispatching(EXECUTEMETHOD,item,data,cfmt,*cl)
    freeDDElParam(WM_DDE_POKE, lparam)
    if ret.is_a?(DDEAckFlags) then
      sendDDEAck shwnd,aitem,(ret.retcode || 0),ret.ack,ret.busy
    else
      sendDDEAck shwnd,aitem
    end
  end
  def self_ddepoke(item,data,cfmt,shwnd,app,topic) end
end

module VRDdeClient
=begin
== VRDdeClient
This module provides features of DDE clients.

=== Methods
--- ddeconnected?
--- ddeready?
    Returns whether it is able to request dde conversation.
--- ddebusy?
    Returns true when it waits DDE_ACK/DDE_DATA message.

--- ddeexecute(appname,topic.command)
    Sends DDE_EXECUTE request to the server specified by the pair of (appname,topic).
--- ddepoke(appname,topic,item,data,fmt)
    Sends DDE_POKE request to the server specified by the pair of (appname,topic).
    The ((|data|)) in the format of ((|fmt|)) (1 as CF_TEXT and so on) is transferred 
    tothe ((|item|)) in the server
--- dderequest(appname,topic,item,fmt)
    Sends DDE_REQUEST request to the server specified by the pair of (appname,topic).
    The server calls back ((<self_dderequestdata>)) method of client window to
    transfer the data of ((|item|)).
--- ddeadvise(appname,topic,item,fmt)
--- ddeunadvise(appname,topic,item,fmt)
    Not tested.

=== Event handlers
???? is the downcase-ed appname of the DDE conversation. It is because the ???? name
is named by WM_DDE_ACK message's App-name which is created by the server and that 
may be different from the requested appname from client.

--- ????_ddeterminate(shwnd)
    Fired when the server disconnects the conversation.
--- ????_dderefused(retcode,busy)
    Fired when the server refused the connection. The return code is ((|retcode|)) and
    busy flag is ((|busy|)).
--- ????_ddeexecdone(retcode)
    Fired when the server accepts the DDE_EXECUTE request.
--- ????_ddepokedone(retcode)
    Fired when the server accepts the DDE_POKE request.
--- ????_ddedata(data,fmt,flag)
    Fired when the server returns the data by the DDE_REQUEST that client sended.
    If the return value is a kind of VRDdeConversation::((<DDEAckFlags>)),
    DDE_ACK message that will be sent is according to this return value.
--- ????_ddeadvisedata(data,fmt,flag)
    Not tested.
=end

  include VRMessageHandler
  include VRDdeConversation
  include DDElParam

 private
  STATUSCONNECT= 1
  STATUSEXECUTE= 2
  STATUSPOKE   = 4
  STATUSREQUEST= 8
  STATUSADVISE = 16
  STATUSTERMINATE=128
  GMEMSTYLE = 0x2042


  def ddeconnect(appname,topic)
    raise "DDE_INITIATE busy!" if @_vr_ddesearching
    raise "Application name must be specified" unless appname
    aApp   = GAtom::Add(appname)
    aTopic = if topic then   GAtom::Add(topic) else 0 end

    srv =  @_vr_servers.find do |key,item| item[1]==appname end

    shwnd = if srv then srv[0] else 0xffffffff end
    @_vr_ddesearching=nil
    SMSG::SendMessage.call shwnd,WM_DDE_INITIATE,self.hWnd,MAKELPARAM(aApp,aTopic)
    shwnd = @_vr_ddesearching unless srv
    @_vr_ddesearching=nil
    
    GAtom::Delete(aApp)   
    GAtom::Delete(aTopic) if topic!=0
    raise "DDE Server (#{appname}:#{topic}) not found" unless shwnd
    shwnd
  end

  def ddeterminate(shwnd)
    SMSG::PostMessage.call shwnd,WM_DDE_TERMINATE,self.hWnd,0
    @_vr_servers[shwnd][2]|=STATUSTERMINATE
#    @_vr_servers.delete(shwnd)
  end

 public
  def vrinit
    super
    ddeclientinit
  end

  def ddeclientinit
    acceptEvents([WM_DDE_ACK,WM_DDE_DATA,WM_DDE_TERMINATE])
    addHandler WM_DDE_ACK, "ddeAck", MSGTYPE::ARGINTINT,nil
    addHandler WM_DDE_DATA,"ddeData",MSGTYPE::ARGINTINT,nil
    addHandler WM_DDE_TERMINATE,"ddeTerminate",MSGTYPE::ARGWINT,nil
    @_vr_servers={} # {server's hwnd => [server's hwnd,appname,status,params]}
    @_vr_ddesearching = false
  end

  def ddeconnected?(appname)
    srv =  @_vr_servers.find do |key,item| item[1].downcase==appname.downcase end
    if srv then srv=srv[1];((srv[2]&STATUSCONNECT)>0) else false end
  end
  def ddebusy?(appname)
    srv =  @_vr_servers.find do |key,item| item[1].downcase==appname.downcase end
    if srv then srv=srv[1]; ((srv[2] & ~STATUSCONNECT)>0) else nil end
  end
  def ddeidle?(appname)
    srv =  @_vr_servers.find do |key,item| item[1].downcase==appname.downcase end
    if srv then
      srv=srv[1]
      ((srv[2]&STATUSCONNECT)>0) and ((srv[2] & ~STATUSCONNECT)==0)
    else
      true
    end
  end
  def ddeready?(appname)
    srv =  @_vr_servers.find do |key,item| item[1].downcase==appname.downcase end
    if srv then
      srv=srv[1]
      ((srv[2]&STATUSCONNECT)>0) and 
      ((srv[2] & ~(STATUSCONNECT | STATUSADVISE))==0)
    else
      true
    end
  end

  def ddeexecute(appname,topic,cmdstr)
    raise "dde(#{appname}:#{topic}) not ready" unless ddeready?(appname)
    shwnd=ddeconnect(appname,topic)
    executemem = GMEM::AllocStr(GMEMSTYLE,cmdstr)
    @_vr_servers[shwnd][2] |= STATUSEXECUTE
    SMSG::PostMessage.call shwnd,WM_DDE_EXECUTE,self.hWnd,executemem
  end

  def ddepoke(appname,topic,item,data,fmt=ClipboardFormat::CF_TEXT) 
    raise "dde(#{appname}:#{topic}) not ready" unless ddeready?(appname)
    shwnd=ddeconnect(appname,topic)

    aItem   = GAtom::Add(item.to_s)

    pokedata=[0,fmt].pack("Ss")+data.to_s
    pokemem = GMEM::AllocStr(GMEMSTYLE,pokedata)

    @_vr_servers[shwnd][2] |= STATUSPOKE
    @_vr_servers[shwnd][3] = pokemem

    lparam = packDDElParam(WM_DDE_POKE,pokemem,aItem)
    SMSG::PostMessage.call shwnd,WM_DDE_POKE,self.hWnd,lparam
  end

  def dderequest(appname,topic,item,fmt=ClipboardFormat::CF_TEXT) 
    raise "dde(#{appname}:#{topic}) not ready" unless ddeready?(appname)
    shwnd=ddeconnect(appname,topic)

    aItem   = GAtom::Add(item.to_s)

    @_vr_servers[shwnd][2] |= STATUSREQUEST
    SMSG::PostMessage.call shwnd,WM_DDE_REQUEST,self.hWnd,
                             MAKELPARAM(fmt.to_i,aItem)
  end

  def ddeadvise(appname,topic,item,fmt=1,defup=false,ackreq=false)
    raise "dde(#{appname}:#{topic}) not ready" unless ddeready?(appname)
    shwnd=ddeconnect(appname,topic)

    aItem   = GAtom::Add(item.to_s)

    flag = 0
    if defup then flag |= 0x4000 end
    if ackreq then flag |= 0x8000 end
    
    advisedata=[flag,fmt].pack("Ss")
    advisemem = GMEM::AllocStr(GMEMSTYLE,advisedata)
    
    @_vr_servers[shwnd][2] |= STATUSADVISE
    lparam = packDDElParam(WM_DDE_POKE,advisemem,aItem)
    SMSG::PostMessage.call shwnd,WM_DDE_ADVISE,self.hWnd,lparam
  end

  def ddeunadvise(appname,topic,item,fmt=0)
    raise "dde(#{appname}:#{topic}) not ready" unless ddeready?(appname)
    shwnd=ddeconnect(appname,topic)

    aItem   = GAtom::Add(item.to_s)

    @_vr_servers[n][2] |= STATUSREQUEST
    SMSG::PostMessage.call @_vr_servers[n][0],WM_DDE_UNADVISE,self.hWnd,
                             MAKELPARAM(fmt.to_i,aItem)
  end


  def self_ddeTerminate(shwnd)
      return unless @_vr_servers[shwnd]
      controlmsg_dispatching(@_vr_servers[shwnd][1],"ddeterminate")
      @_vr_servers.delete(shwnd)
  end

  def self_ddeAck(shwnd,lparam)
#p "ACK"
    sv = @_vr_servers[shwnd]
    fname=""
    if(!sv) then
      aApp,aTopic = LOWORD(lparam),HIWORD(lparam)
      appname=GAtom::GetName(aApp).downcase
      tpcname=GAtom::GetName(aTopic)
      appname2=appname.dup
      def appname2.name() self; end   # tricky thing for parentrelayer
      @_vr_servers[shwnd]=[shwnd,appname2,STATUSCONNECT,nil]

      @_vr_ddesearching=shwnd

      GAtom::Delete(aApp)
      GAtom::Delete(aTopic)

    elsif (sv[2] & ~STATUSCONNECT)>0 then
      wstatus,param = unpackDDElParam(WM_DDE_ACK,lparam)
      freeDDElParam(WM_DDE_ACK,lparam)

      retcode = wstatus&0xf
      busy= 0<(wstatus&0x4000)
      ack = 0<(wstatus&0x8000)
      sname = sv[1]

      unless ack then
        sv[2] &= STATUSCONNECT
#        fname=sname+"_dderefused"
#        __send__(fname,retcode,busy) if respond_to?(fname)
        controlmsg_dispatching(sname,"dderefused",retcode,busy)
      end
      if (sv[2] & STATUSEXECUTE)>0 then
        GMEM::Free(param) unless ack
        sv[2] &= ~STATUSEXECUTE
        fname="ddeexecdone"
        ddeterminate(shwnd)
      elsif(sv[2] & STATUSPOKE)>0 then
        GAtom::Delete(param)
        GMEM::Free(sv[3])
        sv[3]=nil
        sv[2] &= ~STATUSPOKE
        fname="ddepokedone"
        ddeterminate(shwnd)
      elsif(sv[2] & STATUSREQUEST)>0 then
        GAtom::Delete(param)
        sv[2] &= ~STATUSPOKE
        fname=nil
        ddeterminate(shwnd)
      elsif(sv[2] & STATUSADVISE)>0 then
        GMEM::Free(param) unless ack
        sv[2] &= ~STATUSADVISE
        fname=nil
      else
        ddeterminate(shwnd)
      end
      controlmsg_dispatching(sname,fname,retcode) if ack and fname
    else
      raise "DDE MultiSession Error"
    end
  end

  def self_ddeData(shwnd,lparam)
#p "DATA"
    datamem,aItem = unpackDDElParam(WM_DDE_DATA,lparam)
    freeDDElParam(WM_DDE_DATA,lparam)

    sv = @_vr_servers[shwnd]
    fname=sname=nil

    datastr = GMEM::Get(datamem)
    flag,fmt,data = datastr.unpack("Ssa*")

    if (flag&0x2000) > 0 then  # fRelease is asserted
      GMEM::Free(datamem)
    end

    ret = nil
    if(!sv) then
      # Ignored
    else
      sname=sv[1]
      if (sv[2] & STATUSREQUEST)>0 then
        sv[2]&=1
        fname="ddedata"
        ddeterminate(sv[0])
      elsif (sv[2] & STATUSADVISE)>0 
        sv[2]&=1
        fname="ddeadvisedata"
      else
      end

      fn=sname+fname
      if(fn) then
        ret=controlmsg_dispatching(sname,fname,data,fmt,flag)
      end
    end

    if (flag&0x8000) > 0 then  #fAckReq is asserted
      if ret.is_a?(DDEAckFlags) then
        sendDDEAck shwnd,aItem,(ret.retcode || 0),ret.ack,ret.busy
      else
        sendDDEAck shwnd,aItem
      end
    else
      GAtom::Delete(aItem)
    end

  end
end
