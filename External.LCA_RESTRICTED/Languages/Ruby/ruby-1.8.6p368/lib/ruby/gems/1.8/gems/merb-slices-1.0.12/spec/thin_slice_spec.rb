require File.dirname(__FILE__) + '/spec_helper'

slices_path = File.dirname(__FILE__) / 'slices'

describe "A thin slice" do
    
  before(:all) do
    self.current_slice_root = slices_path / 'thin-test-slice'
    
    # Uncomment this re-generate the slice
    
    # FileUtils.rm_rf(slices_path / 'thin-test-slice') rescue nil
    # generator = Merb::Generators::ThinSliceGenerator.new(slices_path, {}, 'thin-test-slice')
    # generator.invoke!
    
    # Add the slice to the search path
    Merb::Plugins.config[:merb_slices][:auto_register] = true
    Merb::Plugins.config[:merb_slices][:search_path]   = slices_path
    
    Merb.start(
      :testing => true, 
      :adapter => 'runner', 
      :environment => ENV['MERB_ENV'] || 'test',
      :merb_root => Merb.root
    )
  end
  
  # Uncomment this re-generate the slice
  
  # after(:all) do
  #   FileUtils.rm_rf(slices_path / 'thin-test-slice') rescue nil
  # end
  
  before :all do
    Merb::Router.prepare { add_slice(:thin_test_slice) }
  end
  
  after :all do
    Merb::Router.reset!
  end
  
  it "should be registered in Merb::Slices.slices" do
    Merb::Slices.slices.should include(ThinTestSlice)
  end
  
  it "should be registered in Merb::Slices.paths" do
    Merb::Slices.paths[ThinTestSlice.name].should == current_slice_root
  end
  
  it "should have an :identifier property" do
    ThinTestSlice.identifier.should == "thin-test-slice"
  end
  
  it "should have an :identifier_sym property" do
    ThinTestSlice.identifier_sym.should == :thin_test_slice
  end
  
  it "should have a :root property" do
    ThinTestSlice.root.should == Merb::Slices.paths[ThinTestSlice.name]
    ThinTestSlice.root_path('app').should == current_slice_root / 'app'
  end
  
  it "should have a :file property" do
    ThinTestSlice.file.should == current_slice_root / 'lib' / 'thin-test-slice.rb'
  end
  
  it "should have metadata properties" do
    ThinTestSlice.description.should == "ThinTestSlice is a thin Merb slice!"
    ThinTestSlice.version.should == "0.0.1"
    ThinTestSlice.author.should == "Engine Yard"
  end
  
  it "should have a :named_routes property" do
    ThinTestSlice.named_routes[:default].should be_kind_of(Merb::Router::Route)
  end
  
  it "should have a :layout config option set" do
    ThinTestSlice.config[:layout].should == :thin_test_slice
  end
  
  it "should have a dir_for method" do
    app_path = ThinTestSlice.dir_for(:application)
    app_path.should == current_slice_root
    [:view].each do |type|
      ThinTestSlice.dir_for(type).should == app_path / "#{type}s"
    end
    public_path = ThinTestSlice.dir_for(:public)
    public_path.should == current_slice_root / 'public'
    [:stylesheet, :javascript, :image].each do |type|
      ThinTestSlice.dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a app_dir_for method" do
    root_path = ThinTestSlice.app_dir_for(:root)
    root_path.should == Merb.root / 'slices' / 'thin-test-slice'
    app_path = ThinTestSlice.app_dir_for(:application)
    app_path.should == root_path 
    [:view].each do |type|
      ThinTestSlice.app_dir_for(type).should == app_path / "#{type}s"
    end
    public_path = ThinTestSlice.app_dir_for(:public)
    public_path.should == Merb.dir_for(:public) / 'slices' / 'thin-test-slice'
    [:stylesheet, :javascript, :image].each do |type|
      ThinTestSlice.app_dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a public_dir_for method" do
    public_path = ThinTestSlice.public_dir_for(:public)
    public_path.should == '/slices' / 'thin-test-slice'
    [:stylesheet, :javascript, :image].each do |type|
      ThinTestSlice.public_dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a public_path_for method" do
    public_path = ThinTestSlice.public_dir_for(:public)
    ThinTestSlice.public_path_for("path", "to", "file").should == public_path / "path" / "to" / "file"
    [:stylesheet, :javascript, :image].each do |type|
      ThinTestSlice.public_path_for(type, "path", "to", "file").should == public_path / "#{type}s" / "path" / "to" / "file"
    end
  end
  
  it "should have a app_path_for method" do
    ThinTestSlice.app_path_for("path", "to", "file").should == ThinTestSlice.app_dir_for(:root) / "path" / "to" / "file"
    ThinTestSlice.app_path_for(:controller, "path", "to", "file").should == ThinTestSlice.app_dir_for(:controller) / "path" / "to" / "file"
  end
  
  it "should have a slice_path_for method" do
    ThinTestSlice.slice_path_for("path", "to", "file").should == ThinTestSlice.dir_for(:root) / "path" / "to" / "file"
    ThinTestSlice.slice_path_for(:controller, "path", "to", "file").should == ThinTestSlice.dir_for(:controller) / "path" / "to" / "file"
  end
  
  it "should keep a list of path component types to use when copying files" do
    (ThinTestSlice.mirrored_components & ThinTestSlice.slice_paths.keys).length.should == ThinTestSlice.mirrored_components.length
  end
    
end