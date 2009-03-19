module DataMapper
  class Query
    attr_accessor :view, :view_options
  end
end

module DataMapper
  module CouchResource
    class View
      attr_reader :model, :name

      def initialize(model, name)
        @model = model
        @name = name

        create_getter
      end

      def create_getter
        @model.class_eval <<-EOS, __FILE__, __LINE__
          def self.#{@name}(*args)
            options = {}
            if args.size == 1 && !args.first.is_a?(Hash)
              options[:key] = args.shift
            else
              options = args.pop
            end
            query = Query.new(repository, self)
            query.view_options = options || {}
            query.view = '#{@name}'
            if options.is_a?(Hash) && options.has_key?(:repository)
              repository(options.delete(:repository)).read_many(query)
            else
              repository.read_many(query)
            end
          end
        EOS
      end
    end
  end
end
