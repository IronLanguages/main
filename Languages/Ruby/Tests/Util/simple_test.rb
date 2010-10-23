$error_count = 0
$error_list = []

class PositiveExpectation
  def initialize(obj)
    @obj = obj
  end

  def ==(other)
    if @obj != other
      msg = "Equality expected for '#{@obj.inspect}' and '#{other.inspect}'"
      $error_count += 1
      $error_list << [msg, caller]
      print 'F'
      raise Exception.new(msg)
    else
      print '.'
    end
  end
  
  def equal?(other)
    if not @obj.equal?(other)
      msg = "Reference equality expected for '#{@obj.inspect}' and '#{other.inspect}'"
      $error_count += 1
      $error_list << [msg, caller]
      print 'F'
      raise Exception.new(msg)
    else
      print '.'
    end
  end
end

class NegativeExpectation
  def initialize(obj)
    @obj = obj
  end

  def ==(other)
    if @obj == other
      msg = "Inequality expected for '#{@obj.inspect}' and '#{other.inspect}'"
      $error_count += 1
      $error_list << [msg, caller]
      print 'F'
      raise Exception.new(msg)
    else
      print '.'
    end
  end
  
  def equal?(other)
    if @obj.equal?(other)
      msg = "Reference inequality expected for '#{@obj.inspect}' and '#{other.inspect}'"
      $error_count += 1
      $error_list << [msg, caller]
      print 'F'
      raise Exception.new(msg)
    else
      print '.'
    end
  end  
end

class Object
  def should
    PositiveExpectation.new(self)
  end

  def should_not
    NegativeExpectation.new(self)
  end
end

def it(name)
  print "\n  it #{name}: "
  $name = name
  begin
    yield
  rescue Exception => exception
    if $error_count == 0
       puts "Exception thrown without recording the error..."
       raise
    end
  end
end

def skip(name)
  print "\n  **skipping** #{name}"
end

def describe(message)
  puts "\n\n#{message}"
  yield
end

def should_raise(expected_exception, expected_message=nil)
  begin
    yield
    msg = "'#{$name}' failed! expected '#{expected_exception}', but no error happened" 
    $error_count += 1
    $error_list << [msg, caller]
    puts msg
  rescue Exception => actual_exception
    if expected_exception.name != actual_exception.class.name
      msg = "'#{$name}' failed! expected '#{expected_exception}' but got '#{actual_exception}'" 
      $error_count += 1
      $error_list << [msg, $@]
      puts msg
    elsif expected_message != nil and actual_exception.message != expected_message
      msg = "'#{$name}' failed! expected message '#{expected_message}' but got '#{actual_exception.message}'" 
      $error_count += 1
      $error_list << [msg, $@]
      puts msg      
    else
      print '.'
    end
  end
end

def finished
  if $error_count > 0
    puts "\n\nErrors:"
    i = 1
    $error_list.each { |msg, trace| puts "#{i})", msg, trace, ''; i += 1 }
    puts "\n\nTests failed == #{$error_count}"
    Kernel.exit(1)
  end  
  puts "\n\nTests passed"
end

def specify(name)
  print "\n  specify #{name}: "
  $name = name
  yield
end

def context(message)
  puts "\n\n#{message}"
  yield
end
