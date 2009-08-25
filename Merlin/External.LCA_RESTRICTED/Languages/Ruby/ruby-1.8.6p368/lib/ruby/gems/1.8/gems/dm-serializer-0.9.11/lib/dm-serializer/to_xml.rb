require 'dm-serializer/common'
require 'dm-serializer/xml_serializers'
require 'rexml/document'

module DataMapper
  module Serialize
    # Serialize a Resource to XML
    #
    # @return <REXML::Document> an XML representation of this Resource
    def to_xml(opts = {})
      to_xml_document(opts).to_s
    end

    protected
    # This method requires certain methods to be implemented in the individual
    # serializer library subclasses:
    # new_document
    # root_node
    # add_property_node
    # add_node
    def to_xml_document(opts={}, doc = nil)
      xml = XMLSerializers::SERIALIZER
      doc ||= xml.new_document
      default_xml_element_name = lambda { Extlib::Inflection.underscore(self.class.name).tr("/", "-") }
      root = xml.root_node(doc, opts[:element_name] || default_xml_element_name[])
      properties_to_serialize(opts).each do |property|
        value = send(property.name)
        attrs = (property.type == String) ? {} : {'type' => property.type.to_s.downcase}
        xml.add_node(root, property.name.to_s, value, attrs)
      end

      (opts[:methods] || []).each do |meth|
        if self.respond_to?(meth)
          xml_name = meth.to_s.gsub(/[^a-z0-9_]/, '')
          value = send(meth)
          xml.add_node(root, xml_name, value.to_s) unless value.nil?
        end
      end
      xml.output(doc)
    end
  end

  class Collection
    def to_xml(opts = {})
      to_xml_document(opts).to_s
    end

    protected

    def to_xml_document(opts = {})
      xml = DataMapper::Serialize::XMLSerializers::SERIALIZER
      doc = xml.new_document
      default_collection_element_name = lambda {Extlib::Inflection.pluralize(Extlib::Inflection.underscore(self.model.to_s)).tr("/", "-")}
      root = xml.root_node(doc, opts[:collection_element_name] || default_collection_element_name[], {'type' => 'array'})
      self.each do |item|
        item.send(:to_xml_document, opts, doc)
      end
      doc
    end
  end
end
