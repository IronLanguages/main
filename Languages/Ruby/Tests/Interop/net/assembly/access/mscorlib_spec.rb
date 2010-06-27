require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/load'

describe "mscorlib" do
  before :each do
    @engine = IronRuby.create_engine
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end

  describe "single load" do
    it "works via require" do
      @engine.execute("require 'mscorlib'")
      @engine.execute("$\"").should == ['mscorlib']
    end

    it "works via load" do
      @engine.execute("load 'mscorlib'")
      lambda {@engine.execute("System")}.should_not raise_error(NameError)
    end

    it "works via load_assembly" do
      @engine.execute("load_assembly 'mscorlib'")
      lambda {@engine.execute("System")}.should_not raise_error(NameError)
    end
  end

  describe "Repeated loading" do
    before :each do
      @assembly = 'mscorlib'
    end

    it_behaves_like :repeated_net_assembly, nil
  end
end

describe "mscorlib with Strong name" do
  before :each do
    @engine = IronRuby.create_engine
  end

  after :each do
    #TODO: Does this release the engine enough to allow GC? We don't want a
    #ton of wasted interpreters hanging around.
    @engine = nil
  end
  describe "single load" do
    it "works via require" do
      @engine.execute("require 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'")
      @engine.execute("$\"").should == ['mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089']
    end

    it "works via load" do
      @engine.execute("load 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'")
      lambda {@engine.execute("System")}.should_not raise_error(NameError)
    end

    it "works via load_assembly" do
      @engine.execute("load_assembly 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'")
      lambda {@engine.execute("System")}.should_not raise_error(NameError)
    end
  end

  describe "Repeated loading" do
    before :each do
      @assembly = 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    end
    
    it_behaves_like :repeated_net_assembly, nil
  end
end

describe "Modifying and reloading mscorlib" do 
  before :each do
    @engine = IronRuby.create_engine
    @scope = @engine.create_scope
    @engine.execute("require 'mscorlib'", @scope)
    str = <<-EOL
      class System::Collections::ArrayList
        def foo
          "foo"
        end
      end
    EOL
    @engine.execute str, @scope
    @engine.execute "al = System::Collections::ArrayList.new", @scope
  end

  after :each do
    @engine = nil
  end
  
  it "is allowed" do
    @engine.execute("al.foo", @scope).should == "foo"
  end
  
  it "doesn't reload with require" do
    @engine.execute("al.foo", @scope).should == "foo"
    @engine.execute("require 'mscorlib'", @scope).should == false
    @engine.execute("al.foo", @scope).should == "foo"
  end

  it "reloads with load, without rewriting the class or module" do
    @engine.execute("al.foo", @scope).should == "foo"
    @engine.execute("load 'mscorlib'", @scope).should == true
    @engine.execute("al.foo", @scope).should == "foo"
  end

  it "reloads with load_assembly, without rewriting the class or module" do
    @engine.execute("al.foo", @scope).should == "foo"
    @engine.execute("load_assembly 'mscorlib'", @scope).should == true
    @engine.execute("al.foo", @scope).should == "foo"
  end
end
