module DataMapper
  module Types
    class ParanoidBoolean < DataMapper::Type(Boolean)
      primitive TrueClass
      default   false
      lazy      true

      def self.bind(property)
        model = property.model
        repository = property.repository

        model.send(:set_paranoid_property, property.name){true}

        model.class_eval <<-EOS, __FILE__, __LINE__

          def self.with_deleted
            with_exclusive_scope(#{property.name.inspect} => true) do
              yield
            end
          end

          def destroy
            self.class.paranoid_properties.each do |name, blk|
              attribute_set(name, blk.call(self))
            end
            save
          end
        EOS

        model.default_scope(repository.name).update(property.name => false)
      end
    end # class ParanoidBoolean
  end # module Types
end # module DataMapper
