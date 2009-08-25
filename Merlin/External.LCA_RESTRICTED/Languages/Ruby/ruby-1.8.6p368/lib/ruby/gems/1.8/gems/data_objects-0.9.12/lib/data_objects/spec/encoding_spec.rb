share_examples_for 'a driver supporting encodings' do

  before :each do
    @connection = DataObjects::Connection.new(CONFIG.uri)
  end

  after :each do
    @connection.close
  end


  it { @connection.should respond_to(:character_set) }

  describe 'character_set' do

    it 'uses utf8 by default' do
      @connection.character_set.should == 'utf8'
    end

    describe 'sets the character set through the URI' do
      before do
        @latin1_connection = DataObjects::Connection.new("#{CONFIG.scheme}://#{CONFIG.user}:#{CONFIG.pass}@#{CONFIG.host}:#{CONFIG.port}#{CONFIG.database}?encoding=latin1")
      end

      after { @latin1_connection.close }

      it { @latin1_connection.character_set.should == 'latin1' }
    end

  end
end
