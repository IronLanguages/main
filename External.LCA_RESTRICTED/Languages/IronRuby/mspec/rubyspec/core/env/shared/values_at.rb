describe :env_values_at, :shared => true do
  it "returns an array of the values referenced by the parameters as keys" do
    ENV["foo"] = "oof"
    ENV["bar"] = "rab"
    ENV.send(@method).should == []
    ENV.send(@method, "bar", "foo").should == ["rab", "oof"]
    ENV.delete "foo"
    ENV.delete "bar"
  end
end
