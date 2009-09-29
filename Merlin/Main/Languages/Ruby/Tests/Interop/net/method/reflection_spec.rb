require File.dirname(__FILE__) + '/../spec_helper'

describe "Reflecting on regular .NET methods" do
  before(:each) do
    @obj = ClassWithMethods.new
  end
  
  it "IronRuby::Clr::Name implements equality comparison so that include? works on dual names" do
    System::AppDomain.singleton_methods.include?(:unload).should be_true
    System::AppDomain.singleton_methods.include?("unload").should be_true
    System::AppDomain.singleton_methods.include?("Unload").should be_false
  end
  
  ruby_version_is "".."1.8.6" do
    it "simple names are represented by strings" do
      System::AppDomain.singleton_methods.include?(:Equals).should be_false
      System::AppDomain.singleton_methods.include?("Equals").should be_true
    end
  end

  ruby_version_is "1.9" do
    it "simple names are represented by symbols" do
      System::AppDomain.singleton_methods.include?(:Equals).should be_true
      System::AppDomain.singleton_methods.include?("Equals").should be_false
    end
  end

  it "are included in an objects method list" do
    #.instance_methods(true)
    #.instance_methods(false) #no ancestors
    #.private_instance_methods(true)
    #.private_instance_methods(false) #no ancestors
    #.protected_instance_methods(true)
    #.protected_instance_methods(false) #no ancestors
    #.public_instance_methods(true)
    #.public_instance_methods(false) #no ancestors
    ##methods(true) #all publicly accessible methods
    ##methods(false) #only singleton methods
    ##private_methods
    ##protected_methods
    ##singleton_methods(true) #all modules included in obj
    ##singleton_methods(false) #
    @obj.methods(true).grep(/public_method/).should_not == []
  end

  it "are able to be grabbed as an object" do
    @obj.method(:public_method).should be_kind_of Method
  end
end

describe "Reflecting on abstract .NET methods" do
  it "should be able to be grabbed as an object after call to #method" do
    #Regression test for Rubyforge 24104
    AbstractClassWithMethods.method(:public_method) rescue nil
    AbstractClassWithMethods.instance_method(:public_method).should be_kind_of UnboundMethod
  end


  it "are able to be grabbed as an object" do
    #currently fails due to previous test -- JD
    @meth = AbstractClassWithMethods.instance_method(:public_method)
    @meth.should be_kind_of UnboundMethod
  end
end

describe "Reflecting on .NET method objects" do
  before(:each) do
    @meth = ClassWithMethods.new.method(:public_method)
  end

  it "are Ruby methods" do
    @meth.should be_kind_of Method
  end

  it "contain a group of CLR Methods" do
    @meth.clr_members[0].should be_kind_of System::Reflection::MemberInfo
  end

  it "can be called" do
    @meth.call.should equal_clr_string("public")
    @meth[].should equal_clr_string("public")
  end

  it "can be unbound" do
    m = @meth.unbind
    m.should be_kind_of UnboundMethod
    m = m.bind(ClassWithMethods.new)
    m.call.should equal_clr_string("public")
  end
end

describe "Overloaded .NET methods" do
  before(:each) do
    @methods = ClassWithOverloads.new.method(:Overloaded)
  end

  it "act as a single Ruby method" do
    @methods.should be_kind_of Method
    @methods.call.should equal_clr_string("empty")
  end

  it "contain .NET method objects" do
    @methods.clr_members.each do |meth|
      meth.should be_kind_of System::Reflection::MemberInfo
    end
  end
end

describe "Generic .NET methods" do
  before :each do
    @klass = ClassWithMethods.new
  end
  it "properly binds and returns the type arguments" do
    %w{public_1_generic_1_arg public_2_generic_2_arg
       public_3_generic_3_arg protected_1_generic_1_arg
       protected_2_generic_2_arg protected_3_generic_3_arg}.each do |m|
      generic_count = m.match(/_(\d)_generic_(\d)_/)[1].to_i
      generics =  [Fixnum, String, Symbol][0..(generic_count -1)]
      @klass.method(m).of(*generics).clr_members[0].get_generic_arguments.should == generics.map {|e| e.to_clr_type}
    end
  end
end

describe "Static .NET methods" do
  it "don't incorrectly get cached when called on an instance" do
    #might be related to Rubyforge 24104
    Klass.new.method(:static_void_method) rescue nil
    Klass.method(:static_void_method).should be_kind_of(Method)
  end
end

