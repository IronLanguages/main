module DataMapper
  module Is
    module Example

      ##
      # fired when your plugin gets included into Resource
      #
      def self.included(base)

      end

      ##
      # Methods that should be included in DataMapper::Model.
      # Normally this should just be your generator, so that the namespace
      # does not get cluttered. ClassMethods and InstanceMethods gets added
      # in the specific resources when you fire is :example
      ##

      def is_example(options)

        # Add class-methods
        extend  DataMapper::Is::Example::ClassMethods
        # Add instance-methods
        include DataMapper::Is::Example::InstanceMethods

      end

      module ClassMethods

      end # ClassMethods

      module InstanceMethods

      end # InstanceMethods

    end # Example
  end # Is
end # DataMapper
