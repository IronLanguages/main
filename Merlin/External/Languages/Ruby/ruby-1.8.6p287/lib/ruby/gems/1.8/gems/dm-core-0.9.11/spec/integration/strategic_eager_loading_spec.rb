require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe "Strategic Eager Loading" do
  include LoggingHelper

  before :all do
    class ::Zoo
      include DataMapper::Resource
      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :name, String

      has n, :exhibits
    end

    class ::Exhibit
      include DataMapper::Resource
      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :name, String
      property :zoo_id, Integer

      belongs_to :zoo
      has n, :animals
    end

    class ::Animal
      include DataMapper::Resource
      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :name, String
      property :exhibit_id, Integer

      belongs_to :exhibit
    end

    [Zoo, Exhibit, Animal].each { |k| k.auto_migrate!(ADAPTER) }

    repository(ADAPTER) do
      Zoo.create(:name => "Dallas Zoo")
      Exhibit.create(:name => "Primates", :zoo_id => 1)
      Animal.create(:name => "Chimpanzee", :exhibit_id => 1)
      Animal.create(:name => "Orangutan", :exhibit_id => 1)

      Zoo.create(:name => "San Diego")
      Exhibit.create(:name => "Aviary", :zoo_id => 2)
      Exhibit.create(:name => "Insectorium", :zoo_id => 2)
      Exhibit.create(:name => "Bears", :zoo_id => 2)
      Animal.create(:name => "Bald Eagle", :exhibit_id => 2)
      Animal.create(:name => "Parakeet", :exhibit_id => 2)
      Animal.create(:name => "Roach", :exhibit_id => 3)
      Animal.create(:name => "Brown Bear", :exhibit_id => 4)
    end
  end

  it "should eager load children" do
    zoo_ids     = Zoo.all.map { |z| z.key }
    exhibit_ids = Exhibit.all.map { |e| e.key }

    repository(ADAPTER) do
      zoos = Zoo.all.entries # load all zoos
      dallas = zoos.find { |z| z.name == 'Dallas Zoo' }

      logger do |log|
        dallas.exhibits.entries # load all exhibits for zoos in identity_map
        dallas.exhibits.size.should == 1

        log.readlines.size.should == 1

        repository.identity_map(Zoo).keys.sort.should == zoo_ids
        repository.identity_map(Exhibit).keys.sort.should == exhibit_ids
      end

      logger do |log|
        zoos.each { |zoo| zoo.exhibits.entries } # issues no queries
        log.readlines.should be_empty
      end

      dallas.exhibits << Exhibit.new(:name => "Reptiles")
      dallas.exhibits.size.should == 2
      dallas.save
    end
    repository(ADAPTER) do
      Zoo.first.exhibits.size.should == 2
    end
  end

  it "should not eager load children when a query is provided" do
    repository(ADAPTER) do
      dallas = Zoo.all.entries.find { |z| z.name == 'Dallas Zoo' }
      exhibits = dallas.exhibits.entries # load all exhibits

      reptiles, primates = nil, nil

      logger do |log|
        reptiles = dallas.exhibits(:name => 'Reptiles')
        reptiles.size.should == 1

        log.readlines.size.should == 1
      end

      logger do |log|
        primates = dallas.exhibits(:name => 'Primates')
        primates.size.should == 1
        primates.should_not == reptiles

        log.readlines.size.should == 1
      end
    end
  end

  it "should eager load parents" do
    animal_ids  = Animal.all.map { |a| a.key }
    exhibit_ids = Exhibit.all.map { |e| e.key }.sort
    exhibit_ids.pop # remove Reptile exhibit, which has no Animals

    repository(ADAPTER) do
      animals = Animal.all.entries
      bear = animals.find { |a| a.name == 'Brown Bear' }

      logger do |log|
        bear.exhibit

        repository.identity_map(Animal).keys.sort.should == animal_ids
        repository.identity_map(Exhibit).keys.sort.should == exhibit_ids

        log.readlines.size.should == 1
      end
    end
  end

  it "should not eager load parents when parent is in IM" do
    repository(ADAPTER) do
      animal = Animal.first
      exhibit = Exhibit.get(1) # load exhibit into IM

      logger do |log|
        animal.exhibit # load exhibit from IM
        log.readlines.should be_empty
      end

      repository.identity_map(Exhibit).keys.should == [exhibit.key]
    end
  end

  it "should return a Collection when no children" do
    Zoo.create(:name => 'Portland')

    Zoo.all.each do |zoo|
      zoo.exhibits.should be_kind_of(DataMapper::Collection)
    end
  end
end
