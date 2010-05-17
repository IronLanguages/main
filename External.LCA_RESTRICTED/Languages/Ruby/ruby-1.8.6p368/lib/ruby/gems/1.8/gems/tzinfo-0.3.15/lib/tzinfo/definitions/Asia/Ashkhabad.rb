require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Asia
      module Ashkhabad
        include TimezoneDefinition
        
        linked_timezone 'Asia/Ashkhabad', 'Asia/Ashgabat'
      end
    end
  end
end
