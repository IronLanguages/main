require 'dm-serializer/xml_serializers/rexml'
require 'dm-serializer/xml_serializers/nokogiri'
require 'dm-serializer/xml_serializers/libxml'

module DataMapper
  module Serialize
    module XMLSerializers
      SERIALIZER = if defined?(::LibXML)
                     LibXML
                   elsif defined?(::Nokogiri)
                     Nokogiri
                   else
                     REXML
                   end
    end
  end
end
