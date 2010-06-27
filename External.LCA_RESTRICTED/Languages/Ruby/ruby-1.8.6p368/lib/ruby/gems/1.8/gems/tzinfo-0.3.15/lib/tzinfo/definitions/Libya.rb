require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Libya
      include TimezoneDefinition
      
      linked_timezone 'Libya', 'Africa/Tripoli'
    end
  end
end
