###############################################################################
# tc_process.rb
#
# Test suite for the win32-process library.  This test suite will start
# at least two instances of Notepad on your system, which will then
# be killed. Requires the sys-proctable package.
#
# I haven't added a lot of test cases for fork/wait because it's difficult
# to run such tests without causing havoc with Test::Unit itself.  Ideas
# welcome.
#
# You should run this test case via the 'rake test' task.
###############################################################################
require 'test/unit'
require 'win32/process'

begin
   require 'sys/proctable'
rescue LoadError => e
   site = 'http://www.rubyforge.org/projects/sysutils'
   msg = %Q{
      Stopping! The sys-proctable library is required to run this test suite.
      You can find it at #{site} or the RAA.
      Or, you can just 'gem install sys-proctable' and try again.
   }
   STDERR.puts msg
   exit!
end   

include Sys

IO.popen("notepad")
IO.popen("notepad")
sleep 1

$pids = []
ProcTable.ps{ |s|
   next unless s.comm =~ /notepad/i
   $pids.push(s.pid)
}

class TC_Win32Process < Test::Unit::TestCase  
   def setup
      @pids = $pids
      @pid = nil
   end
  
   def test_version
      assert_equal('0.5.9', Process::WIN32_PROCESS_VERSION)
   end
   
   def test_kill
      assert_respond_to(Process, :kill)
   end
   
   def test_kill_expected_errors
      assert_raises(ArgumentError){ Process.kill }
      assert_raises(Process::Error){ Process.kill('SIGBOGUS') }
      assert_raises(Process::Error){ Process.kill(0,9999999) }
   end
   
   def test_kill_signal_0
      pid = @pids.first
      assert_nothing_raised{ Process.kill(0, pid) }
   end
   
   def test_kill_signal_1
      pid = @pids.shift
      assert_nothing_raised{ Process.kill(1, pid) }
   end
   
   def test_kill_signal_9
      pid = @pids.pop
      msg = "Could not find pid #{pid}"
      assert_nothing_raised(msg){ Process.kill(9,pid) }
   end

   def test_fork
      assert_respond_to(Process, :fork)
   end
      	
   def test_create
      assert_respond_to(Process, :create)
      assert_nothing_raised{
         @pid = Process.create(
            :app_name         => "notepad.exe",
            :creation_flags   => Process::DETACHED_PROCESS,
            :process_inherit  => false,
            :thread_inherit   => true,
            :cwd              => "C:\\"
         ).process_id
      }
      assert_nothing_raised{ Process.kill(1, @pid) }
   end
   
   def test_create_expected_errors
      assert_raises(TypeError){ Process.create("bogusapp.exe") }
      assert_raises(Process::Error){
         Process.create(:app_name => "bogusapp.exe")
      }
   end
   
   def test_wait
      assert_respond_to(Process, :wait)
   end
   
   def test_wait2
      assert_respond_to(Process, :wait2)
   end
   
   def test_waitpid
      assert_respond_to(Process, :waitpid)
   end
   
   def test_waitpid2
      assert_respond_to(Process, :waitpid2)
   end

   def test_ppid
      assert_respond_to(Process, :ppid)
      assert_equal(true, Process.ppid > 0)
      assert_equal(false, Process.pid == Process.ppid)
   end
  	
   def test_creation_constants
      assert_not_nil(Process::CREATE_DEFAULT_ERROR_MODE)
      assert_not_nil(Process::CREATE_NEW_CONSOLE)
      assert_not_nil(Process::CREATE_NEW_PROCESS_GROUP)
      assert_not_nil(Process::CREATE_NO_WINDOW)
      assert_not_nil(Process::CREATE_SEPARATE_WOW_VDM)
      assert_not_nil(Process::CREATE_SHARED_WOW_VDM)
      assert_not_nil(Process::CREATE_SUSPENDED)
      assert_not_nil(Process::CREATE_UNICODE_ENVIRONMENT)
      assert_not_nil(Process::DEBUG_ONLY_THIS_PROCESS)
      assert_not_nil(Process::DEBUG_PROCESS)
      assert_not_nil(Process::DETACHED_PROCESS)
   end
   
   def teardown
      @pids = []
   end     
end
