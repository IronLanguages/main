require 'test/unit'
require 'fox16'
require 'ftools'
require 'tempfile'

include Fox

class TC_FXFileStream < Test::Unit::TestCase
  def setup
    @filestream = FXFileStream.new
  end

  def test_container
    assert_nil(@filestream.container)
  end
  
  def test_open_non_existing_file
    assert_equal(FXStreamDead, @filestream.direction)
    status = @filestream.open("non_existing_file", FXStreamLoad)
    assert(!status)
    assert_equal(FXStreamDead, @filestream.direction)
  end

  def test_open_existing_file
    assert_equal(FXStreamDead, @filestream.direction)
    status = @filestream.open("README", FXStreamLoad)
    assert(status)
    assert_equal(FXStreamLoad, @filestream.direction)
    status = @filestream.close
    assert(status)
    assert_equal(FXStreamDead, @filestream.direction)
  end

  def test_open_new_file
    assert_equal(FXStreamDead, @filestream.direction)
    status = @filestream.open("goobers", FXStreamSave)
    assert(status)
    assert_equal(FXStreamSave, @filestream.direction)
    status = @filestream.close
    assert(status)
    assert_equal(FXStreamDead, @filestream.direction)
  end
  
  def test_status
    assert_equal(FXStreamOK, @filestream.status)
    @filestream.open("README", FXStreamLoad)
    assert_equal(FXStreamOK, @filestream.status)
    @filestream.close
    assert_equal(FXStreamOK, @filestream.status)
  end
  
  def test_position
    @filestream.open("README", FXStreamLoad)
    assert_equal(0, @filestream.position)
    @filestream.position = 500
    assert_equal(500, @filestream.position)
    @filestream.close
  end
  
  def test_exceptions
    # Non-existing file
    assert_raises(FXStreamNoReadError) {
      FXFileStream.open("non_existing_file", FXStreamLoad) { |s| }
    }

    # Write-only file (i.e. no read permissions)
    tf = Tempfile.new("write_only_file")
    tf.puts("junk")
    tf.close
    File.chmod(0222, tf.path) # --w--w--w-
    assert_raises(FXStreamNoReadError) {
      FXFileStream.open(tf.path, FXStreamLoad) { |s| }
    }

    # Read-only file
    tf = Tempfile.new("read_only_file")
    tf.puts("junk")
    tf.close
    File.chmod(0444, tf.path) # -r--r--r--
    assert_raises(FXStreamNoWriteError) {
      FXFileStream.open(tf.path, FXStreamSave) { |s| }
    }
  end
  
  def teardown
    if File.exists?("goobers")
      File.rm_f("goobers")
    end
  end
end
