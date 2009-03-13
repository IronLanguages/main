require File.join(File.dirname(__FILE__), "one_to_many")
module DataMapper
  module Associations
    module ManyToMany
      extend Assertions

      # Setup many to many relationship between two models
      # -
      # @api private
      def self.setup(name, model, options = {})
        assert_kind_of 'name',    name,    Symbol
        assert_kind_of 'model',   model,   Model
        assert_kind_of 'options', options, Hash

        repository_name = model.repository.name

        model.class_eval <<-EOS, __FILE__, __LINE__
          def #{name}(query = {})
            #{name}_association.all(query)
          end

          def #{name}=(children)
            #{name}_association.replace(children)
          end

          private

          def #{name}_association
            @#{name}_association ||= begin
              unless relationship = model.relationships(#{repository_name.inspect})[#{name.inspect}]
                raise ArgumentError, "Relationship #{name.inspect} does not exist in \#{model}"
              end
              association = Proxy.new(relationship, self)
              parent_associations << association
              association
            end
          end
        EOS

        opts = options.dup
        opts.delete(:through)
        opts[:child_model]              ||= opts.delete(:class_name)  || Extlib::Inflection.classify(name)
        opts[:parent_model]             =   model
        opts[:repository_name]          =   repository_name
        opts[:remote_relationship_name] ||= opts.delete(:remote_name) || Extlib::Inflection.tableize(opts[:child_model])
        opts[:parent_key]               =   opts[:parent_key]
        opts[:child_key]                =   opts[:child_key]
        opts[:mutable]                  =   true

        names        = [ opts[:child_model], opts[:parent_model].name ].sort
        model_name   = names.join.gsub("::", "")
        storage_name = Extlib::Inflection.tableize(Extlib::Inflection.pluralize(names[0]) + names[1])

        opts[:near_relationship_name] = Extlib::Inflection.tableize(model_name).to_sym

        model.has(model.n, opts[:near_relationship_name])

        relationship = model.relationships(repository_name)[name] = RelationshipChain.new(opts)

        unless Object.const_defined?(model_name)
          model = DataMapper::Model.new(storage_name)

          model.class_eval <<-EOS, __FILE__, __LINE__
            def self.name; #{model_name.inspect} end
            def self.default_repository_name; #{repository_name.inspect} end
            def self.many_to_many; true end
          EOS

          names.each do |n|
            model.belongs_to(Extlib::Inflection.underscore(n).gsub('/', '_').to_sym)
          end

          Object.const_set(model_name, model)
        end

        relationship
      end

      class Proxy < DataMapper::Associations::OneToMany::Proxy
        def delete(resource)
          through = near_association.get(*(@parent.key + resource.key))
          near_association.delete(through)
          orphan_resource(super)
        end

        def clear
          near_association.clear
          super
        end

        def destroy
          near_association.destroy
          super
        end

        def save
        end

        private

        def new_child(attributes)
          remote_relationship.parent_model.new(attributes)
        end

        def relate_resource(resource)
          assert_mutable
          add_default_association_values(resource)
          @orphans.delete(resource)

          # TODO: fix this so it does not automatically save on append, if possible
          resource.save if resource.new_record?
          through_resource = @relationship.child_model.new
          @relationship.child_key.zip(@relationship.parent_key) do |child_key,parent_key|
            through_resource.send("#{child_key.name}=", parent_key.get(@parent))
          end
          remote_relationship.child_key.zip(remote_relationship.parent_key) do |child_key,parent_key|
            through_resource.send("#{child_key.name}=", parent_key.get(resource))
          end
          near_association << through_resource

          resource
        end

        def orphan_resource(resource)
          assert_mutable
          @orphans << resource
          resource
        end

        def assert_mutable
        end

        def remote_relationship
          @remote_relationship ||= @relationship.send(:remote_relationship)
        end

        def near_association
          @near_association ||= @parent.send(near_relationship_name)
        end

        def near_relationship_name
          @near_relationship_name ||= @relationship.send(:instance_variable_get, :@near_relationship_name)
        end
      end # class Proxy
    end # module ManyToMany
  end # module Associations
end # module DataMapper
