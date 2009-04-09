require File.dirname(__FILE__) + '/spec_helper'

# Quick rundown of how these specs work
# please read before hacking on this plugin
#
# helpers must be tested through then entire stack
# what that means is that each spec must
# send a request to a controller and render a template
#
# Start by creating a spec controller subclassing SpecController
# which itself is a subclass of Merb::Controller
# specs_controller.rb (available at spec/fixture/app/controllers/specs_controller.rb)
# defines SpecController.
# Create a new controller in the spec/fixture/app/controllers/ if you are adding a new helper
#
# To test your helper, start by initializing a controller
#
#    @controller = CustomHelperSpecs.new(Merb::Request.new({}))
#
# Note that we are sending a real request to the controller, feel free to use the request as needed
#
# You might need to access real objects in your views
# you can do that by setting them up in the controller
#
#    @obj = FakeModel.new # FaKeModel is defined in spec/fixture/models/first_generic_fake_model.rb check it out!
#    @controller.instance_variable_set(:@obj, @obj)
#
# To test a helper, you need to render a view:
#
#    result = @controller.render :view_name
#
# Of course, you need to create a view:
#    spec/fixture/app/views/custom_helper_specs/view_name.html.erb
# in the view, call the helper you want to test
#
# You can now test the helper in the view:
#    result.should match_tag(:form, :method => "post")
#


Merb::Plugins.config[:helpers] = {
  :default_builder => Merb::Helpers::Form::Builder::FormWithErrors
}

describe "error_messages_for" do

  before :each do
    @c = Application.new({})
    @dm_obj = Object.new
    @sq_obj = Object.new
    @dm_errors = [["foo", "bar"],["baz","bat"]]
    @sq_errors = Object.new
    @sq_errors.stub!(:full_messages).and_return(["foo", "baz"])
    @dm_obj.stub!(:errors).and_return(@dm_errors)
    @dm_obj.stub!(:new_record?).and_return(false)
    @sq_obj.stub!(:errors).and_return(@sq_errors)
    @sq_obj.stub!(:new_record?).and_return(false)
  end

  it "should build default error messages for AR-like models" do
    errs = @c.error_messages_for(@dm_obj)
    errs.should include("<h2>Form submission failed because of 2 problems</h2>")
    errs.should include("<li>foo bar</li>")
    errs.should include("<li>baz bat</li>")
  end

  it "should build default error messages for Sequel-like models" do
    errs = @c.error_messages_for(@sq_obj)
    errs.should include("<h2>Form submission failed because of 2 problems</h2>")
    errs.should include("<li>foo</li>")
    errs.should include("<li>baz</li>")
  end

  # it "should build default error messages for symbol" do
  #   errs = error_messages_for(:obj)
  #   errs.should include("<h2>Form submittal failed because of 2 problems</h2>")
  #   errs.should include("<li>foo bar</li>")
  #   errs.should include("<li>baz bat</li>")
  # end

  it "should accept a custom HTML class" do
    errs = @c.error_messages_for(@dm_obj, :error_class => "foo")
    errs.should include("<div class='foo'>")
  end

  it "should accept a custom header block" do
    errs = @c.error_messages_for(@dm_obj, :header => "<h3>Failure: %s issue%s</h3>")
    errs.should include("<h3>Failure: 2 issues</h3>")
  end

#  it "should put the error messages inside a form if :before is false" do
#    ret = @c.form_for @dm_obj do
#      _buffer << error_messages
#    end
#    ret.should =~ /\A\s*<form.*<div class='error'>/
#  end

end

describe "form" do

  before :each do
    @c = FormSpecs.new(Merb::Request.new({}))
  end

  describe "when _default_builder is Merb::Helpers::Form::Builder::ResourcefulFormWithErrors" do

    before(:each) do
      @obj = FakeModel2.new
      @c.instance_variable_set(:@obj, @obj)
    end

    it "should not explode when #form is called" do
      r = @c.render :resourceful_form
      pending
      #r.should =~ /action="fake_model2\/#{@obj.id}"/
    end
  end


  it "should use the post method by default" do
    ret = @c.render(:post_by_default)
    ret.should have_selector("form[method=post]")
    ret.should include("CONTENT")
  end

  it "should use the get method if set" do
    ret = @c.render(:get_if_set)
    ret.should have_selector("form[method=get]")
  end

  it "should fake out the put method if set" do
    ret = @c.render(:fake_put_if_set)
    ret.should have_selector("form[method=post]")
    ret.should have_selector("input[type=hidden][name=_method][value=put]")
  end

  it "should fake out the delete method if set" do
    ret = @c.render(:fake_delete_if_set)
    ret.should have_selector("form[method=post]")
    ret.should have_selector("input[type=hidden][name=_method][value=delete]")
  end

  # TODO: Why is this required?
  # ---------------------------
  #
  # it "should silently set method to post if an unsupported method is used" do
  #     form_tag :method => :dodgy do
  #       _buffer << "CONTENT"
  #     end
  #     _buffer.should match_tag(:form, :method => "post")
  #     _buffer.should_not match_tag(:input, :type => "hidden", :name => "_method", :value => "dodgy")
  # end

  it "should take create a form" do
    ret = @c.render(:create_a_form)
    ret.should have_selector("form[action=foo][method=post]")
    ret.should include("Hello")
  end

  it "should set a form to be multipart" do
    ret = @c.render(:create_a_multipart_form)
    ret.should have_selector("form[action=foo][method=post][enctype='multipart/form-data']")
    ret.should include("CONTENT")
  end
end


describe "form_for" do

  before :each do
    @c = FormForSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should wrap the contents in a form tag" do
    form = @c.render :basic
    form.should have_selector("form[method=post]")
    form.should have_selector("input[type=hidden][value=put][name=_method]")
  end

  it "should set the method to post be default" do
    new_fake_model = FakeModel2.new
    @c.instance_variable_set(:@obj, new_fake_model)
    form = @c.render :basic
    form.should have_selector("form[method=post]")
    form.should_not have_selector("input[type=hidden][name=_method]")
  end

  it "should support PUT if the object passed in is not a new_record? via a hidden field" do
    form = @c.render :basic
    form.should have_selector("form[method=post]")
    form.should have_selector("input[type=hidden][value=put][name=_method]")
  end

end


describe "fields_for" do

  before :each do
    @c = FieldsForSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end


  it "should dump the contents in the context of the object" do
    r = @c.render :basic
    r.should have_selector("input[type=text][value=foowee]")
  end

  it "should be able to modify the context midstream" do
    @c.instance_variable_set(:@obj2, FakeModel2.new)
    r = @c.render :midstream
    r.should have_selector("input[type=text][value=foowee]")
    r.should have_selector("input[name='fake_model2[foo]'][type=text][value=foowee2]")
  end

  it "should handle an explicit nil attribute" do
    r = @c.render :nil
    r.should have_selector("input[name='fake_model[foo]'][value=foowee][type=text]")
  end

  it "should pass context back to the old object after exiting block" do
    @c.instance_variable_set(:@obj2, FakeModel2.new)
    r = @c.render :midstream
    r.should have_selector("input[id=fake_model_foo][name='fake_model[foo]'][type=text][extra=true]")
  end
end

describe "text_field" do

  before :each do
    @c = TextFieldSpecs.new(Merb::Request.new({}))
  end

  it "should return a basic text field based on the values passed in" do
    r = @c.render :basic
    r.should have_selector("input[type=text][id=foo][name=foo][value=bar]")
  end
  
  it "should update an existing :class with a new class" do
    r = @c.render :class
    r.should == "<input type=\"text\" class=\"awesome foobar text\"/>"
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should have_selector("input[type=text][disabled=disabled]")
  end

  it "should provide an additional label tag if the :label option is passed in as a hash" do
    r = @c.render :label
    r.should have_selector("label[class=cool][for=foo]:contains('LABEL')")
  end
  
  it "should allow a symbolized name" do
    r = @c.render :symbolized_name
    r.should have_selector("input[type=text][name=foo][value=bar]")
    r.should have_selector("label[for=foo]:contains('LABEL')")
  end
end

describe "bound_text_field" do

  before :each do
    @c = BoundTextFieldSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should take a string object and return a useful text control" do
    r = @c.render :basic
    r.should have_selector("input[type=text][id=fake_model_foo][name='fake_model[foo]'][value=foowee]")
  end

  it "should take additional attributes and use them" do
    r = @c.render :basic
    r.should have_selector("input[type=text][name='fake_model[foo]'][value=foowee][bar='7']")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    form = @c.render :basic
    form.should match(/<label.*>LABEL<\/label><input/)
    form.should_not have_selector("input[label=LABEL]")
  end
  
  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :basic
    form.should have_selector("label[for=fake_model_foo]:contains('LABEL')")
  end

  it "should not errorify the field for a new object" do
    r = @c.render :basic
    r.should_not have_selector("input[type=text][name='fake_model[foo]'][class=error]")
  end

  it "should errorify a field for a model with errors" do
    model = mock("model")
    model.stub!(:new_record?).and_return(true)
    model.stub!(:class).and_return("MyClass")
    model.stub!(:foo).and_return("FOO")
    errors = mock("errors")
    errors.should_receive(:on).with(:foo).and_return(true)

    model.stub!(:errors).and_return(errors)
    @c.instance_variable_set(:@obj, model)
    r = @c.render :basic
    r.should have_selector("input[class='error text']")
  end
end

describe "bound_radio_button" do

  before :each do
    @c = BoundRadioButtonSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should take a string object and return a useful text control" do
    r = @c.render :basic
    r.should have_selector("input[type=radio][id=fake_model_foo][name='fake_model[foo]'][value=foowee]")
  end

  it "should take additional attributes and use them" do
    r = @c.render :basic
    r.should have_selector("input[type=radio][name='fake_model[foo]'][value=foowee][bar='7']")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    form = @c.render :basic
    form.should have_selector("input + label:contains('LABEL')")
    form.should_not have_selector("input[label]")
  end
  
  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :basic
    form.should have_selector("label[for=fake_model_foo]:contains('LABEL')")
  end

  it "should not errorify the field for a new object" do
    r = @c.render :basic
    r.should_not have_selector("input[type=radio][name='fake_model[foo]'][class=error]")
  end

  it "should errorify a field for a model with errors" do
    model = mock("model")
    model.stub!(:new_record?).and_return(true)
    model.stub!(:class).and_return("MyClass")
    model.stub!(:foo).and_return("FOO")
    errors = mock("errors")
    errors.should_receive(:on).with(:foo).and_return(true)

    model.stub!(:errors).and_return(errors)
    @c.instance_variable_set(:@obj, model)
    r = @c.render :basic
    r.should have_selector("input[class='error radio']")
  end
end

describe "password_field" do

  before :each do
    @c = PasswordFieldSpecs.new(Merb::Request.new({}))
  end

  it "should return a basic password field, but omit the value" do
    r = @c.render :basic
    r.should have_selector("input[type=password][id=foo][name=foo]")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    r = @c.render :basic
    r.should have_selector("label[for=foo]:contains('LABEL')")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should match_tag(:input, :type => "password", :disabled => "disabled")
  end
end

describe "bound_password_field" do

  before :each do
    @c = BoundPasswordFieldSpecs.new(Merb::Request.new({}))
    @obj = FakeModel.new
    @c.instance_variable_set(:@obj, @obj)
  end

  it "should take a string object and return a useful password control, but omit the value" do
    r = @c.render :basic
    r.should match_tag(:input, :type => "password", :id => "fake_model_foo", :name => "fake_model[foo]")
  end

  it "should take additional attributes and use them" do
    r = @c.render :basic
    r.should match_tag(:input, :type => "password", :name => "fake_model[foo]", :bar => "7", :value => @obj.foo)
  end

  it "should provide an additional label tag if the :label option is passed in" do
    r = @c.render :basic
    r.should match(/<label.*>LABEL<\/label><input/)
    r.should_not match_tag(:input, :label => "LABEL")
  end
  
  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :basic
    form.should have_selector("label[for=fake_model_foo]:contains('LABEL')")
  end

  it "should not errorify the field for a new object" do
    r = @c.render :basic
    r.should_not match_tag(:input, :class => "error")
  end

  it "should errorify a field for a model with errors" do
    model = mock("model")
    model.stub!(:new_record?).and_return(true)
    model.stub!(:class).and_return("MyClass")
    model.stub!(:foo).and_return("FOO")
    errors = mock("errors")
    errors.should_receive(:on).with(:foo).and_return(true)

    model.stub!(:errors).and_return(errors)

    @c.instance_variable_set(:@obj, model)
    r = @c.render :basic
    r.should match_tag(:input, :class => "error password")
  end

end

describe "check_box" do

  before :each do
    @c = CheckBoxSpecs.new(Merb::Request.new({}))
  end

  it "should return a basic checkbox based on the values passed in" do
    r = @c.render :basic
    r.should match_tag(:input, :class => "checkbox", :id => "foo", :name => "foo", :checked => "checked")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    result = @c.render :label
    result.should have_selector("label[for=foo]:contains('LABEL')")

    result.should match(/<input.*><label/)
    res = result.scan(/<[^>]*>/)
    res[0].should_not match_tag(:input, :label => "LABEL")
  end

  it 'should remove the checked="checked" attribute if :checked is false or nil' do
    r = @c.render :unchecked
    r.should_not   include('checked="')
  end

  it 'should have the checked="checked" attribute if :checked => true is passed in' do
    r = @c.render :basic
    r.should include('checked="checked"')
  end

  it "should not be boolean by default" do
    r = @c.render :basic
    r.should match_tag(:input, :type => "checkbox", :name => "foo")
  end

  it "should add a hidden input if boolean" do
    r = @c.render :boolean
    r.should have_tag(:input, :type => "checkbox", :value => "1")
    r.should have_tag(:input, :type => "hidden",   :value => "0")
    r.should match(/<input.*?type="hidden"[^>]*>[^<]*<input.*?type="checkbox"[^>]*>/)

  end

  it "should not allow a :value param if boolean" do
    lambda { @c.render :raises_error_if_not_boolean }.
      should raise_error(ArgumentError, /can't be used with a boolean checkbox/)
  end

  it "should not allow :boolean => false if :on and :off are specified" do
    lambda { @c.render :raises_error_if_on_off_and_boolean_false }.
      should raise_error(ArgumentError, /cannot be used/)
  end

  it "should be boolean if :on and :off are specified" do
    html = @c.render :on_off_is_boolean
    html.should have_tag(:input, :type => "checkbox", :value => "YES", :name => "foo")
    html.should have_tag(:input, :type => "hidden",   :value => "NO",  :name => "foo")
  end

  it "should have both :on and :off specified or neither" do
    lambda { @c.render :raise_unless_both_on_and_off }.should raise_error(ArgumentError, /must be specified/)
    lambda { @c.render :raise_unless_both_on_and_off }.should raise_error(ArgumentError, /must be specified/)
  end

  it "should convert :value to a string on a non-boolean checkbox" do
    r = @c.render :to_string
    r.should match_tag(:input, :value => "")
    r.should match_tag(:input, :value => "false")
    r.should match_tag(:input, :value => "0")
    r.should match_tag(:input, :value => "0")
    r.should match_tag(:input, :value => "1")
    r.should match_tag(:input, :value => "1")
    r.should match_tag(:input, :value => "true")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should match_tag(:input, :type => "checkbox", :disabled => "disabled")
  end

  it "should be possible to call with just check_box" do
    r = @c.render :simple
    r.should match_tag(:input, :type => "checkbox", :class => "checkbox")
  end
end

describe "bound_check_box" do

  before :each do
    @c = BoundCheckBoxSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should take a string and return a useful checkbox control" do
    r = @c.render :basic
    r.should match_tag(:input, :type =>"checkbox", :id => "fake_model_baz", :name => "fake_model[baz]", :class => "checkbox", :value => "1", :checked => "checked", :id => "fake_model_baz")
    r.should match_tag(:input, :type =>"hidden",  :name => "fake_model[baz]", :value => "0")
  end

  it "should raise an error if you try to use :value" do
    lambda { @c.render(:raise_value_error) }.should raise_error(ArgumentError, /:value can't be used with a bound_check_box/)
  end

  it "should support models from datamapper" do
    @c.instance_variable_set(:@obj, FakeDMModel.new)
    r = @c.render :basic
    r.should match_tag(:input,
                                              :type    =>"checkbox",
                                              :name    => "fake_dm_model[baz]",
                                              :class   => "checkbox",
                                              :value   => "1",
                                              :checked => "checked",
                                              :id      => "fake_dm_model_baz")

    r.should match_tag(:input, :type =>"hidden",   :name => "fake_dm_model[bat]", :value => "0")
    r.should match_tag(:input, :type =>"checkbox", :name => "fake_dm_model[bat]", :class => "checkbox", :value => "1")
  end

  it "should allow a user to set the :off value" do
    r = @c.render :on_and_off
    r.should match_tag(:input, :type =>"hidden",   :name => "fake_model[bat]", :value => "off")
    r.should match_tag(:input, :type =>"checkbox", :name => "fake_model[bat]", :class => "checkbox", :value => "on")
  end

  it "should render controls with errors if their attribute contains an error" do
    r = @c.render :errors
    r.should match_tag(:input, :type =>"checkbox", :name => "fake_model[bazbad]", :class => "error checkbox", :value => "1", :checked => "checked")
    r.should match_tag(:input, :type =>"hidden",   :name => "fake_model[batbad]", :value => "0")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    form = @c.render :label
    form.should match( /<input.*><label.*>LABEL<\/label>/ )
    form.should_not match_tag(:input, :label => "LABEL")
  end
  
  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :label
    form.should have_selector("label[for=fake_model_foo]:contains('LABEL')")
  end

  it "should not errorify the field for a new object" do
    r = @c.render :basic
    r.should_not match_tag(:input, :type => "checkbox", :class => "error checkbox")
  end

  it "should errorify a field for a model with errors" do
    model = mock("model")
    model.stub!(:new_record?).and_return(true)
    model.stub!(:class).and_return("MyClass")
    model.stub!(:baz).and_return("BAZ")
    model.stub!(:bat).and_return("BAT")
    errors = mock("errors")
    errors.should_receive(:on).with(:baz).and_return(true)
    errors.should_receive(:on).with(:bat).and_return(true)

    model.stub!(:errors).and_return(errors)

    @c.instance_variable_set(:@obj, model)
    r = @c.render :basic
    r.should match_tag(:input, :type => "checkbox", :class => "error checkbox")
  end

  it "should be boolean" do
    r = @c.render :basic
    r.should have_tag(:input, :type => "checkbox", :value => "1")
    r.should have_tag(:input, :type => "hidden",   :value => "0")
  end

  it "should be checked if the value of the model's attribute is equal to the value of :on" do
    r = @c.render :checked
    r.should match_tag(:input, :type =>"checkbox", :value => "foowee", :checked => "checked")
    r.should match_tag(:input, :type =>"checkbox", :value => "YES")
  end

  it "should render false attributes as not checked" do
    @c.instance_variable_set(:@obj, FakeDMModel.new)
    r = @c.render :basic_unchecked
    r.should match_tag(:input, :type =>"checkbox", :name => "fake_dm_model[bat]")
    r.should_not include("checked=")
  end
end

describe "hidden_field" do

  before :each do
    @c = HiddenFieldSpecs.new(Merb::Request.new({}))
  end

  it "should return a basic checkbox based on the values passed in" do
    r = @c.render :basic
    r.should match_tag(:input, :type => "hidden", :id => "foo", :name => "foo", :value => "bar")
  end

  it "should not render a label if the :label option is passed in" do
    res = @c.render :label
    res.should_not match(/<label>LABEL/)
    res.should_not match_tag(:input, :label=> "LABEL")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should match_tag(:input, :type => "hidden", :disabled => "disabled")
  end
end

describe "bound_hidden_field" do

  before :each do
    @c = BoundHiddenFieldSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should take a string and return a hidden field control" do
    r = @c.render :basic
    r.should match_tag(:input, :type =>"hidden", :id => "fake_model_foo", :name => "fake_model[foo]", :value => "foowee")
  end

  it "should render controls with errors if their attribute contains an error" do
    r = @c.render :errors
    r.should match_tag(:input, :type =>"hidden", :name => "fake_model[foobad]", :value => "foowee", :class => "error hidden")
  end

  it "should not render a label if the :label option is passed in" do
    r = @c.render :label
    r.should_not match(/<label>LABEL/)
    r.should_not match_tag(:input, :label=> "LABEL")
  end
  
  it "should not errorify the field for a new object" do
    r = @c.render :basic
    r.should_not match_tag(:input, :type => "hidden", :class => "error")
  end

  it "should not errorify a field for a model with errors" do
    model = mock("model")
    model.stub!(:new_record?).and_return(true)
    model.stub!(:class).and_return("MyClass")
    model.stub!(:foo).and_return("FOO")
    errors = mock("errors")
    errors.should_receive(:on).with(:foo).and_return(true)

    model.stub!(:errors).and_return(errors)

    @c.instance_variable_set(:@model, model)
    r = @c.render :hidden_error
    r.should match_tag(:input, :type => "hidden", :name => "my_class[foo]", :class => "error hidden")
  end

end

describe "radio_button" do

  before :each do
    @c = RadioButtonSpecs.new(Merb::Request.new({}))
  end

  it "should should return a basic radio button based on the values passed in" do
    r = @c.render :basic
    r.should match_tag(:input, :type => "radio", :name => "foo", :value => "bar", :id => "baz")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    result = @c.render :label
    result.should match(/<input.*><label.*>LABEL<\/label>/)
    res = result.scan(/<[^>]*>/)
    res[0].should_not match_tag(:input, :label => "LABEL")
  end

  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :label
    form.should have_selector("label[for='foo']:contains('LABEL')")
  end  

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should match_tag(:input, :type => "radio", :disabled => "disabled")
  end
  
  it "should be checked if :checked => true is passed in" do
    r = @c.render :checked
    r.should match_tag(:input, :type => "radio", :checked => "checked")
  end
  
  it "should be unchecked if :checked => false is passed in" do
    r = @c.render :unchecked
    r.should_not include("checked=")
  end
end

describe "radio_group" do

  before :each do
    @c = RadioGroupSpecs.new(Merb::Request.new({}))
  end

  it "should return a group of radio buttons" do
    radio = @c.render :basic
    radio_tags = radio.scan(/<[^>]*>/)
    radio_tags[0].should match_tag(:input, :type => "radio", :value => "foowee")
    radio_tags[3].should match_tag(:input, :type => "radio", :value => "baree")
  end

  it "should provide an additional label tag for each option in array-based options" do
    radio = @c.render :basic
    radio.scan( /<input.*?><label.*?>(foowee|baree)<\/label>/ ).size.should == 2
    radio = radio.scan(/<[^>]*>/)
    radio[0].should_not match_tag(:input, :label => "LABEL")
    radio[3].should_not match_tag(:input, :label => "LABEL")
  end

  it "should accept array of hashes as options" do
    radio = @c.render :hash
    radio.scan( /<input.*?><label.*?>(Five|Bar)<\/label>/ ).size.should == 2
    radio.scan(/<[^>]*>/).size.should == 6
    radio.should match_tag(:input, :value => 5)
    radio.should match_tag(:label)
    radio.should match_tag(:input, :value => 'bar', :id => 'bar_id')
    radio.should match_tag(:label, :for => 'bar_id')
  end
  
  it "should render the label tags on each radio button with the proper for= atttribute" do
    form = @c.render :hash
    form.should have_selector("label[for='bar_id']:contains('Bar')")
  end  
  
  it "should apply attributes to each element" do
    radio = @c.render :attributes
    radio = radio.scan(/<[^>]*>/)
    radio[0].should match_tag(:input, :type => "radio", :value => "foowee", :class => "CLASS radio")
    radio[3].should match_tag(:input, :type => "radio", :value => "baree", :class => "CLASS radio")
  end

  it "should override universal attributes with specific ones" do
    radio = @c.render :specific_attributes
    radio = radio.scan(/<[^>]*>/)
    radio[0].should match_tag(:input, :type => "radio", :value => "foowee", :class => "CLASS radio")
    radio[3].should match_tag(:input, :type => "radio", :value => "baree", :class => "BAREE radio")
  end
  
  it "should allow specifying a checked radio button" do
    r = @c.render :checked
    r.should match_tag(:input, :value => "bar", :checked => "checked")
  end
end


describe "bound_radio_group" do

  before do
    @c = BoundRadioGroupSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should return a group of radio buttons" do
    r = @c.render :basic
    r.should match_tag(:input, :type => "radio", :id => "fake_model_foo_foowee", :name => "fake_model[foo]", :value => "foowee", :checked => "checked")
    r.should match_tag(:input, :type => "radio", :id => "fake_model_foo_baree", :name => "fake_model[foo]", :value => "baree")
    r.should_not match_tag(:checked => "checked")
  end

  it "should provide an additional label tag for each option in array-based options" do
    r = @c.render :basic
    r.scan( /<input.*?><label.*?>(foowee|baree)<\/label>/ ).size.should == 2
    radio = r.scan(/<[^>]*>/)[2..-2]
    radio[0].should_not match_tag(:input, :label => "LABEL")
    radio[3].should_not match_tag(:input, :label => "LABEL")
  end
  
  it "should render the label tags on each radio option with the proper for= atttribute" do
    form = @c.render :basic
    form.should have_selector("label[for=fake_model_foo_foowee]:contains('foowee')")
    form.should have_selector("label[for=fake_model_foo_baree]:contains('baree')")
  end

  it "should accept array of hashes as options" do
    r = @c.render :hashes
    r.scan( /<input.*?><label.*?>(Five|Bar)<\/label>/ ).size.should == 2
    r.scan(/<[^>]*>/)[2..-2].size.should == 6
    r.should match_tag(:input, :value => 5)
    r.should match_tag(:label)
    r.should match_tag(:input, :value => 'bar', :id => 'bar_id')
    r.should match_tag(:label, :for => 'bar_id')
  end

  it "should provide autogenerated id for inputs" do
    r = @c.render :mixed
    r.should match_tag(:input, :id => 'fake_model_foo_bar')
    r.should match_tag(:label, :for => 'fake_model_foo_bar')
    r.should match_tag(:input, :id => 'fake_model_foo_bar')
    r.should match_tag(:label, :for => 'fake_model_foo_bar')
  end

  it "should override autogenerated id for inputs with hash-given id" do
    r = @c.render :override_id
    r.should match_tag(:input, :id => 'bar_id')
    r.should match_tag(:label, :for => 'bar_id')
  end

  it "should only have one element with the checked property" do
    r = @c.render :basic
    r.should match_tag(:input, :checked => "checked")
    r.should_not match_tag(:input, :checked => "false")
  end
end


describe "text_area" do

  before do
    @c = TextAreaSpecs.new(Merb::Request.new({}))
  end

  it "should should return a basic text area based on the values passed in" do
    r = @c.render :basic
    r.should match_tag(:textarea, :name => "foo", :id => "foo")
  end

  it "should handle a nil content" do
    r = @c.render :nil
    r.should == "<textarea name=\"foo\" id=\"foo\"></textarea>"
  end


  # TODO: Why is this required?
  # ---------------------------
  #
  # it "should handle a nil attributes hash" do
  #   text_area("CONTENT", nil).should == "<textarea>CONTENT</textarea>"
  # end

  it "should render a label when the :label option is passed in" do
    result = @c.render :label
    result.should match(/<label.*>LABEL<\/label><textarea/)
    result.should_not match_tag(:textarea, :label => "LABEL")

    result.should have_selector("label[for=foo]:contains('LABEL')")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should match_tag(:textarea, :disabled => "disabled")
  end
end

describe "bound_text_area" do

  before do
    @c = BoundTextAreaSpecs.new(Merb::Request.new({}))
    @obj = FakeModel.new
    @c.instance_variable_set(:@obj, @obj)
  end

  it "should provide :id attribute" do
    r = @c.render :basic
    r.should match_tag(:textarea, :id => 'fake_model_foo', :name => "fake_model[foo]")
    r.should =~ />\s*#{@obj.foo}\s*</
  end
  
  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :label
    form.should have_selector("label[for='fake_model_foo']:contains('LABEL')")
  end
end

describe "select" do

  before do
    @c = SelectSpecs.new(Merb::Request.new({}))
  end

  it "should provide a blank option if you :include_blank" do
    r = @c.render :blank
    r.should =~ /<option.*>\s*<\/option>/
  end
  
  it "should render the select tag proper attributes" do
    r = @c.render :basic
    r.should match_tag( :select, :name => "foo", :id => "foo")
    r.should have_selector("select[name=foo] option:contains('one')")
    r.should have_selector("select[name=foo] option:contains('two')")
    r.should have_selector("select[name=foo] option:contains('three')")
  end
  
  it "should allow selecting an option by passing in :selected => 'three'" do
    r = @c.render :selected
    r.should_not have_selector("select[name=foo] option[selected]:contains('one')")
    r.should_not have_selector("select[name=foo] option[selected]:contains('two')")
    r.should have_selector("select[name=foo] option[selected]:contains('three')")
  end

  it "should render the select tag with suffix '[]' to name when :multiple => true" do
    r = @c.render :multiple
    r.should match_tag( :select, :name => "foo[]")
  end
  
  it "should render a label when the :label option is passed in" do
    result = @c.render :label
    result.should have_selector("label[for=foo]:contains('LABEL')")
  end
end

describe "bound_select" do

  before do
    @c = BoundSelectSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should render the select tag with the correct id and name" do
    r = @c.render :basic
    r.should match_tag( :select, :id => "fake_model_foo", :name => "fake_model[foo]" )
  end

  it "should render the select tag with suffix '[]' to name when :multiple => true" do
    r = @c.render :multiple
    r.should match_tag( :select, :id => "fake_model_foo", :name => "fake_model[foo][]" )
  end

  it "should include a blank option" do
    r = @c.render :blank
    r.should match_tag(:option, :value => '')
    r.should =~ /<option.*>\s*<\/option>/
  end

  it "should render a prompt option without a value" do
    r = @c.render :prompt
    r.should match_tag(:option, :value => '')
    r.should =~ /<option.*>Choose<\/option>/
  end

  it "should render a select tag with options" do
    r = @c.render :with_options
    r.should match_tag( :select, :class => "class1 class2", :title=> "This is the title" )
    r.should =~ /<select.*>\s*<\/select>/
  end

  it "should render a select tag with options and a blank option" do
    r = @c.render :with_options_with_blank
    r.should match_tag( :select, :title => "TITLE" )
    r.should match_tag( :option, :value => '' )
    r.should =~ /<option.*>\s*<\/option>/
  end
  
  it "should render the label tag with the proper for= atttribute" do
    form = @c.render :label
    form.should have_selector("label[for=fake_model_foo]:contains('LABEL')")
  end

  # Not sure how this makes any sense
  # ---------------------------------
  #
  # it "should render the text as the value if no text_method is specified" do
  #   form_for @obj do
  #     content = select( :foo, :collection => [FakeModel] )
  #     content.should match_tag( :option, :value => "FakeModel" )
  #   end
  # end

end

describe "bound option tags" do

  before do
    @c = BoundOptionTagSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end


  it "should use text_method and value_method for tag generation" do
    r = @c.render :text_and_value
    r.should match_tag( :option, :content => "foowee", :value => "7" )
    r.should match_tag( :option, :content => "foowee2", :value => "barbar" )

    # content = options_from_collection_for_select( [FakeModel.new, FakeModel2.new], :text_method => 'foo', :value_method => 'bar' )
    # content.should match_tag( :option, :content => "foowee", :value => "7" )
    # content.should match_tag( :option, :content => "foowee2", :value => "barbar" )
  end

  it "should render a hash of arrays as a grouped select box" do
    model1 = FakeModel.new ; model1.make = "Ford"   ; model1.model = "Mustang"   ; model1.vin = '1'
    model2 = FakeModel.new ; model2.make = "Ford"   ; model2.model = "Falcon"    ; model2.vin = '2'
    model3 = FakeModel.new ; model3.make = "Holden" ; model3.model = "Commodore" ; model3.vin = '3'
    @c.instance_variable_set(:@model1, model1)
    @c.instance_variable_set(:@model2, model2)
    @c.instance_variable_set(:@model3, model3)
    @c.instance_variable_set(:@collection, [model1, model2, model3].inject({}) {|s,e| (s[e.make] ||= []) << e; s })
    r = @c.render :grouped
    # Blank actually defaults to ""
    r.should =~ /<optgroup label=\"Ford\"><option/
    r.should match_tag( :optgroup, :label => "Ford" )
    r.should match_tag( :option, :selected => "selected", :value => "1", :content => "Mustang" )
    r.should match_tag( :option, :value => "2", :content => "Falcon" )
    r.should match_tag( :optgroup, :label => "Holden" )
    r.should match_tag( :option, :value => "3", :content => "Commodore" )

    # collection = [@model1, @model2, @model3].inject({}) {|s,e| (s[e.make] ||= []) << e; s }
    # content = options_from_collection_for_select(collection, :text_method => 'model', :value_method => 'vin', :selected => '1')
  end

  it "should render a collection of nested value/content arrays" do
    r = @c.render :nested
    r.should match_tag(:select, :id => "fake_model_foo", :name => "fake_model[foo]")
    r.should match_tag(:option, :value => "small",  :content => "Small")
    r.should match_tag(:option, :value => "medium", :content => "Medium")
    r.should match_tag(:option, :value => "large",  :content => "Large")
  end

  # Is this really worth the extra speed hit? I'm thinking not
  # ----------------------------------------------------------
  #
  # it "should humanize and titlize keys in the label for the option group" do
  #   collection = { :some_snake_case_key => [FakeModel.new] }
  #   form_for @obj do
  #     content = select( :foo, :collection => collection )
  #     content.should match_tag( :optgroup, :label => "Some Snake Case Key" )
  #   end
  # end

end

require "hpricot"

describe "option tags" do

  before do
    @c = OptionTagSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@collection, [['rabbit','Rabbit'],['horse','Horse'],['bird','Bird']])
  end

  it "should provide an option tag for each item in the collection" do
    r = @c.render :collection
    doc = Hpricot( r )
    (doc/"option").size.should == 3
  end

  it "should provide a blank option" do
    r = @c.render :with_blank
    r.should match_tag( :option, :value => '' )
  end

  it "should render the prompt option at the top" do
    r = @c.render :with_prompt
    #ontent = select( :collection => [["foo", "Foo"]], :prompt => 'Choose' )
    r.should match(/<option[^>]*>Choose<\/option>[^<]*<option[^>]*>Foo<\/option>/)
  end

  it "should provide selected options by value" do
    r = @c.render :selected
    r.should match_tag( :option, :value => 'rabbit', :selected => 'selected', :content => 'Rabbit' )
    r.should_not match_tag( :option, :value => 'chicken', :selected => nil, :content => 'Chicken' )
  end

  it "should handle arrays for selected when :multiple is true" do
    r = @c.render :multiple_selects
    r.should match_tag( :option, :value => 'minutes', :selected => 'selected', :content => 'Time' )
    r.should match_tag( :option, :value => 'dollars', :selected => 'selected', :content => 'Money' )
  end

  it "should render a hash of options as optgroup" do
    r = @c.render :optgroups
    r.should match_tag( :optgroup, :label => 'Fruit' )
    r.should match_tag( :optgroup, :label => 'Vegetables' )
    r.should match_tag( :option, :value => 'banana', :selected => 'selected', :content => 'Banana' )
  end

  it "should accept an array of strings in :collection as the content/value of each option" do
    r = @c.render :array
    r.should match_tag(:option, :content => "one", :value => "one")
    r.should match_tag(:option, :content => "two", :value => "two")
  end

  it "should only pass :selected and :value attrs to <option> tags" do
    r = @c.render :no_extra_attributes
    r = r.slice(/<option[^>]*>[^<]*<\/option>/)
    r.should match_tag(:option, :value => "rabbit", :content => "Rabbit")
    r.should_not match_tag(:option, :id => "my_id", :name => "my_name", :class => "classy")
  end

  it "should not pollute the <select> attributes with <option> attributes" do
    r = @c.render :clean
    r.should_not match_tag(:select, :value => "banana", :selected => "selected")
  end
end

describe "fieldset" do

  before :each do
    @c = FieldsetSpecs.new(Merb::Request.new({}))
  end

  it "should provide legend option" do
    r = @c.render :legend
    r.should have_selector("fieldset legend:contains('TEST')")
  end

end

describe "label" do

  before :each do
    @c = LabelSpecs.new(Merb::Request.new({}))
  end

  it "should render a label tag" do
    r = @c.render :basic
    r.should have_selector("label[for=user_first_name]:contains('First Name')")
  end

  it "should render a label tag with a :class attribute set" do
    r = @c.render :basic_with_class
    r.should have_selector("label[class=name_class]")
  end

  it "should render a label tag with both rel and style attributes set" do
    r = @c.render :basic_with_attributes
    r.should have_selector("label[rel=tooltip][style='display:none']")
  end

end

describe "file_field" do

  before :each do
    @c = FileFieldSpecs.new(Merb::Request.new({}))
  end

  it "should return a basic file field based on the values passed in" do
    r = @c.render :with_values
    r.should have_selector("input[type=file][id=foo][name=foo][value=bar]")
  end

  it "should wrap the field in a label if the :label option is passed to the file" do
    r = @c.render :with_label
    r.should have_selector("label[for=foo]:contains('LABEL') + input.file[type=file]")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled
    r.should have_selector("input[type=file][disabled=disabled]")
  end

  it "should make the surrounding form multipart" do
    r = @c.render :makes_multipart
    r.should have_selector("form[enctype='multipart/form-data']")
  end
end

describe "bound_file_field" do

  before :each do
    @c = BoundFileFieldSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should take a string object and return a useful file control" do
    r  = @c.render :takes_string
    r.should have_selector("input[type=file][id=fake_model_foo][name='fake_model[foo]'][value=foowee]")
  end

  it "should take additional attributes and use them" do
    r = @c.render :additional_attributes
    r.should have_selector("input[type=file][name='fake_model[foo]'][value=foowee][bar='7']")
  end

  it "should wrap the file_field in a label if the :label option is passed in" do
    r = @c.render :with_label
    r.should have_selector("label[for=fake_model_foo]:contains('LABEL')")
    r.should_not have_selector("input[label=LABEL]")
  end
end

describe "submit" do

  before :each do
    @c = SubmitSpecs.new(Merb::Request.new({}))
  end

  it "should return a basic submit input based on the values passed in" do
    r = @c.render :submit_with_values
    r.should have_selector("input[type=submit][name=foo][value=Done]")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    r = @c.render :submit_with_label
    r.should have_selector("input[type=submit][name=submit][value=Done]")
    r.should have_selector("label:contains('LABEL')")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled_submit
    r.should have_selector("input[type=submit][value=Done][disabled=disabled]")
  end
end

describe "button" do

  before :each do
    @c = ButtonSpecs.new(Merb::Request.new({}))
  end

  it "should return a button based on the values passed in" do
    r = @c.render :button_with_values
    r.should have_selector("button[type=button][name=foo][value=bar]:contains('Click Me')")
  end

  it "should provide an additional label tag if the :label option is passed in" do
    r = @c.render :button_with_label
    r.should have_selector("button[value=foo]")
    r.should have_selector("label:contains('LABEL')")
  end

  it "should be disabled if :disabled => true is passed in" do
    r = @c.render :disabled_button
    r.should have_selector("button[disabled=true]")
  end
end


class MyBuilder < Merb::Helpers::Form::Builder::Base

  def update_bound_controls(method, attrs, type)
    super
    attrs[:bound] = type
  end

  def update_unbound_controls(attrs, type)
    super
    attrs[:unbound] = type
  end

end

describe "custom builder" do

  before :each do
    @c = CustomBuilderSpecs.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should let you override update_bound_controls" do
    r = @c.render :everything
    r.should =~ / bound="file"/
    r.should =~ / bound="text"/
    r.should =~ / bound="hidden"/
    r.should =~ / bound="password"/
    r.should =~ / bound="radio"/
    r.should =~ / bound="text_area"/
  end

  it "should let you override update_unbound_controls" do
    r = @c.render :everything
    r.should have_selector("button[unbound=button]")
    r.should have_selector("input[unbound=submit]")
    r.should have_selector("textarea[unbound=text_area]")
  end
end


describe 'delete_button' do

  before :each do
    @controller = DeleteButtonSpecs.new(Merb::Request.new({}))
    @controller.instance_variable_set(:@obj, FakeModel.new)
  end

  it "should have a default submit button text" do
    result = @controller.render :simple_delete # <%= delete_button @obj %>
    result.should have_selector("input[type=submit][value=Delete]")
  end

  it 'should return a button inside of a form for the object' do
    result = @controller.render :simple_delete # <%= delete_button @obj %>
    result.should have_selector("form[action='/fake_models/fake_model'][method=post]")
    result.should have_selector("input[type=hidden][value=DELETE][name=_method]")
  end

  it 'should allow you to modify the label' do
    result = @controller.render :delete_with_label # <%= delete_button(@obj, "Delete moi!") %>
    result.should have_selector("input[type=submit][value='Delete moi!']")
  end

  it "should allow you to pass some extra params like a class" do
    result = @controller.render :delete_with_extra_params
    result.should have_selector("input.custom-class[type=submit][value=Delete]")
  end

  it "should allow to pass an explicit url as a string" do
    result = @controller.render :delete_with_explicit_url # <%= delete_button('/test/custom_url') %>
    result.should have_selector("form[action='/test/custom_url'][method=post]")
  end

end

describe "escaping values" do

  before :each do
    @c = Hacker.new(Merb::Request.new({}))
    @c.instance_variable_set(:@obj, HackerModel.new)
  end

  it "should escape bound text field values" do
    r = @c.render :text_field
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

  it "should escape bound hidden field values" do
    r = @c.render :hidden_field
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

  it "should escape bound password field values" do
    r = @c.render :password_field
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

  it "should escape bound text area values" do
    r = @c.render :text_area
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

  it "should escape bound file field values" do
    r = @c.render :file_field
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

  it "should escape bound option tag values" do
    r = @c.render :option_tag
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

  it "should escape bound radio button values" do
    r = @c.render :radio_button
    r.should =~ /&amp;&quot;&lt;&gt;/
  end

end