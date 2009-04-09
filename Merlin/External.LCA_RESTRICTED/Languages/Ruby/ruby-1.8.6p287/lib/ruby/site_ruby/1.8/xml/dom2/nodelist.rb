## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

module XML
  module DOM
=begin
== Class XML::DOM::NodeList


=end
    class NodeList

=begin
=== Class Methods

    --- NodeList.new(nodes = nil)

creates a new NodeList.
=end
      def initialize(nodes = nil)
        if nodes.nil?
          @nodes = []
        elsif nodes.is_a?(Array)
          @nodes = nodes
        else
          raise "parameter error"
        end
      end

=begin
=== Methods

    --- NodeList#item(index)

[DOM]
return the indexth item in the NodeList.
=end
      ## [DOM]
      def item(index)
        @nodes[index]
      end

=begin
    --- NodeList#size()

return size of NodeList.
=end
      def size
        @nodes.length
      end
      alias length size

=begin
    --- NodeList#[](index)

return indexth node of the NodeList.
=end
      def [](index)
        @nodes[index]
      end

=begin
    --- NodeList#[]=(*p)

set node of indexth node of the NodeList.
=end
      def []=(*p)
        if p.length == 2
          @nodes[p[0]] = p[1]
        elsif p.length == 3
          @nodes[p[0], p[1]] = p[2]
        end
      end

=begin
    --- NodeList#each

iterates over each node of the NodeList.
=end
      def each
        @nodes.each do |value|
          yield(value)
        end
      end

=begin
    --- NodeList#reversible_each(reverse = false)

iterates over each node of the reversed NodeList.
=end
      ## [Masaki Fukushima]
      def reversible_each(reverse = false)
        if !reverse
          @nodes.each do |value|
            yield(value)
          end
        else
          @nodes.reverse_each do |value|
            yield(value)
          end
        end
      end

=begin
    --- NodeList#push(*nodes)

adds nodes into the NodeList.
=end
      def push(*nodes)
        nodes.each do |node|
          if node.is_a?(Array)
            self.push(*node)
          elsif node.is_a?(NodeList)
            @nodes.concat(node.to_a)
          elsif node.is_a?(Node)
            @nodes << node
          else
            raise "parameter error"
          end
        end
        self
      end

=begin
    --- NodeList#concat(*nodes)

alias of NodeList#push.
=end
      alias concat push

=begin
    --- NodeList#pop

pops and returns the last node of the NodeList.
=end
      def pop
        @nodes.pop
      end

=begin
    --- NodeList#shift

removes and returns the first node of the NodeList.
=end
      def shift
        @nodes.shift
      end

=begin
    --- NodeList#to_s

returns the string representation of the NodeList.
=end
      def to_s
        @nodes.to_s
      end

=begin
    --- NodeList#reverse

returns the reversed NodeList.
=end
      def reverse
        @nodes.reverse
      end

=begin
    --- NodeList#to_a

converts the NodeList into an array.
=end
      def to_a
        @nodes
      end

=begin
    --- NodeList#+(nodes)

return the newly created concatenated NodeList.
=end
      def +(nodes)
        if nodes.nil?
          NodeList.new(@nodes)
        elsif nodes.is_a?(Array)
          NodeList.new(@nodes + nodes)
        elsif nodes.is_a?(NodeList)
          NodeList.new(@nodes + nodes.to_a)
        elsif nodes.is_a?(Node)
          NodeList.new(@nodes + [nodes])
        else
          raise "parameter error"
        end
      end

=begin
    --- NodeList#<<(nodes)

appends nodes to the NodeList.
=end
      ## modified by Masaki Fukushima
      def <<(nodes)
        if nodes.nil?
          ## no change
        elsif nodes.is_a?(Array)
          @nodes.concat(nodes)
        elsif nodes.is_a?(NodeList)
          @nodes.concat(nodes.to_a)
        elsif nodes.is_a?(Node)
          @nodes << nodes
        else
          raise "parameter error"
        end
        self
      end

      ## get nodeValues by names
      ##   names ::= name ('|' name)*
      def _getValues(names)
        ret = []
        names.split('|').each do |name|
          if !@nodes[name].nil?
            ret.push(@nodes[name].nodeValue)
          end
        end
        ret
      end
    end
  end
end
