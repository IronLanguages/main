require File.dirname(__FILE__) + '/../../spec_helper'

describe "Calling indexers" do
  csc <<-EOL
    public partial class ClassWithIndexer {
      public int[,] Values = new int[,] { {0, 10}, {20, 30} };

      public int this[int i, int j] { 
        get { return Values[i,j]; } 
        set { Values[i,j] = value; } 
      }
    }
  EOL
  before :each do
    @klass = ClassWithIndexer.new
  end

  it "can be done on set indexers" do
    @klass[0,0] = 1
    @klass.Values[0,0].should == 1
  end

  it "can be done on get indexers" do
    @klass[0,1].should == 10 
  end

  it "properly respect type" do
    lambda { @klass["string",1] }.should raise_error TypeError
    lambda { @klass[1,"string"] }.should raise_error TypeError
  end
end
