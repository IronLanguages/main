require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module US
      module Central
        include TimezoneDefinition
        
        linked_timezone 'US/Central', 'America/Chicago'
      end
    end
  end
end
