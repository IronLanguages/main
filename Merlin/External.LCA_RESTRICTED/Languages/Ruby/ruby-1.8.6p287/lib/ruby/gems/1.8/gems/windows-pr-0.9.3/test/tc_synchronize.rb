##################################################################
# tc_synchronize.rb
#
# Test case for the windows/synchronize package.
##################################################################
require "windows/synchronize"
require "test/unit"

class SynchFoo
   include Windows::Synchronize
end

class TC_Windows_Synchronize < Test::Unit::TestCase
   def setup
      @handle = (0.chr * 16).unpack('LLLL')
      @foo    = SynchFoo.new
   end
   
   def test_numeric_constants
      assert_not_nil(SynchFoo::INFINITE)
      assert_not_nil(SynchFoo::WAIT_OBJECT_0)
      assert_not_nil(SynchFoo::WAIT_TIMEOUT)
      assert_not_nil(SynchFoo::WAIT_ABANDONED)
      assert_not_nil(SynchFoo::WAIT_FAILED)
      assert_not_nil(SynchFoo::QS_ALLEVENTS)
      assert_not_nil(SynchFoo::QS_ALLINPUT)
      assert_not_nil(SynchFoo::QS_ALLPOSTMESSAGE)
      assert_not_nil(SynchFoo::QS_HOTKEY)
      assert_not_nil(SynchFoo::QS_INPUT)
      assert_not_nil(SynchFoo::QS_KEY)
      assert_not_nil(SynchFoo::QS_MOUSE)
      assert_not_nil(SynchFoo::QS_MOUSEBUTTON)
      assert_not_nil(SynchFoo::QS_MOUSEMOVE)
      assert_not_nil(SynchFoo::QS_PAINT)
      assert_not_nil(SynchFoo::QS_POSTMESSAGE)
      assert_not_nil(SynchFoo::QS_RAWINPUT)
      assert_not_nil(SynchFoo::QS_SENDMESSAGE)
      assert_not_nil(SynchFoo::QS_TIMER)
      assert_not_nil(SynchFoo::MWMO_ALERTABLE)
      assert_not_nil(SynchFoo::MWMO_INPUTAVAILABLE)
      assert_not_nil(SynchFoo::MWMO_WAITALL)
      assert_not_nil(SynchFoo::EVENT_ALL_ACCESS)
      assert_not_nil(SynchFoo::EVENT_MODIFY_STATE)
      assert_not_nil(SynchFoo::MUTEX_ALL_ACCESS)
      assert_not_nil(SynchFoo::MUTEX_MODIFY_STATE)
      assert_not_nil(SynchFoo::SEMAPHORE_ALL_ACCESS)
      assert_not_nil(SynchFoo::SEMAPHORE_MODIFY_STATE)
   end
   
   def test_method_constants
      assert_not_nil(SynchFoo::CreateEvent)
      assert_not_nil(SynchFoo::CreateMutex)
      assert_not_nil(SynchFoo::CreateSemaphore)
      assert_not_nil(SynchFoo::DeleteCriticalSection)
      assert_not_nil(SynchFoo::EnterCriticalSection)
      assert_not_nil(SynchFoo::GetOverlappedResult)
      assert_not_nil(SynchFoo::InitializeCriticalSection)
      assert_not_nil(SynchFoo::InitializeCriticalSectionAndSpinCount)
      assert_not_nil(SynchFoo::LeaveCriticalSection)
      assert_not_nil(SynchFoo::MsgWaitForMultipleObjects)
      assert_not_nil(SynchFoo::MsgWaitForMultipleObjectsEx)
      assert_not_nil(SynchFoo::OpenEvent)
      assert_not_nil(SynchFoo::OpenMutex)
      assert_not_nil(SynchFoo::OpenSemaphore)
      assert_not_nil(SynchFoo::ReleaseMutex)
      assert_not_nil(SynchFoo::ReleaseSemaphore)
      assert_not_nil(SynchFoo::ResetEvent)
      assert_not_nil(SynchFoo::SetEvent)
      assert_not_nil(SynchFoo::WaitForMultipleObjects)
      assert_not_nil(SynchFoo::WaitForMultipleObjectsEx)
      assert_not_nil(SynchFoo::WaitForSingleObject)
      assert_not_nil(SynchFoo::WaitForSingleObjectEx)
   end
   
   def teardown
      @foo = nil
      @handle = nil
   end
end
