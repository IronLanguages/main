## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'

module XML
  module DOM

=begin
== Class XML::DOM::Notation

=== superclass
Node
=end
    class Notation<Node
=begin
=== Class Methods

    --- Notation.new(name, pubid, sysid)

creates a new Notation.
=end
      def initialize(name, pubid, sysid)
        super()
        @name = name.freeze
        @pubid = pubid.freeze
        @sysid = sysid.freeze
      end

=begin
=== Methods

    --- Notation#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        NOTATION_NODE
      end

=begin
    --- Notation#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- Notation#publicId

returns the publicId of the Notation.
=end
      def publicId
        @pubid
      end

=begin
    --- Notation#systemId

returns the systemId of the Notation.
=end
      def systemId
        @sysid
      end

=begin
    --- Notation#cloneNode(deep = true)

[DOM]
returns the copy of the Notation.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name, @pubid, @sysid)
      end
    end
  end
end
