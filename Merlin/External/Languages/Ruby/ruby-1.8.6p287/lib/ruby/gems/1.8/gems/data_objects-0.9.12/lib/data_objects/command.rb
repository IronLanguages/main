module DataObjects
  class Command

    attr_reader :connection

    # initialize creates a new Command object
    def initialize(connection, text)
      raise ArgumentError.new("+connection+ must be a DataObjects::Connection") unless DataObjects::Connection === connection
      @connection, @text = connection, text
    end

    def execute_non_query(*args)
      raise NotImplementedError.new
    end

    def execute_reader(*args)
      raise NotImplementedError.new
    end

    def set_types(column_types)
      raise NotImplementedError.new
    end

    def to_s
      @text
    end

  end

end
