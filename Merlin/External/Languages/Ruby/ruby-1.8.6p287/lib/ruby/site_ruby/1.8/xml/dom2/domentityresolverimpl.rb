## -*- Ruby -*-
## XML::DOM::DOMEntityResolverImpl
## 2001 by yoshidam
##

require 'xml/dom2/domentityresolver'
require 'xml/dom2/dominputsource'

module XML
  module DOM
    class DOMEntityResolverImpl
      include DOMEntityResolver

      ## replace 'open' by WGET::open
      begin
        require 'wget'
##        include WGET
      rescue
        ## ignore
      end

      ## DOMInputSource resolveEntity(publicId, systemId)
      def resolveEntity(publicId, systemId)
        ret = DOMInputSource.new
        ret.publicId = publicId
        ret.systemId = systemId
        if systemId =~ /file:/
          ret.byteStream = open(systemId.sub('^file://', ''))
        else
          ret.byteStream = WGET::open(systemId)
        end
        ret
      end
    end
  end
end

