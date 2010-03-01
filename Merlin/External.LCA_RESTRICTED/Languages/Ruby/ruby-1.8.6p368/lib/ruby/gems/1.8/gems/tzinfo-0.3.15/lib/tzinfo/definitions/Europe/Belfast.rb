require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Europe
      module Belfast
        include TimezoneDefinition
        
        linked_timezone 'Europe/Belfast', 'Europe/London'
      end
    end
  end
end
