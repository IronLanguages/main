class BeAbleToLoadMatcher
  def initialize(assembly)
    @assembly = assembly
    @loaders = []
  end

  def with(method, success = true)
    @loaders << [method, success]
    self
  end

  alias followed_by with 
  
  def twice
    @loaders << @loaders.last
    self
  end

  def once
    flip = @loaders.last.dup
    flip[1] = !flip[1]
    @loaders << flip
    self
  end

  def matches?(engine)
    @result = []
    @loaders.each do |loader|
      @result = [(engine.execute("#{loader[0].to_s} '#{@assembly}'") == loader[1]), loader]
      break unless @result.all?
    end
    @result.all?
  end

  def failure_message
    ["Expected to be able to #{@result.last[0]}", "the assembly #{@assembly}"]
  end

  def negative_failure_message
    ["Expected not to be able to #{@result.last[0]}", "the assembly #{@assembly}"]
  end
end

class ClrStringMatcher
  def initialize(str)
    @expected = str
  end

  def matches?(actual)
    @actual = actual
    @actual.to_s == @expected
  end

  def failure_message
    ["Expected CLR string '#{@actual}'", "to equal Ruby string \"#{@expected}\""]
  end

  def negative_failure_message
    ["Expected CLR string '#{@actual}'", "to not equal Ruby string \"#{@expected}\""]
  end
end

class InferMatcher
  def initialize(meth)
    @meth = meth
    @exceptions = []
    @passing = []
  end

  def with(*args)
    @passing <<  args 
    self
  end
  
  def except(*args)
    @exceptions << args
    self
  end

  def matches?(target)
    @target_type = target.GetType
    pass = []
    @passing.each do |arg|
      pass << (target.send(@meth, *arg) == type_to_string(*arg))
    end
    @exceptions.each do |arg|
      pass << begin
                target.send(@meth, *arg)
                false
              rescue ArgumentError
                true
              end
    end
    pass.all? {|e| e}
  end
  
  def type_to_string(*type)
    type = type.last
    if type == nil
      'System.Object'
    else
      type.GetType.ToString
    end
  end
  
  def failure_message
    ["Expected to be able to infer the generic type", "from calling #{@meth} on #{@target_type}"]
  end

  def negative_failure_message
    ["Expected not to be able to infer the generic type", "from calling #{@meth} on #{@target_type}"]
  end
end

class Object
  def infer(meth)
    InferMatcher.new(meth)
  end
  
  def be_able_to_load(assembly)
    BeAbleToLoadMatcher.new(assembly)
  end

  def equal_clr_string(str)
    ClrStringMatcher.new(str)
  end
end
