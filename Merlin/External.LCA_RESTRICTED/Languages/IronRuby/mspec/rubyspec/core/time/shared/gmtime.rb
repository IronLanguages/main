describe :time_gmtime, :shared => true do
  it "returns the utc representation of time" do
    # Testing with America/Regina here because it doesn't have DST.
    with_timezone("CST", -6) do
      t = Time.local(2007, 1, 9, 6, 0, 0)
      t.send(@method)
      t.should == Time.gm(2007, 1, 9, 12, 0, 0)
    end
  end

  it "changes the receiver" do
    t = Time.now
    t.utc?.should be_false
    t.send(@method)
    t.utc?.should be_true
  end
end
