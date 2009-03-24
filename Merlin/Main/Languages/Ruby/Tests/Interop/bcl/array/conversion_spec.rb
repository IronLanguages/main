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
    a = System::Array.of(Object).new(2)
    a[0], a[1] = 1, "string"
    Klass.new.method(:array_accepting_method).of(Object).call([1, "string"].to_clr_array).should == a
  end

  it "properly converts to typed array" do
    a = System::Array.of(Fixnum).new(3)
    a[0], a[1], a[2] = 1,2,3
    Klass.new.method(:array_accepting_method).of(Fixnum).call([1,2,3].to_clr_array(Fixnum)).should == a
  end
end
