require File.dirname(__FILE__) + '/spec_helper'

slices_path = File.dirname(__FILE__) / 'slices'

describe "A slice" do
    
  before(:all) do
    self.current_slice_root = slices_path / 'full-test-slice'
    
    # Uncomment this re-generate the slice - but remove the specs dir!
    
    # FileUtils.rm_rf(slices_path / 'full-test-slice') rescue nil
    # generator = Merb::Generators::FullSliceGenerator.new(slices_path, {}, 'full-test-slice')
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
  #   FileUtils.rm_rf(slices_path / 'full-test-slice') rescue nil
  # end
  
  before :all do
    Merb::Router.prepare { add_slice(:FullTestSlice) }
  end
  
  it "should be registered in Merb::Slices.slices" do
    Merb::Slices.slices.should include(FullTestSlice)
  end
  
  it "should be registered in Merb::Slices.paths" do
    Merb::Slices.paths[FullTestSlice.name].should == current_slice_root
  end
  
  it "should have an :identifier property" do
    FullTestSlice.identifier.should == "full-test-slice"
  end
  
  it "should have an :identifier_sym property" do
    FullTestSlice.identifier_sym.should == :full_test_slice
  end
  
  it "should have a :root property" do
    FullTestSlice.root.should == Merb::Slices.paths[FullTestSlice.name]
    FullTestSlice.root_path('app').should == current_slice_root / 'app'
  end
  
  it "should have a :file property" do
    FullTestSlice.file.should == current_slice_root / 'lib' / 'full-test-slice.rb'
  end
  
  it "should have metadata properties" do
    FullTestSlice.description.should == "FullTestSlice is a chunky Merb slice!"
    FullTestSlice.version.should == "0.0.1"
    FullTestSlice.author.should == "Engine Yard"
  end
  
  it "should have a :named_routes property" do
    FullTestSlice.named_routes[:default].should be_kind_of(Merb::Router::Route)
    FullTestSlice.named_routes[:index].should be_kind_of(Merb::Router::Route)
  end
  
  it "should have a config property (Hash)" do
    FullTestSlice.config.should be_kind_of(Hash)
  end
  
  it "should have bracket accessors as shortcuts to the config" do
    FullTestSlice[:foo] = 'bar'
    FullTestSlice[:foo].should == 'bar'
    FullTestSlice[:foo].should == FullTestSlice.config[:foo]
  end
  
  it "should have a :layout config option set" do
    FullTestSlice.config[:layout].should == :full_test_slice
  end
  
  it "should have a dir_for method" do
    app_path = FullTestSlice.dir_for(:application)
    app_path.should == current_slice_root / 'app'
    [:view, :model, :controller, :helper, :mailer, :part].each do |type|
      FullTestSlice.dir_for(type).should == app_path / "#{type}s"
    end
    public_path = FullTestSlice.dir_for(:public)
    public_path.should == current_slice_root / 'public'
    [:stylesheet, :javascript, :image].each do |type|
      FullTestSlice.dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a app_dir_for method" do
    root_path = FullTestSlice.app_dir_for(:root)
    root_path.should == Merb.root / 'slices' / 'full-test-slice'
    app_path = FullTestSlice.app_dir_for(:application)
    app_path.should == root_path / 'app'
    [:view, :model, :controller, :helper, :mailer, :part].each do |type|
      FullTestSlice.app_dir_for(type).should == app_path / "#{type}s"
    end
    public_path = FullTestSlice.app_dir_for(:public)
    public_path.should == Merb.dir_for(:public) / 'slices' / 'full-test-slice'
    [:stylesheet, :javascript, :image].each do |type|
      FullTestSlice.app_dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a public_dir_for method" do
    public_path = FullTestSlice.public_dir_for(:public)
    public_path.should == '/slices' / 'full-test-slice'
    [:stylesheet, :javascript, :image].each do |type|
      FullTestSlice.public_dir_for(type).should == public_path / "#{type}s"
    end
  end
  
  it "should have a public_path_for method" do
    public_path = FullTestSlice.public_dir_for(:public)
    FullTestSlice.public_path_for("path", "to", "file").should == public_path / "path" / "to" / "file"
    [:stylesheet, :javascript, :image].each do |type|
      FullTestSlice.public_path_for(type, "path", "to", "file").should == public_path / "#{type}s" / "path" / "to" / "file"
    end
  end
  
  it "should have a app_path_for method" do
    FullTestSlice.app_path_for("path", "to", "file").should == FullTestSlice.app_dir_for(:root) / "path" / "to" / "file"
    FullTestSlice.app_path_for(:controller, "path", "to", "file").should == FullTestSlice.app_dir_for(:controller) / "path" / "to" / "file"
  end
  
  it "should have a slice_path_for method" do
    FullTestSlice.slice_path_for("path", "to", "file").should == FullTestSlice.dir_for(:root) / "path" / "to" / "file"
    FullTestSlice.slice_path_for(:controller, "path", "to", "file").should == FullTestSlice.dir_for(:controller) / "path" / "to" / "file"
  end
  
  it "should keep a list of path component types to use when copying files" do
    (FullTestSlice.mirrored_components & FullTestSlice.slice_paths.keys).length.should == FullTestSlice.mirrored_components.length
  end
  
  it "should have a slice_url helper method for slice-specific routes" do
    controller = dispatch_to(FullTestSlice::Main, 'index')
    
    url = controller.url(:full_test_slice_default, :controller => 'main', :action => 'show', :format => 'html')
    url.should == "/full-test-slice/main/show.html"
    controller.slice_url(:controller => 'main', :action => 'show', :format => 'html').should == url
    
    url = controller.url(:full_test_slice_index, :format => 'html')
    url.should == "/full-test-slice/index.html"
    controller.slice_url(:index, :format => 'html').should == url
    
    url = controller.url(:full_test_slice_home)
    url.should == "/full-test-slice/"
    controller.slice_url(:home).should == url
  end
    
end