module DataObjects

  module Mock
    class Connection < DataObjects::Connection
      def initialize(uri)
        @uri = uri
      end

      def dispose
        nil
      end
    end

    class Command < DataObjects::Command
      def execute_non_query(*args)
        Result.new(self, 0, nil)
      end

      def execute_reader(*args)
        Reader.new
      end
    end

    class Result < DataObjects::Result
    end

    class Reader < DataObjects::Reader
    end
  end

end
