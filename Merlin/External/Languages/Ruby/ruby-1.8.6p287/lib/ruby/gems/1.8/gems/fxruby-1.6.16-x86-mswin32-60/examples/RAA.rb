require 'soap/rpc/driver'
require 'iRAA'

module RAA
  extend SOAP

  class Driver
    def initialize(server = 'http://raa.ruby-lang.org/soap/1.0/', proxy = nil)
      @drv = SOAP::RPC::Driver.new(server, RAA::InterfaceNS)
      @drv.httpproxy = proxy if proxy
      @drv.mapping_registry = RAA::MappingRegistry
      RAA::Methods.each do |name, *params|
	@drv.add_method(name, params)
      end
    end

    def setLogDev(logdev)
      # ignored.
    end

    def method_missing(msg_id, *a, &b)
      @drv.__send__(msg_id, *a, &b)
    end
  end
end
