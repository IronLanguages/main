## -*- Ruby -*-
## SAX Driver for XML::Parser (experimental)
## 1999 by yoshidam
##
## Limitation:
##   * AttributeList#getType always returns 'CDATA'.
##   * DocumentHandler#ignorableWhitespace is never called.
##   * ErrorHandler#warning and ErrorHandler#error are never called.
##   * Locator#getLineNumber and Locator#getColumnNumber do not
##     return the proper value in DocumentHandler#characters method.
##   * Parser#setLocale is not implemented.
##   * Parser cannot parse non-local file.

require 'xml/parser'
require 'xml/sax'

module XML
  class Parser
    class SAXDriver
      include XML::SAX::Parser
      include XML::SAX::AttributeList
      include XML::SAX::Locator

      ## very simple URL parser
      class URL
        attr :scheme
        attr :login
        attr :urlpath

        def initialize(url, url2 = nil)
          @scheme = ''
          @login = ''
          @urlpath = ''
          if url.kind_of?(String) && url2.nil?
            if url =~ /^([a-z0-9\+\-\.]+):\/\/([^\/]+)(\/.*)$/
              @scheme, @login, @urlpath = $1, $2, $3
            else
              url = File::expand_path(url)
              @scheme, @login, @urlpath = "file", "localhost", url
            end
          elsif url.kind_of?(URL) && url2.kind_of?(String)
            if url2 =~ /^([a-z0-9\+\-\.]+):\/\/([^\/]+)(\/.*)$/
              @scheme, @login, @urlpath = $1, $2, $3
            else
              @scheme = url.scheme
              @login = url.login
              if url2 =~ /^\//
                @urlpath = url2
              else
                path = url.urlpath
                path =~ /^([^\#]+)\#?(.*)$/
                path = $1
                path =~ /^([^\?]+)\??(.*)$/
                path = $1
                path =~ /^(.+)\/(.*)/
                path = $1
                @urlpath = File.expand_path(path + '/' + url2)
              end
            end
          end
        end

        def to_s
          @scheme + "://" + @login + @urlpath
        end
      end

      ## All parser events are delegated to SAXDriver
      class SAXParser < XML::Parser
        include XML::SAX::Locator

        def SAXParser.new(saxdriver, *rest)
          obj = super(*rest)
          obj.setDriver(saxdriver)
          obj
        end

        def initialize(*args)
          super(*args)
          @publicId = nil
          @systemId = nil
          if self.respond_to?(:setParamEntityParsing)
            self.setParamEntityParsing(PARAM_ENTITY_PARSING_UNLESS_STANDALONE)
          end
        end

        def setDriver(saxdriver)
          @saxdriver = saxdriver
        end

        def parse(inputSource)
          @systemId = inputSource.getSystemId
          @saxdriver.pushLocator(self)
          setBase(@systemId)
          super(inputSource.getByteStream.read)
          @saxdriver.popLocator
        end

        def getPublicId
          @publicId
        end

        def getSystemId
          @systemId
        end

        def getLineNumber
          self.line
        end

        def getColumnNumber
          self.column
        end

        def startElement(name, attr)
          @saxdriver.startElement(name, attr)
        end

        def endElement(name)
          @saxdriver.endElement(name)
        end

        def character(data)
          @saxdriver.character(data)
        end

        def processingInstruction(target, data)
          @saxdriver.processingInstruction(target, data)
        end

        def notationDecl(name, base, sysid, pubid)
          @saxdriver.notationDecl(name, base, sysid, pubid)
        end

        def unparsedEntityDecl(name, base, sysid, pubid, notation)
          @saxdriver.unparsedEntityDecl(name, base, sysid, pubid, notation)
        end

        def comment(data)
        end

        def externalEntityRef(context, base, systemId, publicId)
          inputSource = @saxdriver.xmlOpen(base, systemId, publicId)
          encoding = inputSource.getEncoding
          if encoding
            parser = SAXParser.new(@saxdriver, self, context, encoding)
          else
            parser = SAXParser.new(@saxdriver, self, context)
          end
          parser.parse(inputSource)
          parser.done
        end
      end

      class DummyLocator
        include XML::SAX::Locator

        def initialize(systemId)
          @systemId = systemId
        end

        def getPublicId; nil end
        def getSystemId; @systemId end
        def getLineNumber; 1 end
        def getColumnNumber; 1 end
      end

      ## open stream if it is not opened
      def openInputStream(stream)
        if stream.getByteStream
          stream
        else stream.getSystemId
          url = URL.new(stream.getSystemId)
          if url.scheme == 'file' && url.login == 'localhost'
            s = open(url.urlpath)
            stream.setByteStream(s)
            return stream
          end
        end
        return nil
      end
      private :openInputStream

      def xmlOpen(base, systemId, publicId)
        if base.nil? || base == ""
          file = URL.new(systemId)
        else
          file = URL.new(URL.new(base), systemId)
        end
        if !@entityResolver.nil?
          stream = @entityResolver.resolveEntity(file.to_s, publicId)
          return openInputStream(stream) if stream
        end
        if file.scheme == 'file' && file.login == 'localhost'
          stream = open(file.urlpath)
          is = XML::SAX::InputSource.new(stream)
          is.setSystemId(file.to_s)
          is.setPublicId(publicId)
          return is
        end
      end

      def initialize
        handler = XML::SAX::HandlerBase.new
        @attributes = nil
        @documentHandler = handler
        @dtdHandler = handler
        @errorHandler = handler
        @entityResolver = handler
        @dataBuf = ''
        @locators = []
      end

      ## implementation of Parser
      def setEntityResolver(handler)
        if !handler.kind_of?(XML::SAX::EntityResolver)
          raise TypeError.new("parameter error")
        end
        @entityResolver = handler
      end

      ## implementation of Parser
      def setDocumentHandler(handler)
        if !handler.kind_of?(XML::SAX::DocumentHandler)
          raise TypeError.new("parameter error")
        end
        @documentHandler = handler
      end

      ## implementation of Parser
      def setDTDHandler(handler)
        if !handler.kind_of?(XML::SAX::DTDHandler)
          raise TypeError.new("parameter error")
        end
        @dtdHandler = handler
      end

      ## implementation of Parser
      def setErrorHandler(handler)
        if !handler.kind_of?(XML::SAX::ErrorHandler)
          raise TypeError.new("parameter error")
        end
        @errorHandler = handler
      end

      ## implementation of Parser
      def setLocale(locale)
        raise SAXException.new("locale not supported")
      end

      def flushData
        if @dataBuf.length > 0
          @documentHandler.characters(@dataBuf, 0, @dataBuf.length)
          @dataBuf = ''
        end
      end
      private :flushData

      def startElement(name, attrs)
        flushData;
        @attributes = attrs
        @documentHandler.startElement(name, self)
      end

      def character(data)
        @dataBuf << data
      end

      def endElement(name)
        flushData;
        @documentHandler.endElement(name)
      end

      def processingInstruction(target, data)
        flushData;
        @documentHandler.processingInstruction(target, data)
      end

      def notationDecl(name, base, sysid, pubid)
        @dtdHandler.notationDecl(name, pubid, sysid)
      end

      def unparsedEntityDecl(name, base, sysid, pubid, notation)
        @dtdHandler.unparsedEntityDecl(name, pubid, sysid, notation)
      end

      ## implementation of AttributeList
      def getLength
        @attributes.length
      end

      ## implementation of AttributeList
      def getName(pos)
        @attributes.keys[pos]
      end

      ## implementation of AttributeList
      def getValue(pos)
        if pos.kind_of?(String)
          @attributes[pos]
        else
          @attributes.values[pos]
        end
      end

      ## implementation of AttributeList
      def getType(pos)
        ## expat cannot get attribyte type
        return "CDATA"
      end

      ## locator is DummyLoacator or SAXParser
      def pushLocator(locator)
        @locators.push(locator)
      end

      def popLocator
        @locators.pop
      end

      ## implementation of Locator
      def getPublicId
        @locators[-1].getPublicId
      end

      ## implementation of Locator
      def getSystemId
        @locators[-1].getSystemId
      end

      ## implementation of Locator
      def getLineNumber
        @locators[-1].getLineNumber
      end

      ## implementation of Locator
      def getColumnNumber
        @locators[-1].getColumnNumber
      end

      ## implementation of Parser
      def parse(sysid)
        @documentHandler.setDocumentLocator(self)
        if sysid.kind_of?(XML::SAX::InputSource)
          inputSource = openInputStream(sysid.dup)
        else
          inputSource = openInputStream(XML::SAX::InputSource.new(sysid))
        end
        encoding = inputSource.getEncoding
        if encoding
          parser = SAXParser.new(self, encoding)
        else
          parser = SAXParser.new(self)
        end

        pushLocator(DummyLocator.new(inputSource.getSystemId))
        begin
          @documentHandler.startDocument
          parser.parse(inputSource)
          @documentHandler.endDocument
        rescue XML::Parser::Error
          @errorHandler.fatalError(XML::SAX::SAXParseException.new($!.to_s,
                                                                   self))
        rescue
          @errorHandler.fatalError($!)
        end
      end
    end
  end
end
