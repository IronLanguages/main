require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Singapore
      include TimezoneDefinition
      
      linked_timezone 'Singapore', 'Asia/Singapore'
    end
  end
end
