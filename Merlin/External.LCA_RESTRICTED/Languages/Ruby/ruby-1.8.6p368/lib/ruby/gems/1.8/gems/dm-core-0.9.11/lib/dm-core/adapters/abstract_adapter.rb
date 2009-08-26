module DataMapper
  module Adapters
    class AbstractAdapter
      include Assertions

      attr_reader :name, :uri
      attr_accessor :resource_naming_convention, :field_naming_convention

      def create(resources)
        raise NotImplementedError
      end

      def read_many(query)
        raise NotImplementedError
      end

      def read_one(query)
        raise NotImplementedError
      end

      def update(attributes, query)
        raise NotImplementedError
      end

      def delete(query)
        raise NotImplementedError
      end

      protected

      def normalize_uri(uri_or_options)
        uri_or_options
      end

      private

      # Instantiate an Adapter by passing it a DataMapper::Repository
      # connection string for configuration.
      def initialize(name, uri_or_options)
        assert_kind_of 'name',           name,           Symbol
        assert_kind_of 'uri_or_options', uri_or_options, Addressable::URI, DataObjects::URI, Hash, String

        @name = name
        @uri  = normalize_uri(uri_or_options)

        @resource_naming_convention = NamingConventions::Resource::UnderscoredAndPluralized
        @field_naming_convention    = NamingConventions::Field::Underscored

        @transactions = {}
      end

      # TODO: move to dm-more/dm-migrations
      module Migration
        #
        # Returns whether the storage_name exists.
        #
        # @param storage_name<String> a String defining the name of a storage,
        #   for example a table name.
        #
        # @return <Boolean> true if the storage exists
        #
        # TODO: move to dm-more/dm-migrations (if possible)
        def storage_exists?(storage_name)
          raise NotImplementedError
        end

        #
        # Returns whether the field exists.
        #
        # @param storage_name<String> a String defining the name of a storage, for example a table name.
        # @param field_name<String> a String defining the name of a field, for example a column name.
        #
        # @return <Boolean> true if the field exists.
        #
        # TODO: move to dm-more/dm-migrations (if possible)
        def field_exists?(storage_name, field_name)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def upgrade_model_storage(repository, model)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def create_model_storage(repository, model)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def destroy_model_storage(repository, model)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def alter_model_storage(repository, *args)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def create_property_storage(repository, property)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def destroy_property_storage(repository, property)
          raise NotImplementedError
        end

        # TODO: move to dm-more/dm-migrations
        def alter_property_storage(repository, *args)
          raise NotImplementedError
        end

        module ClassMethods
          # Default TypeMap for all adapters.
          #
          # @return <DataMapper::TypeMap> default TypeMap
          #
          # TODO: move to dm-more/dm-migrations
          def type_map
            @type_map ||= TypeMap.new
          end
        end
      end

      include Migration
      extend Migration::ClassMethods

      # TODO: move to dm-more/dm-transaction
      module Transaction
        #
        # Pushes the given Transaction onto the per thread Transaction stack so
        # that everything done by this Adapter is done within the context of said
        # Transaction.
        #
        # @param transaction<DataMapper::Transaction> a Transaction to be the
        #   'current' transaction until popped.
        #
        # TODO: move to dm-more/dm-transaction
        def push_transaction(transaction)
          transactions(Thread.current) << transaction
        end

        #
        # Pop the 'current' Transaction from the per thread Transaction stack so
        # that everything done by this Adapter is no longer necessarily within the
        # context of said Transaction.
        #
        # @return <DataMapper::Transaction> the former 'current' transaction.
        #
        # TODO: move to dm-more/dm-transaction
        def pop_transaction
          transactions(Thread.current).pop
        end

        #
        # Retrieve the current transaction for this Adapter.
        #
        # Everything done by this Adapter is done within the context of this
        # Transaction.
        #
        # @return <DataMapper::Transaction> the 'current' transaction for this Adapter.
        #
        # TODO: move to dm-more/dm-transaction
        def current_transaction
          transactions(Thread.current).last
        end

        #
        # Returns whether we are within a Transaction.
        #
        # @return <Boolean> whether we are within a Transaction.
        #
        # TODO: move to dm-more/dm-transaction
        def within_transaction?
          !current_transaction.nil?
        end

        #
        # Produces a fresh transaction primitive for this Adapter
        #
        # Used by DataMapper::Transaction to perform its various tasks.
        #
        # @return <Object> a new Object that responds to :close, :begin, :commit,
        #   :rollback, :rollback_prepared and :prepare
        #
        # TODO: move to dm-more/dm-transaction (if possible)
        def transaction_primitive
          raise NotImplementedError
        end

        private
        def transactions(thread)
          unless @transactions[thread]
            @transactions.delete_if do |key, value|
              !key.respond_to?(:alive?) || !key.alive?
            end
            @transactions[thread] = []
          end
          @transactions[thread]
        end

      end

      include Transaction
    end # class AbstractAdapter
  end # module Adapters
end # module DataMapper
