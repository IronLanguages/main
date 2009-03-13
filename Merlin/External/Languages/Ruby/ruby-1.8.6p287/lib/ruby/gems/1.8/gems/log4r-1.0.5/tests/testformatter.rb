require "include"

class TestFormatter < TestCase
  def test_creation
    assert_no_exception { Formatter.new.format(3) }
    assert_no_exception { DefaultFormatter.new }
    assert_kind_of(Formatter, DefaultFormatter.new)
  end
  def test_simple_formatter
    sf = SimpleFormatter.new
    f = Logger.new('simple formatter')
    event = LogEvent.new(0, f, nil, "some data")
    assert_match(sf.format(event), /simple formatter/)
  end
  def test_basic_formatter
    b = BasicFormatter.new
    f = Logger.new('fake formatter')
    event = LogEvent.new(0, f, caller, "fake formatter")
    event2 = LogEvent.new(0, f, nil, "fake formatter")
    # this checks for tracing
    assert_match(b.format(event), /in/)
    assert_not_match(b.format(event2), /in/)
    e = ArgumentError.new("argerror")
    e.set_backtrace ['backtrace']
    event3 = LogEvent.new(0, f, nil, e)
    assert_match(b.format(event3), /ArgumentError/)
    assert_match(b.format(LogEvent.new(0,f,nil,[1,2,3])), /Array/)
  end
end 
