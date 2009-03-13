## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'

module XML
  module DOM

=begin
== Class XML::DOM::ProcessingInstruction

=== superclass
Node

=end
    class ProcessingInstruction<Node

=begin
=== Class Methods

    --- ProcessingInstruction.new(target = nil, data = nil)

creates a new ProcessingInstruction.
=end
      ## new(target, data)
      ##     target: String
      ##     data: String
      def initialize(target = nil, data = nil)
        super()
        raise "parameter error" if !data
        @target = target.freeze
        @data = data.freeze
        @value = target.dup
        @value << " #{data}" if data != ""
        @value.freeze
      end

=begin
=== Methods

    --- ProcessingInstruction#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        PROCESSING_INSTRUCTION_NODE
      end

=begin
    --- ProcessingInstruction#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#proccessing-instruction"
      end

=begin
    --- ProcessingInstruction#target

[DOM]
returns the target of the ProcessingInstruction.
=end
      ## [DOM]
      def target
        @target
      end

      def target=(p)
        @target = p.freeze
        @value = @target.dup
        @value << " #{@data}" if @data != ""
        @value.freeze
      end

=begin
    --- ProcessingInstruction#data

[DOM]
return the content of the ProcessingInstruction.
=end
      ## [DOM]
      def data
        @data
      end

=begin
    --- ProcessingInstruction#data=(p)

[DOM]
sets p to the content of the ProcessingInstruction.
=end
      ## [DOM]
      def data=(p)
        @data = p.freeze
        @value = @target.dup
        @value << " #{@data}" if @data != ""
        @value.freeze
      end

=begin
    --- ProcessingInstruction#nodeValue

[DOM]
return nodevalue.

=end
      ## [DOM]
      def nodeValue
        @value
      end

      ## inhibit changing value without target= or data=
      undef nodeValue=

=begin
    --- ProcessingInstruction#to_s

returns the string representation of the ProcessingInstruction.
=end
      def to_s
        ret = "<?#{@value}?>"
        ret << "\n" if parentNode.nodeType == DOCUMENT_NODE
        ret
      end

=begin
    --- ProcessingInstruction#dump(depth = 0)

dumps the ProcessingInstruction.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "<?#{@value.inspect}?>\n"
      end

=begin
    --- ProcessingInstruction#cloneNode(deep = true)

[DOM]
returns the copy of the ProcessingInstruction.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @target, @data)
      end
    end
  end
end
