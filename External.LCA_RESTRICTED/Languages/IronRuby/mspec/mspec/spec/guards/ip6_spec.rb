require File.dirname(__FILE__) + '/../spec_helper'
require 'mspec/guards/ip6'

describe Object, "#supports_ip6" do
  before :each do
    @guard = IP6Guard.new
    IP6Guard.stub!(:new).and_return(@guard)
    ScratchPad.clear
  end

  it "does not yield when #ip6? returns false" do
    @guard.stub!(:match?).and_return(false)
    supports_ip6 { ScratchPad.record :yield }
    ScratchPad.recorded.should_not == :yield
  end

  it "yields when #match? returns true" do
    @guard.stub!(:match?).and_return(true)
    supports_ip6 { ScratchPad.record :yield }
    ScratchPad.recorded.should == :yield
  end

  it "sets the name of the guard to :supports_ip6" do
    supports_ip6 {}
    @guard.name.should == :supports_ip6
  end

  it "calls #unregister even when an exception is raised in the guard block" do
    @guard.should_receive(:match?).and_return(true)
    @guard.should_receive(:unregister)
    lambda do
      supports_ip6 { raise Exception }
    end.should raise_error(Exception)
  end
end

describe Object, "#does_not_support_ip6" do
  before :each do
    @guard = IP6Guard.new
    IP6Guard.stub!(:new).and_return(@guard)
    ScratchPad.clear
  end

  it "does not yield when #match? returns true" do
    @guard.stub!(:match?).and_return(true)
    does_not_support_ip6 { ScratchPad.record :yield }
    ScratchPad.recorded.should_not == :yield
  end

  it "yields when #match? returns false" do
    @guard.stub!(:match?).and_return(false)
    does_not_support_ip6 { ScratchPad.record :yield }
    ScratchPad.recorded.should == :yield
  end

  it "sets the name of the guard to :platform_is_not" do
    does_not_support_ip6 {}
    @guard.name.should == :does_not_support_ip6
  end

  it "calls #unregister even when an exception is raised in the guard block" do
    @guard.should_receive(:match?).and_return(false)
    @guard.should_receive(:unregister)
    lambda do
      does_not_support_ip6 { raise Exception }
    end.should raise_error(Exception)
  end
end
