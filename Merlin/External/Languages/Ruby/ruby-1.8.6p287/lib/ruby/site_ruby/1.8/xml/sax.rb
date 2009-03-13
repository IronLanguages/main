## -*- Ruby -*-
## SAX (Simple API for XML) 1.0 for Ruby (experimental)
## 1999 by yoshidam
##
## SAX information: http://www.megginson.com/SAX/
##

module XML
  module SAX
    module AttributeList
      def getLength
        raise "not implemented"
      end

      def getName(pos)
        raise "not implemented"
      end

      def getType(pos_or_name)
        raise "not implemented"
      end

      def getValue(pos_or_name)
        raise "not implemented"
      end
    end

    module DTDHandler
      def notationDecl(name, pubid, sysid)
        raise "not implemented"
      end

      def unparsedEntityDecl(name, pubid, sysid, notation)
        raise "not implemented"
      end
    end

    module DocumentHandler
      def setDocumentLocator(locator)
        raise "not implemented"
      end

      def startDocument
        raise "not implemented"
      end

      def endDocument()
        raise "not implemented"
      end

      def startElement(name, atts)
        raise "not implemented"
      end

      def endElement(name)
        raise "not implemented"
      end

      def characters(ch, start, length)
        raise "not implemented"
      end

      def ignorableWhitespace(ch, start, length)
        raise "not implemented"
      end

      def processingInstruction(target, data)
        raise "not implemented"
      end
    end

    module EntityResolver
      def resolveEntity(pubid, sysid)
        raise "not implemented"
      end
    end

    module ErrorHandler
      def warning(e)
        raise "not implemented"
      end

      def error(e)
        raise "not implemented"
      end

      def fatalError(e)
        raise "not implemented"
      end
    end

    module Locator
      def getPublicId
        raise "not implemented"
      end

      def getSystemId
        raise "not implemented"
      end

      def getLineNumber
        raise "not implemented"
      end

      def getColumnNumber
        raise "not implemented"
      end
    end

    module Parser
      def setLocale(locale)
        raise "not implemented"
      end

      def setEntityResolver(resolver)
        raise "not implemented"
      end

      def setDTDHandler(handler)
        raise "not implemented"
      end

      def setDocumentHandler(handler)
        raise "not implemented"
      end

      def setErrorHandler
        raise "not implemented"
      end

      def parse(source_or_sysid)
        raise "not implemented"
      end
    end

    class HandlerBase
      include EntityResolver
      include DTDHandler
      include DocumentHandler
      include ErrorHandler

      def resolveEntity(pubid, sysid)
        nil
      end

      def notationDecl(name, pubid, sysid)
      end

      def unparsedEntityDecl(name, pubid, sysid, natation)
      end

      def setDocumentLocator(locator)
      end

      def startDocument
      end

      def endDocument
      end

      def startElement(name, atts)
      end

      def endElement(name)
      end

      def characters(ch, start, length)
      end

      def ignorableWhitespace(ch, sart, length)
      end

      def processingInstruction(target, data)
      end

      def warning(e)
      end

      def error(e)
      end

      def fatalError(e)
        raise e
      end
    end

    class InputSource
      def initialize(sysid)
        @publicId = nil
        @systemId = nil
        @stream = nil
        @encoding = nil

        if sysid.kind_of?(String)
          setSystemId(sysid)
        elsif !sysid.nil?
          setByteStream(sysid)
        end
      end

      def setPublicId(pubid)
        @publicId = pubid
      end

      def getPublicId
        @publicId
      end

      def setSystemId(sysid)
        @systemId = sysid
      end

      def getSystemId
        @systemId
      end

      def setByteStream(stream)
        @stream = stream
      end

      def getByteStream
        @stream
      end

      def setEncoding(encoding)
        @encoding = encoding
      end

      def getEncoding
        @encoding
      end

      def setCharacterStream(stream)
        raise "not implemented"
      end

      def getCharacterStream
        raise "not implemented"
      end
    end

    class SAXException < Exception
      ## initialize(String)
      ## initialize(Exception)
      ## initialize(String, Exception)
      def initialize(message, e = nil)
        @message = nil
        @exception = nil
        if message.kind_of?(String) && e.nil?
          @message = message
        elsif message.kind_of?(Exception) && e.nil?
          @exception = e
        elsif message.kind_of?(String) && e.kind_of?(Exception)
          @message = message
          @exception = e
        else
          raise TypeError.new("parameter error")
        end
      end

      def getMessage
        if @message.nil? && !@exception.nil?
          return @exception.to_s
        end
        @message
      end

      def getException
        @exception
      end

      def toString
        getMessage
      end
      alias to_s toString
    end

    class SAXParseException < SAXException
      ## initialize(String, Locator)
      ## initialize(String, Locator, Exception)
      ## initialize(String, String, String, Fixnum, Fixnum)
      ## initialize(String, String, String, Fixnum, Fixnum, Exception)
      def initialize(message, pubid = nil, sysid = nil,
                     line = nil, column = nil, e = nil)
        @publicId = nil
        @systemiId = nil
        @lineNumber = nil
        @columnNumber = nil
        if message.kind_of?(String) && pubid.kind_of?(Locator) &&
            sysid.nil? && line.nil? && column.nil? && e.nil?
          super(message)
          @publicId = pubid.getPublicId
          @systemId = pubid.getSystemId
          @lineNumber = pubid.getLineNumber
          @columnNumber = pubid.getColumnNumber
        elsif message.kind_of?(String) && pubid.kind_of?(Locator) &&
            sysid.kind_of?(Exception) && line.nil? && column.nil? && e.nil?
          super(message, sysid)
          @publicId = pubid.getPublicId
          @systemId = pubid.getSystemId
          @lineNumber = pubid.getLineNumber
          @columnNumber = pubid.getColumnNumber
        elsif message.kind_of?(String) && pubid.kind_of?(String) &&
            sysid.kind_of?(String) && line.kind_of?(Fixnum) &&
            column.kind_of?(Fixnum) && e.nil?
          super(message)
          @publicId = pubid
          @systemId = sysid
          @lineNumber = line
          @columnNumber = column
        elsif message.kind_of?(String) && pubid.kind_of?(String) &&
            sysid.kind_of?(String) && line.kind_of?(Fixnum) &&
            column.kind_of?(Fixnum) && e.kind_of?(Exception)
          super(message, e)
          @publicId = pubid
          @systemId = sysid
          @lineNumber = line
          @columnNumber = column
        else
          raise TypeError.new("parameter error")
        end
      end

      def getPublicId
        @publicId
      end

      def getSystemId
        @systemId
      end

      def getLineNumber
        @lineNumber
      end

      def getColumnNumber
        @columnNumber
      end
    end

    module Helpers
      module ParserFactory
        def ParserFactory::makeParser(klass)
          if klass.kind_of?(Class)
            klass.new
          elsif klass.kind_of?(String)
            eval(klass).new
          end
        end
      end
    end
  end
end
