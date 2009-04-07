## -*- Ruby -*-
## XML::DOM::Builder
## 1999 by yoshidam

require 'xml/parser'
require 'xml/dom/core'

=begin
= XML::DOM::Builder

== Module XML

=end
module XML
module DOM

=begin
== Class XML::DOM::Builder (XML::SimpleTreeBuilder)

=== superclass
XML::Parser

=end
  class Builder<Parser
    include XML::DOM

    attr :createCDATASection, true
    attr :createEntityReference, true

    ## replace 'open' by WGET::open
    begin
      require 'wget'
      include WGET
    rescue
      ## ignore
    end

=begin
=== Class Methods

    --- DOM::Builder.new(level = 0, *args)

Constructor of DOM builder.

usage:
parser = XML::SimpleTreeBuilder.new(level)

  level: 0 -- ignore default events (defualt)
         1 -- catch default events and create the Comment,
              the EntityReference, the XML declaration (as PI) and
              the non-DOM-compliant DocumentType nodes.

=end

    def self.new(level = 0, *args)
      document = Document.new
      ret = super(*args)
      external = false
      external = true if args[0].is_a?(SimpleTreeBuilder)
      ret.__initialize__(level, document, external)
      ret
    end

    ## Constructor
    ##  parser = XML::SimpleTreeBuilder.new(level)
    ##    level: 0 -- ignore default events (defualt)
    ##           1 -- catch default events and create the Comment,
    ##                the EntityReference, the XML declaration (as PI) and
    ##                the non-DOM-compliant DocumentType nodes.
    def __initialize__(level, document, external)
      @tree = nil
      @level = level
      @document = document
      @external = external
      @createCDATASection = false
      @createEntityReference = false
      if @level > 0
        @createCDATASection = true
        @createEntityReference = true
        def self.default(data); defaultHandler(data); end
      end
    end

=begin
=== Methods

    --- Builder#nameConverter(str)

User redefinable name encoding converter

=end
    ## User redefinable name encoding converter
    def nameConverter(str)
      str
    end

=begin
    --- Builder#cdataConverter(str)

User redefinable cdata encoding converter

=end
    ## User redefinable cdata encoding converter
    def cdataConverter(str)
      str
    end

=begin
=== Methods
    --- Builder#parse(xml, parse_ext = false)

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
      super(xml)
      @tree
    end

    def text
      return if @cdata_buf == ''
      textnode = @document.createTextNode(cdataConverter(@cdata_buf))
      @current.appendChild(textnode)
      @cdata_buf = ''
    end
  
    def startElement(name, data)
      text
      elem = @document.createElement(nameConverter(name))
      data.each do |key, value|
        attr = @document.createAttribute(nameConverter(key))
        attr.appendChild(@document.createTextNode(cdataConverter(value)))
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
      @cdata_buf << data
     end

    def processingInstruction(name, data)
      text
      pi = @document.createProcessingInstruction(nameConverter(name),
                                                 cdataConverter(data))
      ## PI data should not be converted
      @current.appendChild(pi)
    end

    def externalEntityRef(context, base, systemId, publicId)
      text
      tree = nil
      if @parse_ext
        extp = self.class.new(@level, self, context)
        extp.setBase(base) if base
        file = systemId
        if systemId !~ /^\/|^\.|^http:|^ftp:/ && !base.nil?
          file = base + systemId
        end
        begin
          tree = extp.parse(open(file).read, @parse_ext)
        rescue XML::ParserError
          raise XML::ParserError.new("#{systemId}(#{extp.line}): #{$!}")
        rescue Errno::ENOENT
          raise Errno::ENOENT.new("#{$!}")
        end
        extp.done
      end
      if @createEntityReference
        entref = @document.createEntityReference(nameConverter(context))
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
    end

    def endCdata
      return unless @createCDATASection
      cdata = @document.createCDATASection(cdataConverter(@cdata_buf))
      @current.appendChild(cdata)
      @cdata_buf = ''
      @cdata_f = false
    end

    def comment(data)
      text
      comment = @document.createComment(cdataConverter(data))
      ## Comment should not be converted
      @current.appendChild(comment)
    end

    def defaultHandler(data)
      if data =~ /^\&(.+);$/
        eref = @document.createEntityReference(nameConverter($1))
        @current.appendChild(eref)
      elsif data =~ /^<\?xml\s*([\s\S]*)\?>$/
        ## XML declaration should not be a PI.
        pi = @document.createProcessingInstruction("xml",
                                       cdataConverter($1))
        @current.appendChild(pi)
      elsif @inDocDecl == 0 && data =~ /^<\!DOCTYPE$/
        @inDocDecl = 1
        @inDecl = 0
        @idRest = 0
        @extID = nil
      elsif @inDocDecl == 1
        if data == "["
          @inDocDecl = 2
        elsif data == ">"
          if !@extID.nil?
##            @current.nodeValue = @extID
          end
          @inDocDecl = 0
##          @current = @current.parentNode
        elsif data == "SYSTEM"
          @idRest = 1
          @extID = data
        elsif data == "PUBLIC"
          @idRest = 2
          @extID = data
        elsif data !~ /^\s+$/
          if @idRest > 0
            ## SysID or PubID
            @extID <<= " " + data
            @idRest -= 1
          else
            ## Root Element Type
            docType = data
##            doctype = DocumentType.new(nameConverter(docType))
##            @current.appendChild(doctype)
##            @current = doctype
          end
        end
      elsif @inDocDecl == 2
        if @inDecl == 0
          if data == "]"
            @inDocDecl = 1
          elsif data =~ /^<\!/
            @decl = data
            @inDecl = 1
          elsif data =~ /^%(.+);$/
            ## PERef
##            cdata = @document.createTextNode(nameConverter(data))
##            @current.appendChild(cdata)
          else
            ## WHITESPCAE
          end
        else ## inDecl == 1
          if data == ">"
            @decl <<= data
            @inDecl = 0
            ## Markup Decl
##            cdata = @document.createTextNode(cdataConverter(@decl))
            ## Markup decl should not be converted
##            @current.appendChild(cdata)
          elsif data =~ /^\s+$/
            ## WHITESPACE
            @decl << " "
          else
            @decl << data
          end
        end
      else
        ## maybe WHITESPACE
##        cdata = @document.createTextNode(cdataConverter(data))
##        @current.appendChild(cdata)
      end
    end

  end
  end

  SimpleTreeBuilder = DOM::Builder
end
