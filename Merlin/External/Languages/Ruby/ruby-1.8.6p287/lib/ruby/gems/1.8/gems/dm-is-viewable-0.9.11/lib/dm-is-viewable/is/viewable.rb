module DataMapper
  module Is
    module Viewable
      ##
      # fired when your plugin gets included into Resource
      #
      def self.included(base)
        # Could include some canned queries here in the future
      end

      ##
      # Methods that should be included in DataMapper::Model.
      # Normally this should just be your generator, so that the namespace
      # does not get cluttered. ClassMethods and InstanceMethods gets added
      # in the specific resources when you fire is :viewable
      ##

      def is_viewable(options={})
        # Add class-methods
        extend  DataMapper::Is::Viewable::ClassMethods
        # Add instance-methods
        include DataMapper::Is::Viewable::InstanceMethods
      end

      module ClassMethods
        # Fetches a views definition, leaving the name nil will return all views
        #
        # @param name [Symbol]
        #   the name of the view
        #
        # @return [Hash]
        #   View details
        def views(name=nil)
          @views ||= {}

          return @views if name.nil?
          return @views[name]
        end

        # Creates a view
        #
        # @param name [Symbol]
        #   the name of the view
        #
        # @param query_params [Hash[Query]]
        #   The query parameters or pass to Resource.all
        #
        def create_view(name,query_params={})
          @views ||= {}
          @views[name] = query_params
        end

        # Queries a view from the repository
        #
        # @param name [Symbol]
        #   the name of the view
        #
        # @param query_params [Hash[Query]]
        #   The query parameters or pass to Resource.all
        #
        # @return [Collection]
        #   result set of the query
        def view(name,query_params={})
          all(views(name).merge(query_params))
        end
      end # ClassMethods

      module InstanceMethods
      end # InstanceMethods

    end # Viewable
  end # Is
end # DataMapper
