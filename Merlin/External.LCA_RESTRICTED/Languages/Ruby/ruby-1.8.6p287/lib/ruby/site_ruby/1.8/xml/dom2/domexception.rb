## -*- Ruby -*-
## XML::DOM
## 1998-2001 by yoshidam
##

module XML
  module DOM
=begin

== Class XML::DOM::DOMException

=== superclass
Exception

DOM exception.
=end

    class DOMException<Exception
      INDEX_SIZE_ERR = 1
      WSTRING_SIZE_ERR = 2
      HIERARCHY_REQUEST_ERR  = 3
      WRONG_DOCUMENT_ERR = 4
      INVALID_NAME_ERR = 5
      NO_DATA_ALLOWED_ERR = 6
      NO_MODIFICATION_ALLOWED_ERR = 7
      NOT_FOUND_ERR = 8
      NOT_SUPPORTED_ERR = 9
      INUSE_ATTRIBUTE_ERR = 10

      ## [DOM2]
      INVALID_STATE_ERR = 11
      SYNTAX_ERR = 12
      INVALID_MODIFICATION_ERR = 13
      NAMESPACE_ERR = 14
      INVALIUD_ACCESS_ERR = 14

      ERRMSG = [
        "no error",

        "index size",
        "wstring size",
        "hierarchy request",
        "wrong document",
        "invalid name",
        "no data allowed",
        "no modification allowed",
        "not found",
        "not supported",
        "inuse attribute",

        ## [DOM2]
        "invalid state",
        "syntax error",
        "invalid modification",
        "namescape erorr",
        "invaliud access"
      ]

=begin
=== Class Methods

    --- DOMException.new(code = 0)

generate DOM exception.
=end

      def initialize(code = 0)
        @code = code
      end

=begin
=== Methods

    --- DOMException#code()

return code of exception.

=end
      def code
        @code
      end

=begin

    --- DOMException#to_s()

return the string representation of the error.

=end
      def to_s
        ERRMSG[@code].dup
      end
    end
  end
end
