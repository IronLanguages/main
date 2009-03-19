require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe "DataMapper::Resource" do
    after do
     repository(:default).adapter.execute('DELETE from green_smoothies');
    end

    before(:all) do
      class ::GreenSmoothie
        include DataMapper::Resource
        property :id, Integer, :serial => true
        property :name, String

        auto_migrate!(:default)
      end
    end

    it "should find/create using find_or_create" do
      repository(:default) do
        green_smoothie = GreenSmoothie.new(:name => 'Banana')
        green_smoothie.save
        GreenSmoothie.find_or_create({:name => 'Banana'}).id.should eql(green_smoothie.id)
        GreenSmoothie.find_or_create({:name => 'Strawberry'}).id.should eql(2)
      end
    end

    it "should use find_by and use the name attribute to find a record" do
      repository(:default) do
        green_smoothie = GreenSmoothie.create({:name => 'Banana'})
        green_smoothie.should == GreenSmoothie.find_by_name('Banana')
      end
    end

    it "should use find_all_by to find records using an attribute" do
      repository(:default) do
        green_smoothie = GreenSmoothie.create({:name => 'Banana'})
        green_smoothie2 = GreenSmoothie.create({:name => 'Banana'})
        found_records = GreenSmoothie.find_all_by_name('Banana')
        found_records.length.should == 2
        found_records.each do |found_record|
          [green_smoothie, green_smoothie2].include?(found_record).should be_true
        end
      end
    end
  end
end
