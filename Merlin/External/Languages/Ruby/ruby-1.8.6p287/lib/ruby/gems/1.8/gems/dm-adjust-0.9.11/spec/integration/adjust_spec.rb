require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES

  class Person
    include DataMapper::Resource
    property :id, Integer, :serial => true
    property :name, String
    property :salary, Integer, :default => 20000
    property :age, Integer
  end

  describe 'Adjust' do

    before do
      Person.auto_migrate!(:default)
      Person.create(:name => 'George', :age => 15)
      Person.create(:name => 'Puff',   :age => 18)
      Person.create(:name => 'Danny',  :age => 26)
      Person.create(:name => 'Selma',  :age => 28)
      Person.create(:name => 'John',   :age => 49)
      Person.create(:name => 'Amadeus',:age => 60)
    end

    describe 'Model#adjust!' do
      it 'should adjust values' do
        repository(:default) do
          Person.adjust!({:salary => 1000},true)
          Person.all.each{|p| p.salary.should == 21000}
        end
      end
    end

    describe 'Collection#adjust!' do
      it 'should adjust values' do
        repository(:default) do |repos|
          @oldies = Person.all(:age.gte => 40)
          @oldies.adjust!({:salary => 5000},true)
          @oldies.each{|p| p.salary.should == 25000}

          Person.get(1).salary.should == 20000

          @children = Person.all(:age.lte => 18)
          @children.adjust!({:salary => -10000},true)
          @children.each{|p| p.salary.should == 10000}
        end
      end

      it 'should load the query if conditions were adjusted' do
        repository(:default) do |repos|
          @specific = Person.all(:salary => 20000)
          @specific.adjust!({:salary => 5000},true)
          @specific.length.should == 6
        end
      end
    end

    describe 'Resource#adjust' do
      it 'should adjust the value' do
        repository(:default) do |repos|
          p = Person.get(1)
          p.salary.should == 20000
          p.adjust!({:salary => 1000},true)
          p.salary.should == 21000
        end
      end
    end
  end
end
