share_examples_for 'a Reader' do

  include DataObjectsSpecHelpers

  before :all do
    setup_test_environment
  end

  before :each do
    @connection = DataObjects::Connection.new(CONFIG.uri)
    @reader     = @connection.create_command("SELECT code, name FROM widgets WHERE ad_description = ? order by id").execute_reader('Buy this product now!')
  end

  after :each do
    @reader.close
    @connection.close
  end

  it { @reader.should respond_to(:fields) }

  describe 'fields' do

    it 'should return the correct fields in the reader' do
      # we downcase the field names as some drivers such as do_derby, do_h2,
      # do_hsqldb return the field names as uppercase
      @reader.fields.map{ |f| f.downcase }.should == ['code', 'name']
    end

  end

  it { @reader.should respond_to(:values) }

  describe 'values' do

    describe 'when the reader is uninitialized' do

      it 'should raise an error' do
        lambda { @reader.values }.should raise_error
      end

    end

    describe 'when the reader is moved to the first result' do

      before  do
        @reader.next!
      end

      it 'should return the correct first set of in the reader' do
        @reader.values.should == ["W0000001", "Widget 1"]
      end

    end

    describe 'when the reader is moved to the second result' do

      before  do
        @reader.next!; @reader.next!
      end

      it 'should return the correct first set of in the reader' do
        @reader.values.should == ["W0000002", "Widget 2"]
      end

    end

    describe 'when the reader is moved to the end' do

      before do
        while @reader.next! ; end
      end

      it 'should raise an error again' do
        lambda { @reader.values }.should raise_error
      end
    end

  end

  it { @reader.should respond_to(:close) }

  describe 'close' do

    describe 'on an open reader' do

      it 'should return true' do
        @reader.close.should be_true
      end

    end

    describe 'on an already closed reader' do

      before do
        @reader.close
      end

      it 'should return false' do
        @reader.close.should be_false
      end

    end

  end

  it { @reader.should respond_to(:next!) }

  describe 'next!' do

    describe 'successfully moving the cursor initially' do

      it 'should return true' do
        @reader.next!.should be_true
      end

    end

    describe 'moving the cursor' do

      before do
        @reader.next!
      end

      it 'should move the cursor to the next value' do
        lambda { @reader.next! }.should change(@reader, :values).
                                          from(["W0000001", "Widget 1"]).
                                            to(["W0000002", "Widget 2"])
      end

    end

    describe 'arriving at the end of the reader' do

      before do
        while @reader.next!; end
      end

      it 'should return false when the end is reached' do
        @reader.next!.should be_false
      end

    end

  end

  it { @reader.should respond_to(:field_count) }

  describe 'field_count' do

    it 'should count the number of fields' do
      @reader.field_count.should == 2
    end

  end

end
