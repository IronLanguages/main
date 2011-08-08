require File.expand_path('../../../spec_helper', __FILE__)

ruby_version_is "1.9" do
  describe "Array#count" do
    before :each do
      @array = [1, 2, 4, 2]
    end
  
    it "returns size when no argument or a block" do
      @array.count.should == 4
    end

    it "counts nils if given nil as an argument" do
      [1, nil, 4, nil, 2, nil].count(nil).should == 3
    end

    it "accepts an argument for comparison using ==" do
      @array.count(2).should == 2
    end

    it "uses a block for comparison" do
      @array.count{|x| x%2==0 }.should == 3
    end

    it "ignores the block when given an argument" do
      @array.count(4){|x| x%2==0 }.should == 1
    end
  end
end
