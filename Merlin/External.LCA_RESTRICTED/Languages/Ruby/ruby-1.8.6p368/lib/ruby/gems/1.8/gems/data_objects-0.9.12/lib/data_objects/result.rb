module DataObjects
  # The Result class is returned from Connection#execute_non_query.
  class Result
    # The ID of a row inserted by the Command
    attr_accessor :insert_id
    # The number of rows affected by the Command
    attr_accessor :affected_rows

    # Create a new Result. Used internally in the adapters.
    def initialize(command, affected_rows, insert_id = nil)
      @command, @affected_rows, @insert_id = command, affected_rows, insert_id
    end

    # Return the number of affected rows
    def to_i
      @affected_rows
    end
  end
end
