require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Kwajalein
      include TimezoneDefinition
      
      linked_timezone 'Kwajalein', 'Pacific/Kwajalein'
    end
  end
end
