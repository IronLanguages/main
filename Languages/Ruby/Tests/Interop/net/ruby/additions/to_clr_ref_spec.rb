require File.dirname(__FILE__) + "/../../spec_helper"

describe "Module#to_clr_ref" do
  it "returns the ref type of its module" do
    [Fixnum, System::String, Object, System::IComparable[Fixnum]].each do |const|
      ref1 = const.to_clr_ref
      ref2 = const.to_clr_type.make_by_ref_type.to_class
      ref1.should == ref2
    end
  end

  it "returns nil for Ruby classes and metaclasses" do
    class Foo; end
    [System, Foo, Foo.new.metaclass, Class.new, Kernel, Module.new].each do |const|
      const.to_clr_ref.should == nil
    end
  end

  it "raises for ref types and arrays of ref types" do
    lambda { Fixnum.to_clr_ref.to_clr_ref}.should raise_error
    lambda { System::Array.of(Fixnum.to_clr_ref)}.should raise_error
    end
end
