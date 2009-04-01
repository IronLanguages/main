module DataObjects
  class Reader

    def fields
      raise NotImplementedError.new
    end

    def values
      raise NotImplementedError.new
    end

    def close
      raise NotImplementedError.new
    end

    # Moves the cursor forward.
    def next!
      raise NotImplementedError.new
    end

    def field_count
      raise NotImplementedError.new
    end

    def row_count
      raise NotImplementedError.new
    end

  end
end
