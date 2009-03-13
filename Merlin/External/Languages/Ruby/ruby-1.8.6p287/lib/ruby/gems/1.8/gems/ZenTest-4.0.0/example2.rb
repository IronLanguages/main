module TestSomething
  class TestThingy
    def test_do_something_normal
      thingy = Thingy.new
      result = thingy.do_something
      assert(result.blahblah)
    end
    def test_do_something_edgecase
      thingy = Thingy.new
      result = thingy.do_something
      assert(result.blahblah)
    end
  end
end

