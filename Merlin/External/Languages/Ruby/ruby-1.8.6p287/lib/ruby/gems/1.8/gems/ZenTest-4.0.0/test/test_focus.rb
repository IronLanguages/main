require 'rubygems'
require 'minitest/autorun'
require 'focus'

class TestFocus < MiniTest::Unit::TestCase
  def setup
    @x = 1
  end

  def teardown
    assert_equal 2, @x
  end

  def test_focus
    @x += 1
  end

  def test_ignore1
    flunk "ignore me!"
  end

  def test_ignore2
    flunk "ignore me!"
  end

  def test_ignore3
    flunk "ignore me!"
  end

  def test_focus2
    @x += 1
  end

  focus :test_focus, :test_focus2
end
