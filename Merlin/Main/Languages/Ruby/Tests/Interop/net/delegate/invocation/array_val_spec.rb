require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../shared/invocation"

class DelegateTester
  def self.test
    ScratchPad << 1
    System::Array.of(Fixnum).new([1, 2])
  end
  
  def self.ref_test(arg)
    ScratchPad << arg
    System::Array.of(Fixnum).new([1, 2])
  end
end
describe "Value array Void delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::AValVoidDelegate
    @result = [1]
    @return = System::Array.of(Fixnum).new([1, 2])
  end
  
  it_behaves_like :void_void_delegate_invocation, DelegateTester.method(:test)
  it_behaves_like :x_void_delegate_invocation, DelegateTester.method(:test)
end

describe "Value array Reference delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::AValRefDelegate
    @args = ["hello", "world"]
    @result = ["hello".to_clr_string, "world".to_clr_string]
    @return = System::Array.of(Fixnum).new([1, 2])
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Value array Value delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::AValValDelegate
    @args = [1,2]
    @result = [1,2]
    @return = System::Array.of(Fixnum).new([1, 2])
  end
  
  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Value array Reference array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    ab = System::Array.of(System::String).new(["a".to_clr_string,"b".to_clr_string])
    cd = System::Array.of(System::String).new(["c".to_clr_string, "d".to_clr_string])
    @class = DelegateHolder::AValARefDelegate
    @args = [ab, cd]
    @result = [ab, cd]
    @return = System::Array.of(Fixnum).new([1, 2])
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Value array Value array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    a1 = System::Array.of(Fixnum).new([1, 2])
    a2 = System::Array.of(Fixnum).new([3, 4])
    @class = DelegateHolder::AValAValDelegate
    @args = [a1, a2]
    @result = [a1, a2]
    @return = System::Array.of(Fixnum).new([1, 2])
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end

describe "Value array Generic array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::AValGenericDelegate.of(Symbol)
    @args = [:a, :b]
    @result = [:a, :b]
    @return = System::Array.of(Fixnum).new([1, 2])
  end

  it_behaves_like :void_x_delegate_invocation, DelegateTester.method(:ref_test)
  it_behaves_like :x_x_delegate_invocation, DelegateTester.method(:ref_test)
end
