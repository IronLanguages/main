require File.dirname(__FILE__) + '/../../../spec_helper'
# has 'all stores' shared group
require File.dirname(__FILE__) + '/abstract_store_spec'

describe Merb::Cache::FileStore do
  it_should_behave_like 'all stores'

  before(:each) do
    @store = Merb::Cache::FileStore.new(:dir => File.dirname(Tempfile.new("").path))
  end

  #
  # ==== #writable
  #

  describe "#writable?" do
    describe "when conditions hash is empty" do
      it "returns true" do
        @store.writable?('foo').should be_true
      end
    end

    describe "when given expire_in option" do
      it "returns false" do
        @store.writable?('foo', {}, :expire_in => 10).should be_false
      end
    end
  end

  #
  # ==== #read
  #

  describe "#read" do
    describe "when cache file does not exist" do
      it "should return nil" do
        key = "body.txt"

        FileUtils.rm(@store.pathify(key)) if File.exists?(@store.pathify(key))
        @store.read(key).should be_nil
      end
    end

    describe "when cache file exists" do
      it "reads the contents of the file" do
        key  = "tmp.txt"
        body = "body of the file"

        File.open(@store.pathify(key), "w+") do |file|
          file << body
        end

        @store.read(key).should == body
      end
    end
  end

  #
  # ==== #write
  #

  describe "#write" do
    describe "if it does not exist" do
      it "create the file" do
        key = "body.txt"

        FileUtils.rm(@store.pathify(key)) if File.exists?(@store.pathify(key))

        File.should_not be_exist(@store.pathify(key))

        @store.write(key, "")

        File.should be_exist(@store.pathify(key))
      end
    end

    describe "when file already exists" do
      it "overwrites the file" do
        key = "tmp.txt"
        old_body, new_body = "old body", "new body"

        File.open(@store.pathify(key), "w+") do |file|
          file << old_body
        end

        File.open(@store.pathify(key), "r") do |file|
          file.read.should == old_body
        end

        @store.write(key, new_body)

        File.open(@store.pathify(key), "r") do |file|
          file.read.should == new_body
        end
      end
    end
  end

  #
  # ==== #fetch
  #

  describe "#fetch" do
    describe "when the entry can be read" do
      it "does not call the block" do
        key, body = "tmp.txt", "body"
        called    = false
        proc      = lambda { called = true }
        
        File.open(@store.pathify(key), "w+") do |file|
          file << body
        end
        
        @store.fetch(key, &proc)
        called.should be_false
      end
    end

    describe "when entry cannot be read" do
      it "calls the block" do
        key    = "tmp.txt"
        called = false
        proc   = lambda { called = true }

        FileUtils.rm(@store.pathify(key)) if File.exists?(@store.pathify(key))
        
        @store.fetch(key, &proc)
        called.should be_true
      end      
    end
  end

  #
  # ==== #delete
  #

  describe "#delete" do
    describe "when file exists" do
      it "deletes the file" do
        key, body = "tmp.txt", "body of the file"

        File.open(@store.pathify(key), "w+") do |file|
          file << body
        end

        File.exists?(@store.pathify(key)).should be_true
        @store.delete(key)
        File.exists?(@store.pathify(key)).should be_false
      end
    end
    
    describe "when file does not exist" do
      it "does not raise" do
        @store.delete("#{rand}-#{rand}-#{Time.now.to_i}.txt")
      end      
    end
  end

  #
  # ==== #delete_all
  #

  describe "#delete_all" do
    it "is not supported" do
      lambda { @store.delete_all }.should raise_error(Merb::Cache::NotSupportedError)
    end
  end

  #
  # ==== #pathify
  #

  describe "#pathify" do
    it "should begin with the cache dir" do
      @store.pathify("tmp.txt").should include(@store.dir)
    end

    it "should add any parameters to the end of the filename" do
      @store.pathify("index.html", :page => 3, :lang => :en).should =~ %r[--#{{:page => 3, :lang => :en}.to_sha2}$]
    end

    it "should separate the parameters from the key by a '?'" do
      @store.pathify("index.html", :page => 3, :lang => :en).should =~ %r[--#{{:page => 3, :lang => :en}.to_sha2}$]
    end
  end
end