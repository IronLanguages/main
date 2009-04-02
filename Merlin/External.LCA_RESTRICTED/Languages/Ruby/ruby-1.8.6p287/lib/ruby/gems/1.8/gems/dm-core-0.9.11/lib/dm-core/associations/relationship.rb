module DataMapper
  module Associations
    class Relationship
      include Assertions

      OPTIONS = [ :class_name, :child_key, :parent_key, :min, :max, :through ]

      # @api private
      attr_reader :name, :options, :query

      # @api private
      def child_key
        @child_key ||= begin
          child_key = nil
          child_model.repository.scope do |r|
            model_properties = child_model.properties(r.name)

            child_key = parent_key.zip(@child_properties || []).map do |parent_property,property_name|
              # TODO: use something similar to DM::NamingConventions to determine the property name
              parent_name = Extlib::Inflection.underscore(Extlib::Inflection.demodulize(parent_model.base_model.name))
              property_name ||= "#{parent_name}_#{parent_property.name}".to_sym

              if model_properties.has_property?(property_name)
                model_properties[property_name]
              else
                options = {}

                [ :length, :precision, :scale ].each do |option|
                  options[option] = parent_property.send(option)
                end

                # NOTE: hack to make each many to many child_key a true key,
                # until I can figure out a better place for this check
                if child_model.respond_to?(:many_to_many)
                  options[:key] = true
                end

                child_model.property(property_name, parent_property.primitive, options)
              end
            end
          end
          PropertySet.new(child_key)
        end
      end

      # @api private
      def parent_key
        @parent_key ||= begin
          parent_key = nil
          parent_model.repository.scope do |r|
            parent_key = if @parent_properties
              parent_model.properties(r.name).slice(*@parent_properties)
            else
              parent_model.key
            end
          end
          PropertySet.new(parent_key)
        end
      end

      # @api private
      def parent_model
        return @parent_model if model_defined?(@parent_model)
        @parent_model = @child_model.find_const(@parent_model)
      rescue NameError
        raise NameError, "Cannot find the parent_model #{@parent_model} for #{@child_model}"
      end

      # @api private
      def child_model
        return @child_model if model_defined?(@child_model)
        @child_model = @parent_model.find_const(@child_model)
      rescue NameError
        raise NameError, "Cannot find the child_model #{@child_model} for #{@parent_model}"
      end

      # @api private
      def get_children(parent, options = {}, finder = :all, *args)
        parent_value = parent_key.get(parent)
        bind_values  = [ parent_value ]

        with_repository(child_model) do |r|
          parent_identity_map = parent.repository.identity_map(parent_model)
          child_identity_map  = r.identity_map(child_model)

          query_values = parent_identity_map.keys
          query_values.reject! { |k| child_identity_map[k] }

          bind_values = query_values unless query_values.empty?
          query = child_key.zip(bind_values.transpose).to_hash

          collection = child_model.send(finder, *(args.dup << @query.merge(options).merge(query)))

          return collection unless collection.kind_of?(Collection) && collection.any?

          grouped_collection = {}
          collection.each do |resource|
            child_value = child_key.get(resource)
            parent_obj = parent_identity_map[child_value]
            grouped_collection[parent_obj] ||= []
            grouped_collection[parent_obj] << resource
          end

          association_accessor = "#{self.name}_association"

          ret = nil
          grouped_collection.each do |parent, children|
            association = parent.send(association_accessor)

            query = collection.query.dup
            query.conditions.map! do |operator, property, bind_value|
              if operator != :raw && child_key.has_property?(property.name)
                bind_value = *children.map { |child| property.get(child) }.uniq
              end
              [ operator, property, bind_value ]
            end

            parents_children = Collection.new(query)
            children.each { |child| parents_children.send(:add, child) }

            if parent_key.get(parent) == parent_value
              ret = parents_children
            else
              association.instance_variable_set(:@children, parents_children)
            end
          end

          ret || child_model.send(finder, *(args.dup << @query.merge(options).merge(child_key.zip([ parent_value ]).to_hash)))
        end
      end

      # @api private
      def get_parent(child, parent = nil)
        child_value = child_key.get(child)
        return nil if child_value.any? { |v| v.nil? }

        with_repository(parent || parent_model) do
          parent_identity_map = (parent || parent_model).repository.identity_map(parent_model.base_model)
          child_identity_map  = child.repository.identity_map(child_model.base_model)

          if parent = parent_identity_map[child_value]
            return parent
          end

          children = child_identity_map.values
          children << child unless child_identity_map[child.key]

          bind_values = children.map { |c| child_key.get(c) }.uniq
          query_values = bind_values.reject { |k| parent_identity_map[k] }

          bind_values = query_values unless query_values.empty?
          query = parent_key.zip(bind_values.transpose).to_hash
          association_accessor = "#{self.name}_association"

          collection = parent_model.send(:all, query)
          unless collection.empty?
            collection.send(:lazy_load)
            children.each do |c|
              c.send(association_accessor).instance_variable_set(:@parent, collection.get(*child_key.get(c)))
            end
            child.send(association_accessor).instance_variable_get(:@parent)
          end
        end
      end

      # @api private
      def with_repository(object = nil)
        other_model = object.model == child_model ? parent_model : child_model if object.respond_to?(:model)
        other_model = object       == child_model ? parent_model : child_model if object.kind_of?(DataMapper::Resource)

        if other_model && other_model.repository == object.repository && object.repository.name != @repository_name
          object.repository.scope { |block_args| yield(*block_args) }
        else
          repository(@repository_name) { |block_args| yield(*block_args) }
        end
      end

      # @api private
      def attach_parent(child, parent)
        child_key.set(child, parent && parent_key.get(parent))
      end

      private

      # +child_model_name and child_properties refers to the FK, parent_model_name
      # and parent_properties refer to the PK.  For more information:
      # http://edocs.bea.com/kodo/docs41/full/html/jdo_overview_mapping_join.html
      # I wash my hands of it!
      def initialize(name, repository_name, child_model, parent_model, options = {})
        assert_kind_of 'name',            name,            Symbol
        assert_kind_of 'repository_name', repository_name, Symbol
        assert_kind_of 'child_model',     child_model,     String, Class
        assert_kind_of 'parent_model',    parent_model,    String, Class

        unless model_defined?(child_model) || model_defined?(parent_model)
          raise 'at least one of child_model and parent_model must be a Model object'
        end

        if child_properties = options[:child_key]
          assert_kind_of 'options[:child_key]', child_properties, Array
        end

        if parent_properties = options[:parent_key]
          assert_kind_of 'options[:parent_key]', parent_properties, Array
        end

        @name              = name
        @repository_name   = repository_name
        @child_model       = child_model
        @child_properties  = child_properties   # may be nil
        @query             = options.reject { |k,v| OPTIONS.include?(k) }
        @parent_model      = parent_model
        @parent_properties = parent_properties  # may be nil
        @options           = options

        # attempt to load the child_key if the parent and child model constants are defined
        if model_defined?(@child_model) && model_defined?(@parent_model)
          child_key
        end
      end

      # @api private
      def model_defined?(model)
        # TODO: figure out other ways to see if the model is loaded
        model.kind_of?(Class)
      end
    end # class Relationship
  end # module Associations
end # module DataMapper
