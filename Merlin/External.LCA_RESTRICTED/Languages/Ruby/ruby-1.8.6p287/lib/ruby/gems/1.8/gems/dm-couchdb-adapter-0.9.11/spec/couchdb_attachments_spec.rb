require File.join(File.dirname(__FILE__), 'spec_helper.rb')

if COUCHDB_AVAILABLE
  require 'base64'
  require 'pathname'

  describe DataMapper::Model do

    before do
      Object.send(:remove_const, :NonCouch) if defined?(NonCouch)
      class ::NonCouch
        include DataMapper::Resource

        property :id, Serial
      end

      Object.send(:remove_const, :Message) if defined?(Message)
      class ::Message
        include DataMapper::CouchResource
        def self.default_repository_name
          :couch
        end

        property :content, String
      end

      @file = File.open(Pathname(__FILE__).dirname.expand_path + "testfile.txt", "r")
    end

    after do
      @file.close
    end

    describe "#add_attachment" do

      it "should add inline attributes to new records" do
        @message = Message.new
        @message.add_attachment(@file, :name => 'test.txt')
        @message.attachments.should == {
          'test.txt' => {
            'content_type' => 'text/plain',
            'data' => Base64.encode64("test string\n").chomp
          }
        }
      end

      it "should upload standalone attachment for existing record" do
        @message = Message.new(:content => 'test message')
        @message.save.should be_true
        @message.add_attachment(@file, :name => 'test.txt')
        @message.attachments['test.txt']['stub'].should be_true
        @message.attachments['test.txt']['content_type'].should == 'text/plain'
        @message.attachments['test.txt']['data'].should be_nil
        @message.destroy.should be_true
      end

      it "should have meta data on load" do
        pending("No CouchDB connection.") if @no_connection
        @message = Message.new
        @message.add_attachment(@file, :name => 'test.txt')
        @message.save.should be_true
        @message.reload
        @message.attachments['test.txt']['stub'].should be_true
        @message.attachments['test.txt']['content_type'].should == 'text/plain'
        @message.attachments['test.txt']['data'].should be_nil
        @message.destroy.should be_true
      end

    end


    describe "#delete_attachment" do

      it "should remove unsaved attachments" do
        @message = Message.new
        @message.add_attachment(@file, :name => 'test.txt')
        @message.delete_attachment('test.txt').should be_true
        @message.attachments.should be_nil
      end

      it "should remove saved attachments" do
        @message = Message.new
        @message.add_attachment(@file, :name => 'test.txt')
        @message.save.should be_true
        @message.reload
        @message.attachments.should_not be_nil
        @message.delete_attachment('test.txt').should be_true
        @message.attachments.should be_nil
        @message = Message.get(@message.id)
        @message.attachments.should be_nil
        @message.destroy.should be_true
      end

    end


    describe "#get_attachment" do

      it "should return nil when there is not attachment" do
        @message = Message.new
        @message.get_attachment('test.txt').should be_nil
      end

      it "should return attachment data when it exists" do
        @message = Message.new
        @message.add_attachment(@file, :name => 'test.txt')
        @message.save.should be_true
        @message.reload
        @message.get_attachment('test.txt').should == "test string\n"
        @message.destroy.should be_true
      end

    end

  end
end
