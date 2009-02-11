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

class Object
  def be_able_to_load(assembly)
    BeAbleToLoadMatcher.new(assembly)
  end
end
