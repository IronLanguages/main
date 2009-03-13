# == Synopsis
# An easy way to implement the facade pattern for your Ruby classes
#
# == Usage
# require "facade"
# class Foo < String
#    extend Facade
#    facade File, :dirname, :basename
# end
#
# f = Foo.new("/home/djberge")
# puts f.basename # 'djberge'
# puts f.dirname  # '/home'
#
# == Author
# Daniel J. Berger
# djberg96 at yahoo dot com
# imperator on IRC (freenode)
#
# == Copyright
# Copyright (c) 2005 Daniel J. Berger
# Licensed under the same terms as Ruby itself.
 
module Facade
   FACADE_VERSION = '1.0.2'

   # Forward a class or module method as an instance method of the
   # including class.
   #
   # call-seq:
   #   forward +klass+, :method
   #   forward +klass+, :method1, :method2, ...
   # 
   # This will not override the class' existing methods.
   #
   def facade(klass, *methods)
      methods.flatten!
      if methods.empty?                                # Default to all methods
         if klass.kind_of?(Class)
            methods = klass.methods(false)
         else
            methods = klass.public_instance_methods(false)
         end
      end
      methods.collect!{ |m| m.to_s }                   # Symbols or strings
      methods -= self.instance_methods                 # No clobber

      methods.each do |methname|
         methname = methname.to_sym
         define_method(methname){ ||
            if klass.kind_of?(Class)
               meth = klass.method(methname)
            else
               meth = Object.new.extend(klass).method(methname)
            end

            if meth.arity.zero?                        # Zero or one argument
               meth.call
            else
               meth.call( self )
            end
         }
      end
   end
end
