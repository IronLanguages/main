require File.dirname(__FILE__) + '/../../spec_helper'

describe :generic_methods, :shared => true do
  it "are callable via call and [] when pubic or protected" do
    @klass.method(:public_1_generic_0_arg).of(Fixnum).call.should equal_clr_string("public generic no args")
    [[@klass, @public_method_list], [@subklass, @protected_method_list]].each do |obj, ms| 
      ms.each do |m|      
        generic_count, arity = m.match(/_(\d)_generic_(\d)_/)[1..2].map {|e| e.to_i}
        generics = Array.new(generic_count, Fixnum)
        args = Array.new(arity, 1)
        args << args.pop.to_s.to_clr_string if arity > generic_count

        obj.method(m).of(*generics).call(*args).should equal_clr_string(args.join(" "))
        obj.method(m).of(*generics)[*args].should equal_clr_string(args.join(" "))
      end
    end
  end

  it "binds struct constraints correctly" do 
    @klass.method(:struct_constraint_method).of(Fixnum).call(1).should == 1
  end

  it "binds class constraints correctly" do
    @klass.method(:class_constraint_method).of(String).call("a").should == "a"
  end

  it "binds constructor constraints correctly" do
    @klass.method(:constructor_constraint_method).of(Klass).call.foo.should == 10
  end

  it "binds secondary type constraints correctly" do
    @klass.method(:type_constraint_method).of(SubKlass, Klass).call(SubKlass.new).foo.should == 10
  end

  if IronRuby.configuration.private_binding
    it "are callable via call and [] when private" do
      @private_method_list.each do |m|
        generic_count, arity = m.match(/_(\d)_generic_(\d)_/)[1..2].map {|e| e.to_i}
        generics = Array.new(generic_count, Fixnum)
        args = Array.new(arity, 1)
        args << args.pop.to_s.to_clr_string if arity > generic_count

        @klass.method(m).of(*generics).call(*args).should equal_clr_string(args.join(" "))
        @klass.method(m).of(*generics)[*args].should equal_clr_string(args.join(" "))
      end
    end
  end

  it "can be called directly when inferrable" do
    [[@klass, @public_method_list], [@subklass, @protected_method_list]].each do |obj, ms|
      ms.each do |m|
        generic_count, arity = m.match(/_(\d)_generic_(\d)_/)[1..2].map {|e| e.to_i}
        args = Array.new(arity, 1)
        args << args.pop.to_s.to_clr_string if arity > generic_count

        obj.method(m).call(*args).should equal_clr_string(args.join(" "))
        obj.method(m)[*args].should equal_clr_string(args.join(" "))
      end
    end
  end

  it "has proper errors for constrained generics" do
    lambda { @klass.method(:struct_constraint_method).of(String).call("a")}.should raise_error(ArgumentError)
    lambda { @klass.method(:class_constraint_method).of(Fixnum).call(1)}.should raise_error(ArgumentError)
    lambda { @klass.method(:constructor_constraint_method).of(System::String).call}.should raise_error(ArgumentError)
    lambda { @klass.method(:type_constraint_method).of(String, Klass).call("a")}.should raise_error(ArgumentError)
  end

  it "can use Ruby types for constrained generics" do
    class Foo
      attr_reader :foo
      def initialize
        @foo = 10
      end
    end

    class SubFoo < Foo
    end
    @klass.method(:constructor_constraint_method).of(Foo).call.foo.should == 10
    @klass.method(:type_constraint_method).of(SubFoo, Foo).call(SubFoo.new).foo.should == 10
  end

  it "has proper error messages for incorrect number of arguments" do
    lambda {@klass.method(:public_1_generic_2_arg).of(Fixnum).call(1)}.should raise_error(ArgumentError, /1 for 2/)
  end
end

describe :generic_conflicting_methods, :shared => true do
  it "binds class type parameter correctly" do
    @klass.method(:public_1_generic_2_arg).of(String).call("hello", 1).should equal_clr_string("hello 1")
  end

  it "binds conflicting type parameter correctly" do
    @klass.method(:conflicting_generic_method).of(String).call("hello").should equal_clr_string("hello")
  end
end
describe "Generic methods" do
  describe "on regular classes" do
    before :each do
      t = ClassWithMethods
      @klass, @subklass = t.new, Class.new(t).new
      
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
    before :each do
      t = GenericClassWithMethods.of(Fixnum)
      @klass, @subklass = t.new, Class.new(t).new
      
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
    it_behaves_like :generic_conflicting_methods, Object.new
  end

  describe "on generic classes with 2 parameters" do
    before :each do
      t = GenericClass2Params.of(Fixnum, String)
      @klass, @subklass = t.new, Class.new(t).new
      
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
    it_behaves_like :generic_conflicting_methods, Object.new
  end
end
