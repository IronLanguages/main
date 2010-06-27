require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Japan
      include TimezoneDefinition
      
      linked_timezone 'Japan', 'Asia/Tokyo'
    end
  end
end
