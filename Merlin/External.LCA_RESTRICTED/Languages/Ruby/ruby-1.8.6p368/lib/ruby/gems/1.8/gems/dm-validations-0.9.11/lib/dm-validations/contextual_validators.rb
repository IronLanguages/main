module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class ContextualValidators

      def dump
        contexts.each_pair do |key, context|
          puts "Key=#{key} Context: #{context}"
        end
      end

      # Get a hash of named context validators for the resource
      #
      # @return <Hash> a hash of validators <GenericValidator>
      def contexts
        @contexts ||= {}
      end

      # Return an array of validators for a named context
      #
      # @return <Array> An array of validators
      def context(name)
        contexts[name] ||= []
      end

      # Clear all named context validators off of the resource
      #
      def clear!
        contexts.clear
      end

      # Execute all validators in the named context against the target
      #
      # @param <Symbol> named_context the context we are validating against
      # @param <Object> target        the resource that we are validating
      # @return <Boolean> true if all are valid, otherwise false
      def execute(named_context, target)
        raise(ArgumentError, 'invalid context specified') if !named_context || (contexts.length > 0 && !contexts[named_context])
        target.errors.clear!
        result = true
        context(named_context).each do |validator|
          next unless validator.execute?(target)
          result = false unless validator.call(target)
        end

        result
      end

    end # module ContextualValidators
  end # module Validate
end # module DataMapper
