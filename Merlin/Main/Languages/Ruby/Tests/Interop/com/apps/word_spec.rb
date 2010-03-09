require File.dirname(__FILE__) + "/../spec_helper"
module WordEventTracker
  def add_event(doc)
    e = WIN32OLE_EVENT.new(doc, "DocEvents")
    e.on_event("WindowSelectionChange") { |obj, event| handler(obj, event) }
  end

  def remove_event(doc)
    e = WIN32OLE_EVENT.new(doc, "DocEvents")
    # WIN32OLE_EVENT does not document any way to unsubscribe from an event
    # So we use this syntax
    e.on_event("WindowSelectionChange")
  end
end

if ENV["THISISSNAP"] || ComHelper.word_installed?
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
      @app.Quit(0) if @app
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
        @useable_suggestions = []
        @suggestions.each {|e| @useable_suggestions << e.name}
      end

      it "returns suggestions" do
        @suggestions.Count.should > 5
      end

      it "supports enumeration" do
        lambda {@suggestions.each { |s| s.name }}.should_not raise_error
      end

      it "contains suggestions" do
        @useable_suggestions.should include("what")
        @useable_suggestions.should include("with")
      end
    end
  end
end
