describe :void_void_delegate_invocation, :shared => true do
  before(:each) do
    @objects = [@class.new { @method.call }]
    [@method, lambda { @method.call }, proc { @method.call }].each do |l|
      @objects << (@class.new(l))
    end
  end
  
  it "can be invoked via #invoke" do
    @objects.each do |delegate|
      delegate.invoke
    end
    ScratchPad.recorded.should == @result * @objects.length
  end
  
  it "can be invoked twice via #invoke" do
    @objects.each do |delegate|
      delegate.invoke
      delegate.invoke
    end
    ScratchPad.recorded.should == @result * 2 * @objects.length
  end
end

describe :void_x_delegate_invocation, :shared => true do
  before(:each) do
    @objects = [@class.new {|args| @method.call(args) }]
    [@method, lambda { |args| @method.call(args) }, proc {|args| @method.call(args) }].each do |l|
      @objects << (@class.new(l))
    end
  end
  
  it "can be invoked via #invoke" do
    @objects.each do |delegate|
      delegate.invoke(@args[0])
    end
    ScratchPad.recorded.should == [ @result[0] ] * @objects.length
  end
  
  it "can be invoked twice via #invoke" do
    @objects.each do |delegate|
      delegate.invoke(@args[0])
      delegate.invoke(@args[0])
    end
    ScratchPad.recorded.should == [ @result[0] ] * 2 * @objects.length
  end
  
  it "can be invoked twice via #invoke without caching" do
    @objects.each do |delegate|
      delegate.invoke(@args[0])
      delegate.invoke(@args[1])
    end
    ScratchPad.recorded.should == @result * @objects.length
  end
end

describe :x_void_delegate_invocation, :shared => true do
  before(:each) do
    @objects = [@class.new { @method.call }]
    [@method, lambda { @method.call }, proc { @method.call }].each do |l|
      @objects << (@class.new(l))
    end
  end
  
  it "can be invoked via #invoke" do
    @objects.each do |delegate|
      delegate.invoke.should == @return
    end
  end
end

describe :x_x_delegate_invocation, :shared => true do
  before(:each) do
    @objects = [@class.new {|args| @method.call(args) }]
    [@method, lambda { |args| @method.call(args) }, proc {|args| @method.call(args) }].each do |l|
      @objects << (@class.new(l))
    end
  end
  
  it "can be invoked via #invoke" do
    @objects.each do |delegate|
      delegate.invoke(@args[0]).should == @return
    end
  end
end
