require File.dirname(__FILE__) + "/../../test_helper"


class BitVectorTest < Test::Unit::TestCase
  include Ferret::Utils

  def test_bv_get_set()
    bv = BitVector.new
    assert_equal 0, bv.count 

    bv.set 10 
    assert bv.get(10) 
    assert bv[10] 
    assert_equal 1, bv.count 

    bv[10] = false
    assert ! bv[10] 

    bv[10] = true
    assert bv[10] 

    bv[10] = nil
    assert ! bv[10] 

    bv[10] = true
    assert bv[10] 

    bv.unset 10 
    assert ! bv[10] 

    bv[10] = true
    assert bv[10] 
  end

  def test_bv_count()
    bv = BitVector.new
    bv.set 10 
    assert_equal 1, bv.count 

    bv.set 20 
    assert bv.get(20) 
    assert_equal 2, bv.count 

    bv.set 21 
    assert bv.get(21) 
    assert_equal 3, bv.count 

    bv.unset 21 
    assert ! bv.get(21) 
    assert_equal 2, bv.count 

    bv[20] = nil
    assert ! bv.get(20) 
    assert_equal 1, bv.count 

    (50..100).each {|i| bv.set i }
    (50..100).each {|i| assert bv[i] }
    assert bv.get(10) 
    assert_equal 52, bv.count 

    bv.clear
    assert_equal 0, bv.count
    (50..100).each {|i| assert ! bv[i] }
    assert ! bv.get(10) 
  end

  def test_bv_eql_hash
    bv1 = BitVector.new
    bv2 = BitVector.new
    assert_equal(bv1, bv2)
    assert_equal(bv1.hash, bv2.hash)

    bv1.set(10)
    assert_not_equal(bv1, bv2)
    assert_not_equal(bv1.hash, bv2.hash)

    bv2.set(10)
    assert_equal(bv1, bv2)
    assert_equal(bv1.hash, bv2.hash)

    10.times {|i| bv1.set(i * 31)}
    assert_not_equal(bv1, bv2)
    assert_not_equal(bv1.hash, bv2.hash)

    10.times {|i| bv2.set(i * 31)}
    assert_equal(bv1, bv2)
    assert_equal(bv1.hash, bv2.hash)

    bv1.clear
    assert_not_equal(bv1, bv2)
    assert_not_equal(bv1.hash, bv2.hash)

    bv2.clear
    assert_equal(bv1, bv2)
    assert_equal(bv1.hash, bv2.hash)
  end

  BV_COUNT = 500
  BV_SIZE = 1000

  def test_bv_and
    bv1 = BitVector.new
    bv2 = BitVector.new
    set1 = set2 = count = 0

    BV_COUNT.times do |i|
      bit = rand(BV_SIZE)
      bv1.set(bit)
      set1 |= (1 << bit)
    end

    BV_COUNT.times do |i|
      bit = rand(BV_SIZE)
      bv2.set(bit)
      bitmask = (1 << bit)
      if ((set1 & bitmask) > 0) && ((set2 & bitmask) == 0)
        set2 |= (1 << bit)
        count += 1
      end
    end

    and_bv = bv1 & bv2
    assert_equal(count, and_bv.count)
    BV_SIZE.times do |i|
      assert_equal(((set2 & (1 << i)) > 0), and_bv[i])
    end

    bv2.and! bv1
    assert_equal(bv2, and_bv)

    bv2 = BitVector.new
    and_bv = bv1 & bv2

    assert_equal(bv2, and_bv, "and_bv should be empty")
    assert_equal(0, and_bv.count)

    bv1 = BitVector.new
    bv2 = BitVector.new.not!
    bv1.set(10)
    bv1.set(11)
    bv1.set(20)
    assert_equal(bv1, bv1 & bv2, "bv anded with empty not bv should be same")
  end
  
  def test_bv_or
    bv1 = BitVector.new
    bv2 = BitVector.new
    set = count = 0

    BV_COUNT.times do |i|
      bit = rand(BV_SIZE)
      bv1.set(bit)
      bitmask = (1 << bit)
      if (set & bitmask) == 0
        count += 1
        set |= bitmask
      end
    end

    BV_COUNT.times do |i|
      bit = rand(BV_SIZE)
      bv2.set(bit)
      bitmask = (1 << bit)
      if (set & bitmask) == 0
        count += 1
        set |= bitmask
      end
    end

    or_bv = bv1 | bv2
    assert_equal(count, or_bv.count)
    BV_SIZE.times do |i|
      assert_equal(((set & (1 << i)) > 0), or_bv[i])
    end

    bv2.or! bv1
    assert_equal(bv2, or_bv)

    bv2 = BitVector.new
    or_bv = bv1 | bv2

    assert_equal(bv1, or_bv)
  end
  
  def test_bv_xor
    bv1 = BitVector.new
    bv2 = BitVector.new
    set1 = set2 = count = 0

    BV_COUNT.times do |i|
      bit = rand(BV_SIZE)
      bv1.set(bit)
      set1 |= (1 << bit)
    end

    BV_COUNT.times do |i|
      bit = rand(BV_SIZE)
      bv2.set(bit)
      set2 |= (1 << bit)
    end

    bitmask = 1
    set1 ^= set2
    BV_SIZE.times do |i|
      count += 1 if (set1 & bitmask) > 0
      bitmask <<= 1
    end

    xor_bv = bv1 ^ bv2
    BV_SIZE.times do |i|
      assert_equal(((set1 & (1 << i)) > 0), xor_bv[i])
    end
    assert_equal(count, xor_bv.count)

    bv2.xor! bv1
    assert_equal(bv2, xor_bv)

    bv2 = BitVector.new
    xor_bv = bv1 ^ bv2

    assert_equal(bv1, xor_bv)
  end

  def test_bv_not
    bv = BitVector.new
    [1, 5, 25, 41, 97, 185].each {|i| bv.set(i)}
    not_bv = ~bv
    assert_equal(bv.count, not_bv.count)
    200.times {|i| assert(bv[i] != not_bv[i])}

    not_bv.not!
    assert_equal(bv, not_bv)
  end


  SCAN_SIZE = 200
  SCAN_INC = 97

  def test_scan
    bv = BitVector.new

    SCAN_SIZE.times {|i| bv.set(i * SCAN_INC)}
    not_bv = ~bv

    SCAN_SIZE.times do |i|
      assert_equal(i * SCAN_INC, bv.next_from((i - 1) * SCAN_INC + 1))
      assert_equal(i * SCAN_INC, not_bv.next_unset_from((i - 1) * SCAN_INC + 1))
    end
    assert_equal(-1, bv.next_from((SCAN_SIZE - 1) * SCAN_INC + 1))
    assert_equal(-1, not_bv.next_unset_from((SCAN_SIZE - 1) * SCAN_INC + 1))

    bit = 0
    bv.each {|i| assert_equal(bit, i); bit += SCAN_INC }
    assert_equal(bit, SCAN_SIZE * SCAN_INC)

    bit = 0
    not_bv.each {|i| assert_equal(bit, i); bit += SCAN_INC }
    assert_equal(bit, SCAN_SIZE * SCAN_INC)

    bv.reset_scan
    not_bv.reset_scan
    SCAN_SIZE.times do |i|
      assert_equal(i * SCAN_INC, bv.next)
      assert_equal(i * SCAN_INC, not_bv.next_unset)
    end
    assert_equal(-1, bv.next)
    assert_equal(-1, not_bv.next_unset)

    bv.clear
    SCAN_SIZE.times {|i| bv.set(i)}
    not_bv = ~bv

    SCAN_SIZE.times do |i|
      assert_equal(i, bv.next)
      assert_equal(i, not_bv.next_unset)
    end
    assert_equal(-1, bv.next)
    assert_equal(-1, not_bv.next_unset)

    bit = 0
    bv.each {|i| assert_equal(bit, i); bit += 1 }
    assert_equal(bit, SCAN_SIZE)

    bit = 0
    not_bv.each {|i| assert_equal(bit, i); bit += 1 }
    assert_equal(bit, SCAN_SIZE)
  end

  def test_to_a
    bv = BitVector.new
    ary = (1..100).collect { rand(1000) }.sort.uniq
    ary.each {|i| bv.set(i)}
    assert_equal(ary, bv.to_a)
  end
end
