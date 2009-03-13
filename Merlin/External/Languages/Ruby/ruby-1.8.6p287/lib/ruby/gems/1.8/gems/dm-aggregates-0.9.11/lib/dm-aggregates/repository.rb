module DataMapper
  class Repository
    def aggregate(query)
      adapter.aggregate(query)
    end
  end
end
