## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

module XML
  module DOM
=begin
== Class XML::DOM::NamedNodeMap

=end

    class NamedNodeMap

=begin
=== Class Methods

    --- NamedNodeMap.new(nodes = nil)

creates a new NamedNodeMap.
=end
      def initialize(nodes = nil)
        @nodes = {}
        nodes.each do |node|
          @nodes[node.nodeName] = node
        end if nodes
      end

=begin
=== Methods

    --- NamedNodeMap#getNamedItem(name)

[DOM]
retrieves a node specified by name.
=end
      ## [DOM]
      def getNamedItem(name)
        @nodes[name]
      end

=begin
    --- NamedNodeMap#setNamedItem(node)

[DOM]
adds a node using its nodeName attribute.
=end
      ## [DOM]
      def setNamedItem(node)
        @nodes[node.nodeName] = node
      end

=begin
    --- NamedNodeMap#removeNamedItem(name)

[DOM]
removes a node specified by name.
=end
      ## [DOM]
      def removeNamedItem(name)
        ret = @nodes[name]
        @nodes.delete(name)
        ret
      end

=begin
    --- NamedNodeMap#item(index)

[DOM]
returns the index item in the map.
=end
      ## [DOM]
      def item(index)
        v = @nodes.to_a[index]
        return v[1] if v
        nil
      end

=begin
    --- NamedNodeMap#[](name)

returns nodes associated to name.
=end
      def [](name)
        @nodes[name]
      end

=begin
    --- NamedNodeMap#[]=(name, node)

sets node named name.
=end
      def []=(name, node)
        raise "parameter error" if node.nodeName != name
        @nodes[name] = node
      end

=begin
    --- NamedNodeMap#each()

iterates over each pair of name and node(name, node) of the namedNodeMap.
=end
      def each
        @nodes.each do |key, value|
          yield(value)
        end
      end

=begin
    --- NamedNodeMap#size()

[DOM]
returns the number of nodes in the map.
=end
      ## [DOM]
      def size
        @nodes.length
      end
      alias length size

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

      ## [DOM2]
      ## def getNamedItemNS(nsuri, localname); end
      ## def removeNamedItemNS(nsuri, localname); end
    end
  end
end
