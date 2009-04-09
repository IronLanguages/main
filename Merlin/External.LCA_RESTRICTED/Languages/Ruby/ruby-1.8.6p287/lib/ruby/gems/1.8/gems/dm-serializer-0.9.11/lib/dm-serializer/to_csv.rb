require 'dm-serializer/common'

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
  module Serialize
    # Serialize a Resource to comma-separated values (CSV).
    #
    # @return <String> a CSV representation of the Resource
    def to_csv(writer = '')
      CSV.generate(writer) do |csv|
        row = []
        self.class.properties(repository.name).each do |property|
          row << send(property.name).to_s
        end
        csv << row
      end
    end
  end

  class Collection
    def to_csv
      result = ""
      each do |item|
        result << item.to_csv + "\n"
      end
      result
    end
  end
end
