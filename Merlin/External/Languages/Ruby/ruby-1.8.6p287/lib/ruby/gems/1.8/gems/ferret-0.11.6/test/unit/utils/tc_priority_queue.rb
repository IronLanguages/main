require File.dirname(__FILE__) + "/../../test_helper"


class PriorityQueueTest < Test::Unit::TestCase
  include Ferret::Utils

  PQ_STRESS_SIZE = 1000

  def test_pq()
    pq = PriorityQueue.new(4)
    assert_equal(0, pq.size)
    assert_equal(4, pq.capacity)
    pq.insert("bword")
    assert_equal(1, pq.size)
    assert_equal("bword", pq.top)

    pq.insert("cword")
    assert_equal(2, pq.size)
    assert_equal("bword", pq.top)

    pq << "dword"
    assert_equal(3, pq.size)
    assert_equal("bword", pq.top)
    
    pq << "eword"
    assert_equal(4, pq.size)
    assert_equal("bword", pq.top)

    pq << "aword"
    assert_equal(4, pq.size)
    assert_equal("bword", pq.top, "aword < all other elements so ignore")

    pq << "fword"
    assert_equal(4, pq.size)
    assert_equal("cword", pq.top, "bword got pushed off the bottom of the queue")

    assert_equal("cword", pq.pop())
    assert_equal(3, pq.size)
    assert_equal("dword", pq.pop())
    assert_equal(2, pq.size)
    assert_equal("eword", pq.pop())
    assert_equal(1, pq.size)
    assert_equal("fword", pq.pop())
    assert_equal(0, pq.size)
    assert_nil(pq.top)
    assert_nil(pq.pop)
  end

  def test_pq_clear()
    pq = PriorityQueue.new(3)
    pq << "word1"
    pq << "word2"
    pq << "word3"
    assert_equal(3, pq.size)
    pq.clear()
    assert_equal(0, pq.size)
    assert_nil(pq.top)
    assert_nil(pq.pop)
  end

  #define PQ_STRESS_SIZE 1000
  def test_stress_pq
    pq = PriorityQueue.new(PQ_STRESS_SIZE)
    PQ_STRESS_SIZE.times do
      pq.insert("<#{rand(PQ_STRESS_SIZE)}>")
    end

    prev = pq.pop()
    (PQ_STRESS_SIZE - 1).times do
      curr = pq.pop()
      assert(prev <= curr, "#{prev} should be less than #{curr}")
      prev = curr
    end
    pq.clear()
  end

  def test_pq_block
    pq = PriorityQueue.new(21) {|a, b| a > b}
    100.times do
      pq.insert("<#{rand(50)}>")
    end

    prev = pq.pop()
    20.times do
      curr = pq.pop()
      assert(prev >= curr, "#{prev} should be greater than #{curr}")
      prev = curr
    end
    assert_equal 0, pq.size 
  end

  def test_pq_proc
    pq = PriorityQueue.new({:less_than => lambda {|a, b| a.size > b.size}, :capacity => 21})
    100.times do
      pq.insert("x" * rand(50))
    end

    prev = pq.pop()
    20.times do
      curr = pq.pop()
      assert(prev.size >= curr.size, "#{prev} should be greater than #{curr}")
      prev = curr
    end
    assert_equal 0, pq.size 
  end
end
