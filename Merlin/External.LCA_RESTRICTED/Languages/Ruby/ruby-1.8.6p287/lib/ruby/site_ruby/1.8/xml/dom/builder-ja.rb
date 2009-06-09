## -*- Ruby -*-
## Tree builder class for Japanese encoding
## 1998 by yoshidam

require 'xml/dom/builder'

module XML
module DOM
  class JapaneseBuilder<Builder
    require 'kconv'
    include Kconv
    require 'uconv'
    include Uconv

    def nameConverter(str)
      u8toeuc(str)
    end
    def cdataConverter(str)
      u8toeuc(str)
    end

    def parseStream(stream, trim = false)
      ## empty file
      if ((xml = stream.gets).nil?); exit 1; end
      ## rewrite encoding in XML decl.
      if xml =~ /^<\?xml\sversion=.+\sencoding=.EUC-JP./i
        xml.sub!(/EUC-JP/i, "UTF-8")
        encoding = 'EUC-JP'
      elsif xml =~ /^<\?xml\sversion=.+\sencoding=.Shift_JIS./i
        xml.sub!(/Shift_JIS/i, "UTF-8")
        encoding = "Shift_JIS"
      elsif xml =~ /^<\?xml\sversion=.+\sencoding=.ISO-2022-JP./i
        xml.sub!(/ISO-2022-JP/i, "UTF-8")
        encoding = "ISO-2022-JP"
      end

      ## read body
      xml += String(stream.read)

      ## convert body encoding
      if encoding == "EUC-JP"
        xml = euctou8(xml)
      elsif encoding == "Shift_JIS"
        xml = euctou8(kconv(xml, EUC, SJIS))
      elsif encoding == "ISO-2022-JP"
        xml = euctou8(kconv(xml, EUC, JIS))
      end

      return parse(xml, trim)
    end


    def Uconv.unknown_unicode_handler(u)
      return '®'
    end
  end
end
end
