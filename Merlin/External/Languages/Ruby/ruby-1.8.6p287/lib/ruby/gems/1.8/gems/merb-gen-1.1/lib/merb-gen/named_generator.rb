module Merb
  module Generators

    class NamedGenerator < Generator
      # NOTE: Currently this is not inherited, it will have to be declared in each generator
      # that inherits from this.
      first_argument :name, :required => true
      
      def initialize(*args)
        super
      end
      
      def class_name
        name.gsub('-', '_').camel_case
      end
      
      alias_method :module_name, :class_name

      def test_class_name
        class_name + "Test"
      end

      def file_name
        name.snake_case
      end
      
      alias_method :base_name, :file_name

      def symbol_name
        file_name.gsub('-', '_')
      end

    end
 
  end
end
