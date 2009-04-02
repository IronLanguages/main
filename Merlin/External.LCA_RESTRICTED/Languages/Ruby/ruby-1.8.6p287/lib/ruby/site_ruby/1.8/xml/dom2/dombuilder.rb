## -*- Ruby -*-
## XML::DOM::DOMBuilder
## 1999-2001 by yoshidam
##
##
##   builder = XML::DOM::DOMImplementation.instance.createDOMBuilder
##   doc = builder.parseURI("http://hoge/hoge.xml")
##   inputSource = XML::DOM::InputSource.new("http://hoge/hoge.xml")
##   doc = builder.parseDOMInputSource(inputSource)
##
##

require 'xml/parserns'
require 'xml/dom2/document'
require 'xml/dom2/domentityresolverimpl'

=begin
= XML::DOM::DOMBuilder

=end
module XML
  module DOM

=begin
== Class XML::DOM::DOMBuilder

=== superclass
XML::Parser

=end
    class DOMBuilder<XML::ParserNS
      NSSEP = '!'

      attr :createCDATASection, true
      attr :createEntityReference, true

=begin
=== Class Methods

    --- DOM::DOMBuilder.new(document = nil, *args)

Constructor of DOM builder.

usage:
parser = XML::DOM::DOMBuilder.new(document)

=end
      ## new(document = nil, encoding = nil)
      ## new(document = nil, parser, context, encoding = nil)

      def self.new(document = nil, *args)
        document ||= Document.new
        if args[0].is_a?(self.class)
          ret = super(*args)
          ret.__initialize__(document, true)
        else
          ret = super(args[0], NSSEP)
          ret.__initialize__(document, false)
        end
        ret.setReturnNSTriplet(true)
        ret
      end

      def __initialize__(document, external = false)
        @tree = nil
        @entityResolver = DOMEntityResolverImpl.new
        @createCDATASection = false
        @createEntityReference = false
        @document = document
        @external = external
      end


=begin
=== Methods
    --- DOMBuilder#parse(xml, parse_ext = false)

parse string or stream of XML contents.

  xml:       string or stream of XML contents
  parse_ext: flag whether parse external entities or not

ex. doctree = parser.parse(xml, parse_ext)

=end
      ## Parse
      ##   doctree = parser.parse(xml, parse_ext)
      ##     xml:       string or stream of XML contents
      ##     parse_ext: flag whether parse external entities or not
      def parse(xml, parse_ext = false)
        if @external
          @tree = @document.createDocumentFragment
        else
          @tree = @document
        end
        @parse_ext = parse_ext
        @current = @tree
        @inDocDecl = 0
        @decl = ""
        @inDecl = 0
        @idRest = 0
        @extID = nil
        @cdata_f = false
        @cdata_buf = ''
        @nsdecl = []
        super(xml)
        @tree
      end

      def parseURI(uri)
        uri =~ /^((\w+):\/\/.+\/).*$/ ## /
        setBase($1) if $1
        xml = @entityResolver.resolveEntity(nil, uri).byteStream.read
        parse(xml, true)
      end

      def text
        return if @cdata_buf == ''
        textnode = @document.createTextNode(@cdata_buf)
        @current.appendChild(textnode)
        @cdata_buf = ''
      end

      def startElement(name, data)
        text
        if !name.index(NSSEP)
          qname = name
          uri = nil
        else
          uri, localname, prefix = name.split(NSSEP)
          if prefix.nil?
            qname = localname
          else
            qname = prefix + ':' + localname
          end
        end
        elem = @document.createElementNS(uri, qname)

        @nsdecl.each do |nsdecl|
          elem.setAttributeNode(nsdecl)
        end
        @nsdecl = []

        attr = {}
        specified = getSpecifiedAttributes
        ## not implemented
        ## elem.idAttribute = getIdAttribute

        data.each do |key, value|
          if !key.index(NSSEP)
            qname = key
            uri = nil
          else
            uri, localname, prefix = key.split(NSSEP)
            if prefix.nil?
              qname = localname
            else
              qname = prefix + ':' + localname
            end
          end
          attr = @document.createAttributeNS(uri, qname)
          attr.appendChild(@document.createTextNode(value))
##          attr.specified = specified[key]
          attr.specified = specified.include?(key)
          elem.setAttributeNode(attr)
        end

        @current.appendChild(elem)
        @current = elem
      end

      def endElement(name)
        text
        @current = @current.parentNode
      end

      def character(data)
##        if @cdata_f
          @cdata_buf << data
##        else
##          cdata = @document.createTextNode(data)
##          @current.appendChild(cdata)
##        end
      end

      def processingInstruction(name, data)
        text
        pi = @document.createProcessingInstruction(name, data)
        @current.appendChild(pi)
      end

      def externalEntityRef(context, base, systemId, publicId)
        text
        tree = nil
        if @parse_ext
          extp = self.class.new(@document, self, context)
          extp.setBase(base) if base
          file = systemId
          if systemId !~ /^\/|^\.|^http:|^ftp:/ && !base.nil? # /
            file = base + systemId
          end
          begin
            xml = @entityResolver.resolveEntity(nil, file).byteStream.read
            tree = extp.parse(xml, @parse_ext)
          rescue XML::Parser::Error
            raise XML::Parser::Error.new("#{systemId}(#{extp.line}): #{$!}")
          rescue Errno::ENOENT
            raise
          end
          extp.done
        end
        if @createEntityReference
          entref = @document.createEntityReference(context)
          @current.appendChild(entref)
          entref.appendChild(tree) if tree
        else
          @current.appendChild(tree) if tree
        end
      end

      def startCdata
        return unless @createCDATASection
        text
        @cdata_f = true
##        @cdata_buf = ''
      end

      def endCdata
        return unless @createCDATASection
        cdata = @document.createCDATASection(@cdata_buf)
        @current.appendChild(cdata)
        @cdata_buf = ''
        @cdata_f = false
      end

      def comment(data)
        text
        comment = @document.createComment(data)
        @current.appendChild(comment)
      end

      def startDoctypeDecl(name, pubid, sysid, internal_subset)
        doctype = @document.implementation.createDocumentType(name,
                                                              pubid, sysid)
        @current.appendChild(doctype)
      end

      def startNamespaceDecl(prefix, uri)
        qname = 'xmlns'
        if prefix
          qname << ':' + prefix
        end
        attr = @document.createAttributeNS(nil, qname)
        attr.appendChild(@document.createTextNode(uri))
        attr.specified = true
        @nsdecl << attr
      end

##      def endNamespaceDecl(prefix, uri)
##      end

##      def defaultHandler(data)
##      end



      ## [DOM3?]
      def entityResolver; @entityResolver; end
      def entityResolver=(resolver)
        raise ArgumentError, 'invalid value for DOMEntityResolver' unless
          resolver.is_a?(DOMEntityResolver)
        @entityResolver = resolver
      end

    end
  end
end
