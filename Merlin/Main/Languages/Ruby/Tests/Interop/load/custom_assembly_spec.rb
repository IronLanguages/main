require File.dirname(__FILE__) + '/../spec_helper'
require 'ironruby'

describe "Single time loading of custom assembly" do
  before :each do
    @engine = IronRuby.create_engine
    str = "$: << '#{ENV["MERLIN_ROOT"] + "\\Bin\\Debug\\"}'".gsub("\\", "/")
    @engine.execute(str)
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  it "works via require" do
    @engine.execute("require 'rowantest.baseclasscs'")
    @engine.execute("$\"").should == ['rowantest.baseclasscs.dll']
  end

  it "works via load" do
    @engine.execute("load 'rowantest.baseclasscs.dll'")
    lambda {@engine.execute("Merlin::Testing::BaseClass::EmptyClass")}.should_not raise_error(NameError)
  end

  it "works via load_assembly" do
    lambda {@engine.execute("load_assembly 'rowantest.baseclasscs.dll'")}.should raise_error(LoadError)
  end
end

describe "Repeated loading of custom assembly" do
  before :each do
    @engine = IronRuby.create_engine
    str = "$: << '#{ENV["MERLIN_ROOT"] + "\\Bin\\Debug\\"}'".gsub("\\", "/")
    @engine.execute(str)
  end

  after :each do
    @engine = nil
  end

  it "only loads once with require followed by require" do
    @engine.execute("require 'rowantest.baseclasscs'").should == true
    @engine.execute("require 'rowantest.baseclasscs'").should == false
  end

  it "loads twice with require followed by load" do
    @engine.execute("require 'rowantest.baseclasscs'").should == true
    @engine.execute("load 'rowantest.baseclasscs.dll'").should == true
  end

  it "loads twice with load followed by require" do
    @engine.execute("load 'rowantest.baseclasscs.dll'").should == true
    @engine.execute("require 'rowantest.baseclasscs'").should == true
  end
  
  it "loads twice with load followed by load" do
    @engine.execute("load 'rowantest.baseclasscs.dll'").should == true
    @engine.execute("load 'rowantest.baseclasscs.dll'").should == true
  end
end

describe "Single time loading of custom assembly with Strong name" do
  before :each do
    @engine = IronRuby.create_engine
    str = "$: << '#{ENV["MERLIN_ROOT"] + "\\Bin\\Debug\\"}'".gsub("\\", "/")
    @engine.execute(str)
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  it "works via require" do
    @engine.execute("require 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'")
    @engine.execute("$\"").should == ['rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null']
  end

  it "works via load" do
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'")
    lambda {@engine.execute("Merlin::Testing::BaseClass::EmptyClass")}.should_not raise_error(NameError)
  end

  it "works via load_assembly" do
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'")
    lambda {@engine.execute("Merlin::Testing::BaseClass::EmptyClass")}.should_not raise_error(NameError)
  end
end

describe "Repeated loading of custom assembly with strong name" do
  before :each do
    @engine = IronRuby.create_engine
    str = "$: << '#{ENV["MERLIN_ROOT"] + "\\Bin\\Debug\\"}'".gsub("\\", "/")
    @engine.execute(str)
  end

  after :each do
    @engine = nil
  end

  it "only loads once with require followed by require" do
    @engine.execute("require 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("require 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == false
  end

  it "loads twice with require followed by load" do
    @engine.execute("require 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
  end

  it "loads twice with require followed by load_assembly" do
    @engine.execute("require 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
  end
  
  it "loads twice with load followed by require" do
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("require 'rowantest.baseclasscs'").should == true
  end
  
  it "loads twice with load followed by load" do
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
  end

  it "loads twice with load followed by load_assembly" do
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
  end

  it "loads twice with load_assembly followed by require" do
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("require 'rowantest.baseclasscs'").should == true
  end
  
  it "loads twice with load_assembly followed by load" do
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("load 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
  end

  it "loads twice with load_assembly followed by load_assembly" do
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
    @engine.execute("load_assembly 'rowantest.baseclasscs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'").should == true
  end
end
