require File.join(File.dirname(__FILE__), 'spec_helper.rb')

class Viewable
  include DataMapper::CouchResource
  def self.default_repository_name
    :couch
  end

  property :name, String
  property :open, Boolean
end

describe DataMapper::CouchResource::View do
  it "should have a view method" do
    Viewable.should respond_to(:view)
  end

  it "should store a view when called" do
    Viewable.view :by_name
    Viewable.views.keys.should include(:by_name)
  end

  it "should initialize a new Procedure instance" do
    proc = Viewable.view :by_name_desc
    proc.should be_an_instance_of(DataMapper::CouchResource::View)
  end

  it "should create a getter method" do
    Viewable.view :open
    Viewable.should respond_to(:open)
  end

  describe "for inherited resources" do
    before(:all) do
      Person.auto_migrate!
    end

    it "should set the correct couchdb types" do
      Person.couchdb_types.include?(Person).should be_true
      Person.couchdb_types.include?(Employee).should be_true
    end

    it "should create views with the correct couchdb type conditions" do
      Person.views[:by_name].should == {"map"=>"function(doc) { if (doc.couchdb_type == 'Person' || doc.couchdb_type == 'Employee') { emit(doc.name, doc); } }"}
    end
  end
end
