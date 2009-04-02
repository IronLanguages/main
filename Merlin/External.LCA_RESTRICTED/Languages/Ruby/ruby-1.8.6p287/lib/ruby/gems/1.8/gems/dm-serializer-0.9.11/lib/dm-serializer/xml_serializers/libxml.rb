module DataMapper
  module Serialize
    module XMLSerializers
      module LibXML
        def self.new_document
          ::LibXML::XML::Document.new
        end

        def self.root_node(doc, name, attrs = {})
          root = ::LibXML::XML::Node.new(name)
          attrs.each do |attr_name, attr_val|
            root[attr_name] = attr_val
          end
          doc.root.nil? ? doc.root = root : doc.root << root
          root
        end

        def self.add_node(parent, name, value, attrs = {})
          value_str = value.to_s unless value.nil?
          node = ::LibXML::XML::Node.new(name, value_str)
          attrs.each do |attr_name, attr_val|
            node[attr_name] = attr_val
          end
          parent << node
        end

        def self.output(doc)
          doc.root.to_s
        end
      end
    end
  end
end
