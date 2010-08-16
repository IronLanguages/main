# Adapted from flexmock (http://onestepback.org/software/flexmock).
#
# Copyright 2006 by Jim Weirich (jweirich@one.net).
# All rights reserved.

# Permission is granted for use, copying, modification, distribution,
# and distribution of modified versions of this work as long as the
# above copyright notice is included.


require 'test/spec'

begin
  require 'flexmock'
rescue LoadError
  context "flexmock" do
    specify "can not be found.  BAIL OUT!" do
    end
  end
else

context "flexmock" do
  include FlexMock::TestCase
    
  setup do
    @mock = FlexMock.new
  end

  specify "should receive and return" do
    args = nil
    @mock.should_receive(:hi).and_return { |a, b| args = [a,b] }
    @mock.hi(1,2)
    args.should.equal [1,2]
  end

  specify "should receive without a block" do
    lambda {
      @mock.should_receive(:blip)
      @mock.blip
    }.should.not.raise
  end

  specify "should receive and return with a block" do
    called = false
    @mock.should_receive(:blip).and_return { |block| block.call }
    @mock.blip { called = true }
    called.should.be true
  end

  specify "should have a return value" do
    @mock.should_receive(:blip).and_return { 10 }
    @mock.blip.should.equal 10
  end

  specify "should handle missing methods" do
    ex = lambda {
      @mock.not_defined
    }.should.raise(NoMethodError)
    ex.message.should.match(/not_defined/)
  end

  specify "should ignore missing methods" do
    lambda {
      @mock.should_ignore_missing
      @mock.blip
    }.should.not.raise
  end

  specify "should count correctly" do
    @mock.should_receive(:blip).times(3)
    @mock.blip
    @mock.blip
    @mock.blip
    lambda { @mock.flexmock_verify }.should.not.raise Test::Unit::AssertionFailedError
  end

  specify "should raise on bad counts" do
    @mock.should_receive(:blip).times(3)
    @mock.blip
    @mock.blip
    lambda { @mock.flexmock_verify }.should.raise Test::Unit::AssertionFailedError
  end

  specify "should handle undetermined counts" do
    lambda {
      FlexMock.use('fs') { |m|
        m.should_receive(:blip)
        m.blip
        m.blip
        m.blip
      }
    }.should.not.raise Test::Unit::AssertionFailedError
  end

  specify "should handle zero counts" do
    lambda {
      FlexMock.use { |m|
        m.should_receive(:blip).never
        m.blip
      }
    }.should.raise Test::Unit::AssertionFailedError
  end

  specify "should have file IO with use" do
    file = FlexMock.use do |m|
      filedata = ["line 1", "line 2"]
      m.should_receive(:gets).times(3).and_return { filedata.shift }
      count_lines(m).should.equal 2
    end
  end

  def count_lines(stream)
    result = 0
    while line = stream.gets
      result += 1
    end
    result    
  end

  specify "should have use" do
    lambda {
      FlexMock.use do |m|
	m.should_receive(:blip).times(2)
	m.blip
      end
    }.should.raise Test::Unit::AssertionFailedError
  end

  specify "should handle failures during use" do
    ex = lambda {
      FlexMock.use do |m|
	m.should_receive(:blip).times(2)
	xyz
      end
    }.should.raise NameError
    ex.message.should.match(/undefined local variable or method/)
  end

  specify "should deal with sequential values" do
    values = [1,4,9,16]
    @mock.should_receive(:get).and_return { values.shift }
    @mock.get.should.equal 1
    @mock.get.should.equal 4
    @mock.get.should.equal 9
    @mock.get.should.equal 16
  end
  
  specify "respond_to? should return false for non handled methods" do
    @mock.should.not.respond_to :blah
  end

  specify "respond_to? should return true for explicit methods" do
    @mock.should_receive(:xyz)
    @mock.should.respond_to :xyz
  end
  
  specify "respond_to? should return true for missing_methods when should_ignore_missing" do
    @mock.should_ignore_missing
    @mock.should.respond_to :yada
  end

  specify "should raise error on unknown method proc" do
    lambda {
      @mock.method(:xyzzy)
    }.should.raise NameError
  end

  specify "should return callable proc on method" do
    got_it = false
    @mock.should_receive(:xyzzy).and_return { got_it = true }
    method_proc = @mock.method(:xyzzy)
    method_proc.should.not.be.nil
    method_proc.call
    got_it.should.be true
  end

  specify "should return do nothing proc for missing methods" do
    @mock.should_ignore_missing
    method_proc = @mock.method(:plugh)
    method_proc.should.not.be.nil
    lambda { method_proc.call }.should.not.raise
  end
end


class TemperatureSampler
  def initialize(sensor)
    @sensor = sensor
  end

  def average_temp
    total = (0...3).collect { @sensor.read_temperature }.inject { |i, s| i + s }
    total / 3.0
  end
end

context "flexmock" do
  include FlexMock::TestCase

  specify "works with test/spec" do
    sensor = flexmock("temp")
    sensor.should_receive(:read_temperature).times(3).and_return(10, 12, 14)

    sampler = TemperatureSampler.new(sensor)
    sampler.average_temp.should.equal 12
  end
end

end                             # if not rescue LoadError
