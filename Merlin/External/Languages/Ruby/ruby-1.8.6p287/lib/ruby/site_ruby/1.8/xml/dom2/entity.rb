## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/domexception'

module XML
  module DOM

=begin
== Class XML::DOM::Entity

=== superclass
Node
=end
    class Entity<Node

=begin
=== Class Methods

    --- Entity.new(name, pubid, sysid, notation)

creates a new Entity.
=end
      def initialize(name, pubid, sysid, notation)
        super()
        @name = name.freeze
        @pubid = pubid.freeze
        @sysid = sysid.freeze
        @notation = notation.freeze
      end

=begin
=== Methods

    --- Entity#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        ENTITY_NODE
      end

=begin
    --- Entity#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- Entity#publicId

returns the publicId of the Entity.
=end
      def publicId
        @pubid
      end

=begin
    --- Entity#systemId

returns the systemId of the Entity.
=end
      def systemId
        @sysid
      end

=begin
    --- Entity#notationName

returns the notationname of the Entity.
=end
      def notationName
        @notation
      end

=begin
    --- Entity#cloneNode(deep = true)

[DOM]
returns the copy of the Entity.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name, @pubid, @sysid, @notation)
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
