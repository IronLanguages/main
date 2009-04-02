## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/domexception'

module XML
  module DOM

=begin
== Class XML::DOM::Attr

=== superclass
Node

=end
    class Attr<Node
      ## new(name, [text1, text2, ...]) or
      ## new(name, text1, text2, ...)
      ##     name:  String
      ##     text?: String or Node

=begin
=== Class Methods

    --- Attr.new(name = nil, *text)

create a new Attr.
=end
      def initialize(name, *text)
        super(text)
        raise "parameter error" if !name
        @name = nil
        @prefix = nil
        @localname = nil
        @uri = nil
        @ownerElement = nil
        if name.is_a?(Array)
          ## namespaces
          raise "parameter error" if name.length != 2
          @localname = name[1]
          if name[1].index(':')
            @prefix, @localname = name[1].split(':')
          end
          @name = name[1] ## qualified name
          @uri =  name[0] ## namespace URI
        else
          @name = name
        end
        @name.freeze
        @prefix.freeze
        @localname.freeze
        @uri.freeze
      end

=begin
=== Methods

    --- Attr#nodeType()

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        ATTRIBUTE_NODE
      end

=begin
    --- Attr#nodeName()

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- Attr#nodeValue()

[DOM]
returns the nodeValue.
=end
      ## [DOM]
      def nodeValue
        ret = ""
        @children.each do |child|
          ret << child.nodeValue
        end if @children
        ret
      end

=begin
    --- Attr#nodeValue=(text)

[DOM]
returns the value of this node.
=end
      ## [DOM]
      def nodeValue=(text)
        self.childNodes = [text]
      end

=begin
    --- Attr#to_s()

return the string representation of the Attr.
=end
      def to_s
        value = ""
        nodeValue.scan(/./um) do |c|
          code = c.unpack("U")[0]
          if code == 9 || code == 10 || code == 13
            value << sprintf("&#x%X;", code)
          elsif c == "&"
            value << "&amp;"
          elsif c == "\""
            value << "&quot;"
          elsif c == "<"
            value << "&lt;"
          else
            value << c
          end
        end
        "#{@name}=\"#{value}\""
      end

=begin
    --- Attr#dump(depth = 0)

dump the Attr.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "// #{self.to_s}\n"
      end

=begin
    --- Attr#cloneNode(deep = true)

[DOM]
returns the copy of the Attr.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name)
      end

=begin
    --- Attr#name()

[DOM]
alias of nodeName.
=end
      ## [DOM]
      alias name nodeName

=begin
    --- Attr#value()

alias of nodeValue.

    --- Attr#value=(value)

[DOM]
alias of nodeValue=.
=end
      ## [DOM]
      alias value nodeValue
      alias value= nodeValue= ##

      ## [DOM]
      def specified; @specified; end
      def specified=(is_specified); @specified = is_specified; end

      ## [DOM2]
      def namespaceURI; @uri; end

      ## [DOM2]
      def prefix; @prefix; end

      ## [DOM2]
      def prefix=(prefix);
        ## to be checked

        @ownerElement.removeAttributeNode(self) if @ownerElement
        @prefix = prefix
        @name = @prefix + ':' + @localname
        @ownerElement.setAttributeNode(self) if @ownerElement
        @prefix.freeze
        @name.freeze
      end

      ## [DOM2]
      def localname; @localname; end

      ## [DOM2]
      def ownerElement; @ownerElement; end
      def ownerElement=(elem); @ownerElement = elem; end

      def _checkNode(node)
        unless node.nodeType == TEXT_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end
    end
  end
end
