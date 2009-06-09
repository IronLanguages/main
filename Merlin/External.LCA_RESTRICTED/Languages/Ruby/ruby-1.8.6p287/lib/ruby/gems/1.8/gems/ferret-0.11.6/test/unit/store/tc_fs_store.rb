require File.dirname(__FILE__) + "/../../test_helper"
require File.dirname(__FILE__) + "/tm_store"
require File.dirname(__FILE__) + "/tm_store_lock"

class FSStoreTest < Test::Unit::TestCase
  include Ferret::Store
  include StoreTest
  include StoreLockTest
  def setup
    @dpath = File.expand_path(File.join(File.dirname(__FILE__),
                       '../../temp/fsdir'))
    @dir = FSDirectory.new(@dpath, true)
  end

  def teardown
    @dir.refresh()
    @dir.close()
  end

  def test_fslock
    lock_name = "lfile"
    lock_file_path = make_lock_file_path(lock_name)
    assert(! File.exists?(lock_file_path), "There should be no lock file")
    lock = @dir.make_lock(lock_name)
    assert(! File.exists?(lock_file_path), "There should still be no lock file")
    assert(! lock.locked?,                 "lock shouldn't be locked yet")

    lock.obtain

    assert(lock.locked?,                   "lock should now be locked")

    assert(File.exists?(lock_file_path),   "A lock file should have been created")

    assert(@dir.exists?(lfname(lock_name)),"The lock should exist")

    lock.release

    assert(! lock.locked?,                 "lock should be freed again")
    assert(! File.exists?(lock_file_path), "The lock file should have been deleted")
  end

#  def make_and_loose_lock
#    lock = @dir.make_lock("finalizer_lock")
#    lock.obtain
#    lock = nil
#  end
#
#  def test_fslock_finalizer
#    lock_name = "finalizer_lock"
#    lock_file_path = make_lock_file_path(lock_name)
#    assert(! File.exists?(lock_file_path), "There should be no lock file")
#
#    make_and_loose_lock
#
#    #assert(File.exists?(lock_file_path), "There should now be a lock file")
#
#    lock = @dir.make_lock(lock_name)
#    assert(lock.locked?, "lock should now be locked")
#
#    GC.start
#
#    assert(! lock.locked?, "lock should be freed again")
#    assert(! File.exists?(lock_file_path), "The lock file should have been deleted")
#  end
#
  def make_lock_file_path(name)
    lock_file_path = File.join(@dpath, lfname(name))
    if File.exists?(lock_file_path) then
      File.delete(lock_file_path)
    end
    return lock_file_path
  end

  def lfname(name)
    "ferret-#{name}.lck"
  end
end
