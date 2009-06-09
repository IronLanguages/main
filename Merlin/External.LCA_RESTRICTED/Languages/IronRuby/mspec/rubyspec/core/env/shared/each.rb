describe :env_each, :shared => true do
  it "returns each pair" do
    orig = ENV.to_hash
    e = []
    begin
      ENV.clear
      ENV["foo"] = "bar"
      ENV["baz"] = "boo"
      ENV.send(@method) { |k, v| e << [k, v] }
      e.should include(["foo", "bar"])
      e.should include(["baz", "boo"])
    ensure
      ENV.replace orig
    end
  end

  it "returns the value of break and stops execution of the loop if break is in the block" do
    e = []
    ENV.send(@method) {|k,v| break 1; e << [k,v]}.should == 1
    e.empty?.should == true
  end

  ruby_version_is "" ... "1.8.7" do
    it "raises LocalJumpError if no block given" do
      lambda { ENV.send(@method) }.should raise_error(LocalJumpError)
    end
  end

  ruby_version_is "1.8.7" do
    it "returns an Enumerator if called without a block" do
      ENV.send(@method).should be_kind_of(Enumerable::Enumerator)
    end
  end
end
