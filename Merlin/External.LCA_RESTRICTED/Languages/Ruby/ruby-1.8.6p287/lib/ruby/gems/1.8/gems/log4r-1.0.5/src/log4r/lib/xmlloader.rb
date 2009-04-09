#:nodoc:
module Log4r
  begin
    require 'rexml/document'
    HAVE_REXML = true
  rescue LoadError
    HAVE_REXML = false
  end
end

if Log4r::HAVE_REXML
  module REXML #:nodoc: all
    class Element
      def value_of(elmt)
        val = attributes[elmt]
        if val.nil?
          sub = elements[elmt]
          val = sub.text unless sub.nil?
        end
        val
      end
    end
  end
end
