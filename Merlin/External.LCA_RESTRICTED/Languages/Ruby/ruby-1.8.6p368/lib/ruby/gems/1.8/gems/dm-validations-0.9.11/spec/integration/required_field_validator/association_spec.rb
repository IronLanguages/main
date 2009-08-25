require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  class Artist
    #
    # Behaviors
    #

    include DataMapper::Resource

    #
    # Properties
    #

    property :id,   Integer, :serial => true
    property :name, String,  :auto_validation => false

    #
    # Associations
    #

    has n, :albums

    #
    # Validations
    #

    validates_present :name
  end

  class Album
    #
    # Behaviors
    #

    include DataMapper::Resource

    #
    # Properties
    #

    property :id,        Integer, :serial => true
    property :name,      String,  :auto_validation => false
    property :artist_id, Integer, :index => :artist

    #
    # Associations
    #

    belongs_to :artist

    #
    # Validations
    #

    validates_present :name, :artist
  end
  Artist.auto_migrate!
  Album.auto_migrate!



  describe Album do
    before :each do
      @artist = Artist.create(:name => "Oceanlab")
      @album  = @artist.albums.new(:name => "Sirens of the sea")
    end

    describe 'with a missing artist' do
      before :each do
        @album.artist = nil
      end

      it 'is not valid' do
        @album.should_not be_valid
      end

      it 'has a meaninful error messages on association key property' do
        @album.valid?
        @album.errors.on(:artist).should include("Artist must not be blank")
      end
    end

    describe 'with specified artist and name' do
      before :each do
        # no op
      end

      it 'is valid' do
        @album.should be_valid
      end
    end
  end
end
