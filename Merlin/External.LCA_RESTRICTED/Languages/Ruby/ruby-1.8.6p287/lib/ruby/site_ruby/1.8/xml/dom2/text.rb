## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/characterdata'
require 'xml/dom2/domexception'

module XML
  module DOM

=begin
== Class XML::DOM::Text

=== superclass
Node

=end
    class Text<CharacterData

=begin
=== Class Methods

    --- Text.new(text)

creates a new Text.
=end
      ## new(text)
      ##     text: String
      def initialize(text = nil)
        super(text)
      end

=begin
=== Methods

    --- Text#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        TEXT_NODE
      end

=begin
    --- Text#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#text"
      end

=begin
    --- Text#to_s

return the string representation of the Text.
=end
      def to_s
        ret = ""
        @value.scan(/./um) do |c|
          code = c.unpack("U")[0]
          if code == 13
            ret << sprintf("&#x%X;", code)
          elsif c == "&"
            ret << "&amp;"
          elsif c == "<"
            ret << "&lt;"
          elsif c == ">"
            ret << "&gt;"
          else
            ret << c
          end
        end
        ret
##        XML.charRef(@value)
      end

=begin
    --- Text#dump(depth = 0)

dumps the Text.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "#{@value.inspect}\n"
      end

=begin
    --- Text#splitText(offset)

[DOM]
breaks this Text node into two Text nodes at the specified offset.
=end
      ## [DOM]
      def splitText(offset)
        if offset > @value.length || offset < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        newText = @value[offset, @value.length]
        newNode = Text.new(newText)
        if !self.parentNode.nil?
          self.parentNode.insertAfter(newNode, self)
        end
        @value[offset, @value.length] = ""
        newNode
      end

=begin
    --- Text#trim(preserve = false)

trim extra whitespaces.
=end
      def trim(preserve = false)
        if !preserve
          @value.sub!(/\A\s*([\s\S]*?)\s*\Z/, "\\1")
          return @value
        end
        nil
      end

    end
  end
end
