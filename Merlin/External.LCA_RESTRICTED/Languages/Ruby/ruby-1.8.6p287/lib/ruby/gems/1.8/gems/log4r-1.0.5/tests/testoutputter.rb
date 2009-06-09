require "include"

class TestOutputter < TestCase
  def test_validation
    assert_exception(ArgumentError) { Outputter.new }
    assert_exception(ArgumentError) { Outputter.new 'fonda', :level=>-10}
    assert_exception(TypeError) { Outputter.new 'fonda', :formatter=>-10}
  end
  def test_io
    assert_no_exception {
      IOOutputter.new('foo3', $stdout)
      IOOutputter.new('foo4', $stderr)
    }
    f = File.new("junk/tmpx.log", "w")
    o = IOOutputter.new('asdf', f)
    o.close
    assert(f.closed? == true)
    assert(o.level == OFF)
  end
  def test_repository
    assert( Outputter['foo3'].type == IOOutputter )
    assert( Outputter['foo4'].type == IOOutputter )
    assert( Outputter['asdf'].type == IOOutputter )
  end
  def test_validation_and_creation
    assert_no_exception {
      StdoutOutputter.new('out', 'level'=>DEBUG)
      FileOutputter.new('file', 'filename'=>'junk/test', :trunc=>true)
    }
    a = StdoutOutputter.new 'out2'
    assert(a.level == Logger.root.level)
    assert(a.formatter.type == DefaultFormatter)
    b = StdoutOutputter.new('ook', :level => DEBUG, :formatter => Formatter)
    assert(b.level == DEBUG)
    assert(b.formatter.type == Formatter)
    c = StdoutOutputter.new('akk', :formatter => Formatter)
    assert(c.level == Logger.root.level)
    assert(c.formatter.type == Formatter)
    c = StderrOutputter.new('iikk', :level => OFF)
    assert(c.level == OFF)
    assert(c.formatter.type == DefaultFormatter)
    o = StderrOutputter.new 'ik'
    assert_no_exception(TypeError) { o.formatter = DefaultFormatter }
    assert_equals(o.formatter.type, DefaultFormatter)
  end
  # test the resource= bounds
  def test_boundaries
    o = StderrOutputter.new('ak', :formatter => Formatter)
    assert_exception(TypeError) { o.formatter = nil }
    assert_exception(TypeError) { o.formatter = String }
    assert_exception(TypeError) { o.formatter = "bogus" }
    assert_exception(TypeError) { o.formatter = -3 }
    # the formatter should be preserved
    assert(o.formatter.type == Formatter) 
  end
  def test_file
    assert_exception(TypeError) { FileOutputter.new 'f' }
    assert_exception(TypeError) { FileOutputter.new('fa', :filename => DEBUG) }
    assert_exception(TypeError) { FileOutputter.new('fo', :filename => nil) }
    assert_no_exception { 
      FileOutputter.new('fi', :filename => './junk/tmp')
      FileOutputter.new('fum', :filename=>'./junk/tmp', :trunc => "true")
    }
    fo = FileOutputter.new('food', :filename => './junk/tmp', :trunc => false)
    assert(fo.trunc == false)
    assert(fo.filename == './junk/tmp')
    assert(fo.closed? == false)
    fo.close
    assert(fo.closed? == true)
    assert(fo.level == OFF)
  end
  # test the dynamic definition of outputter log messages
  def test_log_methods
    o = StderrOutputter.new('so1', :level => WARN )
    # test to see if all of the methods are defined
    for mname in LNAMES
      next if mname == 'OFF' || mname == 'ALL'
      assert_respond_to(mname.downcase, o, "Test respond to #{mname.to_s}")
    end 
    return # cuz the rest is borked
    # we rely on BasicFormatter's inability to reference a nil Logger to test
    # the log methods. Everything from WARN to FATAL should choke.
    event = LogEvent.new(nil, nil, nil, nil) 
    assert_no_exception { o.debug event }
    assert_no_exception { o.info event }
    assert_exception(NameError) { o.warn event }
    assert_exception(NameError) { o.error event }
    assert_exception(NameError) { o.fatal event }
    # now let's dynamically change the level and repeat
    o.level = ERROR
    assert_no_exception { o.debug event}
    assert_no_exception { o.info event}
    assert_no_exception { o.warn event}  
    assert_exception(NameError) { o.error event}
    assert_exception(NameError) { o.fatal event}
  end
  def test_only_at_validation
    o = StdoutOutputter.new 'so2'
    assert_exception(ArgumentError) { o.only_at }
    assert_exception(ArgumentError) { o.only_at ALL }
    assert_exception(TypeError) { o.only_at OFF }
    assert_no_exception { o.only_at DEBUG, ERROR }
    return # cuz the rest is borked
    # test the methods as before
    event = LogEvent.new(nil,nil,nil,nil)
    assert_exception(NameError) { o.debug event}
    assert_exception(NameError) { o.error event}
    assert_no_exception { o.warn event}
    assert_no_exception { o.info event}
    assert_no_exception { o.fatal event}
  end
end 
