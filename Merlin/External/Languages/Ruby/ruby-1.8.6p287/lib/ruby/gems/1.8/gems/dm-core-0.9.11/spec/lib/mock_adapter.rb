module DataMapper
  module Adapters
    class MockAdapter < DataMapper::Adapters::DataObjectsAdapter

      def create(resources)
        1
      end

      def exists?(storage_name)
        true
      end

    end
  end
end

module DataObjects
  module Mock

    def self.logger
    end

    def self.logger=(value)
    end

  end
end
