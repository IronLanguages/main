require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Cuba
      include TimezoneDefinition
      
      linked_timezone 'Cuba', 'America/Havana'
    end
  end
end
