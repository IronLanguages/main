require 'pathname'

module DataMapper
  module Types
    class FilePath < DataMapper::Type
      primitive String

      def self.load(value, property)
        if value.nil?
          nil
        else
          Pathname.new(value)
        end
      end

      def self.dump(value, property)
        return nil if value.nil?
        value.to_s
      end

      def self.typecast(value, property)
        # Leave alone if a Pathname is given.
        value.kind_of?(Pathname) ? value : load(value, property)
      end
    end # class FilePath
  end # module Types
end # module DataMapper
