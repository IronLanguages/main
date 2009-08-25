require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

if ADAPTER
  describe DataMapper::Repository, "with #{ADAPTER}" do
    before :all do
      class ::SerialFinderSpec
        include DataMapper::Resource

        property :id, Serial
        property :sample, String

        auto_migrate!(ADAPTER)
      end

      repository(ADAPTER).create((0...100).map { SerialFinderSpec.new(:sample => rand.to_s) })
    end

    before do
      @repository = repository(ADAPTER)
      @model      = SerialFinderSpec
      @query      = DataMapper::Query.new(@repository, @model)
    end

    it 'should be serializable with Marshal' do
      Marshal.load(Marshal.dump(@repository)).should == @repository
    end

    it "should throw an exception if the named repository is unknown" do
      r = DataMapper::Repository.new(:completely_bogus)
      lambda { r.adapter }.should raise_error(ArgumentError)
    end

    it "should return all available rows" do
      @repository.read_many(@query).should have(100).entries
    end

    it "should allow limit and offset" do
      @repository.read_many(@query.merge(:limit => 50)).should have(50).entries

      collection = @repository.read_many(@query.merge(:limit => 20, :offset => 40))
      collection.should have(20).entries
      collection.map { |entry| entry.id }.should == @repository.read_many(@query)[40...60].map { |entry| entry.id }
    end

    it "should lazy-load missing attributes" do
      sfs = @repository.read_one(@query.merge(:fields => [ :id ], :limit => 1))
      sfs.should be_a_kind_of(@model)
      sfs.should_not be_a_new_record

      sfs.attribute_loaded?(:sample).should be_false
      sfs.sample.should_not be_nil
    end

    it "should translate an Array to an IN clause" do
      ids = @repository.read_many(@query.merge(:fields => [ :id ], :limit => 10)).map { |entry| entry.id }
      results = @repository.read_many(@query.merge(:id => ids))

      results.map { |entry| entry.id }.should == ids
    end
  end
end
