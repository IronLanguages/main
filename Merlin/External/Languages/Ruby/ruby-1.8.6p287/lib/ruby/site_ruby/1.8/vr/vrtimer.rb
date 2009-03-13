###################################
#
# vrtimer.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 1999-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'vruby'
require "Win32API"

=begin
= VisualuRuby(tmp) Module(s) for Timer
<<<handlers.rd
=end



module VRTimerFeasible
=begin
== VRTimerFeasible
Interval Timers.
This module prepares create/delete interval timers.

=== Methods
--- addTimer(interval=1000,timername="self")
    Creates an interval timer whose name is defined by ((|timername|)).
    Interval timers invoke ((|timername|))_timer method every interval times 
    in millisecond.

--- deleteTimer(timername="self")
    Deletes an interval timer whose name is ((|timername|))

--- timeralive?(timername="self")
    Returns boolean whether timer is alive or not.

=== Event handler
--- ????_timer
    Fired every interval times by the interval timer.
=end


  include VRMessageHandler

  SetTimer  = Win32API.new("USER32", "SetTimer",['L', 'I', 'I', 'L'], 'I')
  KillTimer = Win32API.new("USER32", "KillTimer",['L', 'L'], 'I')
  WM_TIMER = 275

 private

  def newtimerid
    @_vr_timernextid=0 unless defined? @_vr_timernextid
    @_vr_timernextid+=1
  end

 public
 
  def self_vrtimer(id)       # WM_TIMER handler
    name=@_vr_timers[id][0].dup
    class << name
      def name; self; end
    end
#    name += "_"+"timer"
#    selfmsg_dispatching(name)
    controlmsg_dispatching(name,"timer")
#
    __send__(name) if respond_to?(name)
  end

 def vrinit
    super
    timerfeasibleinit
  end

  def timerfeasibleinit
    @_vr_timerinterval=1000
    addHandler WM_TIMER,  "vrtimer",  MSGTYPE::ARGWINT,nil  #check
    addEvent WM_TIMER
    addNoRelayMessages [WM_TIMER]
  end

  def addTimer(interval=1000,timername="self")
    id=newtimerid
    SetTimer.call(self.hWnd,id,interval,0)
    @_vr_timers=[] unless defined? @_vr_timers
    @_vr_timers[id] = [timername,interval]
    id
  end

  def deleteTimer(timername="self")
    id=timerid(timername)

   if id then
      KillTimer.call(self.hWnd,id)
      @_vr_timers[id]=nil
    else
      raise RuntimeError,"No such Timer"
    end
  end
  
  def timerid(timername="self")
    return nil unless @_vr_timers
    r=nil
    1.upto(@_vr_timers.size-1) do |i|
      if @_vr_timers[i] and @_vr_timers[i][0]==timername then
        r=i
      end
    end
    r
  end

  def timeralive?(tname="self")
    timerid(tname).is_a?(Integer)
  end
end

=begin sample
require 'vrcontrol'
module Test
  include VRTimerFeasible

  def construct
    self.caption="test"
    go=addControl VRButton,"delete","delete",10,10,100,20
    addTimer 1000,"t1"
    addTimer 1200,"t2"
  end

  def t1_timer
    p "t1"
  end
  def t2_timer
    print timeralive?("t1") ,":", timeralive?("t2"),"\n"
  end

  def delete_clicked
    deleteTimer "t1"
  end

end

VRLocalScreen.showForm(Test)
VRLocalScreen.messageloop
=end
