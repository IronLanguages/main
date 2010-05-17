require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module America
      module Atka
        include TimezoneDefinition
        
        linked_timezone 'America/Atka', 'America/Adak'
      end
    end
  end
end
