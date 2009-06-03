require File.dirname(__FILE__) + '/../spec_helper'

describe "Basic .NET classes" do
  csc <<-EOL
    public class EmptyClass {}
    public partial class Klass {public int m() {return 1;}}
    public abstract class EmptyAbstractClass {}
    public abstract class AbstractClass {public abstract int m();}
    public static class EmptyStaticClass {}
    public static class StaticClass {public static int m() {return 1;}}
    public sealed class SealedClass {public int m() {return 1;}}
    public sealed class EmptySealedClass {}
    public class EmptyGenericClass<T>{}
    public class GenericClass<T>{public int m() {return 1;}}
    public class EmptyGeneric2Class<T,U>{}
    public class Generic2Class<T,U>{public int m() {return 1;}}
  EOL
  it "map to Ruby classes" do
    [EmptyClass, Klass, 
      AbstractClass, EmptyAbstractClass, 
      StaticClass, EmptyStaticClass,
      SealedClass, EmptySealedClass,
      GenericClass, EmptyGenericClass,
      Generic2Class, EmptyGeneric2Class
    ].each do |klass|
        klass.should be_kind_of Class
      end
  end
end

