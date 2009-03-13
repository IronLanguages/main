if RUBY_VERSION >= '1.9.0'
 require 'csv'
else
  begin
    gem 'fastercsv', '~>1.4.0'
    require 'fastercsv'
    CSV = FasterCSV
  rescue LoadError
    nil
  end
end

module DataMapper
  module Types
    class Csv < DataMapper::Type
      primitive Text
      lazy true

      def self.load(value, property)
        case value
        when String then CSV.parse(value)
        when Array then value
        else nil
        end
      end

      def self.dump(value, property)
        case value
        when Array then
          CSV.generate do |csv|
            value.each { |row| csv << row }
          end
        when String then value
        else nil
        end
      end
    end # class Csv
  end # module Types
end # module DataMapper
