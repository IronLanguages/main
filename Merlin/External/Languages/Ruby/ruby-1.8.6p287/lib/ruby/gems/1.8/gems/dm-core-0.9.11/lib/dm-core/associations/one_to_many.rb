module DataMapper
  module Associations
    module OneToMany
      extend Assertions

      # Setup one to many relationship between two models
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

        model.relationships(repository_name)[name] = if options.has_key?(:through)
          opts = options.dup

          if opts.key?(:class_name) && !opts.key?(:child_key)
            warn(<<-EOS.margin)
              You have specified #{model.base_model.name}.has(#{name.inspect}) with :class_name => #{opts[:class_name].inspect}. You probably also want to specify the :child_key option.
            EOS
          end

          opts[:child_model]            ||= opts.delete(:class_name)  || Extlib::Inflection.classify(name)
          opts[:parent_model]             =   model
          opts[:repository_name]          =   repository_name
          opts[:near_relationship_name]   =   opts.delete(:through)
          opts[:remote_relationship_name] ||= opts.delete(:remote_name) || name
          opts[:parent_key]               =   opts[:parent_key]
          opts[:child_key]                =   opts[:child_key]

          RelationshipChain.new( opts )
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

      # TODO: look at making this inherit from Collection.  The API is
      # almost identical, and it would make more sense for the
      # relationship.get_children method to return a Proxy than a
      # Collection that is wrapped in a Proxy.
      class Proxy
        include Assertions

        instance_methods.each { |m| undef_method m unless %w[ __id__ __send__ class object_id kind_of? respond_to? assert_kind_of should should_not instance_variable_set instance_variable_get ].include?(m.to_s) }

        # FIXME: remove when RelationshipChain#get_children can return a Collection
        def all(query = {})
          query.empty? ? self : @relationship.get_children(@parent, query)
        end

        # FIXME: remove when RelationshipChain#get_children can return a Collection
        def first(*args)
          if args.last.respond_to?(:merge)
            query = args.pop
            @relationship.get_children(@parent, query, :first, *args)
          else
            children.first(*args)
          end
        end

        def <<(resource)
          assert_mutable
          return self if !resource.new_record? && self.include?(resource)
          children << resource
          relate_resource(resource)
          self
        end

        def push(*resources)
          assert_mutable
          resources.reject! { |resource| !resource.new_record? && self.include?(resource) }
          children.push(*resources)
          resources.each { |resource| relate_resource(resource) }
          self
        end

        def unshift(*resources)
          assert_mutable
          resources.reject! { |resource| !resource.new_record? && self.include?(resource) }
          children.unshift(*resources)
          resources.each { |resource| relate_resource(resource) }
          self
        end

        def replace(other)
          assert_mutable
          each { |resource| orphan_resource(resource) }
          other = other.map { |resource| resource.kind_of?(Hash) ? new_child(resource) : resource }
          children.replace(other)
          other.each { |resource| relate_resource(resource) }
          self
        end

        def pop
          assert_mutable
          orphan_resource(children.pop)
        end

        def shift
          assert_mutable
          orphan_resource(children.shift)
        end

        def delete(resource)
          assert_mutable
          orphan_resource(children.delete(resource))
        end

        def delete_at(index)
          assert_mutable
          orphan_resource(children.delete_at(index))
        end

        def clear
          assert_mutable
          each { |resource| orphan_resource(resource) }
          children.clear
          self
        end

        def build(attributes = {})
          assert_mutable
          attributes = default_attributes.merge(attributes)
          resource = children.respond_to?(:build) ? children.build(attributes) : new_child(attributes)
          resource
        end

        def new(attributes = {})
          assert_mutable
          raise UnsavedParentError, 'You cannot intialize until the parent is saved' if @parent.new_record?
          attributes = default_attributes.merge(attributes)
          resource = children.respond_to?(:new) ? children.new(attributes) : @relationship.child_model.new(attributes)
          self << resource
          resource
        end

        def create(attributes = {})
          assert_mutable
          raise UnsavedParentError, 'You cannot create until the parent is saved' if @parent.new_record?
          attributes = default_attributes.merge(attributes)
          resource = children.respond_to?(:create) ? children.create(attributes) : @relationship.child_model.create(attributes)
          self << resource
          resource
        end

        def update(attributes = {})
          assert_mutable
          raise UnsavedParentError, 'You cannot mass-update until the parent is saved' if @parent.new_record?
          children.update(attributes)
        end

        def update!(attributes = {})
          assert_mutable
          raise UnsavedParentError, 'You cannot mass-update without validations until the parent is saved' if @parent.new_record?
          children.update!(attributes)
        end

        def destroy
          assert_mutable
          raise UnsavedParentError, 'You cannot mass-delete until the parent is saved' if @parent.new_record?
          children.destroy
        end

        def destroy!
          assert_mutable
          raise UnsavedParentError, 'You cannot mass-delete without validations until the parent is saved' if @parent.new_record?
          children.destroy!
        end

        def reload
          @children = nil
          self
        end

        def save
          return true if children.frozen?

          # save every resource in the collection
          each { |resource| save_resource(resource) }

          # save orphan resources
          @orphans.each do |resource|
            begin
              save_resource(resource, nil)
            rescue
              children << resource unless children.frozen? || children.include?(resource)
              raise
            end
          end

          # FIXME: remove when RelationshipChain#get_children can return a Collection
          # place the children into a Collection if not already
          if children.kind_of?(Array) && !children.frozen?
            @children = @relationship.get_children(@parent).replace(children)
          end

          true
        end

        def kind_of?(klass)
          super || children.kind_of?(klass)
        end

        def respond_to?(method, include_private = false)
          super || children.respond_to?(method, include_private)
        end

        private

        def initialize(relationship, parent)
          assert_kind_of 'relationship', relationship, Relationship
          assert_kind_of 'parent',       parent,       Resource

          @relationship = relationship
          @parent       = parent
          @orphans      = []
        end

        def children
          @children ||= @relationship.get_children(@parent)
        end

        def assert_mutable
          raise ImmutableAssociationError, 'You can not modify this association' if children.frozen?
        end

        def default_attributes
          default_attributes = {}

          @relationship.query.each do |attribute, value|
            next if Query::OPTIONS.include?(attribute) || attribute.kind_of?(Query::Operator)
            default_attributes[attribute] = value
          end

          @relationship.child_key.zip(@relationship.parent_key.get(@parent)) do |property,value|
            default_attributes[property.name] = value
          end

          default_attributes
        end

        def add_default_association_values(resource)
          default_attributes.each do |attribute, value|
            next if !resource.respond_to?("#{attribute}=") || resource.attribute_loaded?(attribute)
            resource.send("#{attribute}=", value)
          end
        end

        def new_child(attributes)
          @relationship.child_model.new(default_attributes.merge(attributes))
        end

        def relate_resource(resource)
          assert_mutable
          add_default_association_values(resource)
          @orphans.delete(resource)
          resource
        end

        def orphan_resource(resource)
          assert_mutable
          @orphans << resource
          resource
        end

        def save_resource(resource, parent = @parent)
          @relationship.with_repository(resource) do |r|
            if parent.nil? && resource.model.respond_to?(:many_to_many)
              resource.destroy
            else
              @relationship.attach_parent(resource, parent)
              resource.save
            end
          end
        end

        def method_missing(method, *args, &block)
          results = children.send(method, *args, &block)
          results.equal?(children) ? self : results
        end
      end # class Proxy
    end # module OneToMany
  end # module Associations
end # module DataMapper
