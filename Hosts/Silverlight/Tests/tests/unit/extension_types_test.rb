include Helpers
load_assembly 'Microsoft.Dynamic'

describe 'Html document extension' do
  before do
    @div ||= tag('div', :id => 'foo')
    HtmlPage.Document.Body.append_child(@div)
  end

  after do
    HtmlPage.document.body.remove_child @div
    @div = nil
  end

  it 'should find a HTML element' do
    div = HtmlDocumentExtension.get_bound_member HtmlPage.document, 'foo'
    div.should.equal @div
  end

  it 'should not find a HTML element' do
    id = 'doesnotexist'
    HtmlPage.document.get_element_by_id(id).should.be.nil
    div = HtmlDocumentExtension.get_bound_member HtmlPage.document, id
    div.should.equal nil
  end
end

describe 'Script object extension' do
  before do
    #@div ||= tag('div', :id => 'foo', :innerHTML => 'test')
    #HtmlPage.Document.Body.append_child(@div)
    $script_obj = HtmlPage.window.eval('test = {
        foo: "bar",
        method: function() {
          return "method-called";
        },  
        method_with_args: function(a, b) {
          return "method(" + a + "," + b + ")-called";
        }
      };
      test;'
    )
    $ext = ScriptObjectExtension
  end
  
  after do
    #HtmlPage.document.body.remove_child @div
    #@div = nil
  end

  it 'should get a property' do
    value = $ext.get_bound_member $script_obj, 'foo'
    value.to_s.should.equal 'bar'
  end
  
  it 'should not get a property' do
    value = $ext.get_bound_member $script_obj, 'doesnotexist'
    value.should.be.nil
  end
  
  it 'should set a property' do
    $script_obj.get_property('baz').to_s.should.not.equal 'updated'
    $ext.set_member($script_obj, 'baz', 'updated'.to_clr_string)
    $script_obj.get_property('baz').to_s.should.equal 'updated'
  end

  it 'should invoke a method' do
    method = $script_obj.get_property('method')
    $ext.get_bound_member($script_obj, 'method').should == method
    $ext.invoke(method).to_s.should == 'method-called'
  end

  it 'should invoke a method with args' do
    args = [rand(100), rand(100)]
    method = $script_obj.get_property('method_with_args')
    $ext.invoke(method, args[0], args[1]).to_s.should == 
      "method(#{args.join(',')})-called"
  end
end

describe 'Framework element extension' do
  before do
    @root = System::Windows::Controls::UserControl.new
    DynamicApplication.load_component @root, "#{File.dirname(__FILE__)}/assets/foo.xaml"
  end

  it 'should find a UIElement' do
    element = FrameworkElementExtension.get_bound_member(@root, 'message')
    element.should.not.be.nil
    element.name.to_s.should.equal 'message'
  end
  
  it 'should not find a UIElement' do
    @root.find_name('doesnotexist').should.be.nil
    result = FrameworkElementExtension.get_bound_member(@root, 'doesnotexist')
    result.should.equal Microsoft::Scripting::Runtime::OperationFailed.value
  end
end
