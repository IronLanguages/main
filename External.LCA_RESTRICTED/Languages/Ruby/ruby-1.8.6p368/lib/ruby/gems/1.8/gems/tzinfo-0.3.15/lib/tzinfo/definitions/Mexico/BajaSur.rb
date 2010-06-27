require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Mexico
      module BajaSur
        include TimezoneDefinition
        
        linked_timezone 'Mexico/BajaSur', 'America/Mazatlan'
      end
    end
  end
end
