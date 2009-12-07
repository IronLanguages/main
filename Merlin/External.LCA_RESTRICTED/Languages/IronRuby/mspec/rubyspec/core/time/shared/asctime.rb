describe :time_asctime, :shared => true do
  ruby_bug "#2417", "1.8.6" do
    it "returns a canonical string representation of time" do
      t = Time.now
      t.send(@method).should == t.strftime("%a %b " + sprintf('%2s', t.day) + " %H:%M:%S %Y")
    end
  end
end
