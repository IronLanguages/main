require 'test/spec'

class TestTestUnit < Test::Unit::TestCase
  def test_still_works_on_its_own
    assert_equal 1, 1
    assert_raise(RuntimeError) { raise "Error" }
  end

  def test_supports_should_good_enough
    (2 + 3).should.be 5
    lambda { raise "Error" }.should.raise
    assert true
  end
end

context "TestUnit" do
  specify "works inside test/spec" do
    assert_equal 1, 1
    assert_raise(RuntimeError) { raise "Error" }
  end
end
    
