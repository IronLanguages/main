## -*- Ruby -*-
## XML::SimpleTree
## 1998-2000 by yoshidam
##
## XPointer support is contributed by Masaki Fukushima 
##     <fukusima@goto.info.waseda.ac.jp>
##                     

require 'singleton'

=begin

= XML::DOM (XML::SimpleTree)

=end


=begin

== Module XML

=end

module XML

=begin
=== Class Methods

    --- XML.charRef(s)

replace character '&','<','>',"'",'"' in string s to character reference.
=end

  def XML.charRef(s)
    str = s.dup
    str.gsub!("&", "&amp;")
    str.gsub!("<", "&lt;")
    str.gsub!(">", "&gt;")
    str.gsub!("'", "&apos;")
    str.gsub!('"', "&quot;")
    str
  end

=begin

== Module XML::Spec

Constants related to XML Specification.

=end
  ## [Masaki Fukushima]
  module Spec
    ## Constants related to XML Specification
    ##   (W3C Recommendation or Working Draft)

    # XML
    Letter_s = '[a-zA-Z]'
    Digit_s = '\d'
    NameChar_s = "(#{Letter_s}|#{Digit_s}|[\\.\\-_:])"
    Name_s = "(#{Letter_s}|[_:])#{NameChar_s}*"
    SkipLit_s = "(\"[^\"]*\"|'[^']*')"
    Name = /^#{Name_s}$/o
    SkipList = /^#{SkipLit_s}$/o

    # XPointer
    Instance_s = "(\\+|-)?[1-9]#{Digit_s}*"
    Instance = /^#{Instance_s}$/o

  end

=begin

== Module XML::DOM (XML::SimpleTree)

DOM-like APIs module.

=end

  module DOM

    ## Fundamental Interfaces

=begin

== Class XML::DOM::DOMException

=== superclass
Exception

DOM exception.
=end

    class DOMException<Exception
      INDEX_SIZE_ERR = 1
      WSTRING_SIZE_ERR = 2
      HIERARCHY_REQUEST_ERR  = 3
      WRONG_DOCUMENT_ERR = 4
      INVALID_NAME_ERR = 5
      NO_DATA_ALLOWED_ERR = 6
      NO_MODIFICATION_ALLOWED_ERR = 7
      NOT_FOUND_ERR = 8
      NOT_SUPPORTED_ERR = 9
      INUSE_ATTRIBUTE_ERR = 10
      ERRMSG = [
        "no error",
        "index size",
        "wstring size",
        "hierarchy request",
        "wrong document",
        "invalid name",
        "no data allowed",
        "no modification allowed",
        "not found",
        "not supported",
        "inuse attribute"
      ]

=begin
=== Class Methods

    --- DOMException.new(code = 0)

generate DOM exception.
=end

      def initialize(code = 0)
        @code = code
      end

=begin
=== Methods

    --- DOMException#code()

return code of exception.

=end
      def code
        @code
      end

=begin

    --- DOMException#to_s()

return the string representation of the error.

=end
      def to_s
        ERRMSG[@code]
      end
    end

=begin
== Class XML::DOM::DOMImplementation

=end
    class DOMImplementation
      include Singleton

=begin
   --- DOMImplementation#hasFeature(feature, version)

test if DOM implementation has correct feature and version.

=end
      def hasFeature(feature, version)
        if feature =~ /^XML$/i && (version.nil? || version == "1.0")
          return true
        end
        false
      end
    end

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
#        NODE = 0
#        ELEMENT = 1
#        ATTRIBUTE = 2
#        TEXT = 3
#        CDATA_SECTION = 4
#        ENTITY_REFERENCE = 5
#        ENTITY = 6
#        PI = 7
#        PROCESSING_INSTRUCTION = 7
#        COMMENT  = 8
#        DOCUMENT = 9
#        DOCUMENT_TYPE = 10
#        DOCUMENT_FRAGMENT = 11
#        NOTATION = 12

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


=begin
    --- Node#childNodes=(p)

set child node as p.
=end
      def childNodes=(p)
        if @children.nil?
          @children = NodeList.new
        else
          @children.to_a.clear
        end
        if p.nil? || (p.is_a?(Array) && p.length == 0)
          return
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

#  =begin
#      --- Node#insertAfter(newChild, refChild)
#
#  insert newChild into the node after refChild.
#  =end
#        def insertAfter(newChild, refChild)
#          if @children.nil? || @children.length == 0
#            raise DOMException.new(DOMException::NOT_FOUND_ERR)
#          end
#          index = _getChildIndex(refChild)
#          raise DOMException.new(DOMException::NOT_FOUND_ERR) if index.nil?
#          _insertNodes(index, newChild)
#        end

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

      ## get the Node object by IDs
      ## [experimental implement]
      def _searchID(value, ids = nil)
        if ids.nil?
          doc = nil
          if nodeType == DOCUMENT_NODE
            doc = self
          elsif !ownerDocument.nil?
            doc = ownerDocument
          else
            return nil
          end
          ids = doc._getIDAttrs
        end
        if nodeType == ELEMENT_NODE && _getIDVals(ids).include?(value)
          return self
        elsif !@children.nil?
          @children.each do |node|
            if !(match = node._searchID(value, ids)).nil?
              return match
            end
          end
        end
        return nil
      end

      def _getMyLocation(parent)
        index = parent._getChildIndex(self)
        if !index.nil?
          "child(#{index + 1},#all)"
        else
          nil
        end
      end

=begin
    --- Node#makeXPointer(use_id = true)

return XPointer's expression of this node.
=end
      def makeXPointer(use_id = true)
        if use_id && !attributes.nil? && !(idvals = _getIDVals).empty?
          "id(#{idvals[0]})"
        elsif @parent.nil? || @parent.nodeType == DOCUMENT_NODE
          "root()"
        else
          @parent.makeXPointer(use_id) + "." + self._getMyLocation(@parent)
        end
      end

      ## [Masaki Fukushima]
      def _child(reverse = false)
        return if @children.nil?
        @children.reversible_each(reverse) do |child|
          yield child
        end
      end

      ## [Masaki Fukushima]
      def _descendant(reverse = false)
        return if @children.nil?
        @children.reversible_each(reverse) do |child|
          yield child
          child._descendant(reverse) do |node|
            yield node
          end
        end
      end

      ## [Masaki Fukushima]
      def _ancestor(reverse = false)
        return if @parent.nil?
        yield @parent if !reverse
        @parent._ancestor(reverse) do |node| yield node end
        yield @parent if reverse
      end

      ## [Masaki Fukushima]
      def __sibling(reverse, only_appeared_before_self)
        return if @parent.nil?
        self_appeared = false
        @parent.childNodes.reversible_each(reverse) do |node|
          if node == self
            self_appeared = true
            next
          end
          if only_appeared_before_self
            break if self_appeared
            yield node
          else # only appeared after self
            yield node if self_appeared
          end
        end
      end

      ## [Masaki Fukushima]
      def _psibling(reverse = false)
        __sibling(!reverse, reverse) do |sib|
          yield sib
        end
      end

      ## [Masaki Fukushima]
      def _fsibling(reverse = false)
        __sibling(reverse, reverse) do |sib|
          yield sib
        end
      end

      ## [Masaki Fukushima]
      def _preceding(reverse = false)
        return if @parent.nil?
        prev_sib = previousSibling
        if prev_sib
          prev_sib._preceding(reverse)   {|node| yield node} if reverse
          yield prev_sib
          prev_sib._descendant(!reverse) {|node| yield node}
          prev_sib._preceding(reverse)   {|node| yield node} if !reverse
        else
          @parent._preceding(reverse) {|node| yield node} if reverse
          yield @parent
          @parent._preceding(reverse) {|node| yield node} if !reverse
        end
      end

      ## [Masaki Fukushima]
      def _following(reverse = false)
        return if @parent.nil?
        next_sib = nextSibling
        if next_sib
          next_sib._following(reverse)  {|node| yield node} if reverse
          yield next_sib
          next_sib._descendant(reverse) {|node| yield node}
          next_sib._following(reverse)  {|node| yield node} if !reverse
        else
          @parent._following(reverse) {|node| yield node} if reverse
          yield @parent
          @parent._following(reverse) {|node| yield node} if !reverse
        end
      end

      ## [Masaki Fukushima]
      def _matchAttribute?(attr, value)
        case value
        when '*'
          return !attr.nil?
        when '#IMPLIED'
          return attr.nil?
        else
          return false if attr.nil?
        end

        case value
        when /^"([^"]*)"$/, /^'([^']*)'$/
          ignore_case = false
          value = $1
        when Spec::Name
          ignore_case = true
        else
          raise "invalid attribute value: #{value}"
        end
        if ignore_case
          return attr.nodeValue.downcase == value.downcase
        else
          return attr.nodeValue == value
        end
      end

      ## [Masaki Fukushima]
      def _matchNodeAttributes?(node, attributes)
        return true     if attributes.nil?
        raise TypeError if !attributes.is_a?(Hash)
        return true     if attributes.length == 0
        return false    if node.nodeType != ELEMENT_NODE

        attributes.each do |name, value|
          case name
          when '*'
            return catch(:match) {
              node.attributes.each do |attr|
                throw(:match, true) if _matchAttribute?(attr, value)
              end
              false
            }
          when Spec::Name
            attr = node.attributes[name] unless node.attributes.nil?
            return _matchAttribute?(attr, value)
          else
            raise "invalid attribute name: '#{name}'"
          end
        end
      end

      ## [Masaki Fukushima]
      def _matchNodeType?(node, ntype)
        case ntype
        when '#element'
          return (node.nodeType == ELEMENT_NODE)
        when '#pi'
          return (node.nodeType == PROCESSING_INSTRUCTION_NODE)
        when '#comment'
          return (node.nodeType == COMMENT_NODE)
        when '#text'
          return (node.nodeType == TEXT_NODE ||
                  node.nodeType == CDATA_SECTION_NODE)
        when '#cdata'
          return (node.nodeType == CDATA_SECTION_NODE)
        when '#all'
          case node.nodeType
          when ELEMENT_NODE, PROCESSING_INSTRUCTION_NODE, COMMENT_NODE,
              TEXT_NODE, CDATA_SECTION_NODE
            return true
          else
            return false
          end
        when /^#/
          raise "unknown node type: '#{ntype}'"
        when Spec::Name
          return (node.nodeType == ELEMENT_NODE && node.nodeName == ntype)
        else
          raise "invalid element type: '#{ntype}'"
        end
      end

      ## [Masaki Fukushima]
      def _matchNode?(node, ntype, attributes)
        _matchNodeType?(node, ntype) &&
          _matchNodeAttributes?(node, attributes)
      end

      ## [Masaki Fukushima]
      def _nodesByRelativeLocationTerm(location)
        if location !~ /^([a-z]+)\(([^\)]*)\)$/
          raise "invalid relative location: '#{location}'"
        end
        keyword = $1
        args = $2.split(/,/)
        number = args.shift
        ntype = args.shift
        ntype = '#element' if ntype.nil?
        attributes = args

        reverse = false
        # check instance number
        case number
        when nil, ''
          raise "missing instance number: '#{location}'"
        when 'all'
        when Spec::Instance
          number = number.to_i
          if number < 0
            reverse = true
            number = -number
          end
        else
          raise "unknown instance number: '#{number}'"
        end

        # check attributes
        if attributes.length % 2 != 0
          raise " missing attribute value: '#{location}'"
        end
        attributes = Hash[*attributes]

        # iterate over nodes specified with keyword
        i = 0
        self.send("_#{keyword}", reverse) do |node|
          next unless _matchNode?(node, ntype, attributes)
          if number == "all"
            yield node
          else
            i += 1
            if i >= number
              yield node
              break
            end
          end
        end
      end

      ## [Masaki Fukushima]
      def _nodesByLocationTerms(location, pre_keyword = nil)
        if location !~ /^([a-z]*)\(([^)]*)\)(\.(.+))?$/
          raise "invalid location: \"#{location}\""
        end
        keyword = $1
        args = $2
        rest = $4
        ## omitted keyword
        keyword = pre_keyword if keyword == ''
        if keyword.nil?
          raise "cannot determine preceding keyword: \"#{location}\""
        end

        case keyword
        when 'child', 'descendant', 'ancestor', 'psibling', 'fsibling',
            'preceding', 'following'
          # relative location term
          _nodesByRelativeLocationTerm("#{keyword}(#{args})") do |node|
            if rest.nil?
              yield node
            else
              node._nodesByLocationTerms(rest, keyword) do |n|
                yield n
              end
            end
          end
        when 'attr'
          # attribute location term
          if args !~ Spec::Name
            raise "invalid attribute name: '#{args}'"
          end
          attr = attributes[args]
          value = (attr.nil? ? nil : Text.new(attr.nodeValue))
          if rest.nil?
            yield value
          elsif !value.nil?
            value._nodesByLocationTerms(rest) do |node|
              yield node
            end
          end
        when 'span', 'string'
          raise "unsupported keyword: '#{keyword}'"
        else
          raise "unknown keyword: '#{keyword}'"
        end
      end

      ## [Masaki Fukushima]
      def _getNodeByAbsoluteLocationTerm(location)
        case location
        when 'root()', ''
          if nodeType == DOCUMENT_NODE
            root = documentElement
          elsif !ownerDocument.nil?
            root = ownerDocument.documentElement
          end
          root = self if root.nil?
          return root
        when 'origin()'
          return self
        when /^id\(([^\)]*)\)$/
          value = $1
          raise "invalid id value: #{value}" if value !~ Spec::Name
          return _searchID(value)
        when /^html\(([^\)]*)\)$/
          value = $1
          return getNodesByXPointer("root().descendant(1,A,NAME,\"#{value}\")")[0]
        else
          raise "unknown keyword: #{location}"
        end
      end

=begin
    --- Node#getNodeByXPointer(pointer)

return node indicated by the XPointer pointer.
=end
      ## [Masaki Fukushima]
      def getNodesByXPointer(pointer)
        if pointer !~ /^([a-z]+)\(([^)]*)\)(\.(.+))?$/
          raise "invalid XPointer: \"#{pointer}\""
        end
        keyword = $1
        args = $2
        rest = $4

        case keyword
        when 'root', 'origin', 'id', 'html'
          src = _getNodeByAbsoluteLocationTerm("#{keyword}(#{args})")
        else
          src = _getNodeByAbsoluteLocationTerm("root()")
          rest = pointer
        end

        ret = NodeList.new
        if src.nil?
          # no match
        elsif rest.nil?
          yield src if iterator?
          ret << src
        else
          src._nodesByLocationTerms(rest) do |node|
            yield node if iterator?
            ret << node
          end
        end
        ret
      end

=begin
    --- Node#ownerDocument()

[DOM]
Document object associated with this node.
=end
      ## [DOM]
      ## Floating objects are not owned by any documents.
      def ownerDocument
        return @ownerDocument if @ownerDocument
        parent = self.parentNode
        return nil if parent.nil?
        if parent.nodeType == DOCUMENT_NODE
          return parent
        else
          return parent.ownerDocument
        end
      end

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
      ## if attribute 'xml:space' is 'preserve',
      ## don't trim any white spaces
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


    end

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
        @nodes[name] = nil
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
    end

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

=begin
== Class XML::DOM::DocumentFragment

=== superclass
Node

=end
    class DocumentFragment<Node

=begin
=== Class Methods

    --- DocumentFragment.new(*children)

creates a new DocumentFragment.
=end

      def initialize(*children)
        super(*children)
      end

=begin
=== Methods

    --- DocumentFragment#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        DOCUMENT_FRAGMENT_NODE
      end

=begin
    --- DocumentFragment#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#document-fragment"
      end

=begin
    --- DocumentFragment#parentNode=(p)

returns the parent of this node.
=end
      ## DocumentFragment should not have the parent node.
      def parentNode=(p)
        @children.each do |child|
          child.parentNode = p
        end if @children
      end

      def _checkNode(node)
        unless node.nodeType == ELEMENT_NODE ||
            node.nodeType == PROCESSING_INSTRUCTION_NODE ||
            node.nodeType == COMMENT_NODE ||
            node.nodeType == TEXT_NODE ||
            node.nodeType == CDATA_SECTION_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end

    end

=begin
== Class XML::DOM::Document

=== superclass
Node

=end
    class Document<Node
=begin
=== Class Methods
    --- Document.new(*children)

creates a new Document.
=end

      ## new([child1, child2, ...]) or
      ## new(child1, child2, ...)
      ##     child?: String or Node
      def initialize(*children)
        super(*children)
      end

=begin
=== Methods

    --- Document#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        DOCUMENT_NODE
      end

=begin
    --- Document#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        "#document"
      end

=begin
    --- Document#documentElement

[DOM]
returns root element of the Docuemnt.
=end
      ## [DOM]
      def documentElement
        @children.each do |child|
          if child.nodeType == ELEMENT_NODE
            return child
          end
        end if @children
        nil
      end

=begin
    --- Document#doctype

[DOM]
returns DTD associated with this document.
=end
      ## [DOM]
      def doctype
        @children.each do |child|
          if child.nodeType == DOCUMENT_TYPE_NODE
            return child
          end
        end if @children
        nil
      end

=begin
    --- Document#getElementsByTagName(tagname)

[DOM]
returns a NodeList of all the Elements with a given tag name.
=end
      ## [DOM] (but this is not "live")
      def getElementsByTagName(tagname)
        ret = NodeList.new
        @children.each do |node|
          if node.nodeType == ELEMENT_NODE
            if tagname == '*' || node.nodeName == tagname
              ret << node
            end
            ret << node.getElementsByTagName(tagname)
          end
        end if @children
        ret
      end

=begin
    --- Document#createElement(tagName)

[DOM]
creates a Element.
=end
      ## [DOM]
      def createElement(tagName)
        ret = Element.new(tagName)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createTextNode(data)

[DOM]
creates a TextNode.
=end
      ## [DOM]
      def createTextNode(data)
        ret = Text.new(data)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createCDATASection(data)

[DOM]
creates a CDATASection.
=end
      ## [DOM]
      def createCDATASection(data)
        ret = CDATASection.new(data)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createComment(data)

[DOM]
create a Comment.
=end
      ## [DOM]
      def createComment(data)
        ret = Comment.new(data)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createProcessingInstruction(target, data)

[DOM]
create a ProcessingInstruction.
=end
      ## [DOM]
      def createProcessingInstruction(target, data)
        ret = ProcessingInstruction.new(target, data)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createAttribute(name)

[DOM]
create a Attribute.
=end
      ## [DOM]
      def createAttribute(name)
        ret = Attr.new(name)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createEntityReference(name)

[DOM]
create a EntityReference.
=end
      ## [DOM]
      def createEntityReference(name)
        ret = EntityReference.new(name)
        ret.ownerDocument = self
        ret
      end

=begin
    --- Document#createDocumentFragment()

[DOM]
create a DocumentFragment.
=end
      ## [DOM]
      def createDocumentFragment
        ret = DocumentFragment.new
        ret.ownerDocument = self
        ret
      end

      ## set the ID list by the attribute name with the element name
      ## (or wildcard)
      ## [experimental implement]
      def _setIDAttr(attrname, elemname = '*')
        @idattrs = {} if @idattrs.nil?
        @idattrs[elemname] = attrname
      end

      ## get the ID list
      ## [experimental implement]
      def _getIDAttrs
        return {'*'=>'id'} if @idattrs.nil?
        @idattrs
      end

      ## [DOM]
      def implementation
        return @implemantation if @implemantation
        ## singleton
        @implemantation = DOMImplementation.instance
      end

      def implementation=(impl)
        @implemantation = impl
      end

      def _checkNode(node)
        unless node.nodeType == ELEMENT_NODE ||
            node.nodeType == PROCESSING_INSTRUCTION_NODE ||
            node.nodeType == COMMENT_NODE ||
            node.nodeType == DOCUMENT_TYPE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end

        if node.nodeType == ELEMENT_NODE
          @children.each do |n|
            if n.nodeType == ELEMENT_NODE
              raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
            end
          end
        end

        if node.nodeType == DOCUMENT_TYPE_NODE
          @children.each do |n|
            if n.nodeType == DOCUMENT_TYPE_NODE
              raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
            end
          end
        end
      end

    end

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
      def initialize(name = nil, *text)
        super(text)
        raise "parameter error" if !name
        @name =  name
        @name.freeze
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
        nodeValue.each_byte do |code|
          case code
          when 9, 10, 13
            value << sprintf("&#x%X;", code)
          when ?&
            value << "&amp;"
          when ?"
            value << "&quot;"
          when ?<
            value << "&lt;"
          else
            value << code
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
      alias value= nodeValue=

      ## [DOM]
      def specified; @specified; end
      def specified=(is_specified); @specified = is_specified; end

      def _checkNode(node)
        unless node.nodeType == TEXT_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end

    end

=begin
== Class XML::DOM::Attribute

alias of Attr.
=end
    Attribute = Attr

=begin
== Class XML::DOM::Element

=== superclass
Node

=end
    class Element<Node

=begin
=== Class Methods

    --- Element.new(tag = nil, attrs = nil, *children)

create a new Element.
=end
      ## new(tag, attrs, [child1, child2, ...]) or
      ## new(tag, attrs, child1, child2, ...)
      ##     tag:    String
      ##     attrs:  Hash, Attr or Array of Attr (or nil)
      ##     child?: String or Node
      def initialize(tag = nil, attr = nil, *children)
        super(*children)
        raise "parameter error" if !tag
        @name = tag.freeze
        if attr.nil?
          @attr = NamedNodeMap.new([])
        elsif attr.is_a?(Hash)
          nodes = []
          attr.each do |key, value|
            nodes.push(Attr.new(key, value))
          end
          @attr = NamedNodeMap.new(nodes)
        elsif attr.is_a?(Array)
          @attr = NamedNodeMap.new(attr)
        elsif attr.is_a?(Attr)
          @attr = NamedNodeMap.new([attr])
        else
          raise "parameter error: #{attr}"
        end
      end

=begin
=== Methods

    --- Element#nodeType()

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        ELEMENT_NODE
      end

=begin
    --- Element#nodeName()

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- Element#attributes()

[DOM]
returns the attributes of this Element.
=end
      ## [DOM]
      def attributes
        if iterator?
          @attr.each do |key, value|
            yield(value)
          end if @attr
        else
          @attr
        end
      end

=begin
    --- Element#to_s()

return the string representation of the Element.
=end
      def to_s
        attr = ''
        @attr.each do |a|
          attr << ' ' + a.to_s
        end if @attr
        content = super
        if content != ''
          ret = "<#{@name}#{attr}>#{content}</#{@name}>"
        else
          ret = "<#{@name}#{attr}/>"
        end
        ret << "\n" if parentNode.nodeType == DOCUMENT_NODE
        ret
      end

=begin
    --- Element#dump(depth = 0)

dumps the Element.
=end
      def dump(depth = 0)
        attr = ''
        @attr.each do |a|  ## self.attributes do |a|
          attr += a.to_s + ", "
        end if @attr
        attr.chop!
        attr.chop!
        print ' ' * depth * 2
        print "#{@name}(#{attr})\n"
        @children.each do |child|
          child.dump(depth + 1)
        end if @children
      end

=begin
    --- Element#tagName()

[DOM]
alias of nodeName.
=end
      ## [DOM]
      alias tagName nodeName

=begin
    --- Element#getAttribute(name)

[DOM]
retrieves an attribute value by name.
=end
      ## [DOM]
      def getAttribute(name)
        attr = getAttributeNode(name)
        if attr.nil?
          ''
        else
          attr.nodeValue
        end
      end

=begin
    --- Element#setAttribute(name, value)

[DOM]
adds a new attribute.
=end
      ## [DOM]
      def setAttribute(name, value)
        if @ownerDocument
          attr = @ownerDocument.createAttribute(name)
          attr.appendChild(@ownerDocument.createTextNode(value))
        else
          attr = Attribute.new(name)
          attr.appendChild(Text.new(value))
        end
        setAttributeNode(attr)
      end

=begin
    --- Element#removeAttribute(name)

[DOM]
remove an attribute by name.
=end
      ## [DOM]
      def removeAttribute(name)
        ret = getAttributeNode(name)
        removeAttributeNode(ret) if ret
      end

=begin
    --- Element#getAttributeNode(name)

[DOM]
retrieves an Attr node by name.
=end
      ## [DOM]
      def getAttributeNode(name)
        @attr.getNamedItem(name)
      end

=begin
    --- Element#setAttributeNode(newAttr)

[DOM]
adds a new attribute.
=end
      ## [DOM]
      def setAttributeNode(newAttr)
        ret = getAttributeNode(newAttr.nodeName)
        if ret == newAttr
          raise DOMException.new(DOMException::INUSE_ATTRIBUTE_ERR)
        end
        @attr.setNamedItem(newAttr)
        ret
      end

=begin
    --- Element#removeAttributeNode(oldAttr)

[DOM]
removes the specified attribute.
=end
      ## [DOM]
      def removeAttributeNode(oldAttr)
        ret = getAttributeNode(oldAttr.nodeName)
        if ret.nil? || ret != oldAttr
          raise DOMException.new(DOMException::NOT_FOUND_ERR)
        end
        @attr.removeNamedItem(oldAttr.nodeName)
        ret
      end

=begin
    --- Element#getElementsByTagName(tagname)

[DOM]
returns a NodeList of all descendant elements with given tag name.
=end
      ## [DOM] (but this is not "live")
      def getElementsByTagName(tagname)
        ret = NodeList.new
        @children.each do |node|
          if node.nodeType == ELEMENT_NODE
            if tagname == '*' || node.nodeName == tagname
              ret << node
            end
            ret << node.getElementsByTagName(tagname)
          end
        end if @children
        ret
      end

      def _getMyLocation(parent)
        index = 1
        parent.childNodes do |child|
          if child == self
            return "child(#{index},#{@name})"
          end
          if child.nodeType == ELEMENT_NODE && child.nodeName == @name
            index += 1
          end
        end
        nil
      end


=begin
    --- Element#normalize

[DOM]
puts all Text nodes in the full depth of the sub-tree under this
Eelemnt.
=end
      ## [DOM]
      def normalize
        return if @children.nil?
        old = nil
        children = @children.to_a.dup
        children.each do |child|
          if !old.nil? && old.nodeType == TEXT_NODE &&
              child.nodeType == TEXT_NODE
            old.appendData(child.nodeValue)
            self.removeChild(child)
          else
            if child.nodeType == ELEMENT_NODE
              child.normalize
            end
            old = child
          end
        end
      end

=begin
    --- Element#cloneNode(deep = true)

[DOM]
returns the copy of the Element.
=end
      ## [DOM]
      def cloneNode(deep = true)
        attrs = []
        @attr.each do |attr|
          attrs.push(attr.cloneNode(true))
        end
        super(deep, @name, attrs)
      end

      ## get the list of nodeValues by IDs
      ## [experimental implement]
      def _getIDVals(ids = nil)
        if ids.nil?
          doc = ownerDocument
          return [] if doc.nil?
          ids = doc._getIDAttrs
        end

        idelem = []
        if !ids[nodeName].nil?
          return attributes._getValues(ids[nodeName])
        elsif !ids['*'].nil?
          return attributes._getValues(ids['*'])
        end
        return []
      end

=begin
    --- Element#trim(preserve = false)

trim extra whitespaces.
=end
      ## trim extra whitespaces
      ## if attribute 'xml:space' is 'preserve',
      ## don't trim any white spaces
      def trim(preserve = false)
        if !attributes['xml:space'].nil?
          value = attributes['xml:space'].nodeValue
          if value == 'preserve'
            preserve = true
          elsif value == 'default'
            preserve = false
          end
        end
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

      def _checkNode(node)
        unless node.nodeType == ELEMENT_NODE ||
            node.nodeType == TEXT_NODE ||
            node.nodeType == COMMENT_NODE ||
            node.nodeType == PROCESSING_INSTRUCTION_NODE ||
            node.nodeType == CDATA_SECTION_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end

    end

=begin
== Class XML::DOM::CharacterData

=== superclass
Node

=end
    class CharacterData<Node

=begin
=== Class Methods

    --- CharacterData.new(text)

creates a new CharacterData.
=end
      ## new(text)
      ##     text: String
      def initialize(text = nil)
        super()
        raise "parameter error" if !text
        @value = text
      end

=begin
=== Methods

    --- CharacterData#data()

[DOM]
returns the character data of the node.
=end
      ## [DOM]
      def data
        @value.dup
      end

=begin
    --- CharacterData#data=(p)

[DOM]
set the character data of the node.
=end
      ## [DOM]
      def data=(p)
          @value = p
      end

=begin
    --- CharacterData#length()

[DOM]
returns length of this CharacterData.
=end
      ## [DOM]
      def length
        @value.length
      end

=begin
    --- CharacterData#substringData(start, count)

[DOM]
extracts a range of data from the node.
=end
      ## [DOM]
      def substringData(start, count)
        if start < 0 || start > @value.length || count < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        ## if the sum of start and count > length,
        ##  return all characters to the end of the value.
        @value[start, count]
      end

=begin
    --- CharacterData#appendData(str)

[DOM]
append the string to the end of the character data.
=end
      ## [DOM]
      def appendData(str)
        @value << str
      end

=begin
    --- CharacterData#insertData(offset, str)

[DOM]
insert a string at the specified character offset.
=end
      ## [DOM]
      def insertData(offset, str)
        if offset < 0 || offset > @value.length
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        @value[offset, 0] = str
      end

=begin
    --- CharacterData#deleteData(offset, count)

[DOM]
removes a range of characters from the node.
=end
      ## [DOM]
      def deleteData(offset, count)
        if offset < 0 || offset > @value.length || count < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        @value[offset, count] = ''
      end

=begin
    --- CharacterData#replaceData(offset, count, str)

[DOM]
replaces the characters starting at the specified character offset
with specified string.
=end
      ## [DOM]
      def replaceData(offset, count, str)
        if offset < 0 || offset > @value.length || count < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        @value[offset, count] = str
      end

=begin
    --- CharacterData#cloneData(deep = true)

[DOM]
returns the copy of the CharacterData.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @value.dup)
      end

=begin
    --- Text#nodeValue

[DOM]
return nodevalue.

=end
      ## [DOM]
      def nodeValue
        @value
      end

=begin
    --- CharacterData#nodeValue=(p)

[DOM]
set nodevalue as p.
=end
      ## [DOM]
      def nodeValue=(p)
        @value = p
      end

    end

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
        @value.each_byte do |code|
          case (code)
          when 13
            ret << sprintf("&#x%X;", code)
          when ?&
            ret << "&amp;"
          when ?<
            ret << "&lt;"
          when ?>
            ret << "&gt;"
          else
            ret << code
          end
        end
        ret
      end

=begin
    --- Text#dump(depth = 0)

dumps the Text.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "#{@value.inspect}\n"
      end

      def _getMyLocation(parent)
        index = 1
        parent.childNodes do |child|
          if child == self
            return "child(#{index},#text)"
          end
          if child.nodeType == TEXT_NODE
            index += 1
          end
        end
        nil
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

      def _getMyLocation(parent)
        index = 1
        parent.childNodes do |child|
          if child == self
            return "child(#{index},#comment)"
          end
          if child.nodeType == COMMENT_NODE
            index += 1
          end
        end
        nil
      end
    end


    ## Extended Interfaces

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

      def _getMyLocation(parent)
        index = 1
        parent.childNodes do |child|
          if child == self
            return "child(#{index},#cdata)"
          end
          if child.nodeType == CDATA_SECTION_NODE
            index += 1
          end
        end
        nil
      end
    end

=begin
== Class XML::DOM::DocumentType

=== superclass
Node
=end
    class DocumentType<Node

=begin
=== Class Methods

    --- DocumentType.new(name, value = nil, *children)

creates a new DocuemntType.
=end
      def initialize(name, value = nil, *children)
        super(*children)
        raise "parameter error" if !name
        @name = name.freeze
        @value = value.freeze
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
        ret = "<!DOCTYPE " + @name
        if !@value.nil?
          ret <<= " " + @value
        end
        if !@children.nil? && @children.length > 0
          ret <<= " [\n"
          @children.each do |child|
            if child.nodeType == PROCESSING_INSTRUCTION_NODE ||
                child.nodeType == COMMENT_NODE
              ret <<= child.to_s + "\n"
            else
              ret <<= child.nodeValue + "\n"
            end
          end
          ret <<= "]"
        end
        ret <<= ">"
      end

=begin
    --- DocumentType#dump(depth = 0)

dumps the DocumentType.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "<!DOCTYPE #{@name} #{@value} [\n"
        @children.each do |child|
          print ' ' * (depth + 1) * 2
          if child.nodeType == PROCESSING_INSTRUCTION_NODE ||
              child.nodeType == COMMENT_NODE
            child.dump
          else
            print child.nodeValue, "\n"
          end
        end if @children
        print ' ' * depth * 2
        print "]>\n"
      end

=begin
    --- DocumentType#cloneNode(deep = true)

[DOM]
returns the copy of the DocumentType.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name, @value)
      end

      ## [DOM]
      ## def entities; @entities; end
      ## def notations; @notations; end
    end

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

=begin
== Class XML::DOM::Entity

=== superclass
Node
=end
    class Entity<Node

=begin
=== Class Methods

    --- Entity.new(name, pubid, sysid, notation)

creates a new Entity.
=end
      def initialize(name, pubid, sysid, notation)
        super()
        @name = name.freeze
        @pubid = pubid.freeze
        @sysid = sysid.freeze
        @notation = notation.freeze
      end

=begin
=== Methods

    --- Entity#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        ENTITY_NODE
      end

=begin
    --- Entity#nodeName

[DOM]
returns the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- Entity#publicId

returns the publicId of the Entity.
=end
      def publicId
        @pubid
      end

=begin
    --- Entity#systemId

returns the systemId of the Entity.
=end
      def systemId
        @sysid
      end

=begin
    --- Entity#notationName

returns the notationname of the Entity.
=end
      def notationName
        @notation
      end

=begin
    --- Entity#cloneNode(deep = true)

[DOM]
returns the copy of the Entity.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name, @pubid, @sysid, @notation)
      end

      def _checkNode(node)
        unless node.nodeType == ELEMENT_NODE ||
            node.nodeType == PROCESSING_INSTRUCTION_NODE ||
            node.nodeType == COMMENT_NODE ||
            node.nodeType == TEXT_NODE ||
            node.nodeType == CDATA_SECTION_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end

    end

=begin
== Class XML::DOM::EntityReference

=== superclass
Node
=end
    class EntityReference<Node

=begin
=== Class Methods

    --- EntityReference.new(name, *children)

creates a new EntityReference.
=end
      def initialize(name, *children)
        super(*children)
        raise "parameter error" if !name
        @name = name.freeze
      end

=begin
=== Methods

    --- EntityReference#nodeType

[DOM]
returns the nodeType.
=end
      ## [DOM]
      def nodeType
        ENTITY_REFERENCE_NODE
      end

=begin
    --- EntityReference#nodeName

[DOM]
returns  the nodeName.
=end
      ## [DOM]
      def nodeName
        @name
      end

=begin
    --- EntityReference#to_s

returns the string representation of the EntityReference.
=end
      ## reference form or expanded form?
      def to_s
        "&#{@name};"
      end

=begin
    --- EntityReference#dump(depth = 0)

dumps the EntityReference.
=end
      def dump(depth = 0)
        print ' ' * depth * 2
        print "&#{@name}{\n"
        @children.each do |child|
          child.dump(depth + 1)
        end if @children
        print ' ' * depth * 2
        print "}\n"
      end

=begin
    --- EntityReference#cloneNode(deep = true)

[DOM]
returns the copy of the EntityReference.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @name)
      end

      def _checkNode(node)
        unless node.nodeType == ELEMENT_NODE ||
            node.nodeType == PROCESSING_INSTRUCTION_NODE ||
            node.nodeType == COMMENT_NODE ||
            node.nodeType == TEXT_NODE ||
            node.nodeType == CDATA_SECTION_NODE ||
            node.nodeType == ENTITY_REFERENCE_NODE
          raise DOMException.new(DOMException::HIERARCHY_REQUEST_ERR)
        end
      end

    end

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

=begin
    --- ProcessingInstruction#target=(p)

[DOM]
set p to the target of the ProcessingInstruction.
=end
      ## [DOM]
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

      def _getMyLocation(parent)
        index = 1
        parent.childNodes do |child|
          if child == self
            return "child(#{index},#pi)"
          end
          if child.nodeType == PROCESSING_INSTRUCTION_NODE
            index += 1
          end
        end
        nil
      end

=begin
    --- ProcessingInstruction#cloneNode(deep = true)

[DOM]
returns the copy of the ProcessingInstruction.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @target.dup, @data.dup)
      end
    end

  end

  SimpleTree = DOM

end
