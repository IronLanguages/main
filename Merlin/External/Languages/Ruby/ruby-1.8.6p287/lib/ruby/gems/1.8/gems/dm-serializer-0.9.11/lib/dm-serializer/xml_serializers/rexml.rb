module DataMapper
  module Serialize
    module XMLSerializers
      module REXML
        def self.new_document
          ::REXML::Document.new
        end

        def self.root_node(document, name, attrs = {})
          add_node(document.root || document, name, nil, attrs)
        end

        def self.add_node(parent, name, value, attrs = {})
          node = parent.add_element(name)
          attrs.each {|attr_name, attr_val| node.attributes[attr_name] = attr_val}
          node << ::REXML::Text.new(value.to_s) unless value.nil?
          node
        end

        def self.output(doc)
          doc.to_s
        end
      end
    end
  end
end
