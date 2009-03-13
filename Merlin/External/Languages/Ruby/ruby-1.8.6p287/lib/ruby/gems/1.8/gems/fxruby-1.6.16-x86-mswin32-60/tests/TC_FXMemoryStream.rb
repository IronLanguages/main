require 'test/unit'
require 'fox16'

include Fox

class TC_FXMemoryStream < Test::Unit::TestCase

  DEFAULT_BUFFER_SIZE = 16

  private
  
  def assert_closed(stream)
    stream.open(FXStreamSave, nil) == false
  end

  public

  def test_open_streamload_nil_data
    s = FXMemoryStream.new
    assert(s.open(FXStreamLoad, nil))
    assert_equal(DEFAULT_BUFFER_SIZE, s.space)
    s.close
  end

  def test_open_streamsave_nil_data
    s = FXMemoryStream.new
    assert(s.open(FXStreamSave, nil))
    assert_equal(DEFAULT_BUFFER_SIZE, s.space)
    s.close
  end

  def test_open_streamload_unknown_size
  	s = FXMemoryStream.new
  	assert(s.open(FXStreamLoad, "foo"))
  	s.close
	end

	def test_open_streamsave_unknown_size
		s = FXMemoryStream.new
		assert(s.open(FXStreamSave, "foo"))
		s.close
	end

  def test_open_s_load_nil_buffer
    s = FXMemoryStream.open(FXStreamLoad, nil)
    assert_equal(DEFAULT_BUFFER_SIZE, s.space)
    s.close
  end

  def test_open_s_save_nil_buffer
    s = FXMemoryStream.open(FXStreamSave, nil)
    assert_equal(DEFAULT_BUFFER_SIZE, s.space)
    s.close
  end

  def test_open_s_load_unknown_buffer_size
    s = FXMemoryStream.open(FXStreamLoad, "foo")
    assert_equal(3, s.space)
    s.close
  end

  def test_open_s_save_unknown_buffer_size
    s = FXMemoryStream.open(FXStreamSave, "foo")
    assert_equal(3, s.space)
    s.close
  end

  def test_open_s_with_block
    stream = nil
    FXMemoryStream.open(FXStreamLoad, "foo") do |s|
      stream = s
    end
    assert_closed(stream)
  end

  def test_setSpace
    FXMemoryStream.open(FXStreamSave, nil) do |stream|
      stream.space = 5000
      assert_equal(5000, stream.space)
    end
  end

  def test_takeBuffer_empty
    FXMemoryStream.open(FXStreamSave, nil) do |stream|
      buffer = stream.takeBuffer
      assert_equal("\0" * 16, buffer)
    end
  end

  def test_giveBuffer
    FXMemoryStream.open(FXStreamLoad, nil) do |stream|
      assert_equal(DEFAULT_BUFFER_SIZE, stream.space) 
      stream.giveBuffer("foo")
      assert_equal(3, stream.space)
    end
  end
end
