require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Canada
      module Yukon
        include TimezoneDefinition
        
        linked_timezone 'Canada/Yukon', 'America/Whitehorse'
      end
    end
  end
end
