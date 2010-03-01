require 'tzinfo/timezone_definition'

module TZInfo
  module Definitions
    module Pacific
      module Apia
        include TimezoneDefinition
        
        timezone 'Pacific/Apia' do |tz|
          tz.offset :o0, 45184, 0, :LMT
          tz.offset :o1, -41216, 0, :LMT
          tz.offset :o2, -41400, 0, :SAMT
          tz.offset :o3, -39600, 0, :WST
          tz.offset :o4, -39600, 3600, :WSDT
          
          tz.transition 1879, 7, :o1, 3250172219, 1350
          tz.transition 1911, 1, :o2, 3265701269, 1350
          tz.transition 1950, 1, :o3, 116797583, 48
          tz.transition 2009, 10, :o4, 1254654000
          tz.transition 2010, 3, :o3, 1269770400
        end
      end
    end
  end
end
