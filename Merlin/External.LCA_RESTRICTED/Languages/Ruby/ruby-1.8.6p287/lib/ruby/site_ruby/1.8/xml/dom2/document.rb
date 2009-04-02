## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/nodelist'
require 'xml/dom2/domexception'

require 'xml/dom2/documentfragment'
require 'xml/dom2/attr'
require 'xml/dom2/element'
require 'xml/dom2/text'
require 'xml/dom2/comment'
require 'xml/dom2/cdatasection'
require 'xml/dom2/entityreference'
require 'xml/dom2/processinginstruction'
require 'xml/dom2/domimplementation'


module XML
  module DOM

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

      ## [DOM]
      def implementation
        return @implemantation if @implemantation
        ## singleton
        @implemantation = DOMImplementation.instance
      end

      def implementation=(impl)
        @implemantation = impl
      end

      ## [DOM2]
      def importNode(impnode, deep)
        ## [NOT IMPLEMENTED]
        raise "not implemented"
      end

=begin
    --- Document#createElementNS(nsuri, qname)

[DOM2]
creates a Element with namespace.
=end
      ## [DOM2]
      def createElementNS(nsuri, qname)
        ret = Element.new([nsuri, qname])
        ret.ownerDocument = self
        ret
      end

      XMLNSNS = 'http://www.w3.org/2000/xmlns/'
      ## [DOM2]
      def createAttributeNS(nsuri, qname)
        nsuri = XMLNSNS if qname == 'xmlns' or qname =~ /^xmlns:/u
        ret = Attr.new([nsuri, qname])
        ret.ownerDocument = self
        ret
      end

      ## [DOM2]
      def getElementsByTagNameNS(nsuri, localname)
        ret = NodeList.new
        @children.each do |node|
          if node.nodeType == ELEMENT_NODE
            if (localname == '*' || node.localname == localname) and
                (nsuri == '*' || node.namespaceURI == nsuri)
              ret << node
            end
            ret << node.getElementsByTagNameNS(nsuri, localname)
          end
        end if @children
        ret
      end

      ## [DOM2]
      def getElementById(elementId)
        ## [NOT IMPLEMENTED]
        raise "not implemented"
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

      def _getNamespaces(parentNamespaces = {}, all = false)
        { nil => nil }
      end

    end
  end
end
