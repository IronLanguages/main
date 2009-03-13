## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/domexception'

module XML
  module DOM

=begin
== Class XML::DOM::DocumentFragment

=== superclass
Node

=end
    class DocumentFragment<Node

=begin
=== Class Methods

    --- DocumentFragment.new(*children)

creates a new DocumentFragment.
=end

      def initialize(*children)
        super(*children)
      end

=begin
=== Methods

    --- DocumentFragment#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        DOCUMENT_FRAGMENT_NODE
      end

=begin
    --- DocumentFragment#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#document-fragment"
      end

=begin
    --- DocumentFragment#parentNode=(p)

returns the parent of this node.
=end
      ## DocumentFragment should not have the parent node.
#      def parentNode=(p)
#        @children.each do |child|
#          child.parentNode = p
#        end if @children
#      end

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
