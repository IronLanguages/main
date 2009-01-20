require File.dirname(__FILE__) + '/../spec_helper'
require 'ironruby'

describe "Single time loading of a .NET BCL assembly without Strong Name" do
  before :each do
    @engine = IronRuby.create_engine
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  it "raises LoadError via require" do
    lambda {@engine.execute("require 'System.Core'")}.should raise_error(LoadError)
  end

  it "raises LoadError via load" do
    lambda {@engine.execute("load 'System.Core'")}.should raise_error(LoadError)
  end

  it "raises LoadError via load_assembly" do
    lambda {@engine.execute("load_assembly 'System.Core'")}.should raise_error(LoadError)
  end
end

describe "Single time loading of a .NET BCL Assembly with Strong name" do
  before :each do
    @engine = IronRuby.create_engine
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  it "works via require" do
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'")
    @engine.execute("$\"").should == ['System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089']
  end

  it "works via load" do
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'")
    lambda {@engine.execute("System::Linq")}.should_not raise_error(NameError)
  end

  it "works via load_assembly" do
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'")
    lambda {@engine.execute("System::Linq")}.should_not raise_error(NameError)
  end
end

describe "Repeated loading of a .NET BCL Assembly with Strong name" do
  before :each do
    @engine = IronRuby.create_engine
  end

  after :each do
    @engine = nil
  end

  it "only loads once with require followed by require" do
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == false
  end

  it "loads twice with require followed by load" do
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end

  it "loads twice with require followed by load_assembly" do
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end
  
  it "loads twice with load followed by require" do
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end
  
  it "loads twice with load followed by load" do
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end

  it "loads twice with load followed by load_assembly" do
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end

  it "loads twice with load_assembly followed by require" do
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("require 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end
  
  it "loads twice with load_assembly followed by load" do
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("load 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end

  it "loads twice with load_assembly followed by load_assembly" do
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
    @engine.execute("load_assembly 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'").should == true
  end
end
