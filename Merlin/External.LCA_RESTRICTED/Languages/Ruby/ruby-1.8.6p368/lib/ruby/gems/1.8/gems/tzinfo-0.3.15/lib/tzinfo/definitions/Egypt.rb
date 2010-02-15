require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Egypt
      include TimezoneDefinition
      
      linked_timezone 'Egypt', 'Africa/Cairo'
    end
  end
end
