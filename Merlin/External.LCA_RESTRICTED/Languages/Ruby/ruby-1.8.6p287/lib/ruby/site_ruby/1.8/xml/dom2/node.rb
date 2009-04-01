## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/domexception'
require 'xml/dom2/nodelist'

module XML
  module DOM

=begin
== Class XML::DOM::Node

=end
    class Node
      ## [DOM]
      NODE_NODE = 0
      ELEMENT_NODE = 1
      ATTRIBUTE_NODE = 2
      TEXT_NODE = 3
      CDATA_SECTION_NODE = 4
      ENTITY_REFERENCE_NODE = 5
      ENTITY_NODE = 6
      PROCESSING_INSTRUCTION_NODE = 7
      COMMENT_NODE  = 8
      DOCUMENT_NODE = 9
      DOCUMENT_TYPE_NODE = 10
      DOCUMENT_FRAGMENT_NODE = 11
      NOTATION_NODE = 12

      ## non-DOM
#      NODE = 0
#      ELEMENT = 1
#      ATTRIBUTE = 2
#      TEXT = 3
#      CDATA_SECTION = 4
#      ENTITY_REFERENCE = 5
#      ENTITY = 6
#      PI = 7
#      PROCESSING_INSTRUCTION = 7
#      COMMENT  = 8
#      DOCUMENT = 9
#      DOCUMENT_TYPE = 10
#      DOCUMENT_FRAGMENT = 11
#      NOTATION = 12

=begin
=== Class Methods

    --- Node.new(*children)

make a Node.
children is a Array of child, or sequence of child.
child is a String or Node.

=end
      ## new([child1, child2, ...]) or
      ## new(child1, child2, ...)
      ##     child?: String or Node
      def initialize(*children)
        @ownerDocument = nil
        @parent = nil
        @children = nil
        self.childNodes = children if children.length > 0
      end

=begin
=== Methods

    --- Node#parentNode

[DOM]
return parent node.

=end
      ## [DOM]
      def parentNode
        @parent
      end

=begin
    --- Node#parentNode=(p)

set node p as parent.
=end

      def parentNode=(p)
        @parent = p
      end

=begin
    --- Node#nodeType

[DOM]
return nodetype.

=end
      ## [DOM]
      def nodeType
        NODE_NODE
      end

=begin
    --- Node#nodeName

[DOM]
return nodename.

=end
      ## [DOM]
      def nodeName
        "#node"
      end

#      def nodeName=(p)
#        @name = p
#      end

=begin
    --- Node#nodeValue

[DOM]
return nodevalue.

=end
      ## [DOM]
      def nodeValue; nil; end

=begin
    --- Node#nodeValue=(p)

[DOM]
set nodevalue as p.
=end
      ## [DOM]
      def nodeValue=(p)
        ## no effect
      end

=begin
    --- Node#childNodes()

[DOM]
if method has block, apply block for children nodes.
without block, return children nodelist.
=end
      ## [DOM]
      def childNodes
        if iterator?
          @children.each do |child|
            yield(child)
          end if @children
        else
          return @children if !@children.nil?
          @children = NodeList.new
        end
      end

      def childNodes=(p)
        if p.nil? || (p.is_a?(Array) && p.length == 0)
          return
        end
        if @children.nil?
          @children = NodeList.new
        else
          @children.to_a.clear
        end
        p.flatten!
        p.each do |child|
          if child.is_a?(String)
            c = Text.new(child)
            @children.push(c)
            c.parentNode = self
          elsif child.is_a?(Node)
            @children.push(child)
            child.parentNode = self
          else
            raise "parameter error"
          end
        end if p
      end

=begin
    --- Node#attributes

[DOM]
return attributes of node(but always return nil?).
=end
      ## [DOM]
      def attributes
        nil
      end

      ## proper parameter type?
#      def attributes=(p)
#      end

=begin
    --- Node#[]=(index, nodes)

set children node as nodes with []-style.
=end
      def []=(index, nodes)
        @children[index..index] = nodes
        @children.each do |child|
          child.parentNode = self
        end if @children
      end

=begin
    --- Node#[](index)

get children node with []-style.
=end
      def [](index)
        @children[index]
      end

=begin
    --- Node#+(node)

concat node to Node.
=end
      def +(node)
        [self, node]
      end

=begin
    --- Node#to_s

returns the string representation of the Node.
=end
      def to_s
        @children.to_s
      end

=begin
    --- Node#dump(depth = 0)

dump the Node.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print nodeName + "\n"
        @children.each do |child|
          child.dump(depth + 1)
        end if @children
      end

=begin
    --- Node#inspect()

returns the human-readable string representation.
=end
      def inspect
        "#<#{self.class}: #{self.nodeName}>"
      end

=begin
    --- Node#firstChild()

[DOM]
return the first child node.
=end
      ## [DOM]
      def firstChild
        return nil if !@children || @children.length == 0
        return @children[0]
      end

=begin
    --- Node#lastChild()

[DOM]
return the last child node.
=end
      ## [DOM]
      def lastChild
        return nil if !@children || @children.length == 0
        return @children[-1]
      end

=begin
    --- Node#previousSibling()

[DOM]
return the previous sibling node.
=end
      ## [DOM]
      def previousSibling
        return nil if !@parent
        prev = nil
        @parent.childNodes do |child|
          return prev if child == self
          prev = child
        end
        nil
      end

=begin
    --- Node#nextSibling()

[DOM]
return the next sibling node.
=end
      ## [DOM]
      def nextSibling
        return nil if !@parent
        nexts = nil
        @parent.childNodes.reverse.each do |child|
          return nexts if child == self
          nexts = child
        end
        nil
      end

      def _getChildIndex(node)
        index = 0
        @children.each do |child|
          if child == node
            return index
          end
          index += 1
        end
        nil
      end

      def _removeFromTree
        parent = parentNode
        if parent
          parent.removeChild(self)
        end
      end

      def _checkNode(node)
        raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
      end

      def _insertNodes(index, node)
        if node.nodeType == DOCUMENT_FRAGMENT_NODE

          node.childNodes.to_a.each_with_index do |n, i|
            if index == -1
              _insertNodes(-1, n)
            else
              _insertNodes(index + i, n)
            end
          end
        elsif node.is_a?(Node)
          ## to be checked
          _checkNode(node)
          node._removeFromTree
          if index == -1
            @children.push(node)
          else
            @children[index, 0] = node
          end
          node.parentNode = self
        else
          raise ArgumentError, "invalid value for Node"
        end
      end

      def _removeNode(index, node)
        @children[index, 1] = nil
        node.parentNode = nil
      end

# =begin
#     --- Node#insertAfter(newChild, refChild)
# 
# insert newChild into the node after refChild.
# =end
#       def insertAfter(newChild, refChild)
#         if @children.nil? || @children.length == 0
#           raise DOMException.new(DOMException::NOT_FOUND_ERR)
#         end
#         index = _getChildIndex(refChild)
#         raise DOMException.new(DOMException::NOT_FOUND_ERR) if index.nil?
#         _insertNodes(index + 1, newChild)
#       end

=begin
    --- Node#insertBefore(newChild, refChild)

[DOM]
insert newChild into the node before refChild.
=end
      ## [DOM]
      def insertBefore(newChild, refChild)
        if @children.nil? || @children.length == 0
          raise DOMException.new(DOMException::NOT_FOUND_ERR)
        end
        index = _getChildIndex(refChild)
        raise DOMException.new(DOMException::NOT_FOUND_ERR) if !index
        _insertNodes(index, newChild)
      end

=begin
    --- Node#replaceChild(newChild, oldChild)

[DOM]
replace the child node oldChild with newChild.
=end
      ## [DOM]
      def replaceChild(newChild, oldChild)
        if @children.nil? || @children.length == 0
          raise DOMException.new(DOMException::NOT_FOUND_ERR)
        end
        index = _getChildIndex(oldChild)
        raise DOMException.new(DOMException::NOT_FOUND_ERR) if !index
        _removeNode(index, oldChild)
        _insertNodes(index, newChild)
      end

=begin
    --- Node#removeChild(oldChild)

[DOM]
remove the children node oldChild.
=end
      ## [DOM]
      def removeChild(oldChild)
        if @children.nil? || @children.length == 0
          raise DOMException.new(DOMException::NOT_FOUND_ERR)
        end
        index = _getChildIndex(oldChild)
        raise DOMException.new(DOMException::NOT_FOUND_ERR) if !index
        _removeNode(index, oldChild)
        oldChild
      end

=begin
    --- Node#appendChild(newChild)

[DOM]
adds the node newChild to the end of the list of children of this node.
=end
      ## [DOM]
      def appendChild(newChild)
        @children = NodeList.new if !@children
        _insertNodes(-1, newChild)
      end

=begin
    --- Node#hasChildNodes()

[DOM]
returns true if node has children, or return false if node has no children.
=end
      ## [DOM]
      def hasChildNodes
        !@children.nil? && @children.length > 0
      end

=begin
    --- Node#ownerDocument()

[DOM]
Document object associated with this node.
=end
      ## [DOM]
      def ownerDocument; @ownerDocument; end

      def ownerDocument=(document); @ownerDocument = document; end

=begin
    --- Node#cloneNode()

[DOM]
return the copy of the Node.
=end
      ## [DOM]
      def cloneNode(deep = true, *args)
        ret = self.class.new(*args)
        if (deep)
          @children.each do |child|
            ret.appendChild(child.cloneNode(true))
          end
        end if @children
        ret
      end

=begin
    --- Node#trim(preserve = false)

trim extra whitespaces.
=end
      ## trim extra whitespaces
      def trim(preserve = false)
        return nil if @children.nil?
        children = @children.to_a.dup
        children.each do |child|
          if !preserve && (child.nodeType == TEXT_NODE ||
                           child.nodeType == CDATA_SECTION_NODE)
            if child.trim == ""
              self.removeChild(child)
            end
          else
            child.trim(preserve)
          end
        end
        nil
      end


      ## [DOM2]
      def isSupported(feature, version)
        if (feature =~ /^XML$/i || feature =~ /^Core$/i) &&
            (version.nil? || version == "1.0" || version == "2.0")
          return true
        end
        false
      end

      ## [DOM2]
      def namespaceURI; nil; end

      ## [DOM2]
      def prefix; nil; end

      ## [DOM2]
      def prefix=(prefix);
        ## no effect
      end

      ## [DOM2]
      def localname; nil; end

      ## [DOM2]
      def hasAttributes(); false; end


      include Enumerable
      def each
        sibstack = []
        siblings = [ self ]
        while true
          if siblings.length == 0
            break if sibstack.length == 0
            siblings = sibstack.pop
            next
          end
          node = siblings.shift
          yield(node)
          children = node.childNodes
          if !children.nil?
            sibstack.push(siblings)
            siblings = children.to_a.dup
          end
        end
      end

      include Comparable

      def ==(node)
        equal?(node)
      end

      def <=>(node)
        ancestors1 = [self]
        ancestors2 = [node]
        p = self
        while p = p.parentNode
          ancestors1.unshift(p)
        end
        p = node
        while p = p.parentNode
          ancestors2.unshift(p)
        end
        raise "different document" unless ancestors1[0].equal?(ancestors2[0])
        ret = 0
        i = 0
        for i in 1...ancestors1.size
          next if ancestors1[i].equal?(ancestors2[i])
          return 1 if ancestors2[i].nil?
          children = ancestors1[i - 1].childNodes.to_a
          return children.index(ancestors1[i]) - children.index(ancestors2[i])
        end
        return -1 if ancestors2.size > i + 1
        0
      end

    end
  end
end
