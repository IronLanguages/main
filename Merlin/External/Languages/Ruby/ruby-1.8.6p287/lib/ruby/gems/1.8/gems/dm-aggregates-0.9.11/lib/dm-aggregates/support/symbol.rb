class Symbol
  def count
    DataMapper::Query::Operator.new(self, :count)
  end

  def min
    DataMapper::Query::Operator.new(self, :min)
  end

  def max
    DataMapper::Query::Operator.new(self, :max)
  end

  def avg
    DataMapper::Query::Operator.new(self, :avg)
  end

  def sum
    DataMapper::Query::Operator.new(self, :sum)
  end
end # class Symbol
