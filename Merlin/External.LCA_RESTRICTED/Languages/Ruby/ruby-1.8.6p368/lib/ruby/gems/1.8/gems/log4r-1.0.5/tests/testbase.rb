require "include"
require "runit/cui/testrunner"

# must be run independently
class TestBase < TestCase
  def test_default_const
    Logger.root # create the default levels
    assert_equal(ALL,0)
    assert_equal(DEBUG,1)
    assert_equal(INFO,2)
    assert_equal(WARN,3)
    assert_equal(ERROR,4)
    assert_equal(FATAL,5)
    assert_equal(OFF,6)
    assert_equal(LEVELS, 7)
    assert_equal(LNAMES.size, 7)
  end
  def test_validate
    7.times{|i| assert_no_exception {Log4rTools.validate_level(i)} }
    assert_exception(ArgumentError) {Log4rTools.validate_level(-1)}
    assert_exception(ArgumentError) {Log4rTools.validate_level(LEVELS)}
    assert_exception(ArgumentError) {Log4rTools.validate_level(String)}
    assert_exception(ArgumentError) {Log4rTools.validate_level("bogus")}
  end
  def test_decode_bool
    assert(Log4rTools.decode_bool({:data=>'true'},:data,false) == true)
    assert(Log4rTools.decode_bool({:data=>true},:data,false) == true)
    assert(Log4rTools.decode_bool({:data=>'false'},:data,true) == false)
    assert(Log4rTools.decode_bool({:data=>false},:data,true) == false)
    assert(Log4rTools.decode_bool({:data=>nil},:data,true) == true)
    assert(Log4rTools.decode_bool({:data=>nil},:data,false) == false)
    assert(Log4rTools.decode_bool({:data=>String},:data,true) == true)
    assert(Log4rTools.decode_bool({:data=>String},:data,false) == false)
    assert(Log4rTools.decode_bool({'data'=>'true'},:data,false) == true)
    assert(Log4rTools.decode_bool({'data'=>true},:data,false) == true)
    assert(Log4rTools.decode_bool({'data'=>'false'},:data,true) == false)
    assert(Log4rTools.decode_bool({'data'=>false},:data,true) == false)
    assert(Log4rTools.decode_bool({'data'=>nil},:data,true) == true)
    assert(Log4rTools.decode_bool({'data'=>nil},:data,false) == false)
    assert(Log4rTools.decode_bool({'data'=>String},:data,true) == true)
    assert(Log4rTools.decode_bool({'data'=>String},:data,false) == false)
  end
end

CUI::TestRunner.run(TestBase.suite)
