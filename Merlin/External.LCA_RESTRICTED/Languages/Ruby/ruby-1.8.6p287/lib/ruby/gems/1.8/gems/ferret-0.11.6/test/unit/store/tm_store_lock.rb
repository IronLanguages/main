module StoreLockTest
  class Switch
    @@counter = 0
    def Switch.counter() return @@counter end
    def Switch.counter=(counter) @@counter = counter end
  end

  def test_locking()
    lock_time_out = 0.001 # we want this test to run quickly
    lock1 = @dir.make_lock("l.lck")
    lock2 = @dir.make_lock("l.lck")

    assert(!lock2.locked?)
    assert(lock1.obtain(lock_time_out))
    assert(lock2.locked?)

    assert(! can_obtain_lock?(lock2))

    exception_thrown = false
    begin
      lock2.while_locked(lock_time_out) do
        assert(false, "lock should not have been obtained")
      end
    rescue
      exception_thrown = true
    ensure
      assert(exception_thrown)
    end

    lock1.release()
    assert(lock2.obtain(lock_time_out))
    lock2.release()

    t = Thread.new() do
      lock1.while_locked(lock_time_out) do
        Switch.counter = 1
        # make sure lock2 obtain test was run
        while Switch.counter < 2
        end
        Switch.counter = 3
      end
    end
    t.run()

    #make sure thread has started and lock been obtained
    while Switch.counter < 1
    end
    
    assert(! can_obtain_lock?(lock2))

    Switch.counter = 2
    while Switch.counter < 3
    end
    
    assert(lock2.obtain(lock_time_out))
    lock2.release()
  end

  def can_obtain_lock?(lock)
    lock_time_out = 0.001 # we want this test to run quickly
    begin
      lock.obtain(lock_time_out)
      return true
    rescue
    end
    return false
  end
end
