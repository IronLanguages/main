require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Asia
      module Saigon
        include TimezoneDefinition
        
        linked_timezone 'Asia/Saigon', 'Asia/Ho_Chi_Minh'
      end
    end
  end
end
