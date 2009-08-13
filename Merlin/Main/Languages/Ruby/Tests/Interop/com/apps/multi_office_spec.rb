require File.dirname(__FILE__) + "/../spec_helper"

if ENV["THISISSNAP"] || (ComHelper.excel_installed? && ComHelper.word_installed?)
  describe "Multiple Office application COM interop support" do
    before(:each) do
      raise "Excel is not installed" unless ComHelper.excel_installed?
      raise "Word is not installed" unless ComHelper.word_installed?

      @excel = ComHelper.create_excel_app
      @excel.DisplayAlerts = false
      @workbook = @excel.Workbooks.Add

      @word = ComHelper.create_word_app
      @word.DisplayAlerts = false
      @doc = @word.Documents.Add
    end

    after :each do
      @workbook = nil
      @doc = nil
      System::GC.Collect
      System::GC.WaitForPendingFinalizers
      @excel.Quit if @excel
      @word.Quit if @word
    end

    it "allows multiple applications at once" do
      lambda { @excel.Workbooks(1) }.should_not raise_error
      lambda { @word.Documents(1) }.should_not raise_error
    end
  end
end
