require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../shared/invocation"

class DelegateTester
  def self.test
    ScratchPad << 1
    "a".to_clr_string
  end
  
  def self.ref_test(arg)
    ScratchPad << arg
    "a".to_clr_string
  end
end
describe "Reference Void delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::RefVoidDelegate
    @result = [1]
    @return = "a".to_clr_string
  end
  
  it_behaves_like :void_void_delegate_invocation, DelegateTester.method(:test)
  it_behaves_like :x_void_delegate_invocation, DelegateTester.method(:test)
end

describe "Reference Reference delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::RefRefDelegate
    @args = ["hello", "world"]
    @result = ["hello".to_clr_string, "world".to_clr_string]
    @return = "a".to_clr_string
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Reference Value delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::RefValDelegate
    @args = [1,2]
    @result = [1,2]
    @return = "a".to_clr_string
  end
  
  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Reference Reference array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    ab = %w{a b}.to_clr_array(System::String, :to_clr_string)
    cd = %w{c d}.to_clr_array(System::String, :to_clr_string)
    @class = DelegateHolder::RefARefDelegate
    @args = [ab, cd]
    @result = [ab, cd]
    @return = "a".to_clr_string
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Reference Value array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    a1 = [1,2].to_clr_array(Fixnum)
    a2 = [1,2].to_clr_array(Fixnum)
    @class = DelegateHolder::RefAValDelegate
    @args = [a1, a2]
    @result = [a1, a2]
    @return = "a".to_clr_string
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Reference Generic array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::RefGenericDelegate.of(Symbol)
    @args = [:a, :b]
    @result = [:a, :b]
    @return = "a".to_clr_string
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end
