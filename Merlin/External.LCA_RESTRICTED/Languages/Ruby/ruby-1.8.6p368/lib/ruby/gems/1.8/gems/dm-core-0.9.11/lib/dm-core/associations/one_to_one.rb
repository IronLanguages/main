module DataMapper
  module Associations
    module OneToOne
      extend Assertions

      # Setup one to one relationship between two models
      # -
      # @api private
      def self.setup(name, model, options = {})
        assert_kind_of 'name',    name,    Symbol
        assert_kind_of 'model',   model,   Model
        assert_kind_of 'options', options, Hash

        repository_name = model.repository.name

        model.class_eval <<-EOS, __FILE__, __LINE__
          def #{name}
            #{name}_association.first
          end

          def #{name}=(child_resource)
            #{name}_association.replace(child_resource.nil? ? [] : [ child_resource ])
          end

          private

          def #{name}_association
            @#{name}_association ||= begin
              unless relationship = model.relationships(#{repository_name.inspect})[:#{name}]
                raise ArgumentError, "Relationship #{name.inspect} does not exist in \#{model}"
              end
              association = Associations::OneToMany::Proxy.new(relationship, self)
              parent_associations << association
              association
            end
          end
        EOS

        model.relationships(repository_name)[name] = if options.has_key?(:through)
          RelationshipChain.new(
            :child_model              => options.fetch(:class_name, Extlib::Inflection.classify(name)),
            :parent_model             => model,
            :repository_name          => repository_name,
            :near_relationship_name   => options[:through],
            :remote_relationship_name => options.fetch(:remote_name, name),
            :parent_key               => options[:parent_key],
            :child_key                => options[:child_key]
          )
        else
          Relationship.new(
            name,
            repository_name,
            options.fetch(:class_name, Extlib::Inflection.classify(name)),
            model,
            options
          )
        end
      end
    end # module HasOne
  end # module Associations
end # module DataMapper
