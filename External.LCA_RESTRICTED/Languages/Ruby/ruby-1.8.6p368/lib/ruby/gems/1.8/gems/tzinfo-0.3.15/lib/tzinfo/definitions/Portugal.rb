require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Portugal
      include TimezoneDefinition
      
      linked_timezone 'Portugal', 'Europe/Lisbon'
    end
  end
end
