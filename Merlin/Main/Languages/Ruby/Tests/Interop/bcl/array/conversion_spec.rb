require File.dirname(__FILE__) + '/../../spec_helper'

describe "Converting Ruby arrays to .NET arrays" do
  csc <<-EOL
    public partial class Klass {
      public T[] ArrayAcceptingMethod<T>(T[] arg0) {
        return arg0;
      }
    }
  EOL
  it "properly converts to object array" do
    Klass.new.method(:array_accepting_method).of(Object).call([1, "string"].to_clr_array).should == [1, "string"]
  end

  it "properly converts to typed array" do
    Klass.new.method(:array_accepting_method).of(Fixnum).call([1,2,3].to_clr_array(Fixnum)).should == [1,2,3]
  end
end
