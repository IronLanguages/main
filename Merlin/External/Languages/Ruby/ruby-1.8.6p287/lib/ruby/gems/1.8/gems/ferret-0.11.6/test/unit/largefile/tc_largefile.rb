require File.dirname(__FILE__) + "/../../test_helper"

class SampleLargeTest < Test::Unit::TestCase
  include Ferret::Index
  include Ferret::Search
  include Ferret::Store
  include Ferret::Utils
  
  INDEX_DIR = File.dirname(__FILE__) + "/../../temp/largefile"
  RECORDS = 750
  RECORD_SIZE = 10e5
  
  def setup
    @index = Index.new(:path => INDEX_DIR, :create_if_missing => true, :key => :id)
    create_index! if @index.size == 0 or ENV["RELOAD_LARGE_INDEX"]
  end

  def test_file_index_created
    assert @index.size == RECORDS, "Index size should be #{RECORDS}, is #{@index.size}"
  end
  
  def test_keys_work
    @index << {:content => "foo", :id => RECORDS - 4}
    assert @index.size == RECORDS, "Index size should be #{RECORDS}, is #{@index.size}"
  end
  
  def test_read_file_after_two_gigs
    assert @index.reader[RECORDS - 5].load.is_a?Hash
  end
  
  def create_index!
    @@already_built_large_index ||= false
    return if @@already_built_large_index
    @@already_built_large_index = true
    a = "a"
    RECORDS.times { |i|
      seq = (a.succ! + " ") * RECORD_SIZE
      record = {:id => i, :content => seq}
    	@index << record
    	print "i"
    	STDOUT.flush
    }
    puts "o"
    @index.optimize
  end
end
