module DataMapper
  module Model
    def all(query={},&block)
      query = DataMapper::Querizer.translate(&block) if block
      query = scoped_query(query)
      query.repository.read_many(query)
    end

    def first(*args)
      query = DataMapper::Querizer.translate(args.pop) if args.last.is_a? Proc
      query = args.last.respond_to?(:merge) ? args.pop : {}
      query = scoped_query(query.merge(:limit => args.first || 1))

      if args.any?
        query.repository.read_many(query)
      else
        query.repository.read_one(query)
      end
    end
  end
end
