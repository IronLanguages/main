require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../shared/invocation"

class DelegateTester
  def self.test
    ScratchPad << 1
    :a
  end
  
  def self.ref_test(arg)
    ScratchPad << arg
    :a
  end
end
describe "Generic Void delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::GenericVoidDelegate.of(Symbol)
    @result = [1]
    @return = :a
  end
  
  it_behaves_like :void_void_delegate_invocation, DelegateTester.method(:test)
  it_behaves_like :x_void_delegate_invocation, DelegateTester.method(:test)
end

describe "Generic Reference delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::GenericRefDelegate.of(Symbol)
    @args = ["hello", "world"]
    @result = ["hello".to_clr_string, "world".to_clr_string]
    @return = :a
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Generic Value delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::GenericValDelegate.of(Symbol)
    @args = [1,2]
    @result = [1,2]
    @return = :a
  end
  
  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Generic Reference array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    ab = %w{a b}.to_clr_array(System::String, :to_clr_string)
    cd = %w{c d}.to_clr_array(System::String, :to_clr_string)
    @class = DelegateHolder::GenericARefDelegate.of(Symbol)
    @args = [ab, cd]
    @result = [ab, cd]
    @return = :a
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Generic Value array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    a1 = [1,2].to_clr_array(Fixnum)
    a2 = [1,2].to_clr_array(Fixnum)
    @class = DelegateHolder::GenericAValDelegate.of(Symbol)
    @args = [a1, a2]
    @result = [a1, a2]
    @return = :a
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Generic Generic array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::GenericGenericDelegate.of(Symbol, Symbol)
    @args = [:a, :b]
    @result = [:a, :b]
    @return = :a
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end
