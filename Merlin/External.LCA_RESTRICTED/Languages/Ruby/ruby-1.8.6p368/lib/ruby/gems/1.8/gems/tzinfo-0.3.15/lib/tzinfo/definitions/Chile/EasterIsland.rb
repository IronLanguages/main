require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Chile
      module EasterIsland
        include TimezoneDefinition
        
        linked_timezone 'Chile/EasterIsland', 'Pacific/Easter'
      end
    end
  end
end
