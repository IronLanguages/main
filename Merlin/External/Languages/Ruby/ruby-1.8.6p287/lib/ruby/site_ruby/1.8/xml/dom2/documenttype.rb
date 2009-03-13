## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'

module XML
  module DOM

=begin
== Class XML::DOM::DocumentType

=== superclass
Node
=end
    class DocumentType<Node

=begin
=== Class Methods

    --- DocumentType.new(name, pubid, sysid, *children)

creates a new DocuemntType.
=end
      def initialize(name, pubid, sysid, *children)
        super(*children)
        raise "parameter error" if !name
        @name = name.freeze
        @pubid = pubid.freeze
        @sysid = sysid.freeze
      end

=begin
=== Methods

    --- DocumentType#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        DOCUMENT_TYPE_NODE
      end

=begin
    --- DocumentType#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- DocumentType#to_s

returns the string representation of the DocumentType.
=end
      def to_s
        ""
      end

=begin
    --- DocumentType#dump(depth = 0)

dumps the DocumentType.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "<!DOCTYPE #{@name}>\n"
      end

=begin
    --- DocumentType#cloneNode(deep = true)

[DOM]
returns the copy of the DocumentType.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name, @pubid, @sysid)
      end

      ## [DOM]
      ## def entities; @entities; end
      ## def notations; @notations; end

      ## [DOM2]
      def publicId; @pubid; end

      ## [DOM2]
      def systemId; @sysid; end

      ## [DOM2]
      def internalSubset; end
    end
  end
end
