module DataObjects
  class Result
    attr_accessor :insert_id, :affected_rows

    def initialize(command, affected_rows, insert_id = nil)
      @command, @affected_rows, @insert_id = command, affected_rows, insert_id
    end

    def to_i
      @affected_rows
    end
  end
end
