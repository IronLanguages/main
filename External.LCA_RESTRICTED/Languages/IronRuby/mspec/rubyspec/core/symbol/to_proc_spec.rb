require File.dirname(__FILE__) + '/../../spec_helper'

ruby_version_is "1.8.7" do
  describe "Symbol#to_proc" do
    it "returns a new Proc" do
      proc = :to_s.to_proc
      proc.should be_kind_of(Proc)
    end
  
    it "sends self to arguments passed when calling #call on the proc" do
      obj = mock("Receiving #to_s")
      obj.should_receive(:to_s).and_return("Received #to_s")
      :to_s.to_proc.call(obj).should == "Received #to_s"
    end
    
    it "sends arguments passed when calling #call on the proc" do
      obj = mock("Receiving #foo")
      obj.should_receive(:foo).with(1)
      :foo.to_proc.call(obj, 1)
    end

    it "can send different number of arguments passed when calling #call on the proc" do
      p = :foo.to_proc
      obj1 = mock("Receiving #foo with 1 argument")
      obj1.should_receive(:foo).with(1)
      obj2 = mock("Receiving #foo with 2 argument")
      obj2.should_receive(:foo).with(1, 2)
      p.call(obj1, 1)
      p.call(obj2, 1, 2)
    end
  end
end