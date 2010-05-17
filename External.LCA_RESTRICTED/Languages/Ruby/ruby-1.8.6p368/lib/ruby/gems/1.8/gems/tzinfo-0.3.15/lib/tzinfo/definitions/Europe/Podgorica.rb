require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Europe
      module Podgorica
        include TimezoneDefinition
        
        linked_timezone 'Europe/Podgorica', 'Europe/Belgrade'
      end
    end
  end
end
