module DataMapper
  class Collection
    def all(query = {},&block)
      query = DataMapper::Querizer.translate(&block) if block
      return self if query.kind_of?(Hash) ? query.empty? : query == self.query
      query = scoped_query(query)
      query.repository.read_many(query)
    end
  end
end
