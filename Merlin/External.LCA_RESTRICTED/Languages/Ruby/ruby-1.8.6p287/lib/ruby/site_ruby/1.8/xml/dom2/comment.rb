## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/characterdata'

module XML
  module DOM

=begin
== Class XML::DOM::Comment

=== superclass
CharacterData

=end
    class Comment<CharacterData

=begin
=== Class Methods

    --- Comment.new(text)

creates a new Comment.
=end
      ## new(text)
      ##     text: String
      def initialize(text = nil)
        super(text)
        raise "parameter error" if !text
      end

=begin
=== Methods

    --- Comment#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        COMMENT_NODE
      end

=begin
    --- Comment#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#comment"
      end

=begin
    --- Comment#to_s

returns the string  representation of the Comment.
=end
      def to_s
        ret = "<!--#{@value}-->"
        ret << "\n" if parentNode.nodeType == DOCUMENT_NODE
        ret
      end

=begin
    --- Comment#dump(depth  =0)

dumps the Comment.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "<!--#{@value.inspect}-->\n"
      end

    end
  end
end
