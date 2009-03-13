## -*- Ruby -*-
## Sample XMLEncoding class for Japanese (EUC-JP, Shift_JIS)
## 1998 by yoshidam
##
## Usage:
##    require 'xml/encoding-ja'
##    include XML::Encoding_ja

module XML
module Encoding_ja
  require 'xml/parser'
  require 'uconv'

  class EUCHandler<XML::Encoding
    def map(i)
      return i if i < 128
      return -1 if i < 160 or i == 255
      return -2 
    end
    def convert(s)
      Uconv.euctou2(s)
    end
  end

  class SJISHandler<XML::Encoding
    def map(i)
      return i if i < 128
      return -2 
    end
    def convert(s)
      Uconv.sjistou2(s)
    end
  end

  def unknownEncoding(name)
    return EUCHandler.new if name =~ /^euc-jp$/i
    return SJISHandler.new if name =~ /^shift_jis$/i
    nil
  end

end
end
