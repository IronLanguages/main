## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/domexception'
require 'xml/dom2/nodelist'
require 'xml/dom2/namednodemap'

module XML
  module DOM

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
        @name = nil
        @prefix = nil
        @localname = nil
        @uri = nil
        @idAttribute = nil
        if tag.is_a?(Array)
          ## namespaces
          raise "parameter error" if tag.length != 2
          @localname = tag[1]
          if tag[1].index(':')
            @prefix, @localname = tag[1].split(':')
          end
          @name = tag[1] ## qualified name
          @uri =  tag[0] ## namespace URI
        else
          @name = tag
        end
        @name.freeze
        @localname.freeze
        @uri.freeze
        @prefix.freeze

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
        elsif attr.nodeType == ATTRIBUTE_NODE
          @attr = NamedNodeMap.new([attr])
        else
          raise "parameter error: #{attr}"
        end
        @attr.each do |attr|
          attr.ownerElement = self
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

      def _getNamespaces(parentNamespaces = {}, all = false)
        if !parentNamespaces
          parentNamespaces = parentNode._getNamespaces(nil, true)
        end
        namespaces = {}
        attributes.each do |a|
          namespaces[a.prefix] = a.namespaceURI if a.prefix
        end
        if @localname
          namespaces[@prefix] = @uri
        end
        parentNamespaces.each do |prefix, uri|
          if all
            if !namespaces.include?(prefix)
              namespaces[prefix] = uri
            end
          else
            if namespaces[prefix] == parentNamespaces[prefix]
              namespaces.delete(prefix)
            end
          end
        end
        namespaces
      end

=begin
    --- Element#to_s()

return the string representation of the Element.
=end
      def to_s
        attr = ''

        namespaces = {}
        attributes.each do |a|
          namespaces[a.prefix] = a.namespaceURI if a.prefix
        end
        if @localname
          namespaces[@prefix] = @uri
        end

        namespaces.each do |prefix, uri|
          ## skip the namespace declaration of xml or xmlns.
          next if prefix == 'xml' or
            uri == 'http://www.w3.org/2000/xmlns/'
          nsattrname = 'xmlns'
          nsattrname << ':' + prefix if prefix
          ## skip duplicated namespace declarations.
          next if @attr.getNamedItem(nsattrname)
          attr << " #{nsattrname}=\"#{uri}\""
        end

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
        if @ownerElement
          attr = @ownerDocument.createAttribute(name)
          attr.appendChild(@ownerDocument.createTextNode(value))
        else
          attr = Attr.new(name)
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
        ret.ownerElement = nil if ret
        @attr.setNamedItem(newAttr)
        newAttr.ownerElement = self
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
        ret.ownerElement = nil
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

      ## [DOM2]
      def namespaceURI; @uri; end

      ## [DOM2]
      def prefix; @prefix; end

      ## [DOM2]
      def prefix=(prefix);
        ## to be checked
        @prefix = prefix
        @name = @prefix + ':' + @localname
        @prefix.freeze
        @name.freeze
      end

      ## [DOM2]
      def localname; @localname; end

      ## [DOM2]
      def hasAttributes()
        attributes.length > 0
      end

      ## [DOM2]
      def getAttributeNS(nsuri, localname)
        attr = getAttributeNodeNS(nsuri, localname)
        if attr.nil?
          ""
        else
          attr.nodeValue
        end
      end

      ## [DOM2]
      def setAttributeNS(nsuri, qname, value)
        if qname.index(':')
          prefix, localname = qname.split(':')
          raise DOMException.new(DOMException::NAMESPACE_ERR) if
            nsuri.nil? or
            (prefix == 'xml' and
             nsuri != 'http://www.w3.org/XML/1998/namespace')
        else
          raise DOMException.new(DOMException::NAMESPACE_ERR) if
            qname == 'xmlns' and
            nsuri != 'http://www.w3.org/2000/xmlns/'
        end
        attr = @ownerDocument.createAttributeNS(nsuri, qname)
        attr.appendChild(@ownerDocument.createTextNode(value))
        setAttributeNodeNS(attr)
      end

      ## [DOM2]
      def removeAttributeNS(nsuri, localname)
        ret = getAttributeNodeNS(nsuri, localname)
        removeAttributeNode(ret) if ret
      end

      ## [DOM2]
      def getAttributeNodeNS(nsuri, localname)
        attributes.each do |attr|
          return attr if
            attr.namespaceURI == nsuri && attr.localname == localname
        end
        nil
      end

      ## [DOM2]
      def setAttributeNodeNS(newAttr)
        ret = getAttributeNodeNS(newAttr.namespaceURI, newAttr.localname)
        removeAttributeNode(ret) if ret
        setAttributeNode(newAttr)
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
      def hasAttribute(name)
        !getAttributeNode(name).nil?
      end

      ## [DOM2]
      def hasAttributeNS(nsuri, localname)
        !getAttributeNodeNS(nsuri, localname).nil?
      end

      def idAttribute; @idAttribute; end
      def idAttribute=(name); @idAttribute = name; end

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
  end
end
