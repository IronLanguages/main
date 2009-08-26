module DataMapper
  module Adapters
    class InMemoryAdapter < AbstractAdapter
      def initialize(name, uri_or_options)
        @records = Hash.new { |hash,model| hash[model] = Array.new }
      end

      def create(resources)
        resources.each do |resource|
          @records[resource.model] << resource
        end.size # just return the number of records
      end

      def update(attributes, query)
        read_many(query).each do |resource|
          attributes.each do |property,value|
            property.set!(resource, value)
          end
        end.size
      end

      def read_one(query)
        read(query, query.model, false)
      end

      def read_many(query)
        Collection.new(query) do |set|
          read(query, set, true)
        end
      end

      def delete(query)
        records = @records[query.model]

        read_many(query).each do |resource|
          records.delete(resource)
        end.size
      end

      private

      def read(query, set, many = true)
        model      = query.model
        conditions = query.conditions

        match_with = many ? :select : :detect

        # Iterate over the records for this model, and return
        # the ones that match the conditions
        result = @records[model].send(match_with) do |resource|
          conditions.all? do |tuple|
            operator, property, bind_value = *tuple

            value = property.get!(resource)

            case operator
              when :eql, :in then equality_comparison(bind_value, value)
              when :not      then !equality_comparison(bind_value, value)
              when :like     then Regexp.new(bind_value) =~ value
              when :gt       then !value.nil? && value >  bind_value
              when :gte      then !value.nil? && value >= bind_value
              when :lt       then !value.nil? && value <  bind_value
              when :lte      then !value.nil? && value <= bind_value
              else raise "Invalid query operator: #{operator.inspect}"
            end
          end
        end

        return result unless many

        # TODO Sort

        # TODO Limit

        set.replace(result)
      end

      def equality_comparison(bind_value, value)
        case bind_value
          when Array, Range then bind_value.include?(value)
          when NilClass     then value.nil?
          else                   bind_value == value
        end
      end
    end
  end
end
