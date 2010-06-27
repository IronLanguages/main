module Facade
   # The version of the facade library
   FACADE_VERSION = '1.0.4'

   # The facade method will forward a singleton method as an instance
   # method of the extending class. If no arguments are provided, then all
   # singleton methods of the class or module become instance methods.
   #
   # Existing instance methods are NOT overridden, but are instead ignored.
   #
   # Example:
   #
   #  require 'facade'
   # 
   #  class MyString < String
   #     extend Facade
   #     facade File, :dirname, :basename
   #  end
   #
   #  s = MyString.new('/home/djberge')
   #  s.basename # => 'djberge'
   #  s.dirname  # => '/home'
   #
   def facade(klass, *methods)
      methods = methods.flatten

      if methods.empty? # Default to all methods
         if klass.kind_of?(Class)
            methods = klass.methods(false)
         else
            methods = klass.public_instance_methods(false)
         end
      end

      # Convert all strings to symbols to stay sane between 1.8.x and 1.9.x
      methods = methods.map{ |m| m.to_sym }
      methods -= self.instance_methods.map{ |m| m.to_sym } # No clobber

      methods.each do |methname|
         define_method(methname){
            if klass.kind_of?(Class)
               meth = klass.method(methname)
            else
               meth = Object.new.extend(klass).method(methname)
            end

            if meth.arity.zero? # Zero or one argument
               meth.call
            else
               meth.call(self)
            end
         }
      end
   end
end
