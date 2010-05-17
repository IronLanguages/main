require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module ROK
      include TimezoneDefinition
      
      linked_timezone 'ROK', 'Asia/Seoul'
    end
  end
end
