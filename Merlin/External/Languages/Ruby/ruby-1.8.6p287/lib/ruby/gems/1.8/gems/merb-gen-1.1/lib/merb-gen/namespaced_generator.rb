module Merb
  module Generators

    class NamespacedGenerator < NamedGenerator
      
      # NOTE: Currently this is not inherited, it will have to be declared in each generator
      # that inherits from this.
      first_argument :name, :required => true
      
      def modules
        chunks[0..-2]
      end

      def class_name
        chunks.last.gsub('-', '_').camel_case
      end
      
      alias_method :module_name, :class_name
      
      def file_name
        chunks.last.snake_case
      end
      
      alias_method :base_name, :file_name

      def full_class_name
        (modules + [class_name]).join('::')
      end
      
      def base_path
        File.join(*snake_cased_chunks[0..-2])
      end

      protected

      def snake_cased_chunks
        chunks.map { |c| c.snake_case }
      end

      def chunks
        name.gsub('/', '::').split('::').map { |c| c.camel_case }
      end
      
    end
  
  end
end