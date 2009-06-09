module StoreTest
  # declare dir so inheritors can access it.
  attr_accessor :dir

  # test the basic file manipulation methods;
  # - exists?
  # - touch
  # - delete
  # - file_count
  def test_basic_file_ops
    assert_equal(0, @dir.file_count(), "directory should be empty")
    assert(! @dir.exists?('filename'), "File should not exist")
    @dir.touch('tmpfile1')
    assert_equal(1, @dir.file_count(), "directory should have one file")
    @dir.touch('tmpfile2')
    assert_equal(2, @dir.file_count(), "directory should have two files")
    assert(@dir.exists?('tmpfile1'), "'tmpfile1' should exist")
    @dir.delete('tmpfile1')
    assert(! @dir.exists?('tmpfile1'), "'tmpfile1' should no longer exist")
    assert_equal(1, @dir.file_count(), "directory should have one file")
  end
  
  def test_rename
    @dir.touch("from")
    assert(@dir.exists?('from'), "File should exist")
    assert(! @dir.exists?('to'), "File should not exist")
    cnt_before = @dir.file_count()
    @dir.rename('from', 'to')
    cnt_after = @dir.file_count()
    assert_equal(cnt_before, cnt_after, "the number of files shouldn't have changed")
    assert(@dir.exists?('to'), "File should now exist")
    assert(! @dir.exists?('from'), "File should no longer exist")
  end
end
