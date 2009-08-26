# Adapted from mocha (http://mocha.rubyforge.org/).
#
# Copyright (C) 2006 Revieworld Ltd.
#
# You may use, copy and redistribute this library under the same terms
# as Ruby itself (see www.ruby-lang.org/en/LICENSE.txt) or under the
# MIT license (see MIT-LICENSE file).

require 'test/spec'

begin
  require 'mocha'
rescue LoadError
  context "mocha" do
    specify "can not be found.  BAIL OUT!" do
    end
  end
else

context "mocha" do
  specify "works with test/spec" do
    object = mock()
    object.expects(:expected_method).with(:p1, :p2).returns(:result)
    object.expected_method(:p1, :p2).should.equal :result
  end
end

class Enterprise
  def initialize(dilithium)
    @dilithium = dilithium
  end

  def go(warp_factor)
    warp_factor.times { @dilithium.nuke(:anti_matter) }
  end
end

context "mocha" do
  specify "works with test/spec and Enterprise example" do
    dilithium = mock()
    dilithium.expects(:nuke).with(:anti_matter).at_least_once  # auto-verified at end of test
    enterprise = Enterprise.new(dilithium)
    enterprise.go(2)
  end
end

class Order
  attr_accessor :shipped_on

  def total_cost
    line_items.inject(0) { |total, line_item| total + line_item.price } + shipping_cost
  end

  def total_weight
    line_items.inject(0) { |total, line_item| total + line_item.weight }
  end

  def shipping_cost
    total_weight * 5 + 10
  end

  class << self

    def find_all
      # Database.connection.execute('select * from orders...
    end

    def number_shipped_since(date)
      find_all.select { |order| order.shipped_on > date }.size
    end

    def unshipped_value
      find_all.inject(0) { |total, order| order.shipped_on ? total : total + order.total_cost }
    end

  end

end

context "stubba" do
  specify "works with test/spec and instance method stubbing" do
    order = Order.new
    order.stubs(:total_weight).returns(10)
    order.shipping_cost.should.equal 60
  end

  specify "works with test/spec and class method stubbing" do
    now = Time.now; week_in_secs = 7 * 24 * 60 * 60
    order_1 = Order.new; order_1.shipped_on = now - 1 * week_in_secs
    order_2 = Order.new; order_2.shipped_on = now - 3 * week_in_secs
    Order.stubs(:find_all).returns([order_1, order_2])
    Order.number_shipped_since(now - 2 * week_in_secs).should.equal 1
  end

  specify "works with test/spec and global instance method stubbing" do
    Order.stubs(:find_all).returns([Order.new, Order.new, Order.new])
    Order.any_instance.stubs(:shipped_on).returns(nil)
    Order.any_instance.stubs(:total_cost).returns(10)
    Order.unshipped_value.should.equal 30
  end
end

end                             # if not rescue LoadError

