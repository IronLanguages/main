require File.dirname(__FILE__) + "/../../test_helper"
require 'ferret/number_tools'


class NumberToolsTest < Test::Unit::TestCase
  include Ferret::Utils

  def test_to_i_lex_near_zero()
    (-10..10).each do |num|
      assert(num.to_s_lex > (num-1).to_s_lex,
             "Strings should sort correctly but " +
             "#{num.to_s_lex} <= #{(num-1).to_s_lex}")
      assert_equal(num, num.to_s_lex.to_i_lex)
    end
  end

  def test_to_i_pad_near_zero()
    (1..10).each do |num|
      assert(num.to_s_pad(3) > (num-1).to_s_pad(3),
             "Strings should sort correctly but " +
             "#{num.to_s_pad(3)} <= #{(num-1).to_s_pad(3)}")
      assert_equal(num, num.to_s_pad(3).to_i)
    end
  end

  def test_to_i_lex_larger_numbers
    100.times do
      num1 = rand(10000000000000000000000000000000000)
      num2 = rand(10000000000000000000000000000000000)
      num1 *= -1 if rand(2) == 0
      num2 *= -1 if rand(2) == 0

      assert_equal(num1, num1.to_s_lex.to_i_lex)
      assert_equal(num2, num2.to_s_lex.to_i_lex)
      assert_equal(num1 < num2, num1.to_s_lex < num2.to_s_lex, 
                   "Strings should sort correctly but " +
                   "#{num1} < #{num2} == #{num1 < num2} but " +
                   "#{num1.to_s_lex} < #{num2.to_s_lex} == " +
                   "#{num1.to_s_lex < num2.to_s_lex}")
    end
  end

  def test_to_i_pad
    100.times do
      num1 = rand(10000000000000000000000000000000000)
      num2 = rand(10000000000000000000000000000000000)
      assert_equal(num1, num1.to_s_pad(35).to_i)
      assert_equal(num2, num2.to_s_pad(35).to_i)
      assert_equal(num1 < num2, num1.to_s_pad(35) < num2.to_s_pad(35), 
                   "Strings should sort correctly but " +
                   "#{num1} < #{num2} == #{num1 < num2} but " +
                   "#{num1.to_s_pad(35)} < #{num2.to_s_pad(35)} == " +
                   "#{num1.to_s_pad(35) < num2.to_s_pad(35)}")
    end
  end
  
  def test_time_to_s_lex
    t_num = Time.now.to_i - 365*24*60*60 # prevent range error

    10.times do
      t1 = Time.now - rand(t_num)
      t2 = Time.now - rand(t_num)
      assert_equal(t1.to_s, t1.to_s_lex(:second).to_time_lex.to_s)
      assert_equal(t2.to_s, t2.to_s_lex(:second).to_time_lex.to_s)
      [:year, :month, :day, :hour, :minute, :second, :millisecond].each do |prec|
        t1_x = t1.to_s_lex(prec).to_time_lex
        t2_x = t2.to_s_lex(prec).to_time_lex
        assert_equal(t1_x < t2_x, t1.to_s_lex(prec) < t2.to_s_lex(prec), 
                     "Strings should sort correctly but " +
                     "#{t1_x} < #{t2_x} == #{t1_x < t2_x} but " +
                     "#{t1.to_s_lex(prec)} < #{t2.to_s_lex(prec)} == " +
                     "#{t1.to_s_lex(prec) < t2.to_s_lex(prec)}")
      end
    end
  end

  def test_date_to_s_lex
    10.times do
      d1 = Date.civil(rand(2200), rand(12) + 1, rand(28) + 1)
      d2 = Date.civil(rand(2200), rand(12) + 1, rand(28) + 1)
      assert_equal(d1.to_s, d1.to_s_lex(:day).to_date_lex.to_s)
      assert_equal(d2.to_s, d2.to_s_lex(:day).to_date_lex.to_s)
      [:year, :month, :day].each do |prec|
        d1_x = d1.to_s_lex(prec).to_date_lex
        d2_x = d2.to_s_lex(prec).to_date_lex
        assert_equal(d1_x < d2_x, d1.to_s_lex(prec) < d2.to_s_lex(prec), 
                     "Strings should sort correctly but " +
                     "#{d1_x} < #{d2_x} == #{d1_x < d2_x} but " +
                     "#{d1.to_s_lex(prec)} < #{d2.to_s_lex(prec)} == " +
                     "#{d1.to_s_lex(prec) < d2.to_s_lex(prec)}")
      end

    end
  end

  def test_date_time_to_s_lex
    10.times do
      d1 = "#{rand(600) + 1600}-#{rand(12)+1}-#{rand(28)+1} " +
           "#{rand(24)}:#{rand(60)}:#{rand(60)}"
      d2 = "#{rand(600) + 1600}-#{rand(12)+1}-#{rand(28)+1} " +
           "#{rand(24)}:#{rand(60)}:#{rand(60)}"
      d1 = DateTime.strptime(d1, "%Y-%m-%d %H:%M:%S")
      d2 = DateTime.strptime(d2, "%Y-%m-%d %H:%M:%S")
      assert_equal(d1.to_s, d1.to_s_lex(:second).to_date_time_lex.to_s)
      assert_equal(d2.to_s, d2.to_s_lex(:second).to_date_time_lex.to_s)
      [:year, :month, :day, :hour, :minute, :second].each do |prec|
        d1_x = d1.to_s_lex(prec).to_date_lex
        d2_x = d2.to_s_lex(prec).to_date_lex
        assert_equal(d1_x < d2_x, d1.to_s_lex(prec) < d2.to_s_lex(prec), 
                     "Strings should sort correctly but " +
                     "#{d1_x} < #{d2_x} == #{d1_x < d2_x} but " +
                     "#{d1.to_s_lex(prec)} < #{d2.to_s_lex(prec)} == " +
                     "#{d1.to_s_lex(prec) < d2.to_s_lex(prec)}")
      end
    end
  end
end
