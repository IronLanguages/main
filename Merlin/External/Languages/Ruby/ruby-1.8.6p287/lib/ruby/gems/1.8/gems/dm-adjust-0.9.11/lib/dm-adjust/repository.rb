module DataMapper
  class Repository
    def adjust(attributes, query)
      adapter.adjust(attributes, query)
    end
  end
end
