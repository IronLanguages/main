require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../fixtures/classes'
include Socket::Constants

describe "Socket::Constants" do
  it "defines socket types" do
    consts = ["SOCK_DGRAM", "SOCK_RAW", "SOCK_RDM", "SOCK_SEQPACKET", "SOCK_STREAM"]
    consts.each do |c|
      Socket::Constants.should have_constant(c)
    end
  end

  platform_is_not :windows do
    it "defines protocol families" do
      consts = ["PF_INET6", "PF_INET", "PF_IPX", "PF_UNIX", "PF_UNSPEC"] 
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end

  platform_is :windows do
    it "defines protocol families" do
      consts = ["PF_APPLETALK", "PF_ATM", "PF_CCITT", "PF_CHAOS", "PF_DATAKIT", "PF_DLI", "PF_ECMA",
                "PF_HYLINK", "PF_IMPLINK", "PF_INET", "PF_IPX", "PF_ISO", "PF_LAT", "PF_MAX",
                "PF_NS", "PF_OSI", "PF_PUP", "PF_SNA", "PF_UNIX", "PF_UNSPEC"]
 
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is_not :windows do
    it "defines address families" do
      consts = ["AF_INET6", "AF_INET", "AF_IPX", "AF_UNIX", "AF_UNSPEC"] 
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is :windows do
    it "defines address families" do
      consts = ["AF_APPLETALK", "AF_ATM", "AF_CCITT", "AF_CHAOS", "AF_DATAKIT", "AF_DLI", "AF_ECMA",
                "AF_HYLINK", "AF_IMPLINK", "AF_INET", "AF_IPX", "AF_ISO", "AF_LAT", "AF_MAX", 
                "AF_NETBIOS", "AF_NS", "AF_OSI", "AF_PUP", "AF_SNA", "AF_UNIX", "AF_UNSPEC"]
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end

  it "defines send/receive options" do
    consts = ["MSG_DONTROUTE", "MSG_OOB", "MSG_PEEK"]     
    consts.each do |c|
      Socket::Constants.should have_constant(c)
    end
  end

  it "defines socket level options" do
    consts = ["SOL_SOCKET"]
    consts.each do |c|
      Socket::Constants.should have_constant(c)
    end
  end

  platform_is_not :windows do
    it "defines socket options" do
      consts = ["SO_BROADCAST", "SO_DEBUG", "SO_DONTROUTE", "SO_ERROR", "SO_KEEPALIVE", "SO_LINGER", 
                "SO_OOBINLINE", "SO_RCVBUF", "SO_REUSEADDR", "SO_SNDBUF", "SO_TYPE"]  
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is :windows do
    it "defines socket options" do
      consts = ["SO_ACCEPTCONN", "SO_BROADCAST", "SO_DEBUG", "SO_DONTROUTE", "SO_ERROR", 
                "SO_KEEPALIVE", "SO_LINGER", "SO_OOBINLINE" , "SO_RCVBUF", "SO_RCVLOWAT", 
                "SO_RCVTIMEO", "SO_REUSEADDR", "SO_SNDBUF", "SO_SNDLOWAT", "SO_SNDTIMEO", "SO_TYPE", 
                "SO_USELOOPBACK"]
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is_not :windows do
    it "defines multicast options" do
      consts = ["IP_ADD_MEMBERSHIP", "IP_DEFAULT_MULTICAST_LOOP", "IP_DEFAULT_MULTICAST_TTL", 
                "IP_MAX_MEMBERSHIPS", "IP_MULTICAST_LOOP", "IP_MULTICAST_TTL"]
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is :windows do
    it "defines multicast options" do
      consts = ["IP_ADD_MEMBERSHIP", "IP_DROP_MEMBERSHIP", "IP_HDRINCL", "IP_MULTICAST_IF",
                "IP_MULTICAST_LOOP", "IP_MULTICAST_TTL", "IP_OPTIONS", "IP_TOS", "IP_TTL"]
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is_not :windows do
    it "defines TCP options" do
      consts = ["TCP_MAXSEG", "TCP_NODELAY"]
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
  
  platform_is :windows do
    it "defines TCP options" do
      consts = ["TCP_NODELAY"]
      consts.each do |c|
        Socket::Constants.should have_constant(c)
      end
    end
  end
end
