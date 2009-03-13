require File.dirname(__FILE__) + '/spec_helper'

slices_path = File.dirname(__FILE__) / 'slices'

describe "A very thin slice" do
    
  before(:all) do
    self.current_slice_root = slices_path / 'very-thin-test-slice'
    
    # Uncomment this re-generate the slice
    
    # FileUtils.rm_rf(slices_path / 'very-thin-test-slice') rescue nil
    # generator = Merb::Generators::VeryThinSliceGenerator.new(slices_path, {}, 'very-thin-test-slice')
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
  #   FileUtils.rm_rf(slices_path / 'very-thin-test-slice') rescue nil
  # end
  
  before :all do
    Merb::Router.prepare { add_slice(:VeryThinTestSlice) }
  end
  
  after :all do
    Merb::Router.reset!
  end
  
  it "should be registered in Merb::Slices.slices" do
    Merb::Slices.slices.should include(VeryThinTestSlice)
  end
  
  it "should be registered in Merb::Slices.paths" do
    Merb::Slices.paths[VeryThinTestSlice.name].should == current_slice_root
  end
  
  it "should have an :identifier property" do
    VeryThinTestSlice.identifier.should == "very-thin-test-slice"
  end
  
  it "should have an :identifier_sym property" do
    VeryThinTestSlice.identifier_sym.should == :very_thin_test_slice
  end
  
  it "should have a :root property" do
    VeryThinTestSlice.root.should == Merb::Slices.paths[VeryThinTestSlice.name]
    VeryThinTestSlice.root_path('app').should == current_slice_root / 'app'
  end
  
  it "should have a :file property" do
    VeryThinTestSlice.file.should == current_slice_root / 'lib' / 'very-thin-test-slice.rb'
  end
  
  it "should have metadata properties" do
    VeryThinTestSlice.description.should == "VeryThinTestSlice is a very thin Merb slice!"
    VeryThinTestSlice.version.should == "0.0.1"
    VeryThinTestSlice.author.should == "Engine Yard"
  end
  
  it "should have a :named_routes property" do
    VeryThinTestSlice.named_routes[:default].should be_kind_of(Merb::Router::Route)
  end
  
  it "should have a dir_for method" do
    app_path = VeryThinTestSlice.dir_for(:application)
    app_path.should == current_slice_root
  end
  
  it "should have a app_dir_for method" do
    root_path = VeryThinTestSlice.app_dir_for(:root)
    root_path.should == Merb.root / 'slices' / 'very-thin-test-slice'
    app_path = VeryThinTestSlice.app_dir_for(:application)
    app_path.should == root_path
  end
  
  it "should have a public_dir_for method" do
    public_path = VeryThinTestSlice.public_dir_for(:public)
    public_path.should == '/slices' / 'very-thin-test-slice'
    [:stylesheet, :javascript, :image].each do |type|
      VeryThinTestSlice.public_dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a public_path_for method" do
    public_path = VeryThinTestSlice.public_dir_for(:public)
    VeryThinTestSlice.public_path_for("path", "to", "file").should == public_path / "path" / "to" / "file"
    [:stylesheet, :javascript, :image].each do |type|
      VeryThinTestSlice.public_path_for(type, "path", "to", "file").should == public_path / "#{type}s" / "path" / "to" / "file"
    end
  end
  
  it "should have a app_path_for method" do
    VeryThinTestSlice.app_path_for("path", "to", "file").should == VeryThinTestSlice.app_dir_for(:root) / "path" / "to" / "file"
    VeryThinTestSlice.app_path_for(:controller, "path", "to", "file").should == VeryThinTestSlice.app_dir_for(:controller) / "path" / "to" / "file"
  end
  
  it "should have a slice_path_for method" do
    VeryThinTestSlice.slice_path_for("path", "to", "file").should == VeryThinTestSlice.dir_for(:root) / "path" / "to" / "file"
    VeryThinTestSlice.slice_path_for(:controller, "path", "to", "file").should == VeryThinTestSlice.dir_for(:controller) / "path" / "to" / "file"
  end
  
  it "should keep a list of path component types to use when copying files" do
    (VeryThinTestSlice.mirrored_components & VeryThinTestSlice.slice_paths.keys).length.should == VeryThinTestSlice.mirrored_components.length
  end
    
end