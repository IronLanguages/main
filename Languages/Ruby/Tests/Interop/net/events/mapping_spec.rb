require File.dirname(__FILE__) + '/../spec_helper'

describe ".NET events" do
  before :all do
    Foo = IronRuby
    Object.send :remove_const, :IronRuby
    # This makes sure that the version stays correct
    require System::Reflection::Assembly.get_assembly(Foo.to_clr_type).to_s
  end

  after :all do
    verb, $VERBOSE = $VERBOSE, nil
    IronRuby = Foo
    Object.send :remove_const, :Foo
    $VERBOSE = verb
  end
  it "map to a custom class" do
    BasicEventClass.new.on_event.should be_kind_of IronRuby::Builtins::RubyEvent
  end
end
