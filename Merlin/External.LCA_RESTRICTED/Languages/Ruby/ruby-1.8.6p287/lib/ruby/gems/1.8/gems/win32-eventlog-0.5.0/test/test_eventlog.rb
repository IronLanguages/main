##############################################################################
# test_eventlog.rb
#
# Test case for the win32-eventlog package. You should run this test case
# via the 'rake test' Rakefile task. This test will take a minute or two
# to complete.
#############################################################################
require 'rubygems'
gem 'test-unit'
require 'test/unit'
require 'win32/eventlog'
require 'socket'
include Win32

print "\nRelax - this will take a few moments\n\n"

class TC_EventLog < Test::Unit::TestCase
   def setup
      @log      = EventLog.new('Application')
      @logfile  = 'temp.evt'
      @bakfile  = 'C:\event_log.bak'
      @hostname = Socket.gethostname
      @records  = []
      @last     = nil
   end
 
   def test_version
      assert_equal('0.5.0', EventLog::VERSION)
   end
   
   # Use the alias to validate it as well.
   def test_constructor
      assert_respond_to(EventLog, :open)
      assert_nothing_raised{ EventLog.open }
      assert_nothing_raised{ EventLog.open{ |log| } }
      assert_nothing_raised{ EventLog.open('System') }
      assert_nothing_raised{ EventLog.open('System', @hostname) }
   end
   
   def test_constructor_expected_errors
      assert_raises(EventLog::Error){ EventLog.new('System', @hostname, 'foo') }
      assert_raises(TypeError){ EventLog.open(1) }
      assert_raises(TypeError){ EventLog.open('System', 1) }
   end
   
   def test_constructor_instance_variables
      assert_nothing_raised{ @log = EventLog.new('Application', @hostname) }
      assert_equal(@hostname, @log.server)
      assert_equal('Application', @log.source)
   end
   
   def test_open_backup
      assert_respond_to(EventLog, :open_backup)
      assert_nothing_raised{ EventLog.new('Application').backup(@bakfile) }
      assert_nothing_raised{ @log = EventLog.open_backup(@bakfile) }
      assert_kind_of(EventLog, @log)
      assert_nothing_raised{ @log.read{ break } }
      assert_nothing_raised{ @log.close }
   end
  
   # Ensure that an Array is returned in non-block form and that none of the
   # descriptions are nil.
   # 
   # The test for descriptions was added as a result of ruby-talk:116528.
   # Thanks go to Joey Gibson for the spot.  The test for unique record
   # numbers was added to ensure no dups.
   # 
   def test_class_read_verification
      assert_nothing_raised{ @array = EventLog.read }
      assert_kind_of(Array, @array)
      
      record_numbers = []
      @array.each{ |log|
         assert_not_nil(log.description)
         assert_equal(false, record_numbers.include?(log.record_number))
         record_numbers << log.record_number
      }
   end
  
   # I've added explicit breaks because an event log could be rather large.
   # 
   def test_class_read_basic    
      assert_nothing_raised{ EventLog.read{ break } }    
      assert_nothing_raised{ EventLog.read("Application"){ break } }
      assert_nothing_raised{ EventLog.read("Application", nil){ break } }
      assert_nothing_raised{ EventLog.read("Application", nil, nil){ break } }
      assert_nothing_raised{ EventLog.read("Application", nil, nil, 10){ break } } 
   end   
   
   def test_class_read_expected_errors   
      assert_raises(ArgumentError){
         EventLog.read("Application", nil, nil, nil, nil){}
      }
   end
   
   def test_read
      flags = EventLog::FORWARDS_READ | EventLog::SEQUENTIAL_READ
      assert_respond_to(@log, :read)
      assert_nothing_raised{ @log.read{ break } }
      assert_nothing_raised{ @log.read(flags){ break } }
      assert_nothing_raised{ @log.read(flags, 500){ break } }
   end
   
   def test_read_expected_errors
      flags = EventLog::FORWARDS_READ | EventLog::SEQUENTIAL_READ
      assert_raises(ArgumentError){ @log.read(flags, 500, 'foo') }
   end
   
   def test_seek_read
      flags = EventLog::SEEK_READ | EventLog::FORWARDS_READ
      assert_nothing_raised{ @last = @log.read[-10].record_number }
      assert_nothing_raised{
         @records = EventLog.read(nil, nil, flags, @last)
      }
      assert_equal(10, @records.length)
   end
   
   # This test could fail, since a record number + 10 may not actually exist.
   def test_seek_read_backwards
      flags = EventLog::SEEK_READ | EventLog::BACKWARDS_READ
      assert_nothing_raised{ @last = @log.oldest_record_number + 10 }
      assert_nothing_raised{ @records = EventLog.read(nil, nil, flags, @last) }
      assert_equal(11, @records.length)
   end
  
   def test_server
      assert_respond_to(@log, :server)
      assert_raises(NoMethodError){ @log.server = 'foo' }
   end

   def test_source
      assert_respond_to(@log, :source)
      assert_kind_of(String, @log.source)
      assert_raises(NoMethodError){ @log.source = 'foo' }
   end
   
   def test_file
      assert_respond_to(@log, :file)
      assert_nil(@log.file)
      assert_raises(NoMethodError){ @log.file = 'foo' }
   end 
   
   def test_backup
      assert_respond_to(@log, :backup)
      assert_nothing_raised{ @log.backup(@bakfile) }
      assert(File.exists?(@bakfile))
      assert_raises(EventLog::Error){ @log.backup(@bakfile) }
   end
   
   # Since I don't want to actually clear anyone's event log, I can't really
   # verify that it works.
   # 
   def test_clear
      assert_respond_to(@log, :clear)
   end
   
   def test_full
      assert_respond_to(@log, :full?)
      assert_nothing_raised{ @log.full? }
   end
      
   def test_close
      assert_respond_to(@log, :close)
      assert_nothing_raised{ @log.close }
   end
   
   def test_oldest_record_number
      assert_respond_to(@log, :oldest_record_number)
      assert_kind_of(Fixnum, @log.oldest_record_number)
   end
   
   def test_total_records
      assert_respond_to(@log, :total_records)
      assert_kind_of(Fixnum, @log.total_records)
   end
   
   # We can't test that this method actually executes properly since it goes
   # into an endless loop.
   # 
   def test_tail
      assert_respond_to(@log, :tail)
      assert_raises(EventLog::Error){ @log.tail } # requires block
   end
   
   # We can't test that this method actually executes properly since it goes
   # into an endless loop.
   # 
   def test_notify_change
      assert_respond_to(@log, :notify_change)
      assert_raises(EventLog::Error){ @log.notify_change } # requires block
   end
   
   # I can't really do more in depth testing for this method since there
   # isn't an event source I can reliably and safely write to.
   # 
   def test_report_event
      assert_respond_to(@log, :report_event)
      assert_respond_to(@log, :write) # alias
      assert_raises(ArgumentError){ @log.report_event }
   end
  
   def test_read_event_constants
      assert_not_nil(EventLog::FORWARDS_READ)
      assert_not_nil(EventLog::BACKWARDS_READ)
      assert_not_nil(EventLog::SEEK_READ)
      assert_not_nil(EventLog::SEQUENTIAL_READ)
   end

   def test_event_type_constants
      assert_not_nil(EventLog::SUCCESS)
      assert_not_nil(EventLog::ERROR)
      assert_not_nil(EventLog::WARN)
      assert_not_nil(EventLog::INFO)
      assert_not_nil(EventLog::AUDIT_SUCCESS)
      assert_not_nil(EventLog::AUDIT_FAILURE)
   end
   
   def teardown
      @log.close rescue nil
      File.delete(@bakfile) rescue nil
      @logfile  = nil
      @hostname = nil
      @records  = nil
      @last     = nil
   end
end
