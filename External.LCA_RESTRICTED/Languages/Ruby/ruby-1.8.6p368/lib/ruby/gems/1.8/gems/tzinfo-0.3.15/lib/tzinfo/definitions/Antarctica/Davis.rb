require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Antarctica
      module Davis
        include TimezoneDefinition
        
        timezone 'Antarctica/Davis' do |tz|
          tz.offset :o0, 0, 0, :zzz
          tz.offset :o1, 25200, 0, :DAVT
          
          tz.transition 1957, 1, :o1, 4871703, 2
          tz.transition 1964, 10, :o0, 58528805, 24
          tz.transition 1969, 2, :o1, 4880507, 2
        end
      end
    end
  end
end
