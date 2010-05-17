require File.dirname(__FILE__) + '/../../spec_helper'

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

  it "works via load_assembly" do
      @engine.execute("load_assembly 'System.Core'")
      lambda {@engine.execute("System::Linq")}.should_not raise_error(NameError)
  end
end

describe ".NET BCL Assembly with Strong name" do
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

  describe "repeated loading" do
    before :each do
      @assembly = 'System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    end
    it_behaves_like :repeated_net_assembly, nil
  end
end

describe "Modifying and reloading a .NET BCL Assembly" do 
  before :each do
    @engine = IronRuby.create_engine
    @scope = @engine.create_scope
    @engine.execute("require 'System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'", @scope)
    str = <<-EOL
      class System::Web::HttpApplication
        def foo
          "foo"
        end
      end
    EOL
    @engine.execute str, @scope
    @engine.execute "ha = System::Web::HttpApplication.new", @scope
  end

  after :each do
    @engine = nil
  end
  
  it "is allowed" do
    @engine.execute("ha.foo", @scope).should == "foo"
  end
  
  it "doesn't reload with require" do
    @engine.execute("ha.foo", @scope).should == "foo"
    @engine.execute("require 'System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'", @scope).should == false
    @engine.execute("ha.foo", @scope).should == "foo"
  end

  it "reloads with load, without rewriting the class or module" do
    @engine.execute("ha.foo", @scope).should == "foo"
    @engine.execute("load 'System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'", @scope).should == true
    @engine.execute("ha.foo", @scope).should == "foo"
  end

  it "reloads with load_assembly, without rewriting the class or module" do
    @engine.execute("ha.foo", @scope).should == "foo"
    @engine.execute("load_assembly 'System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'", @scope).should == true
    @engine.execute("ha.foo", @scope).should == "foo"
  end
end
