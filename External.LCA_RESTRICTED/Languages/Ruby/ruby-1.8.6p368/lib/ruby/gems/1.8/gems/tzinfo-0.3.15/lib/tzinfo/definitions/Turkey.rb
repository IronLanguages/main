require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Turkey
      include TimezoneDefinition
      
      linked_timezone 'Turkey', 'Europe/Istanbul'
    end
  end
end
