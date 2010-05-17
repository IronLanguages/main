require File.dirname(__FILE__) + "/../fixtures/classes"

describe :delegate_instantiation, :shared => true do
  before(:each) do
    @class = @method
  end

  it "creates a delegate from bound methods" do
    @class.new(DelegateTester.method(:bar)).should be_kind_of @class 
  end

  it "creates a delegate from lambdas" do
    @class.new(lambda { puts '123' }).should be_kind_of @class
  end

  it "creates a delegate from procs" do
    @class.new(proc { puts '123' }).should be_kind_of @class
  end

  it "creates a delegate from blocks" do
    (@class.new {puts '123'}).should be_kind_of @class
  end

  it "requires an argument" do
    lambda {@class.new}.should raise_error ArgumentError
  end

  it "requires a proc-like object" do
    lambda {@class.new(1)}.should raise_error TypeError
  end
end
