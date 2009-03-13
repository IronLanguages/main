module DataMapper
  module Associations
    class RelationshipChain < Relationship
      OPTIONS = [
        :repository_name, :near_relationship_name, :remote_relationship_name,
        :child_model, :parent_model, :parent_key, :child_key,
        :min, :max
      ]

      undef_method :get_parent
      undef_method :attach_parent

      # @api private
      def child_model
        near_relationship.child_model
      end

      # @api private
      def get_children(parent, options = {}, finder = :all, *args)
        query = @query.merge(options).merge(child_key.to_query(parent_key.get(parent)))

        query[:links]  = links
        query[:unique] = true

        with_repository(parent) do
          results = grandchild_model.send(finder, *(args << query))
          # FIXME: remove the need for the uniq.freeze
          finder == :all ? (@mutable ? results : results.freeze) : results
        end
      end

      private

      # @api private
      def initialize(options)
        if (missing_options = OPTIONS - [ :min, :max ] - options.keys ).any?
          raise ArgumentError, "The options #{missing_options * ', '} are required", caller
        end

        @repository_name          = options.fetch(:repository_name)
        @near_relationship_name   = options.fetch(:near_relationship_name)
        @remote_relationship_name = options.fetch(:remote_relationship_name)
        @child_model              = options.fetch(:child_model)
        @parent_model             = options.fetch(:parent_model)
        @parent_properties        = options.fetch(:parent_key)
        @child_properties         = options.fetch(:child_key)
        @mutable                  = options.delete(:mutable) || false

        @name        = near_relationship.name
        @query       = options.reject{ |key,val| OPTIONS.include?(key) }
        @extra_links = []
        @options     = options
      end

      # @api private
      def near_relationship
        parent_model.relationships[@near_relationship_name]
      end

      # @api private
      def links
        if remote_relationship.kind_of?(RelationshipChain)
          remote_relationship.send(:links) + [ remote_relationship.send(:near_relationship) ]
        else
          [ remote_relationship ]
        end
      end

      # @api private
      def remote_relationship
        near_relationship.child_model.relationships[@remote_relationship_name] ||
          near_relationship.child_model.relationships[@remote_relationship_name.to_s.singularize.to_sym]
      end

      # @api private
      def grandchild_model
        Class === @child_model ? @child_model : (Class === @parent_model ? @parent_model.find_const(@child_model) : Object.find_const(@child_model))
      end
    end # class Relationship
  end # module Associations
end # module DataMapper
