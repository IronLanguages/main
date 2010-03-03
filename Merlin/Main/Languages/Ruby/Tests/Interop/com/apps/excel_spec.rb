require File.dirname(__FILE__) + "/../spec_helper"
module ExcelEventTracker
  def add_event(ws)
    e = WIN32OLE_EVENT.new(ws, "DocEvents")
    e.on_event("SelectionChange") { |obj, event| handler(obj, event) }
  end

  def remove_event(ws)
    e = WIN32OLE_EVENT.new(ws, "DocEvents")
    # WIN32OLE_EVENT does not document any way to unsubscribe from an event
    # So we use this syntax
    e.on_event("SelectionChange")
  end
end

if ENV["THISISSNAP"] || ComHelper.excel_installed?
  describe "Excel" do
    before :each do
      raise "Excel is not installed" unless ComHelper.excel_installed?

      @app = ComHelper.create_excel_app
      @app.DisplayAlerts = false
      @workbook = @app.Workbooks.Add
      @worksheet = @workbook.Worksheets(1)
    end

    after :each do
      @worksheet = nil
      @workbook = nil
      System::GC.Collect
      System::GC.WaitForPendingFinalizers
      @app.Quit if @app
    end

    describe "COM interop" do
      it "should not require PIA" do
        $LOADED_FEATURES.grep(/Excel/).should == []
      end

      it "has assignable 'properties'" do
        @app.DisplayAlerts.should == false
      end

      it "can create worksheets" do
        @worksheet.Name.should == "Sheet1"
      end

      it "converts Ruby types to COM types" do
        @worksheet.setproperty("Cells", 1, 1, 1)
        @worksheet.cells(1, 1).value.should == 1

        @worksheet.setproperty("Cells", 1, 1, 2.0)
        @worksheet.cells(1, 1).value.should == 2.0

        @worksheet.setproperty("Cells", 1, 1, 1024**3)
        @worksheet.cells(1, 1).value.should == 1073741824.0
        
        @worksheet.setproperty("Cells", 1, 1, "hello")
        @worksheet.cells(1, 1).value.should == "hello"
        
        @worksheet.setproperty("Cells", 1, 1, :rubysym)
        @worksheet.cells(1, 1).value.should == 'rubysym'
      end

      it "can select ranges" do 
        range = @worksheet.Range('A1','B3') 
        range.Count.should == 6 
      end 
      
      it "can create charts" do 
        lambda {
          range = @worksheet.Range('A1','B3')
          chart = @worksheet.ChartObjects
          graph = chart.Add(100,100,200,200)
          #Microsoft::Office::Interop::Excel::XlChartType.xl3DColumn = -4100
          graph.Chart.ChartWizard(range, -4100)
          1.should == 1
        }.should_not raise_error(RuntimeError)
      end
    end
    
    describe "COM event handling" do
      before(:each) do
        @tracker = ComHelper::EventTracker.new
        class << @tracker
          include ExcelEventTracker
        end
      end

      it "fires for single event" do
        @tracker.add_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.counter.should == 1
      end

      it "fires for multiple events" do
        @tracker.add_event(@worksheet)
        @tracker.add_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.counter.should == 2
      end

      it "fires after removing an event" do
        @tracker.add_event(@worksheet)
        @tracker.add_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.remove_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.counter.should == 3
      end

      it "fires after removing all events" do
        @tracker.add_event(@worksheet)
        @tracker.add_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.remove_event(@worksheet)
        @tracker.remove_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.counter.should == 2
      end

      it "fires after removing all events, then adding one back" do
        @tracker.add_event(@worksheet)
        @tracker.add_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.remove_event(@worksheet)
        @tracker.remove_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.add_event(@worksheet)
        @app.ActiveCell.Offset(1,0).Activate
        @tracker.counter.should == 3
      end
    end
  end
end
