include Helpers
load_assembly 'Microsoft.Dynamic'

shared 'Create HTML element' do
  before do
    @div ||= tag('div', :id => 'foo')
    HtmlPage.document.body.append_child @div
  end

  after do
    HtmlPage.document.body.remove_child @div
    @div = nil
  end
end

shared 'Create XAML element' do
  before do
    @root = System::Windows::Controls::UserControl.new
    DynamicApplication.load_component @root, "#{File.dirname(__FILE__)}/assets/foo.xaml"
  end

  after do
    @root = nil
  end
end

describe "Ruby extensions" do
  it 'should define document on Kernel' do
    lambda { Kernel.method(:document) }.should.not.raise
  end

  it 'should define window on Kernel' do
    lambda { Kernel.method(:window) }.should.not.raise
  end

  it 'allows a string to be blank' do
    "".blank?.should.be.true
    "1".blank?.should.not.be.true
  end

  it 'allows nil to be blank' do
    nil.blank?.should.be.true
  end
end

describe "Ruby extensions for System::Windows::Browser::HtmlDocument" do
  behaves_like 'Create HTML element'

  it 'should get an element by id on method missing' do
    document.foo.should == HtmlPage.document.get_element_by_id("foo")
  end

  it 'should return nil when an element does not exist' do
    document.bar.should.be.nil
  end

  it 'should return a list of tags' do
    document.tags('div').class.should == ScriptObjectCollection
  end
end

describe "Ruby extensions for System::Windows::Browser::HtmlElement" do
  behaves_like 'Create HTML element'

  before do
    @div.set_attribute('align', 'center')
  end

  it 'can get properties with method_missing' do
    document.foo.innerHTML.should == ''
  end

  it 'can set properties with method_missing' do
    document.foo.innerHTML = 'Hi!'
    document.foo.innerHTML.should == 'Hi!'
  end

  it 'can get attributes with indexers' do
    document.foo['align'].should == 'center'
  end

  it 'can set attributes with indexers' do
    document.foo['align'] = 'right'
    document.foo['align'].should == 'right'
  end

  it 'accesses a special "style" property' do
    document.foo.style.class.should == HtmlStyle
    document.foo.style.instance_variable_get(:"@element").should == @div
  end
end

describe "Ruby extensions for System::Windows::Browser::HtmlStyle" do
  behaves_like 'Create HTML element'

  before do
    @div.set_style_attribute('color', 'red')
    @style = HtmlStyle.new(@div)
  end

  it 'can get a style attribute with method_missing' do
    @style.color.should == 'red'
  end

  it 'can set a style attribute with method_missing' do
    @style.color = 'blue'
    HtmlStyle.new(@div).color.should == 'blue'
  end

  it 'can get a style attribute with indexers' do
    @style[:color].should == 'red'
  end

  it 'can set a style attribute with indexers' do
    @style[:color] = 'blue'
    HtmlStyle.new(@div)[:color].should == 'blue'
  end
end

describe 'Ruby extensions for System::Windows::Browser::ScriptObjectCollection' do
  behaves_like 'Create HTML element'

  it 'behaves like an Enumerable' do
    # Is there a better way to test this?
    document.foo.children.respond_to?(:map).should.be.true
    document.foo.children.respond_to?(:map).should.be.true
  end

  it 'has a size' do
    document.body.children.size.should == document.body.children.count
  end

  it 'has a first element' do
    document.foo.children.first.should.be.nil
    document.body.children.first.should == document.body.children[0]
  end

  it 'has a last element' do
    document.foo.children.last.should.be.nil
    document.body.children.last.should == document.body.children[document.body.children.count - 1]
  end

  it 'can be empty' do
    document.foo.children.empty?.should.be.true
    document.body.children.empty?.should.not.be.true
  end
end

describe 'Ruby extensions for System::Windows::FrameworkElement' do
  behaves_like 'Create XAML element'

  it 'can get named elements with method_missing' do
    @root.layout_root.class.should == System::Windows::Controls::Grid
    @root.message.text.should == "Foo.xaml" 
  end
end
