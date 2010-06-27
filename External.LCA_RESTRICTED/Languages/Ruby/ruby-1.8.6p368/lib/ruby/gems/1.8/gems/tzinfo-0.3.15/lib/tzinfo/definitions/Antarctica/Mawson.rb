require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Antarctica
      module Mawson
        include TimezoneDefinition
        
        timezone 'Antarctica/Mawson' do |tz|
          tz.offset :o0, 0, 0, :zzz
          tz.offset :o1, 21600, 0, :MAWT
          
          tz.transition 1954, 2, :o1, 4869573, 2
        end
      end
    end
  end
end
