## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/domexception'

module XML
  module DOM

=begin
== Class XML::DOM::EntityReference

=== superclass
Node
=end
    class EntityReference<Node

=begin
=== Class Methods

    --- EntityReference.new(name, *children)

creates a new EntityReference.
=end
      def initialize(name, *children)
        super(*children)
        raise "parameter error" if !name
        @name = name.freeze
        @value = nil
      end

=begin
=== Methods

    --- EntityReference#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        ENTITY_REFERENCE_NODE
      end

=begin
    --- EntityReference#nodeName

[DOM]
returns  the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- EntityReference#to_s

returns the string representation of the EntityReference.
=end
      ## reference form or expanded form?
      def to_s
        "&#{@name};"
      end

=begin
    --- EntityReference#dump(depth = 0)

dumps the EntityReference.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "&#{@name}{\n"
        @children.each do |child|
          child.dump(depth + 1)
        end if @children
        print ' ' * depth * 2
        print "}\n"
      end

=begin
    --- EntityReference#cloneNode(deep = true)

[DOM]
returns the copy of the EntityReference.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name)
      end

      def _checkNode(node)
        unless node.nodeType == ELEMENT_NODE ||
            node.nodeType == PROCESSING_INSTRUCTION_NODE ||
            node.nodeType == COMMENT_NODE ||
            node.nodeType == TEXT_NODE ||
            node.nodeType == CDATA_SECTION_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end

    end
  end
end
