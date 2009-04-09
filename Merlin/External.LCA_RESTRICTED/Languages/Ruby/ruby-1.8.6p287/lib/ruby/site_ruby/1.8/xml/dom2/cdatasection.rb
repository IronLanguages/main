## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/text'

module XML
  module DOM

=begin
== Class XML::DOM::CDATASection

=== superclass
Text

=end
    class CDATASection<Text
=begin
=== Class Methods

    --- CDATASection.new(text = nil)

creates a new CDATASection.
=end
      def initialize(text = nil)
        super(text)
        raise "parameter error" if !text
      end

=begin
=== Methods

    --- CDATASection#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        CDATA_SECTION_NODE
      end

=begin
    --- CDATASection#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#cdata-section"
      end

=begin
    --- CDATASection#to_s

returns the string representation of the CDATASection.
=end
      def to_s
        "<![CDATA[#{@value}]]>"
      end

=begin
    --- CDATASection#dump(depth = 0)

dumps the CDATASection.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "<![CDATA[#{@value.inspect}]]>\n"
      end

    end
  end
end
