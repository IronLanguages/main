describe :env_store, :shared => true do
  it "sets the environment variable to the given value" do
    ENV.send(@method, "foo", "bar")
    env.key?("foo").should == true
    env.value?("bar").should == true
    ENV.delete "foo"
    ENV["foo"].should == nil
    ENV.store "foo", "bar"
    env.key?("foo").should == true
    env.value?("bar").should == true
    ENV.delete "foo"
  end
  
  it "can set the environment variable to an empty string" do
    ENV.send(@method, "foo", "")
    ENV["foo"].should == ""
  end
end
