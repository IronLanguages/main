module DataMapper
  module Constraints
    module DeleteConstraint

      def self.included(base)
        base.extend(ClassMethods)
      end

      module ClassMethods
        DELETE_CONSTRAINT_OPTIONS = [:protect, :destroy, :destroy!, :set_nil, :skip]

        ##
        # Checks that the constraint type is appropriate to the relationship
        #
        # @param cardinality [Fixnum] cardinality of relationship
        #
        # @param name [Symbol] name of relationship to evaluate constraint of
        #
        # @param options [Hash] options hash
        #
        # @raises ArgumentError
        #
        # @return [nil]
        #
        # @api semi-public
        def check_delete_constraint_type(cardinality, name, options = {})
          #Make sure options contains :constraint key, whether nil or not
          options[:constraint] ||= nil
          constraint_type = options[:constraint]
          return if constraint_type.nil?

          delete_constraint_options = DELETE_CONSTRAINT_OPTIONS.map { |o| ":#{o}" }
          if !DELETE_CONSTRAINT_OPTIONS.include?(constraint_type)
            raise ArgumentError, ":constraint option must be one of #{delete_constraint_options * ', '}"
          end

          if constraint_type == :set_nil && self.relationships[name].is_a?(DataMapper::Associations::RelationshipChain)
            raise ArgumentError, "Constraint type :set_nil is not valid for M:M relationships"
          end

          if cardinality == 1 && constraint_type == :destroy!
            raise ArgumentError, "Constraint type :destroy! is not valid for 1:1 relationships"
          end
        end

        ##
        # Temporarily changes the visibility of a method so a block can be evaluated against it
        #
        # @param method [Symobl] method to change visibility of
        #
        # @param from_visibility [Symbol] original visibility
        #
        # @param to_visibility [Symbol] temporary visibility
        #
        # @param block [Proc] proc to run
        #
        # @notes  TODO: this should be moved to a 'util-like' module
        #
        # @return [nil]
        #
        # @api semi-public
        def with_changed_method_visibility(method, from_visibility, to_visibility, &block)
          send(to_visibility, method)
          yield
          send(from_visibility, method)
        end

      end

      ##
      # Addes the delete constraint options to a relationship
      #
      # @param params [*ARGS] Arguments passed to Relationship#initialize or RelationshipChain#initialize
      #
      # @notes This takes *params because it runs before the initializer for Relationships and RelationshipChains
      #   which have different method signatures
      #
      # @return [nil]
      #
      # @api semi-public
      def add_delete_constraint_option(*params)
        opts = params.last

        if opts.is_a?(Hash)
          #if it is a chain, set the constraint on the 1:M near relationship(anonymous)
          if self.is_a?(DataMapper::Associations::RelationshipChain)
            opts = params.last
            near_rel = opts[:parent_model].relationships[opts[:near_relationship_name]]
            near_rel.options[:constraint] = opts[:constraint]
            near_rel.instance_variable_set "@delete_constraint", opts[:constraint]
          end

          @delete_constraint = params.last[:constraint]
        end
      end

      ##
      # Checks delete constraints prior to destroying a dm resource or collection
      #
      # @throws :halt
      #
      # @notes
      #   - It only considers a relationship's constraints if this is the parent model (ie a child shouldn't delete a parent)
      #   - RelationshipChains are skipped, as they are evaluated by their underlying 1:M relationships
      #
      # @returns [nil]
      #
      # @api semi-public
      def check_delete_constraints
        model.relationships.each do |rel_name, rel|
          #Only look at relationships where this model is the parent
          next if rel.parent_model != model

          #Don't delete across M:M relationships, instead use their anonymous 1:M Relationships
          next if rel.is_a?(DataMapper::Associations::RelationshipChain)

          children = self.send(rel_name)
          if children.kind_of?(DataMapper::Collection)
            check_collection_delete_constraints(rel,children)
          elsif children
            check_resource_delete_constraints(rel,children)
          end
        end # relationships
      end # check_delete_constraints

      ##
      # Performs the meat of the check_delete_constraints method for a collection of resources
      #
      # @param rel [DataMapper::Associations::Relationship] relationship being evaluated
      #
      # @param children [~DataMapper::Collection] child records to constrain
      #
      # @see #check_delete_constraints
      #
      # @api semi-public
      def check_collection_delete_constraints(rel, children)
        case rel.delete_constraint
        when nil, :protect
          unless children.empty?
            DataMapper.logger.error("Could not delete #{self.class} a child #{children.first.class} exists")
            throw(:halt,false)
          end
        when :destroy
          children.each{|child| child.destroy}
        when :destroy!
          children.destroy!
        when :set_nil
          children.each do |child|
            child.class.many_to_one_relationships.each do |mto_rel|
              child.send("#{mto_rel.name}=", nil) if child.send(mto_rel.name).eql?(self)
            end
          end
        end
      end

      ##
      # Performs the meat of check_delete_constraints method for a single resource
      #
      # @param rel [DataMapper::Associations::Relationship] the relationship to evaluate
      #
      # @param child [~DataMapper::Model] the model to constrain
      #
      # @see #check_delete_constraints
      #
      # @api semi-public
      def check_resource_delete_constraints(rel, child)
        case rel.delete_constraint
        when nil, :protect
          unless child.nil?
            DataMapper.logger.error("Could not delete #{self.class} a child #{child.class} exists")
            throw(:halt,false)
          end
        when :destroy
          child.destroy
        when :destroy!
          #not supported in dm-master, an exception should have been raised on class load
        when :set_nil
          child.class.many_to_one_relationships.each do |mto_rel|
            child.send("#{mto_rel.name}=", nil) if child.send(mto_rel.name).eql?(self)
          end
        end
      end

    end # DeleteConstraint
  end # Constraints
end # DataMapper
