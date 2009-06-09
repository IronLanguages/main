require "include"
require "runit/cui/testrunner"

class TestPatternFormatter < TestCase
  def test_pattern
    l = Logger.new 'test::this::that'
    l.trace = true
    o = StdoutOutputter.new 'test' 
    l.add o
    assert_no_exception { 
    f = PatternFormatter.new :pattern=> "%d %6l [%C]%c %% %-40.30M"
                             #:date_pattern=> "%Y"
                             #:date_method => :usec
    Outputter['test'].formatter = f
    l.debug "And this?"
    l.info "How's this?"
    l.error "and a really freaking huge line which we hope will be trimmed?"
    e = ArgumentError.new("something barfed")
    e.set_backtrace Array.new(5, "trace junk at thisfile.rb 154")
    l.fatal e
    l.info [1, 3, 5]
    }
  end
end

CUI::TestRunner.run(TestPatternFormatter.suite)
