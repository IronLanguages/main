require File.dirname(__FILE__) + "/../../spec_helper"
require 'WindowsBase'

describe "ObservableCollections" do
  it "can be created" do
    System::Collections::ObjectModel::ObservableCollection[String].new.should be_kind_of System::Collections::ObjectModel::ObservableCollection[String]
  end

  it "can be used" do
    coll = System::Collections::ObjectModel::ObservableCollection[String].new
    coll.add "hello"
    coll[0].should == "hello"
  end
end
