require File.dirname(__FILE__) + '/../spec_helper'

describe ".NET events" do
  before :all do
    Foo = IronRuby
    Object.send :remove_const, :IronRuby
    require "IronRuby, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null"
  end

  after :all do
    verb, $VERBOSE = $VERBOSE, nil
    IronRuby = Foo
    $VERBOSE = verb
  end
  csc <<-EOL
  #pragma warning disable 67
  public delegate void EventHandler(object source, int count);
  public partial class BasicEventClass {
    public event EventHandler OnEvent;
  }
  #pragma warning restore 67
  EOL
  it "map to a custom class" do
    BasicEventClass.new.on_event.should be_kind_of IronRuby::Builtins::RubyEvent
  end
end
