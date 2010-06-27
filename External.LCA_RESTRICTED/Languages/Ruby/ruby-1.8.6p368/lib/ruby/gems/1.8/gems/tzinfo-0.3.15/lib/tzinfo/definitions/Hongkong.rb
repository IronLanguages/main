require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Hongkong
      include TimezoneDefinition
      
      linked_timezone 'Hongkong', 'Asia/Hong_Kong'
    end
  end
end
