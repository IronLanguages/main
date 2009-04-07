class EqlMatcher
  def initialize(expected)
    @expected = expected
  end

  def matches?(actual)
    @actual = actual
    @actual.eql?(@expected)
  end

  def failure_message
    ["Expected #{PP.singleline_pp(@actual, '')} (#{PP.singleline_pp(@actual.class, '')})",
     "to have same value and type as #{PP.singleline_pp(@expected, '')} (#{PP.singleline_pp(@expected.class, '')})"]
  end

  def negative_failure_message
    ["Expected #{PP.singleline_pp(@actual, '')} (#{PP.singleline_pp(@actual.class, '')})",
     "not to have same value and type as #{PP.singleline_pp(@expected, '')} (#{PP.singleline_pp(@expected.class, '')})"]
  end
end

class Object
  def eql(expected)
    EqlMatcher.new(expected)
  end
end
