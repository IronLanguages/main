require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Australia
      module NSW
        include TimezoneDefinition
        
        linked_timezone 'Australia/NSW', 'Australia/Sydney'
      end
    end
  end
end
