require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/load'

describe "Custom Assembly" do
  before :each do
    @engine = SpecHelper.create_engine
    @assembly = 'fixtures.generated.dll'
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  describe "single load" do
    it "works via require" do
      @engine.execute("require '#{@assembly}'")
      @engine.execute("$\"").should == [@assembly]
    end

    it "works via load" do
      @engine.execute("load '#{@assembly}'")
      lambda {@engine.execute("Klass")}.should_not raise_error(NameError)
    end

    it "works via load_assembly" do
      lambda {@engine.execute("load_assembly '#{@assembly}'")}.should raise_error(LoadError)
    end
  end

  describe "Repeated loading" do
    it "only loads once with require followed by require" do
      @engine.should be_able_to_load(@assembly).with('require').once
    end

    it "loads twice with require followed by load" do
      @engine.should be_able_to_load(@assembly).with('require').followed_by('load')
    end

    it "loads twice with load followed by require" do
      @engine.should be_able_to_load(@assembly).with('load').followed_by('require')
    end
    
    it "loads twice with load followed by load" do
      @engine.should be_able_to_load(@assembly).with('load').twice
    end
  end
end

describe "Custom Assembly with StrongName" do
  before :each do
    @engine = SpecHelper.create_engine
    @assembly = 'fixtures.generated, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  describe "single time loading" do
    it "works via require" do
      @engine.execute("require '#{@assembly}'")
      @engine.execute("$\"").should == [@assembly]
    end

    it "works via load" do
      @engine.execute("load '#{@assembly}'")
      lambda {@engine.execute("Klass")}.should_not raise_error(NameError)
    end

    it "works via load_assembly" do
      @engine.execute("load_assembly '#{@assembly}'")
      lambda {@engine.execute("Klass")}.should_not raise_error(NameError)
    end
  end

  describe "Repeated loading" do
    it_behaves_like :repeated_net_assembly, nil
  end
end

describe "Loading of custom assembly outside of the load path" do
  it "raises a LoadError" do
    engine = IronRuby.create_engine
    engine.execute("$:.clear")
    lambda {engine.execute("require 'fixtures.generated'")}.should raise_error(LoadError)
    lambda {engine.execute("load 'fixtures.generated.dll'")}.should raise_error(LoadError)
    lambda {engine.execute("load_assembly 'fixtures.generated.dll'")}.should raise_error(LoadError)
  end

  it "doesn't raise LoadError for strong names" do 
    engine = IronRuby.create_engine
    lambda {engine.execute("require 'fixtures.generated, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'")}.should_not raise_error(LoadError)
    lambda {engine.execute("load 'fixtures.generated, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'")}.should_not raise_error(LoadError)
    lambda {engine.execute("load_assembly 'fixtures.generated, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'")}.should_not raise_error(LoadError)
  end
end

describe "Static dependency loading" do
  it "loads a dependent assembly from load paths" do
    ruby_exe("dependencies1/test1.rb", :dir => File.dirname(__FILE__)).chomp.should == "1"
  end
  
  it "does propagate load exceptions" do
    ruby_exe("dependencies1/test2.rb", :dir => File.dirname(__FILE__)).chomp.should == "true"
  end
end


describe "Modifying and reloading custom assembly" do 
  before :each do

    @engine, @scope = SpecHelper.create_scoped_engine
    @engine.execute("require 'fixtures.generated'", @scope)
    str = <<-EOL
      class Klass
        def foo
          "foo"
        end
      end
    EOL
    @engine.execute str, @scope
    @engine.execute "ec = Klass.new", @scope
  end

  after :each do
    @engine = nil
  end
  
  it "is allowed" do
    @engine.execute("ec.foo", @scope).should == "foo"
  end
  
  it "doesn't reload with require" do
    @engine.execute("ec.foo", @scope).should == "foo"
    @engine.execute("require 'fixtures.generated'", @scope).should == false
    @engine.execute("ec.foo", @scope).should == "foo"
  end

  it "reloads with load, without rewriting the class or module" do
    @engine.execute("ec.foo", @scope).should == "foo"
    @engine.execute("load 'fixtures.generated.dll'", @scope).should == true
    @engine.execute("ec.foo", @scope).should == "foo"
  end

  it "reloads with load_assembly, without rewriting the class or module" do
    @engine.execute("ec.foo", @scope).should == "foo"
    @engine.execute("load_assembly 'fixtures.generated'", @scope).should == true
    @engine.execute("ec.foo", @scope).should == "foo"    
  end
end
