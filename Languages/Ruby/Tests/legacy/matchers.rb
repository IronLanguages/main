class FlowMatcher
  def initialize(method, *expected)
    @method = method
    @expected = expected
    @append = []
    @prepend = []
  end

  def expected_pad
    @prepend + @expected + @append
  end

  def resulting_in(result)
    @resulting_in = result
    self
  end
  
  def with(arg)
    @with = arg
    self
  end
  
  def appending(sym)
    @append << sym
    self
  end

  def arg(arg)
    @arg = arg
    self
  end
  
  def prepending(sym)
    @prepend.unshift(sym)
    self
  end
  
  def matches?(actual, &blk)
    begin
      @actual = actual
      if @arg
        @result = @actual.send(@method, @arg, &blk)
      else
        @result = @actual.send(@method, &blk) 
      end
    ensure
      return (check_result(@result) && ScratchPad.recorded == expected_pad)
    end
  end
  
  def check_result(result)
    result == @expected_result
  end
  
  def failure_message
    ["Expected #{@actual.name}.#{@method.to_s}", "to #{@message} correctly from the block.\n\n ScratchPad: #{ScratchPad.recorded.inspect}\n Expected Pad: #{expected_pad.inspect}\n Result: #{@result.inspect}\n Expected Result: #{@expected_result.inspect}"]
  end

  def negative_failure_message
    ["Expected #{@actual.name}.#{@method.to_s}", "not to #{@message} correctly from the block.\n\n ScratchPad: #{ScratchPad.recorded.inspect}\n Expected Pad: #{expected_pad.inspect}\n Result: #{@result.inspect}\n Expected Result: #{@expected_result.inspect}"]
  end
end
class BreakMatcher < FlowMatcher
  def initialize(method)
    @message = "break"
    super(method, :before_break)
  end

  def matches?(actual)
    super(actual) do
      ScratchPad << :before_break
      if @with
        break @with
      else
        break
      end
      ScratchPad << :after_break
    end
  end
end

class NextMatcher < FlowMatcher
  def initialize(method)
    @message = "handle next"
    super(method, :before_next)
  end

  def matches?(actual)
    @expected_result = @resulting_in
    super(actual) do
      ScratchPad << :before_next
      if @with
        next @with
      else
        next
      end
      ScratchPad << :after_next
    end
  end
end

class RedoMatcher < FlowMatcher
  def initialize(method)
    @message = "handle redo"
    super(method, :after_redo)
  end

  def matches?(actual)
    @expected_result = @resulting_in || @with
    @expected = ([:before_redo] * @with) + @expected
    c=0
    super(actual) do
      ScratchPad << :before_redo
      c+=1
      redo if c < @with
      ScratchPad << :after_redo
      c
    end
  end
end

class RetryMatcher < FlowMatcher
  def initialize(method)
    @message = "handle retry"
    super(method, :after_retry)
  end

  def matches?(actual)
    @expected_result = @resulting_in || @with
    @expected = ([:before_retry] * @with) + @expected
    c=0
    super(actual) do
      ScratchPad << :before_retry
      c+=1
      retry if c < @with
      ScratchPad << :after_retry
      c
    end
  end
end

class Object
  def break_from(method)
    BreakMatcher.new(method)
  end

  def handle_next_for(method)
    NextMatcher.new(method)
  end

  def handle_redo_for(method)
    RedoMatcher.new(method)
  end
  
  def handle_retry_for(method)
    RetryMatcher.new(method)
  end
end
