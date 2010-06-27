require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module UCT
      include TimezoneDefinition
      
      linked_timezone 'UCT', 'Etc/UCT'
    end
  end
end
