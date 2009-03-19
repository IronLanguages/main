require File.expand_path(File.join(File.dirname(__FILE__), "..", "..", "spec_helper"))

describe DataMapper::Associations::ManyToMany::Proxy do
  before :all do
    class ::Editor
      include DataMapper::Resource

      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :name, String

      has n, :books, :through => Resource
    end

    Object.send(:remove_const, :Book) if defined?(Book)

    class ::Book
      include DataMapper::Resource

      def self.default_repository_name; ADAPTER end

      property :id, Serial
      property :title, String

      has n, :editors, :through => Resource
    end
  end

  before do
    [ Book, Editor, BookEditor ].each { |k| k.auto_migrate! }

    repository(ADAPTER) do
      book_1 = Book.create(:title => "Dubliners")
      book_2 = Book.create(:title => "Portrait of the Artist as a Young Man")
      book_3 = Book.create(:title => "Ulysses")

      editor_1 = Editor.create(:name => "Jon Doe")
      editor_2 = Editor.create(:name => "Jane Doe")

      BookEditor.create(:book => book_1, :editor => editor_1)
      BookEditor.create(:book => book_2, :editor => editor_1)
      BookEditor.create(:book => book_1, :editor => editor_2)

      @parent      = book_3
      @association = @parent.editors
      @other       = [ editor_1 ]
    end
  end

  it "should provide #replace" do
    @association.should respond_to(:replace)
  end

  describe "#replace" do
    it "should remove the resource from the collection" do
      @association.should have(0).entries
      @association.replace(@other)
      @association.should == @other
    end

    it "should not automatically save that the resource was removed from the association" do
      @association.replace(@other)
      @parent.reload.should have(0).editors
    end

    it "should return the association" do
      @association.replace(@other).object_id.should == @association.object_id
    end

    it "should add the new resources so they will be saved when saving the parent" do
      @association.replace(@other)
      @association.should == @other
      @parent.save
      @association.reload.should == @other
    end

    it "should instantiate the remote model if passed an array of hashes" do
      @association.replace([ { :name => "Jim Smith" } ])
      other = [ Editor.first(:name => "Jim Smith") ]
      other.first.should_not be_nil
      @association.should == other
      @parent.save
      @association.reload.should == other
    end
  end

  it "should correctly link records" do
    Book.get(1).should have(2).editors
    Book.get(2).should have(1).editors
    Book.get(3).should have(0).editors
    Editor.get(1).should have(2).books
    Editor.get(2).should have(1).books
  end

  it "should be able to have associated objects manually added" do
    book = Book.get(3)
    book.should have(0).editors

    be = BookEditor.new(:book_id => book.id, :editor_id => 2)
    book.book_editors << be
    book.save

    book.reload.should have(1).editors
  end

  it "should automatically added necessary through class" do
    book = Book.get(3)
    book.should have(0).editors

    book.editors << Editor.get(1)
    book.editors << Editor.new(:name => "Jimmy John")
    book.save

    book.reload.should have(2).editors
  end

  it "should react correctly to a new record" do
    book = Book.new(:title => "Finnegan's Wake")
    editor = Editor.get(2)
    book.should have(0).editors
    editor.should have(1).books

    book.editors << editor
    book.save

    book.reload.should have(1).editors
    editor.reload.should have(2).books
  end

  it "should be able to delete intermediate model" do
    book = Book.get(1)
    book.should have(2).book_editors
    book.should have(2).editors

    be = BookEditor.get(1,1)
    book.book_editors.delete(be)
    book.save

    book.reload
    book.should have(1).book_editors
    book.should have(1).editors
  end

  it "should be clearable" do
    repository(ADAPTER) do
      book = Book.get(2)
      book.should have(1).book_editors
      book.should have(1).editors

      book.editors.clear
      book.save

      book.reload
      book.should have(0).book_editors
      book.should have(0).editors
    end
    repository(ADAPTER) do
      Book.get(2).should have(0).editors
    end
  end

  it "should be able to delete one object" do
    book = Book.get(1)
    book.should have(2).book_editors
    book.should have(2).editors

    editor = book.editors.first
    book.editors.delete(editor)
    book.save

    book.reload
    book.should have(1).book_editors
    book.should have(1).editors
    editor.reload.books.should_not include(book)
  end

  it "should be destroyable" do
    pending "cannot destroy a collection yet" do
      book = Book.get(2)
      book.should have(1).editors

      book.editors.destroy
      book.save

      book.reload
      book.should have(0).editors
    end
  end

  describe "with natural keys" do
    before :all do
      class ::Author
        include DataMapper::Resource

        def self.default_repository_name; ADAPTER end

        property :name, String, :key => true

        has n, :books, :through => Resource
      end

      class ::Book
        has n, :authors, :through => Resource
      end
    end

    before do
      [ Author, AuthorBook ].each { |k| k.auto_migrate! }

      @author = Author.create(:name =>  "James Joyce")

      @book_1 = Book.get!(1)
      @book_2 = Book.get!(2)
      @book_3 = Book.get!(3)

      AuthorBook.create(:book => @book_1, :author => @author)
      AuthorBook.create(:book => @book_2, :author => @author)
      AuthorBook.create(:book => @book_3, :author => @author)
    end

    it "should have a join resource where the natural key is a property" do
      AuthorBook.properties[:author_name].primitive.should == String
    end

    it "should have a join resource where every property is part of the key" do
      AuthorBook.key.should == AuthorBook.properties.to_a
    end

    it "should correctly link records" do
      @author.should have(3).books
      @book_1.should have(1).authors
      @book_2.should have(1).authors
      @book_3.should have(1).authors
    end
  end

  describe "When join model has non-serial (integer) natural keys." do
    before :all do
      class ::Tag
        include DataMapper::Resource

        def self.default_repository_name; ADAPTER end

        property :id,    Serial
        property :name,  String, :size => 128

        has n, :book_taggings
        has n, :books, :through => :book_taggings
      end

      class ::BookTagging
        include DataMapper::Resource

        def self.default_repository_name; ADAPTER end

        property :book_id, Integer, :key => true
        property :tag_id, Integer, :key => true

        belongs_to :book
        belongs_to :tag
      end

      class ::Book
        has n, :book_taggings
        has n, :tags, :through => :book_taggings
      end
    end

    before do
      [ Tag, BookTagging ].each { |k| k.auto_migrate! }

      @tag_1 = Tag.create(:name =>  "good")
      @tag_2 = Tag.create(:name =>  "long")

      @book_1 = Book.get!(1)
      @book_2 = Book.get!(2)
      @book_3 = Book.get!(3)

      BookTagging.create(:book => @book_2, :tag => @tag_1)
      BookTagging.create(:book => @book_2, :tag => @tag_2)
      BookTagging.create(:book => @book_3, :tag => @tag_2)
    end

    it "should fetch all tags for a book" do
      @book_1.tags.should have(0).tags
      @book_2.tags.should have(2).tags
      @book_3.tags.should have(1).tags
    end

    it "should allow for adding an association using the << operator" do
      @book_1.book_taggings << @tag_1
      @book_1.tags.should have(0).tags
    end
  end

  describe "with renamed associations" do
    before :all do
      class ::Singer
        include DataMapper::Resource

        def self.default_repository_name; ADAPTER end

        property :id, Serial
        property :name, String

        has n, :tunes, :through => Resource, :class_name => 'Song'
      end

      class ::Song
        include DataMapper::Resource

        def self.default_repository_name; ADAPTER end

        property :id, Serial
        property :title, String

        has n, :performers, :through => Resource, :class_name => 'Singer'
      end
    end

    before do
      [ Singer, Song, SingerSong ].each { |k| k.auto_migrate! }

      song_1 = Song.create(:title => "Dubliners")
      song_2 = Song.create(:title => "Portrait of the Artist as a Young Man")
      song_3 = Song.create(:title => "Ulysses")

      singer_1 = Singer.create(:name => "Jon Doe")
      singer_2 = Singer.create(:name => "Jane Doe")

      SingerSong.create(:song => song_1, :singer => singer_1)
      SingerSong.create(:song => song_2, :singer => singer_1)
      SingerSong.create(:song => song_1, :singer => singer_2)

      @parent      = song_3
      @association = @parent.performers
      @other       = [ singer_1 ]
    end

    it "should provide #replace" do
      @association.should respond_to(:replace)
    end

    it "should correctly link records" do
      Song.get(1).should have(2).performers
      Song.get(2).should have(1).performers
      Song.get(3).should have(0).performers
      Singer.get(1).should have(2).tunes
      Singer.get(2).should have(1).tunes
    end

    it "should be able to have associated objects manually added" do
      song = Song.get(3)
      song.should have(0).performers

      be = SingerSong.new(:song_id => song.id, :singer_id => 2)
      song.singer_songs << be
      song.save

      song.reload.should have(1).performers
    end

    it "should automatically added necessary through class" do
      song = Song.get(3)
      song.should have(0).performers

      song.performers << Singer.get(1)
      song.performers << Singer.new(:name => "Jimmy John")
      song.save

      song.reload.should have(2).performers
    end

    it "should react correctly to a new record" do
      song = Song.new(:title => "Finnegan's Wake")
      singer = Singer.get(2)
      song.should have(0).performers
      singer.should have(1).tunes

      song.performers << singer
      song.save

      song.reload.should have(1).performers
      singer.reload.should have(2).tunes
    end

    it "should be able to delete intermediate model" do
      song = Song.get(1)
      song.should have(2).singer_songs
      song.should have(2).performers

      be = SingerSong.get(1,1)
      song.singer_songs.delete(be)
      song.save

      song.reload
      song.should have(1).singer_songs
      song.should have(1).performers
    end

    it "should be clearable" do
      repository(ADAPTER) do
        song = Song.get(2)
        song.should have(1).singer_songs
        song.should have(1).performers

        song.performers.clear
        song.save

        song.reload
        song.should have(0).singer_songs
        song.should have(0).performers
      end
      repository(ADAPTER) do
        Song.get(2).should have(0).performers
      end
    end

    it "should be able to delete one object" do
      song = Song.get(1)
      song.should have(2).singer_songs
      song.should have(2).performers

      editor = song.performers.first
      song.performers.delete(editor)
      song.save

      song.reload
      song.should have(1).singer_songs
      song.should have(1).performers
      editor.reload.tunes.should_not include(song)
    end

    it "should be destroyable" do
      pending "cannot destroy a collection yet" do
        song = Song.get(2)
        song.should have(1).performers

        song.performers.destroy
        song.save

        song.reload
        song.should have(0).performers
      end
    end
  end

end
