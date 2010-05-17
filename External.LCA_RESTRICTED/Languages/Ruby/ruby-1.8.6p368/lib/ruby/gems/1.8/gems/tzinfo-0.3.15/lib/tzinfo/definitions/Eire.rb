require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Eire
      include TimezoneDefinition
      
      linked_timezone 'Eire', 'Europe/Dublin'
    end
  end
end
