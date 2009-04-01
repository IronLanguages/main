module DataMapper
  module Is
    module Searchable

      def is_searchable(options = {})
        search_repository = options.delete(:repository) || :search
        @search_repository = search_repository

        extend ClassMethods

        after(:save) do |success, *args|
          if success
            # We use the adapter directly to bypass our after :save,
            # and because create caches the repository and new_record? state.
            DataMapper.repository(search_repository).adapter.create([self])
          end
        end

        after(:destroy) do |success|
          if success
            # Since this is after the model has been destroyed, it is
            # a new record, and a simple to_query will return nil.
            query = model.to_query(repository, key, {})
            DataMapper.repository(search_repository).adapter.delete(query)
          end
        end
      end

      module ClassMethods
        def search(search_options = {}, options = {})
          docs = repository(@search_repository) { self.all(search_options) }
          ids = docs.collect { |doc| doc[:id] }
          self.all(options.merge(key.first => ids))
        end
      end # module ClassMethods

    end # module Searchable
  end # module Is
end # module DataMapper
