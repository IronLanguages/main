require File.dirname(__FILE__) + "/../spec_helper"
module WordEventTracker
  def add_event(doc)
    doc.WindowSelectionChange.add(method(:handler))
  end

  def remove_event(doc)
    doc.WindowSelectionChange.remove(method(:handler))
  end
end

describe "Word COM interop support" do
  before :each do
    raise "Word is not installed" unless ComHelper.word_installed?

    @app = ComHelper.create_word_app
    @app.DisplayAlerts = false
    @doc = @app.Documents.Add
    @doc.Range.Text = "test"
    @tracker = ComHelper::EventTracker.new
    class << @tracker
      include WordEventTracker
    end
  end

  after :each do
    @doc = nil
    System::GC.Collect
    System::GC.WaitForPendingFinalizers
    @app.Quit if @app
  end
  
  it "fires for single event" do
    @tracker.add_event(@doc)
    @app.Range(1,1).Select
    @tracker.counter.should == 1
  end

  it "fires for multiple events" do
    @tracker.add_event(@doc)
    @tracker.add_event(@doc)
    @app.Range(1,2).Select
    @tracker.counter.should == 2
  end

  it "fires after removing an event" do
    @tracker.add_event(@doc)
    @tracker.add_event(@doc)
    @app.Range(2,2).Select
    @tracker.remove_event(@doc)
    @app.Range(2,3).Select
    @tracker.counter.should == 3
  end

  it "fires after removing all events" do
    @tracker.add_event(@doc)
    @tracker.add_event(@doc)
    @app.Range(3,3).Select
    @tracker.remove_event(@doc)
    @tracker.remove_event(@doc)
    @app.Range(3,4).Select
    @tracker.counter.should == 2
  end

  it "fires after removing all events, then adding one back" do
    @tracker.add_event(@doc)
    @tracker.add_event(@doc)
    @app.Range(3,4).Select
    @tracker.remove_event(@doc)
    @tracker.remove_event(@doc)
    @app.Range(3,4).Select
    @tracker.add_event(@doc)
    @app.Range(3,4).Select
    @tracker.counter.should == 3
  end

  describe "spellchecker" do
    before(:each) do
      @suggestions = @app.GetSpellingSuggestions("waht")
    end

    it "returns suggestions" do
      @suggestions.Count.should > 5
    end

    it "supports enumeration" do
      @suggestions.each { |s| s.name }
    end

    it "contains suggestions" do
      @suggestions.should include("what")
      @suggestions.should include("with")
    end
  end
end

