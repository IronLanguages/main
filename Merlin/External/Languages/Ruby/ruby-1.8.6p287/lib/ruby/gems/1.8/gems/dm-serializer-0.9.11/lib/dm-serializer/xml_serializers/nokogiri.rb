module DataMapper
  module Serialize
    module XMLSerializers
      module Nokogiri
        def self.new_document
          ::Nokogiri::XML::Document.new
        end

        def self.root_node(doc, name, attrs = {})
          root = ::Nokogiri::XML::Node.new(name, doc)
          attrs.each do |attr_name, attr_val|
            root[attr_name] = attr_val
          end
          doc.root.nil? ? doc.root = root : doc.root << root
          root
        end

        def self.add_node(parent, name, value, attrs = {})
          node = ::Nokogiri::XML::Node.new(name, parent.document)
          node << ::Nokogiri::XML::Text.new(value.to_s, parent.document) unless value.nil?
          attrs.each {|attr_name, attr_val| node[attr_name] = attr_val }
          parent << node
          node
        end

        def self.output(doc)
          doc.root.to_s
        end
      end
    end
  end
end
