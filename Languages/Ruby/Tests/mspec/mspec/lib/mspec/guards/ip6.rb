require 'mspec/guards/guard'
require 'socket'

class IP6Guard < SpecGuard
  def match?
    Socket.constants.include?('AF_INET6') && (Socket::AF_INET6.class != Object)
  end
end

class Object
  def supports_ip6(*args)
    g = IP6Guard.new(*args)
    g.name = :supports_ip6
    yield if g.yield?
  ensure
    g.unregister
  end

  def does_not_support_ip6(*args)
    g = IP6Guard.new(*args)
    g.name = :does_not_support_ip6
    yield if g.yield? true
  ensure
    g.unregister
  end
end

