#
# xpath-dom.rb
#
#   Copyright (C) Ueno Katsuhiro 2000
#   DOM2 support by yoshidam
#
# $Id: xpath.rb,v 1.2 2003/03/12 06:38:28 yoshidam Exp $
#

require 'xml/dom2/core'
require 'xml/xpath'

module XMLScan
  XPath = ::XPath unless
    defined?(::XMLScan::XPath)

module XPath

  module DOM

    class AbstractNodeAdapter < NullNodeAdapter

      def wrap(node, visitor)
        @node = node
        self
      end

      attr_reader :node

      def root
        @node.ownerDocument
      end

      def parent
        @node.parentNode
      end

      def children
        @node.childNodes.to_a
      end

      def each_following_siblings
        node = @node
        yield node while node = node.nextSibling
      end

      def each_preceding_siblings
        node = @node
        yield node while node = node.previousSibling
      end

      def index
        @node.parentNode.childNodes.to_a.index(@node)
      end

      def lang
        node = @node
        lang = nil
        until a = node.attributes and lang = a.getNamedItem('xml:lang')
          node = node.parentNode
        end
        lang and lang.nodeValue
      end

    end


    class TextNodeAdapter < AbstractNodeAdapter

      def node_type
        :text
      end

      def string_value
        @node.nodeValue
      end

    end


    class CommentNodeAdapter < TextNodeAdapter

      def node_type
        :comment
      end

    end


    class PINodeAdapter < AbstractNodeAdapter

      def node_type
        :processing_instruction
      end

      def name_localpart
        @node.nodeName
      end

      def string_value
        @node.nodeValue
      end

    end


    class ParentNodeAdapter < AbstractNodeAdapter

      def string_value
        dst = ''
        stack = @node.childNodes.to_a.reverse
        while node = stack.pop
          s = node.nodeValue
          dst << s if s
          stack.concat node.childNodes.to_a.reverse
        end
        dst
      end

    end


    class RootNodeAdapter < ParentNodeAdapter

      def node_type
        :root
      end

      alias root node

      def index
        0
      end

    end


    class ElementNodeAdapter < ParentNodeAdapter

      def wrap(node, visitor)
        @node = node
        @visitor = visitor
        self
      end

      def node_type
        :element
      end

      def name_localpart
        @node.nodeName
      end

      def namespace_uri
        @node.namespaceURI
      end

      def qualified_name
        @node.nodeName
      end

      def attributes
        map = @node.attributes
        attrs = @visitor.get_attributes(@node)
        unless attrs then
          attrs = []
          map.length.times { |i| attrs.push map.item(i) }
          @visitor.regist_attributes @node, attrs
        end
        attrs
      end

    end


    class AttrNodeAdapter < AbstractNodeAdapter

      def wrap(node, visitor)
        @node = node
        @visitor = visitor
        self
      end

      def node_type
        :attribute
      end

      def name_localpart
        @node.nodeName
      end

      def namespace_uri
        @node.namespaceURI
      end

      def qualified_name
        @node.nodeName
      end

      def parent
        @visitor.get_attr_parent @node
      end

      def index
        -@visitor.get_attributes(parent).index(@node)
      end

      def string_value
        @node.nodeValue
      end

    end



    class NodeVisitor

      def initialize
        @adapters = Array.new(12, NullNodeAdapter.new)
        @adapters[XML::DOM::Node::ELEMENT_NODE] = ElementNodeAdapter.new
        @adapters[XML::DOM::Node::ATTRIBUTE_NODE] = AttrNodeAdapter.new
        @adapters[XML::DOM::Node::TEXT_NODE] =
          @adapters[XML::DOM::Node::CDATA_SECTION_NODE] = TextNodeAdapter.new
        @adapters[XML::DOM::Node::PROCESSING_INSTRUCTION_NODE] =
          PINodeAdapter.new
        @adapters[XML::DOM::Node::COMMENT_NODE] = CommentNodeAdapter.new
        @adapters[XML::DOM::Node::DOCUMENT_NODE] = RootNodeAdapter.new
        @attr = {}
      end

      def visit(node)
        @adapters[node.nodeType].wrap(node, self)
      end

      def regist_attributes(node, attrs)
        @attr[node] = attrs
        attrs.each { |i| @attr[i] = node }
      end

      def get_attributes(node)
        @attr[node]
      end

      def get_attr_parent(node)
        @attr[node]
      end

    end



    class Context < XMLScan::XPath::Context

      def initialize(node, namespace = nil, variable = nil)
        super node, namespace, variable, NodeVisitor.new
      end

    end


  end

end ## module XPath
end ## module XMLScan



module XML

  module DOM

    class Node

      def __collectDescendatNS(ns = {})
        childNodes.each do |node|
          next if node.nodeType != ELEMENT_NODE
          prefix = node.prefix
          uri = node.namespaceURI
          ns[prefix] = uri unless ns.has_key?(prefix)
          node.__collectDescendatNS(ns)
        end
      end

      def __collectAncestorNS(ns = {})
        node = self
        while node
          prefix = node.prefix
          uri = node.namespaceURI
          ns[prefix] = uri unless ns.has_key?(prefix)
          node = node.parentNode
        end
      end

      def getNodesByXPath(xpath, ns = {})
        xpath = XMLScan::XPath.compile(xpath) unless xpath.is_a? XMLScan::XPath
        if ns.length == 0
          ## collect namespaces
          __collectAncestorNS(ns)
          __collectDescendatNS(ns)
        end
        ret = xpath.call(XPath::DOM::Context.new(self, ns))
        raise "return value is not NodeSet" unless ret.is_a? Array
        ret
      end

      def _getMyLocationInXPath(parent)
        n = parent.childNodes.index(self)
        "node()[#{n + 1}]"
      end

      def makeXPath
        dst = []
        node = self
        while parent = node.parentNode
          dst.push node._getMyLocationInXPath(parent)
          node = parent
        end
        dst.reverse!
        '/' + dst.join('/')
      end

    end


    class Element

      def _getMyLocationInXPath(parent)
        name = nodeName
        n = parent.childNodes.to_a.select { |i|
          i.nodeType == ELEMENT_NODE and i.nodeName == name
        }.index(self)
        "#{name}[#{n + 1}]"
      end

    end


    class Text

      def _getMyLocationInXPath(parent)
        n = parent.childNodes.to_a.select { |i|
          i.nodeType == TEXT_NODE or i.nodeType == CDATA_SECTION_NODE
        }.index(self)
        "text()[#{n + 1}]"
      end

    end


    class CDATASection

      def _getMyLocationInXPath(parent)
        n = parent.childNodes.to_a.select { |i|
          i.nodeType == TEXT_NODE or i.nodeType == CDATA_SECTION_NODE
        }.index(self)
        "text()[#{n + 1}]"
      end

    end


    class Comment

      def _getMyLocationInXPath(parent)
        n = parent.childNodes.to_a.select { |i|
          i.nodeType == COMMENT_NODE
        }.index(self)
        "comment()[#{n + 1}]"
      end

    end


    class ProcessingInstruction

      def _getMyLocationInXPath(parent)
        n = parent.childNodes.to_a.select { |i|
          i.nodeType == PROCESSING_INSTRUCTION_NODE
        }.index(self)
        "processing-instruction()[#{n + 1}]"
      end

    end


    class Attr

      def makeXPath
        '@' + nodeName
      end

    end


  end

end

