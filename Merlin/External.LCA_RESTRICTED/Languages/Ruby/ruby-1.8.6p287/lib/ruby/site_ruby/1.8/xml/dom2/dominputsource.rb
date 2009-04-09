## -*- Ruby -*-
## XML::DOM::DOMEntityResolver
## 2001 by yoshidam
##

module XML
  module DOM
    class DOMInputSource
      def initialize
        @bstream = nil
        @cstream = nil
        @encoding = nil
        @pubid = nil
        @sysid = nil
      end

      def byteStream; @bstream; end
      def byteStream=(stream); @bstream = stream; end
      def characterStream; @cstream;  end
      def characterStream=(stream); @cstream = stream; end
      def encoding; @encoding; end
      def encoding=(enc);@encoding = enc; end
      def publicId; @pubid; end
      def publicId=(pubid); @pubid = pubid; end
      def systemId; @sysid; end
      def systemId=(sysid); @sysid = sysid; end
    end
  end
end
