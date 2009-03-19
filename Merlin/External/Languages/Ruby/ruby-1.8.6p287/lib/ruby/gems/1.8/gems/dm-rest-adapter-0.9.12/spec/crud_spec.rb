$LOAD_PATH << File.dirname(__FILE__)
require 'spec_helper'

describe 'A REST adapter' do

  before do
    @book = Book.new(:title => 'Hello, World!', :author => 'Anonymous')
    @adapter = DataMapper::Repository.adapters[:default]
  end

  describe 'when saving a new resource' do

    before(:each) do
      @mock_resp = mock("response")

      @book.id = 1
      @mock_resp.should_receive(:body).and_return @book.to_xml
    end

    it "should create a book" do
      @mock_http = mock("http")
      Net::HTTP.should_receive(:start).and_yield @mock_http

      @mock_resp.should_receive(:code).and_return 200

      @mock_http.should_receive(:request).and_return @mock_resp

      @book.save.should eql(true)
    end

    it 'should make an HTTP POST' do
      @adapter.connection.should_receive(:http_post).with('books', @book.to_xml).and_return @mock_resp
      @book.save
    end

    it 'should call run_verb with POST' do
      @adapter.connection.should_receive(:run_verb).with('post', @book.to_xml).and_return @mock_resp
      @book.save
    end

  end

  describe 'when returning incorrect xml from a save' do
    before(:all) do
      @mock_resp = mock("response")
    end

    it "should raise error on missing root element in xml" do
      @mock_resp.should_receive(:body).and_return ""
      @adapter.connection.should_receive(:run_verb).with('post', @book.to_xml).and_return @mock_resp

      lambda {@book.save}.should raise_error(RuntimeError, "No root element matching book in xml")
    end

    it "should not raise an error if the root xml is empty" do
      @mock_resp.should_receive(:body).and_return "<book></book>"
      @adapter.connection.should_receive(:run_verb).with('post', @book.to_xml).and_return @mock_resp

      lambda {@book.save}.should_not raise_error(RuntimeError)
    end
  end

  describe 'when deleting an existing resource' do
    before do
      @book.stub!(:new_record?).and_return(false)
    end

    it 'should do an HTTP DELETE' do
      @adapter.connection.should_receive(:http_delete)
      @book.destroy
    end

    it "should raise NotImplementedError if is not a single resource query" do
      @adapter.should_receive(:is_single_resource_query?).and_return(false)
      lambda {@book.destroy}.should raise_error(NotImplementedError)
    end

    it 'should call run_verb with DELETE and no data' do
      @adapter.connection.should_receive(:run_verb).with('delete', nil)
      @book.destroy
    end

    it "should return false if the record does not exist in the repository" do
      @book.should_receive(:new_record?).and_return(true)
      @book.destroy.should eql(false)
    end
  end

  describe 'when getting one resource' do

    describe 'if the resource exists' do

      before do
        book_xml = <<-BOOK
        <?xml version='1.0' encoding='UTF-8'?>
        <book>
          <author>Stephen King</author>
          <created-at type='datetime'>2008-06-08T17:03:07Z</created-at>
          <id type='integer'>1</id>
          <title>The Shining</title>
          <updated-at type='datetime'>2008-06-08T17:03:07Z</updated-at>
        </book>
        BOOK
        @id = 1
        @response = mock(Net::HTTPResponse)
        @response.stub!(:body).and_return(book_xml)
        @adapter.connection.stub!(:http_get).and_return(@response)
      end

      it 'should return the resource' do
        book = Book.get(@id)
        book.should_not be_nil
        book.id.should be_an_instance_of(Fixnum)
        book.id.should == 1
      end

      it "should have its attributes well formed" do
        book = Book.get(@id)
        book.author.should == 'Stephen King'
        book.title.should == 'The Shining'
      end

      it 'should do an HTTP GET' do
        @adapter.connection.should_receive(:http_get).with('books/1').and_return(@response)
        Book.get(@id)
      end

      it "should be equal to itself" do
        Book.get(@id).should == Book.get(@id)
      end

      it "should return its cached version when it was already fetched" do
        book = mock(Book, :kind_of? => Book)
        repo = mock(DataMapper::Repository)
        ident_map = mock(DataMapper::IdentityMap)

        Book.should_receive(:repository).and_return(repo)
        repo.should_receive(:identity_map).and_return(ident_map)
        ident_map.stub!(:get).with([@id]).and_return(book)

        # The remote resource won't be called when a cached object exists
        Book.should_receive(:first).never
        Book.get(@id).should be_a_kind_of(Book)
      end

      it "should call read_one method" do
        @adapter.should_receive(:read_one)
        Book.get(@id)
      end
    end

    describe 'if the resource does not exist' do
      it 'should raise DataMapperRest::ResourceNotFound' do
        @mock_resp = mock("response")
        @mock_http = mock("http")
        Net::HTTP.should_receive(:start).and_yield @mock_http

        @mock_resp.should_receive(:code).and_return 404
        @mock_http.should_receive(:request).and_return @mock_resp

        lambda{ Book.get(5000) }.should raise_error(DataMapperRest::ResourceNotFound)
      end
    end
  end

  describe 'when getting all resource of a particular type' do
    before do
      books_xml = <<-BOOK
      <?xml version='1.0' encoding='UTF-8'?>
      <books type='array'>
        <book>
          <author>Ursula K LeGuin</author>
          <created-at type='datetime'>2008-06-08T17:02:28Z</created-at>
          <id type='integer'>1</id>
          <title>The Dispossed</title>
          <updated-at type='datetime'>2008-06-08T17:02:28Z</updated-at>
        </book>
        <book>
          <author>Stephen King</author>
          <created-at type='datetime'>2008-06-08T17:03:07Z</created-at>
          <id type='integer'>2</id>
          <title>The Shining</title>
          <updated-at type='datetime'>2008-06-08T17:03:07Z</updated-at>
        </book>
      </books>
      BOOK
      @response = mock(Net::HTTPResponse)
      @response.stub!(:body).and_return(books_xml)
    end

    it 'should get a non-empty list' do
      @adapter.connection.stub!(:http_get).and_return(@response)
      Book.all.should_not be_empty
    end

    it 'should receive one Resource for each entity in the XML' do
      @adapter.connection.stub!(:http_get).and_return(@response)
      Book.all.size.should == 2
    end

    it "should call read_many method" do
      @adapter.connection.stub!(:http_get).and_return(@response)
      @adapter.should_receive(:read_many)
      Book.all
    end

    it "should raise NotImplementedError if conditions are specified" do
      # Have to find a way to set an expectation for a method call inside a block
      # Book.all(:title => "NonExistentTitle")
    end
  end

  describe 'when updating an existing resource' do
    before do
      @books_xml = <<-XML
      <book>
        <id type='integer'>42</id>
        <title>Starship Troopers</title>
        <author>Robert Heinlein</author>
        <created-at type='datetime'>2008-06-08T17:02:28Z</created-at>
      </book>
      XML
      repository do |repo|
        @repository = repo
        @book = Book.new(:id => 42,
                         :title => 'Starship Troopers',
                         :author => 'Robert Heinlein',
                         :created_at => DateTime.parse('2008-06-08T17:02:28Z'))
        @book.instance_eval { @new_record = false }
        @repository.identity_map(Book)[@book.key] = @book
        @book.title = "Mary Had a Little Lamb"
      end
    end

    it 'should do an HTTP PUT' do
      @adapter.connection.should_receive(:http_put).with('books/42', @book.to_xml)
      @repository.scope do
        @book.save
      end
    end

    it "should not do an HTTP PUT for non-dirty resources" do
      @book.should_receive(:dirty_attributes).and_return({})
      @adapter.connection.should_receive(:http_put).never
      @repository.scope do
        @book.save
      end
    end
  end
end
