## -*- Ruby -*-
## XML::DOM::Visitor
## 1998 by yoshidam
##
## Oct 23, 1998 yoshidam Fix each
##


=begin
= XML::DOM::Visitor

== Module XML

=end
module XML

=begin
== Module XML::DOM (XML::SimpleTree)

=end
  module DOM

=begin
== Class XML::DOM::Visitor

Skelton class of Visitor.

You can override the following methods and implement the other
"visit_TYPE" methods.

You should implement some "visit_name_NAME" methods and
"method_missing" method for accept_name.

=end
    ## Skeleton visitor
    class Visitor
      ## You can override the following methods and implement the other
      ## "visit_TYPE" methods.
      ## You should implement some "visit_name_NAME" methods and
      ## "method_missing" method for accept_name.

=begin
=== Methods

    --- Visitor#visit_Document(grove, *rest)

callback method.
=end
      def visit_Document(grove, *rest)
        grove.children_accept(self, *rest)
      end

=begin
    --- Visitor#visit_Element(element, *rest)

callback method.
=end
      def visit_Element(element, *rest)
        element.children_accept(self, *rest)
      end

=begin
    --- Visitor#visit_Text(text, *rest)

callback method.
=end
      def visit_Text(text, *rest)
      end

=begin
    --- Visitor#visit_CDATASection(text, *rest)

callback method.
=end
      def visit_CDATASection(text, *rest)
      end

=begin
    --- Visitor#visit_Comment(comment, *rest)

callback method.
=end
      def visit_Comment(comment, *rest)
      end

=begin
    --- Visitor#visit_ProcessingInstruction(pi, *rest)

callback method.
=end
      def visit_ProcessingInstruction(pi, *rest)
      end

    end

=begin

== Class XML::DOM::Node

XML::Grove::Visitor like interfaces.
=end
    class Node

=begin
    --- Node#accept(visitor, *rest)

call back visit_* method.
=end
      ## XML::Grove::Visitor like interfaces
      def accept(visitor, *rest)
        typename = self.class.to_s.sub(/.*?([^:]+)$/, '\1')
        visitor.send("visit_" + typename, self, *rest)
      end

=begin
    --- Node#accept_name(visitor, *rest)

call back visit_name_* method.
=end
      def accept_name(visitor, *rest)
        if nodeType == ELEMENT_NODE
          name_method = "visit_name_" + nodeName
          visitor.send(name_method, self, *rest)
        else
          self.accept(visitor, *rest)
        end
      end

=begin
    --- Node#children_accept(visitor, *rest)

for each children, call back visit_* methods.
=end
      def children_accept(visitor, *rest)
        ret = []
        @children && @children.each { |node|
          ret.push(node.accept(visitor, *rest))
        }
        ret
      end

=begin
    --- Node#children_accept_name(visitor, *rest)

for each children, call back visit_name_* method.
=end
      def children_accept_name(visitor, *rest)
        ret = []
        @children && @children.each { |node|
          ret.push(node.accept_name(visitor, *rest))
        }
        ret
      end

=begin
    --- Node#each

iterator interface.
=end
      ## Iterator interface
      include Enumerable
      def each
        sibstack = []
        siblings = [ self ]
        while true
          if siblings.length == 0
            break if sibstack.length == 0
            siblings = sibstack.pop
            next
          end
          node = siblings.shift
          yield(node)
          children = node.childNodes
          if !children.nil?
            sibstack.push(siblings)
            siblings = children.to_a.dup
          end
        end
      end
    end
  end
end
