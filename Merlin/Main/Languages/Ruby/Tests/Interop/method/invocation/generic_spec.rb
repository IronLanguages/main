require File.dirname(__FILE__) + '/../../spec_helper'
require 'ironruby'

@methods_string = <<-EOL
  #region private methods
  private string Private1Generic0Arg<T>() {
    return "private generic no args";
  }
  
  private string Private1Generic1Arg<T>(T arg0) {
    return Public1Generic1Arg<T>(arg0);
  }

  private string Private1Generic2Arg<T>(T arg0, string arg1) {
    return Public1Generic2Arg<T>(arg0, arg1);
  }

  private string Private2Generic2Arg<T, U>(T arg0, U arg1) {
    return Public2Generic2Arg<T, U>(arg0, arg1);
  }

  private string Private2Generic3Arg<T, U>(T arg0, U arg1, string arg2) {
    return Public2Generic3Arg<T, U>(arg0, arg1, arg2);
  }

  private string Private3Generic3Arg<T, U, V>(T arg0, U arg1, V arg2) {
    return Public3Generic3Arg<T, U, V>(arg0, arg1, arg2);
  }

  private string Private3Generic4Arg<T, U, V>(T arg0, U arg1, V arg2, string arg3) {
    return Public3Generic4Arg<T, U, V>(arg0, arg1, arg2, arg3);
  }
  #endregion
  
  #region protected methods
  protected string Protected1Generic0Arg<T>() {
    return "protected generic no args";
  }
  
  protected string Protected1Generic1Arg<T>(T arg0) {
    return Public1Generic1Arg<T>(arg0);
  }

  protected string Protected1Generic2Arg<T>(T arg0, string arg1) {
    return Public1Generic2Arg<T>(arg0, arg1);
  }

  protected string Protected2Generic2Arg<T, U>(T arg0, U arg1) {
    return Public2Generic2Arg<T, U>(arg0, arg1);
  }

  protected string Protected2Generic3Arg<T, U>(T arg0, U arg1, string arg2) {
    return Public2Generic3Arg<T, U>(arg0, arg1, arg2);
  }

  protected string Protected3Generic3Arg<T, U, V>(T arg0, U arg1, V arg2) {
    return Public3Generic3Arg<T, U, V>(arg0, arg1, arg2);
  }

  protected string Protected3Generic4Arg<T, U, V>(T arg0, U arg1, V arg2, string arg3) {
    return Public3Generic4Arg<T, U, V>(arg0, arg1, arg2, arg3);
  }
  #endregion
 
  #region public methods
  public string Public1Generic0Arg<T>() {
    return "public generic no args";
  }

  public string Public1Generic1Arg<T>(T arg0) {
    return arg0.ToString();
  }

  public string Public1Generic2Arg<T>(T arg0, string arg1) {
    return System.String.Format("{0} {1}", arg0, arg1);
  }

  public string Public2Generic2Arg<T, U>(T arg0, U arg1) {
    return Public1Generic2Arg<T>(arg0, arg1.ToString());
  }

  public string Public2Generic3Arg<T, U>(T arg0, U arg1, string arg2) {
    return System.String.Format("{0} {1} {2}", arg0, arg1, arg2);
  }

  public string Public3Generic3Arg<T, U, V>(T arg0, U arg1, V arg2) {
    return Public2Generic3Arg<T, U>(arg0, arg1, arg2.ToString());
  }

  public string Public3Generic4Arg<T, U, V>(T arg0, U arg1, V arg2, string arg3) {
    return System.String.Format("{0} {1} {2} {3}", arg0, arg1, arg2, arg3);
  }
  #endregion
  
  #region Constrained methods
  public T StructConstraintMethod<T>(T arg0)
  where T : struct {
    return arg0;
  }

  public T ClassConstraintMethod<T>(T arg0)
  where T : class {
    return arg0;
  }

  public T ConstructorConstraintMethod<T>()
  where T : new() {
    return new T();
  }
  #endregion
EOL
describe :generic_methods, :shared => true do
  it "are callable via call and [] when pubic or protected" do
    @klass.method(:public_1_generic_0_arg).of(Fixnum).call.to_s.should == "public generic no args"
    @klass.method(:struct_constraint_method).of(Fixnum).call(1).should == 1
    @klass.method(:class_constraint_method).of(String).call("a").should == "a"
    @klass.method(:constructor_constraint_method).of(Klass).call.foo.should == 10
    (@public_method_list + @protected_method_list).each do |m|
      generic_count, arity = m.match(/_(\d)_generic_(\d)_/)[1..2].map {|e| e.to_i}
      generics = Array.new(generic_count, Fixnum)
      args = Array.new(arity, 1)
      args << args.pop.to_s.to_clr_string if arity > generic_count

      @klass.method(m).of(*generics).call(*args).to_s.should == args.join(" ")
      @klass.method(m).of(*generics)[*args].to_s.should == args.join(" ")
    end
  end

  if IronRuby.dlr_config.private_binding
    it "are callable via call and [] when private" do
      @private_method_list.each do |m|
        generic_count, arity = m.match(/_(\d)_generic_(\d)_/)[1..2].map {|e| e.to_i}
        generics = Array.new(generic_count, Fixnum)
        args = Array.new(arity, 1)
        args << args.pop.to_s.to_clr_string if arity > generic_count

        @klass.method(m).of(*generics).call(*args).to_s.should == args.join(" ")
        @klass.method(m).of(*generics)[*args].to_s.should == args.join(" ")
      end
    end
  end

  it "cannot be called directly" do
    (@public_method_list + @protected_method_list).each do |m|
      generic_count, arity = m.match(/_(\d)_generic_(\d)_/)[1..2].map {|e| e.to_i}
      args = Array.new(arity, 1)
      args << args.pop.to_s.to_clr_string if arity > generic_count
      lambda {@klass.send(m, *args)}.should raise_error(ArgumentError)
    end
  end

  it "has proper errors for constrained generics" do
    lambda { @klass.method(:struct_constraint_method).of(String).call("a")}.should raise_error(ArgumentError)
    lambda { @klass.method(:class_constraint_method).of(Fixnum).call(1)}.should raise_error(ArgumentError)
    lambda { @klass.method(:constructor_constraint_method).of(String).call("a")}.should raise_error(ArgumentError)
  end
end

describe "Generic methods" do
  describe "on regular classes" do
    csc <<-EOL
    public partial class ClassWithMethods {
      #{@methods_string}
    }

    public partial class Klass {
      private int _foo;
      
      public int Foo {
        get { return _foo; }
      }

      public Klass() {
        _foo = 10;
      }
    }
    EOL
    before :each do
      @klass = ClassWithMethods.new
      @public_method_list = %w{public_1_generic_1_arg public_1_generic_2_arg
                        public_2_generic_2_arg public_2_generic_3_arg
                        public_3_generic_3_arg public_3_generic_3_arg}
      @private_method_list = %w{private_1_generic_1_arg private_1_generic_2_arg
                        private_2_generic_2_arg private_2_generic_3_arg
                        private_3_generic_3_arg private_3_generic_3_arg}
      @protected_method_list = %w{protected_1_generic_1_arg protected_1_generic_2_arg
                        protected_2_generic_2_arg protected_2_generic_3_arg
                        protected_3_generic_3_arg protected_3_generic_3_arg}
    end
    it_behaves_like :generic_methods, Object.new

  end

  describe "on generic classes with one parameter" do
    csc <<-EOL
    #pragma warning disable 693
    public partial class GenericClassWithMethods<K> {
    #{@methods_string}
      public string Public1Generic2Arg<T>(T arg0, K arg1) {
        return Public2Generic2Arg<T, K>(arg0, arg1);
      }
      
      public string ConflictingGenericMethod<K>(K arg0) {
        return arg0.ToString();
      }
    }
    EOL
    before :each do
      @klass = GenericClassWithMethods.of(Fixnum).new
      @public_method_list = %w{public_1_generic_1_arg public_1_generic_2_arg
                        public_2_generic_2_arg public_2_generic_3_arg
                        public_3_generic_3_arg public_3_generic_3_arg}
      @private_method_list = %w{private_1_generic_1_arg private_1_generic_2_arg
                        private_2_generic_2_arg private_2_generic_3_arg
                        private_3_generic_3_arg private_3_generic_3_arg}
      @protected_method_list = %w{protected_1_generic_1_arg protected_1_generic_2_arg
                        protected_2_generic_2_arg protected_2_generic_3_arg
                        protected_3_generic_3_arg protected_3_generic_3_arg}
    end
    it_behaves_like :generic_methods, Object.new
  end

  describe "on generic classes with 2 parameters" do
    csc <<-EOL
    public partial class GenericClass2Params<K, J> {
    #{@methods_string}
      public string Public1Generic2Arg<T>(T arg0, K arg1) {
        return Public2Generic2Arg<T, K>(arg0, arg1);
      }
      
      public string ConflictingGenericMethod<K>(K arg0) {
        return arg0.ToString();
      }
    }
    #pragma warning restore 693
    EOL
    before :each do
      @klass = GenericClass2Params.of(Fixnum, String).new
      @public_method_list = %w{public_1_generic_1_arg public_1_generic_2_arg
                        public_2_generic_2_arg public_2_generic_3_arg
                        public_3_generic_3_arg public_3_generic_3_arg}
      @private_method_list = %w{private_1_generic_1_arg private_1_generic_2_arg
                        private_2_generic_2_arg private_2_generic_3_arg
                        private_3_generic_3_arg private_3_generic_3_arg}
      @protected_method_list = %w{protected_1_generic_1_arg protected_1_generic_2_arg
                        protected_2_generic_2_arg protected_2_generic_3_arg
                        protected_3_generic_3_arg protected_3_generic_3_arg}
    end
    it_behaves_like :generic_methods, Object.new
  end
end
