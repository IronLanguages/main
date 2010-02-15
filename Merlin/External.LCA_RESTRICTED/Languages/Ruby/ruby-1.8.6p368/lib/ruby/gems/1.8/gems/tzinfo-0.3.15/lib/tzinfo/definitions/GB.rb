require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module GB
      include TimezoneDefinition
      
      linked_timezone 'GB', 'Europe/London'
    end
  end
end
