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
      pass << ([*target.send(@meth, *arg)][0] == type_to_string(*arg))
    end
    @exceptions.each do |arg|
      pass << begin
                target.send(@meth, *arg)
                false
              rescue ArgumentError
                true
              end
    end
    pass.all? 
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

class ClasslikeMatcher
  def matches?(klass)
    @klass = klass
    x = klass.new
    kind = x.kind_of? klass
    name = x.inspect.match(/#{klass}/)
    name && kind
  end

  def failure_message
    ["Expected class #{klass.name}", "to include it's classname in inspect and have the right type'"]
  end
end
module MethodsMatcher
  class MatcherBuilder
    def initialize(&blk)
      @method_holder = MethodHolder.new
      instance_eval &blk
      run if @obj
    end

    def add_method(args)
      @method_holder.add_method(args)
    end
    
    def match(obj)
      @obj = obj
    end

    def run
      @obj.should Matcher.new(@method_holder)
    end
  end
  
  class Matcher
    def initialize(method_holder)
      @method_holder = method_holder
    end
    
    def matches?(obj)
      @obj = obj
      @method_holder.pass?(@obj)
    end

    def class_name
      (@obj === Class ? @obj : @obj.class).name
    end
  
    def failure_message
      ["Expected #{class_name} to have methods: #{@method_holder}", @method_holder.failure_string]
    end

    def negative_failure_message
      ["Expected #{class_name} not to have methods: #{@method_holder}", @method_holder.failure_string]
    end
  end

  class MethodHolder
    include Enumerable
    def initialize
      reset
    end
    
    def reset
      @methods = []
    end

    def add_method(args)
      @methods += ClrMember.create_methods(args)
    end

    def each
      @methods.each {|e| yield e}
    end

    def to_s
      @methods.map(&:name).join(" ")
    end

    def pass?(obj)
      @methods.all? do |m|
        m.bind(obj)
        m.pass?
      end
    end

    def failure_string
      @methods.select(&:failed?).map(&:failure).join("\n")
    end
  end

  class ClrMember
    def self.create_methods(args)
      r_name = IronRuby::Clr::Name.new(args[:name])
      args1 = nil
      if r_name.HasMangledName
        args1 = args.dup
        args1[:name] = r_name.clr_name
        args[:name] = r_name.ruby_name
      end
      res = [new(args)]
      res << new(args1) if args1
      res
    end
    
    def initialize(hash)
      @failure = nil
      @exception_types = []
      @hash = hash
    end

    def base
      @hash[:base]
    end

    def name
      @hash[:name]
    end

    def args
      @hash[:args]
    end

    def result
      parse_result(@hash[:result])
    end

    def blk
      @hash[:blk]
    end

    def bind(obj)
      @obj = obj
    end

    def failed?
      @failure
    end

    def failure
      "#{base}.#{name}: #{@failure}"
    end

    def get
      clr_args = [name]
      clr_args.unshift(base) unless base.nil?
      @obj.clr_member(*clr_args)
    end

    def call
      args ? get.call(*args, &blk) : get.call(&blk)
    end

    def call_exception
      call
    rescue result => e
      result
    rescue => e
      e.class.to_s + ": " + e.message
    end

    def pass?
      res = if @exception_types.include?(result)
              call_exception
            else
              call
            end
      (res == result) || fail_with(res)
    end

    private
    def parse_result(obj)
      add_exception case obj
      when :mm
        NoMethodError
      when :ne
        NameError
      else
        obj
      end
    end

    def add_exception(exp)
      if (exp < Exception rescue false)
        (@exception_types << exp)
      end
      exp
    end

    def fail_with(res)
      @failure = "Expected #{result}, got #{res}"
      false
    end
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

  def be_classlike
    ClasslikeMatcher.new
  end

  def method_matcher(&blk)
    MethodsMatcher::MatcherBuilder.new(&blk)
  end
end
