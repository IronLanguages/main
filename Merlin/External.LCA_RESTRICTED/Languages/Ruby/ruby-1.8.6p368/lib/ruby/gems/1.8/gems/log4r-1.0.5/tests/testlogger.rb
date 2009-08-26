require "include"

class MyFormatter1 < Formatter
  def format(event)
    return "MyFormatter1\n"
  end
end

class MyFormatter2 < Formatter
  def format(event)
    return "MyFormatter2\n"
  end
end

class TestLogger < TestCase
  def test_root
    l1 = Logger.root
    l2 = Logger['root']
    l3 = Logger.global
    assert(l1 == l2, "RootLogger wasn't singleton!")
    assert(l1 == l3)
    assert(l1.is_root? == true, "is_root? not working")
    assert(l1.parent == nil, "Root's parent wasn't nil!")
  end
  def test_validation
    assert_exception(ArgumentError) { Logger.new }
    assert_no_exception { Logger.new('validate', nil) }
  end
  def test_all_off
    l = Logger.new("create_method")
    l.level = WARN
    assert(l.debug? == false)
    assert(l.info? == false)
    assert(l.warn? == true)
    assert(l.error? == true)
    assert(l.fatal? == true)
    assert(l.off? == false)
    assert(l.all? == false)
    l.level = OFF
    assert(l.off? == true)
    assert(l.all? == false)
    l.level = ALL
    assert(l.off? == false)
    assert(l.all? == true)
  end
  def test_add_outputters
    StdoutOutputter.new('fake1')
    StdoutOutputter.new('fake2')
    a = Logger.new("add")
    assert_exception(TypeError) { a.add 'bogus' }
    assert_exception(TypeError) { a.add Class }
    assert_exception(TypeError) { a.add 'fake1', Class }
    assert_no_exception { a.add 'fake1', 'fake2' }
  end
  def test_repository
    assert_exception(NameError) { Logger.get('bogusbogus') }
    assert_no_exception { Logger['bogusbogus'] }
  end
  def test_heiarchy
    a = Logger.new("a")
    a.additive = true
    assert(a.name == "a", "name wasn't set properly")
    assert(a.path == "", "path wasn't set properly")
    assert(a.level == Logger.root.level, "didn't inherit root's level") 
    assert(a.parent == Logger.root)
    a.level = WARN
    b = Logger.new("a::b")
    assert(b.name == "b", "name wasn't set properly")
    assert(b.path == "a", "path wasn't set properly")
    assert(b.level == a.level, "didn't inherit parent's level") 
    assert(b.parent == a, "parent wasn't what is expected")
    c = Logger.new("a::b::c")
    assert(Logger["a::b::c"] == c)
    assert(c.name == "c", "name wasn't set properly")
    assert(c.path == "a::b", "path wasn't set properly")
    assert(c.level == b.level, "didn't inherit parent's level") 
    assert(c.parent == b, "parent wasn't what is expected")
    d = Logger.new("a::d")
    assert(Logger["a::d"] == d)
    assert(d.name == "d", "name wasn't set properly")
    assert(d.path == "a", "path wasn't set properly")
    assert(d.level == a.level, "didn't inherit parent's level") 
    assert(d.parent == a, "parent wasn't what is expected")
    assert_exception(ArgumentError) { Logger.new("::a") }
  end
  def test_undefined_parents
    a = Logger.new 'has::no::real::parents::me'
    assert(a.parent == Logger.root)
    b = Logger.new 'has::no::real::parents::me::child'
    assert(b.parent == a)
    c = Logger.new 'has::no::real::parents::metoo'
    assert(c.parent == Logger.root)
    p = Logger.new 'has::no::real::parents'
    assert(p.parent == Logger.root)
    assert(a.parent == p)
    assert(b.parent == a)
    assert(c.parent == p)
    Logger.each{|fullname, logger|
      if logger != a and logger != c
        assert(logger.parent != p)
      end
    }
  end
  def test_levels
    l = Logger.new("levels", WARN)
    assert(l.level == WARN, "level wasn't changed")
    assert(l.fatal? == true)
    assert(l.error? == true)
    assert(l.warn? == true)
    assert(l.info? == false)
    assert(l.debug? == false)
    l.debug "debug message should NOT show up"
    l.info "info message should NOT show up"
    l.warn "warn messge should show up. 3 total"
    l.error "error messge should show up. 3 total"
    l.fatal "fatal messge should show up. 3 total"
    l.level = ERROR
    assert(l.level == ERROR, "level wasn't changed")
    assert(l.fatal? == true)
    assert(l.error? == true)
    assert(l.warn? == false)
    assert(l.info? == false)
    assert(l.debug? == false)
    l.debug "debug message should NOT show up"
    l.info "info message should NOT show up"
    l.warn "warn messge should NOT show up."
    l.error "error messge should show up. 2 total"
    l.fatal "fatal messge should show up. 2 total"
    l.level = WARN
  end
  def test_log_blocks
    l = Logger.new 'logblocks'
    l.level = WARN
    l.add(Outputter.stdout)
    assert_no_exception {
      l.debug { puts "should not show up"; "LOGBLOCKS" }
      l.fatal { puts "should show up"; "LOGBLOCKS" }
      l.fatal { nil }
      l.fatal {}
    }
  end
  def test_heiarchial_logging
    a = Logger.new("one")
    a.add(StdoutOutputter.new 'so1')
    b = Logger.new("one::two")
    b.add(StdoutOutputter.new 'so2')
    c = Logger.new("one::two::three")
    c.add(StdoutOutputter.new 'so3')
    d = Logger.new("one::two::three::four")
    d.add(StdoutOutputter.new 'so4')
    d.additive = false
    e = Logger.new("one::two::three::four::five")
    e.add(StdoutOutputter.new 'so5')

    a.fatal "statement from a should show up once"
    b.fatal "statement from b should show up twice"
    c.fatal "statement from c should show up thrice"
    d.fatal "statement from d should show up once"
    e.fatal "statement from e should show up twice"
  end
  def test_multi_outs
    f1 = FileOutputter.new('f1', :filename => "./junk/tmp1.log", :level=>ALL)
    f2 = FileOutputter.new('f2', :filename => "./junk/tmp2.log", :level=>DEBUG)
    f3 = FileOutputter.new('f3', :filename => "./junk/tmp3.log", :level=>ERROR)
    f4 = FileOutputter.new('f4', :filename => "./junk/tmp4.log", :level=>FATAL)

    l = Logger.new("multi")
    l.add(f1, f3, f4)

    a = Logger.new("multi::multi2")
    a.level = ERROR
    a.add(f2, f4)
    
    l.debug "debug test_multi_outputters"
    l.info "info test_multi_outputters"
    l.warn "warn test_multi_outputters"
    l.error "error test_multi_outputters"
    l.fatal "fatal test_multi_outputters"

    a.debug "debug test_multi_outputters"
    a.info "info test_multi_outputters"
    a.warn "warn test_multi_outputters"
    a.error "error test_multi_outputters"
    a.fatal "fatal test_multi_outputters"
    
    f1.close; f2.close; f3.close; f4.close
  end
  def test_custom_formatter
    l = Logger.new('custom_formatter')
    o = StdoutOutputter.new('formatter'=>MyFormatter1.new)
    l.add o
    l.error "try myformatter1"
    l.fatal "try myformatter1"
    o.formatter = MyFormatter2.new
    l.error "try formatter2"
    l.fatal "try formatter2"
  end
end 
