require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/abstract_store_spec'

begin
  require 'memcached'
  servers = ['127.0.0.1:43042', '127.0.0.1:43043']
  namespace = 'memcached_test_namespace'
  
  options = {      
    :namespace    => @namespace,
    :hash         => :default,
    :distribution => :modula
  }
  cache = Memcached.new(servers, options)
  key, value = Time.now.to_i.to_s, Time.now.to_s
  cache.set(key, value)
  raise Exception unless cache.get(key) == value
rescue Exception => e
  puts e.message
  puts "Memcached connection failed.  Try starting memcached with the memcached:start rake task or installing memcached gem with sudo gem install memcached."
else

  describe Merb::Cache::MemcachedStore do
    it_should_behave_like 'all stores'

    before(:each) do
      @store = Merb::Cache::MemcachedStore.new(:namespace => "specs", :servers => ["127.0.0.1:43042", "127.0.0.1:43043"])
      @memcached = @store.memcached
      @memcached.flush
    end

    after(:each) do
      @memcached.flush
    end
  

    it "has accessor for namespace" do
      @store.namespace.should == "specs"
    end

    it "has accessor for servers" do
      @store.servers.should == ["127.0.0.1:43042", "127.0.0.1:43043"]
    end
  
    it "has accessor for memcached connector" do
      @store.memcached.should == @memcached
    end


    #
    # ==== #writable?
    #

    describe "#writable?" do
      describe "when conditions hash is empty" do
        it "returns true" do
          @store.writable?('foo').should be_true
        end
      end
    end
  
    #
    # ==== #read
    #  

    describe "#read" do
      describe "when cache has NO entry matching key" do
        it "returns nil" do
          key = "foo"

          @memcached.delete(key) rescue nil
          @store.read(key).should be_nil
        end
      end
    
      describe "when cache has entry matching key" do
        it "returns the entry matching the key" do
          key, data = "foo", "bar"

          @memcached.set(key, data)

          @store.read(key).should == data
        end      
      end    
    end

    #
    # ==== #write
    #

    describe "#write" do
      describe "when entry with the same key does not exist" do
        it "create a new entry" do
          key, data = "foo", "bar"

          @memcached.delete(key) rescue nil
          lambda { @memcached.get(key) }.should raise_error(Memcached::NotFound)

          @store.write(key, data)
          @memcached.get(key).should == data
        end      
      end
    
      describe "when entry with the same key already exists" do
        it "overwrites the entry in the cache" do
          key, data = "foo", "bar"

          @memcached.set(key, "baz")
          @memcached.get(key).should == "baz"

          @store.write(key, data)
          @memcached.get(key).should == data
        end      
      end    
    end

    #
    # ==== #fetch
    #

    describe "#fetch" do
      describe "when the entry exists in the cache" do
        it "does NOT call the block" do
           key, data = "foo", "bar"
           called = false
           proc = lambda { called = true }

           @memcached.set(key, data)
           @store.fetch(key, &proc)

           called.should be_false
        end      
      end
    
      describe "when the entry does not exist in the cache" do
        it "calls the block" do
          key, data = "foo", "bar"
           called = false
           proc = lambda { called = true }

           @memcached.delete(key) rescue nil
           @store.fetch(key, &proc)
         
           called.should be_true
        end      
      end    
    end

    #
    # ==== #delete
    #

    describe "#delete" do
      describe "when the entry exists in the cache" do
        it "deletes the entry" do
          key, data = "foo", "bar"

          @memcached.set(key, data)
          @memcached.get(key).should == data

          @store.delete(key)
          lambda { @memcached.get(key) }.should raise_error(Memcached::NotFound)
        end      
      end
    
      describe "when the entry does not exist in the cache" do
        it "raises Memcached::NotFound" do
          lambda { @memcached.delete("#{rand}-#{rand}-#{Time.now.to_i}") }.
            should raise_error(Memcached::NotFound)
        end      
      end
    end

    #
    # ==== #delete_all
    #

    describe "#delete_all" do
      it "flushes Memcached object" do
        @memcached.set("ruby", "rb")
        @memcached.set("python", "py")
        @memcached.set("perl", "pl")

        @store.delete_all
      
        @store.exists?("ruby").should be_nil
        @store.exists?("python").should be_nil
        @store.exists?("perl").should be_nil           
      end
    end

    #
    # ==== #clone
    #

    describe "#clone" do
      it "clones Memcached instance" do
        clone = @store.clone
      
        clone.memcached.object_id.should_not == @store.memcached.object_id
      end
    end

    #
    # ==== #normalize
    #

    describe "#normalize" do
      it "should begin with the key" do
        @store.normalize("this/is/the/key").should =~ /^this\/is\/the\/key/
      end

      it "should not add the '?' if there are no parameters" do
        @store.normalize("this/is/the/key").should_not =~ /\?/
      end

      it "should seperate the parameters from the key by a '?'" do
        @store.normalize("this/is/the/key", :page => 3, :lang => :en).
          should =~ %r!this\/is\/the\/key--#{{:page => 3, :lang => :en}.to_sha2}$!
      end
    end

    #
    # ==== #expire_time
    #

    describe "#expire_time" do
      describe "when there is NO :expire_in parameter" do
        it "returns 0" do
          @store.expire_time.should == 0
        end      
      end
    
      describe "when there is :expire_in parameter" do
        it "returns Time.now + the :expire_in parameter" do
          now = Time.now
          Time.should_receive(:now).and_return now

          @store.expire_time(:expire_in => 100).should == now + 100
        end      
      end
    end
  end
end