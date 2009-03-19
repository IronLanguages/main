require File.dirname(__FILE__) + '/../spec_helper'

describe "Basic .NET classes" do
  csc <<-EOL
    public class EmptyClass {}
    public class Klass {public int m() {return 1;}}
    public abstract class EmptyAbstractClass {}
    public abstract class AbstractClass {public abstract int m();}
    public static class EmptyStaticClass {}
    public static class StaticClass {public static int m() {return 1;}}
  EOL
  it "map to Ruby classes" do
    [EmptyClass, Klass, 
      AbstractClass, EmptyAbstractClass, 
      StaticClass, EmptyStaticClass].each do |klass|
        klass.should be_kind_of Class
      end
  end
end

