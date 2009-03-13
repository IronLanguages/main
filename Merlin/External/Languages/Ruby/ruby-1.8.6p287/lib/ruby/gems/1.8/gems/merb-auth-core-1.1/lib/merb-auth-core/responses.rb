module Merb
  # These are not intended to be used directly
  class Authentication
    attr_accessor :body
    
    def redirected?
      !!headers["Location"]
    end
    
    def headers
      @headers ||= {}
    end
  
    def status
      @status ||= 200
    end
    
    def status=(sts)
      @status = sts
    end
  
    def halted?
      !!@halt
    end
    
    def headers=(headers)
      raise ArgumentError, "Need to supply a hash to headers.  Got #{headers.class}" unless headers.kind_of?(Hash)
      @headers = headers
    end
    
    def halt!
      @halt = true
    end
  
  end # Merb::Authentication
end # Merb
