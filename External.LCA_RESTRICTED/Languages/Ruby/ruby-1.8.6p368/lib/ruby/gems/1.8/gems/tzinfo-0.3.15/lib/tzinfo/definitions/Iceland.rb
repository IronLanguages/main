require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Iceland
      include TimezoneDefinition
      
      linked_timezone 'Iceland', 'Atlantic/Reykjavik'
    end
  end
end
