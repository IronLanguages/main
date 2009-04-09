require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe "DataMapper::IdentityMap" do
  before(:all) do
    class ::Cow
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :name, String
    end

    class ::Chicken
      include DataMapper::Resource
      property :name, String
    end

    class ::Pig
      include DataMapper::Resource
      property :id, Integer, :key => true
      property :composite, Integer, :key => true
      property :name, String
    end
  end

  it "should use a second level cache if created with on"

  it "should return nil on #get when it does not find the requested instance" do
    map = DataMapper::IdentityMap.new
    map.get([23]).should be_nil
  end

  it "should return an instance on #get when it finds the requested instance" do
    betsy = Cow.new({:id=>23,:name=>'Betsy'})
    map = DataMapper::IdentityMap.new
    map.set(betsy.key, betsy)
    map.get([23]).should == betsy
  end

  it "should store an instance on #set" do
    betsy = Cow.new({:id=>23,:name=>'Betsy'})
    map = DataMapper::IdentityMap.new
    map.set(betsy.key, betsy)
    map.get([23]).should == betsy
  end

  it "should store instances with composite keys on #set" do
    pig = Pig.new({:id=>1,:composite=>1,:name=> 'Pig'})
    piggy = Pig.new({:id=>1,:composite=>2,:name=>'Piggy'})

    map = DataMapper::IdentityMap.new
    map.set(pig.key, pig)
    map.set(piggy.key, piggy)

    map.get([1,1]).should == pig
    map.get([1,2]).should == piggy
  end

  it "should remove an instance on #delete" do
    betsy = Cow.new({:id=>23,:name=>'Betsy'})
    map = DataMapper::IdentityMap.new
    map.set(betsy.key, betsy)
    map.delete([23])
    map.get([23]).should be_nil
  end
end

describe "Second Level Caching" do

  before :all do
    @mock_class = Class.new do
      def get(key);           raise NotImplementedError end
      def set(key, instance); raise NotImplementedError end
      def delete(key);        raise NotImplementedError end
    end
  end

  it 'should expose a standard API' do
    cache = @mock_class.new
    cache.should respond_to(:get)
    cache.should respond_to(:set)
    cache.should respond_to(:delete)
  end

  it 'should provide values when the first level cache entry is empty' do
    cache = @mock_class.new
    key   = %w[ test ]

    cache.should_receive(:get).with(key).and_return('resource')

    map = DataMapper::IdentityMap.new(cache)
    map.get(key).should == 'resource'
  end

  it 'should be set when the first level cache entry is set' do
    cache = @mock_class.new
    betsy = Cow.new(:id => 23, :name => 'Betsy')

    cache.should_receive(:set).with(betsy.key, betsy).and_return(betsy)

    map = DataMapper::IdentityMap.new(cache)
    map.set(betsy.key, betsy).should == betsy
  end

  it 'should be deleted when the first level cache entry is deleted' do
    cache = @mock_class.new
    betsy = Cow.new(:id => 23, :name => 'Betsy')

    cache.stub!(:set)
    cache.should_receive(:delete).with(betsy.key).and_return(betsy)

    map = DataMapper::IdentityMap.new(cache)
    map.set(betsy.key, betsy).should == betsy
    map.delete(betsy.key).should == betsy
  end

  it 'should not provide values when the first level cache entry is full' do
    cache = @mock_class.new
    betsy = Cow.new(:id => 23, :name => 'Betsy')

    cache.stub!(:set)
    cache.should_not_receive(:get)

    map = DataMapper::IdentityMap.new(cache)
    map.set(betsy.key, betsy).should == betsy
    map.get(betsy.key).should == betsy
  end
end
