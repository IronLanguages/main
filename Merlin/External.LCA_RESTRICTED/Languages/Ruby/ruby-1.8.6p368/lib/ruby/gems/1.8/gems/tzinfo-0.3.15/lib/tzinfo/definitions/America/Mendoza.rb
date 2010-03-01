require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module America
      module Mendoza
        include TimezoneDefinition
        
        linked_timezone 'America/Mendoza', 'America/Argentina/Mendoza'
      end
    end
  end
end
