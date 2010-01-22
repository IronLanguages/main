describe :kernel_lambda, :shared => true do
  it "returns a Proc object" do
    send(@method) { true }.kind_of?(Proc).should == true
  end

  it "raises an ArgumentError when no block is given" do
    lambda { send(@method) }.should raise_error(ArgumentError)
  end

  it "raises an ArgumentError when given too many arguments" do
    lambda {
      send(@method) { |a, b| a + b }.call(1, 2, 5)
    }.should raise_error(ArgumentError)
  end

  it "raises an ArgumentError when given too few arguments" do
    lambda {
      send(@method) { |a, b| a + b }.call(1)
    }.should raise_error(ArgumentError)
  end

  it "returns from block into caller block" do
    # More info in the pickaxe book pg. 359
    def some_method(cmd)
      p = send(cmd) { return 99 }
      res = p.call
      "returned #{res}"
    end

    # Have to pass in the @method errors otherwise
    some_method(@method).should == "returned 99"

    def some_method2(&b) b end
    a_proc = send(@method) { return true }
    res = some_method2(&a_proc)

    res.call.should == true
  end
end

describe :kernel_lambda_return_like_method, :shared => true do
  it "returns from the #{@method} itself; not the creation site of the #{@method}" do
    @reached_end_of_method = nil
    def test
      send(@method) { return }.call
      @reached_end_of_method = true
    end
    test
    @reached_end_of_method.should be_true
  end

  it "allows long returns to flow through it" do
    KernelSpecs::Lambda.new.outer(@method).should == :good
  end
end
