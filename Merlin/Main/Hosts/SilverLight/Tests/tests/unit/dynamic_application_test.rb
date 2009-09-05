describe 'DynamicApplication' do
  it 'can only have one instance' do
    app = DynamicApplication.current
    app.should.not.equal nil
    app.should.be.same_as DynamicApplication.current
    
    # TODO try to make another instance
  end
end

describe 'Parsing init parameters' do
  # TODO test parsing init params

  # TODO test that errors are always turned on while parsing init params

  it 'should have a nil entry point' do
    DynamicApplication.current.entry_point.should.equal nil 
  end

  it 'should not have a debug flag' do
    DynamicApplication.current.debug.should.be.false
  end

  it 'should have an init params collection' do
    DynamicApplication.current.init_params.should.not.be.nil
    DynamicApplication.current.init_params.kind_of?(System::Collections::Generic::Dictionary.of(System::String, System::String))
  end

  it 'should know the current DynamicApplication' do
    DynamicApplication.current.should.not.be.nil
  end

  it 'should have a report unhandled errors flag' do
    DynamicApplication.current.report_unhandled_errors.should.be.true
    # TODO check that the unhandled exception event is handled
  end

  it 'should know where errors are reported to' do
    DynamicApplication.current.ErrorTargetID.should.equal 'errorLocation'.to_clr_string
  end
end

describe 'Hosting API' do
  it 'should create a ScriptRuntimeSetup' do
    setup = DynamicApplication.CreateRuntimeSetup
    setup.host_type.class.superclass.to_s.should.equal 'System::Type'
    setup.host_type.name.should.equal 'BrowserScriptHost'.to_clr_string
  end
  
  it 'should have a runtime property' do  
    DynamicApplication.current.runtime.class.should.equal Microsoft::Scripting::Hosting::ScriptRuntime
  end
  
  it 'should set the search path to empty' do
    options = {}
    DynamicApplication.current.runtime.setup.options.each do |kvp|
      options[kvp.key.to_s] = kvp.value.class.to_s == "System::String[]" ? kvp.value.collect{ |i| i.to_s } : kvp.value
    end
    options["SearchPaths"].kind_of?(Array).should.be.true
    options["SearchPaths"].include?("").should.be.true   
  end
  
  it 'should set the debug mode' do
    DynamicApplication.current.runtime.setup.debug_mode.should.be.false
  end
  
  it 'should load Silverlight platform DLLs' do
    begin
      [
        Microsoft::Scripting::Silverlight,
        System::Collections::Generic::Dictionary, # mscorlib
        System,
        System::Windows,
        System::Windows::Browser,
        System::Net,
      ].each {|t| t.kind_of?(Module).should.be.true}
    rescue NameError => e
      should.flunk e.message
    end
  end
end

describe 'Utility methods' do
  it 'should make a relative Uri' do
    uri = "unit/assets/foo.xaml"
    DynamicApplication.current.make_uri(uri).should.equal System::Uri.new(uri, System::UriKind.relative)
  end
end

describe 'XAML support' do
  def options
    @options ||= {
      :object => System::Windows::Controls::UserControl.new,
      :xamlfile => "unit/assets/foo.xaml"
    }
  end

  def reset_options
    @options = nil
  end

  def check_xaml_support(obj = nil)
    obj ||= DynamicApplication.current.root_visual
    # Rough test that the xaml file was loaded
    msg = obj.find_name('message')
    msg.text.should.equal 'Foo.xaml'.to_clr_string
  end

  it 'should load a XAML file, represented by a string, and set it as the visual root' do
    reset_options
    DynamicApplication.current.load_root_visual(options[:object], options[:xamlfile])
    DynamicApplication.current.root_visual.should.equal options[:object]
    check_xaml_support
  end

  it 'should load a xaml file, represented by a Uri, and set it as the visual root' do
    reset_options
    DynamicApplication.current.load_root_visual(options[:object], System::Uri.new(options[:xamlfile], System::UriKind.relative))
    DynamicApplication.current.root_visual.should.equal options[:object]
    check_xaml_support
  end

  it 'should load a xaml file into a object' do
    reset_options
    DynamicApplication.current.load_component(options[:object], options[:xamlfile])
    check_xaml_support(options[:object])
  end
end
