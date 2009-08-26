require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe DataMapper do
  describe ".dependency_queue" do
    before(:all) do
      @q = DataMapper.dependency_queue
    end

    it "should return a dependency queue" do
      @q.should be_a_kind_of(DataMapper::DependencyQueue)
    end

    it "should only create one dependency queue" do
      @q.should == DataMapper.dependency_queue
    end
  end

  describe ".prepare" do
    it "should pass the default repository to the block if no argument is given" do
      DataMapper.should_receive(:repository).with(no_args).and_return :default_repo

      DataMapper.prepare do |r|
        r.should == :default_repo
      end
    end

    it "should allow custom type maps to be defined inside the prepare block" do
      lambda {
        DataMapper.prepare do |r|
          r.map(String).to(:VARCHAR).with(:size => 1000)
        end
      }.should_not raise_error
    end
  end
end
