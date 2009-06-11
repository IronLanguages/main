require File.dirname(__FILE__) + "/../../spec_helper"
require File.dirname(__FILE__) + "/../shared/invocation"

describe "Void Void delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::VoidVoidDelegate
    @result = [1]
  end
  
  class DelegateTester
    def self.test
      ScratchPad << 1
    end
  end
  
  it_behaves_like :void_void_delegate_invocation, DelegateTester.method(:test)
end

describe "Void Reference delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::VoidRefDelegate
    @args = ["hello", "world"]
    @result = ["hello".to_clr_string, "world".to_clr_string]
  end

  it_behaves_like :void_x_delegate_invocation, ScratchPad.method(:<<)
end

describe "Void Value delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::VoidValDelegate
    @args = [1, 2]
    @result = [1, 2]
  end

  it_behaves_like :void_x_delegate_invocation, ScratchPad.method(:<<)
end

describe "Void Reference array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    ab = %w{a b}.to_clr_array(System::String, :to_clr_string)
    cd = %w{c d}.to_clr_array(System::String, :to_clr_string)
    @class = DelegateHolder::VoidARefDelegate
    @args = [ab, cd]
    @result = [ab, cd]
  end

  it_behaves_like :void_x_delegate_invocation, ScratchPad.method(:<<)
end

describe "Void Value array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    a1 = [1,2].to_clr_array(Fixnum)
    a2 = [1,2].to_clr_array(Fixnum)
    @class = DelegateHolder::VoidAValDelegate
    @args = [a1, a2]
    @result = [a1, a2]
  end

  it_behaves_like :void_x_delegate_invocation, ScratchPad.method(:<<)
end

describe "Void Generic array delegate invocation" do
  before(:each) do
    ScratchPad.clear
    ScratchPad.record []
    @class = DelegateHolder::VoidGenericDelegate.of(Symbol)
    @args = [:a, :b]
    @result = [:a, :b]
  end

  it_behaves_like :void_x_delegate_invocation, ScratchPad.method(:<<)
end
