require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Poland
      include TimezoneDefinition
      
      linked_timezone 'Poland', 'Europe/Warsaw'
    end
  end
end
