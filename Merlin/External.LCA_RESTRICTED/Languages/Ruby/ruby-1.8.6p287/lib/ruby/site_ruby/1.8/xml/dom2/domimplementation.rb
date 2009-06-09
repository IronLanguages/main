## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##                     

require 'xml/dom2/domexception'
require 'xml/dom2/dombuilder'
require 'xml/dom2/documenttype'
require 'singleton'

module XML
  module DOM

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
        if (feature =~ /^XML$/i || feature =~ /^Core$/i) &&
            (version.nil? || version == "1.0" || version == "2.0")
          return true
        end
        false
      end

      ## [DOM2]
      def createDocumentType(qname, pubid, sysid)
        DocumentType.new(qname, pubid, sysid)
      end

      ## [DOM2]
      def createDocument(nsuri, qname, doctype)
        raise DOMException.new(DOMException::WRONG_DOCUMENT_ERR) if
          doctype && doctype.ownerDocument
        doc = Document.new
        if doctype
          doc.appendChild(doctype)
          doctype.ownerDocument = doc
        end
        elem = doc.createElementNS(nsuri, qname)
        doc.appendChild(elem)
        doc.implementation = self
        doc
      end

      ## [DOM3?]
      def createDOMBuilder
        XML::DOM::DOMBuilder.new(Document.new)
      end
    end
  end
end
