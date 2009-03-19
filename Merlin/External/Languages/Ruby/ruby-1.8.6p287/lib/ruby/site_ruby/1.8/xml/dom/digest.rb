## -*- Ruby -*-
## DOMHASH test implementation
## 1999 by yoshidam
##
## Apr 20, 1999 Change for draft-hiroshi-dom-hash-01.txt
##

require 'xml/dom/core'
require 'md5'
require 'uconv'

module XML
  module DOM

    def self.tou16(str)
      Uconv.u16swap(Uconv.u8tou16(str))
    end

    class Node
      def getDigest(force = false)
        nil
      end
    end

    class Text
      def getDigest(force = false)
        (!force && @digest) ||
          @digest = MD5.new([TEXT_NODE].pack("N") + DOM.tou16(nodeValue)).digest
      end
    end

##    class Comment
##      def getDigest(force = false)
##        (!force && @digest) ||
##          @digest = MD5.new([COMMENT_NODE].pack("N") + DOM.tou16(data)).digest
##      end
##    end

    class ProcessingInstruction
      def getDigest(force = false)
        (!force && @digest) ||
          @digest = MD5.new([PROCESSING_INSTRUCTION_NODE].pack("N") +
                            DOM.tou16(target) + "\0\0" + DOM.tou16(data)).digest
      end
    end

    class Attr
      def getDigest(force = false)
        (!force && @digest) ||
          @digest = MD5.new([ATTRIBUTE_NODE].pack("N") +
                            DOM.tou16(nodeName) + "\0\0" + DOM.tou16(nodeValue)).digest
      end
    end

    class NamedNodeMap
      include Enumerable
    end

    class NodeList
      include Enumerable
    end

    class Element
      def getDigest(force = false)
        return @digest if (!force && @digest)
        attr = attributes
        children = childNodes
        attr_digests = ""
        children_digests = ""
        if attr
          attr_array = attr.sort {|a, b|
            DOM.tou16(a.nodeName) <=> DOM.tou16(b.nodeName)}
          attr_array.each {|a|
            attr_digests << a.getDigest(force)
          }
        end
        children_num = 0
        children.each {|c|
          next if c.nodeType == COMMENT_NODE
          children_num += 1
          children_digests << c.getDigest(force)
        }
        @digest = MD5.new([ELEMENT_NODE].pack("N") +
                          DOM.tou16(nodeName) +
                          "\0\0" +
                          [attr.length].pack("N") +
                          attr_digests +
                          [children_num].pack("N") +
                          children_digests).digest
      end
    end

  end
end
