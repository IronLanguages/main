## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

require 'xml/dom2/node'
require 'xml/dom2/domexception'

module XML
  module DOM

=begin
== Class XML::DOM::CharacterData

=== superclass
Node

=end
    class CharacterData<Node

=begin
=== Class Methods

    --- CharacterData.new(text)

creates a new CharacterData.
=end
      ## new(text)
      ##     text: String
      def initialize(text = nil)
        super()
        raise "parameter error" if !text
        @value = text
      end

=begin
=== Methods

    --- CharacterData#data()

[DOM]
returns the character data of the node.
=end
      ## [DOM]
      def data
        @value.dup
      end

=begin
    --- CharacterData#data=(p)

[DOM]
set the character data of the node.
=end
      ## [DOM]
      def data=(p)
        @value = p
      end

=begin
    --- CharacterData#length()

[DOM]
returns length of this CharacterData.
=end
      ## [DOM]
      def length
        @value.length
      end

=begin
    --- CharacterData#substringData(start, count)

[DOM]
extracts a range of data from the node.
=end
      ## [DOM]
      def substringData(start, count)
        if start < 0 || start > @value.length || count < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        ## if the sum of start and count > length,
        ##  return all characters to the end of the value.
        @value[start, count]
      end

=begin
    --- CharacterData#appendData(str)

[DOM]
append the string to the end of the character data.
=end
      ## [DOM]
      def appendData(str)
        @value << str
      end

=begin
    --- CharacterData#insertData(offset, str)

[DOM]
insert a string at the specified character offset.
=end
      ## [DOM]
      def insertData(offset, str)
        if offset < 0 || offset > @value.length
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        @value[offset, 0] = str
      end

=begin
    --- CharacterData#deleteData(offset, count)

[DOM]
removes a range of characters from the node.
=end
      ## [DOM]
      def deleteData(offset, count)
        if offset < 0 || offset > @value.length || count < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        @value[offset, count] = ''
      end

=begin
    --- CharacterData#replaceData(offset, count, str)

[DOM]
replaces the characters starting at the specified character offset
with specified string.
=end
      ## [DOM]
      def replaceData(offset, count, str)
        if offset < 0 || offset > @value.length || count < 0
          raise DOMException.new(DOMException::INDEX_SIZE_ERR)
        end
        @value[offset, count] = str
      end

=begin
    --- CharacterData#cloneData(deep = true)

[DOM]
returns the copy of the CharacterData.
=end
      ## [DOM]
      def cloneNode(deep = true)
        super(deep, @value.dup)
      end

=begin
    --- CharacterData#nodeValue

[DOM]
return nodevalue.

=end
      ## [DOM]
      def nodeValue
        @value
      end

=begin
    --- CharacterData#nodeValue=(p)

[DOM]
set nodevalue as p.
=end
      ## [DOM]
      def nodeValue=(p)
        @value = p
      end

    end
  end
end
