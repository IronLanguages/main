require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Serialize, '#to_csv' do
  #
  # ==== blah, it's CSV
  #

  before(:all) do
    query = DataMapper::Query.new(DataMapper::repository(:default), Cow)

    @collection = DataMapper::Collection.new(query) do |c|
      c.load([1, 2, 'Betsy', 'Jersey'])
      c.load([10, 20, 'Berta', 'Guernsey'])
    end

    @empty_collection = DataMapper::Collection.new(query) {}
  end

  it "should serialize a resource to CSV" do
    peter = Cow.new
    peter.id = 44
    peter.composite = 344
    peter.name = 'Peter'
    peter.breed = 'Long Horn'
    peter.to_csv.chomp.split(',')[0..3].should == ['44','344','Peter','Long Horn']
  end

  it "should serialize a collection to CSV" do
    result = @collection.to_csv.gsub(/[[:space:]]+\n/, "\n")
    result.split("\n")[0].split(',')[0..3].should == ['1','2','Betsy','Jersey']
    result.split("\n")[1].split(',')[0..3].should == ['10','20','Berta','Guernsey']
  end

  describe "multiple repositories" do
    before(:all) do
      QuanTum::Cat.auto_migrate!
      repository(:alternate){QuanTum::Cat.auto_migrate!}
    end

    it "should use the repsoitory for the model" do
      gerry = QuanTum::Cat.create(:name => "gerry")
      george = repository(:alternate){QuanTum::Cat.create(:name => "george", :is_dead => false)}
      gerry.to_csv.should_not match(/false/)
      george.to_csv.should match(/false/)
    end
  end
end
