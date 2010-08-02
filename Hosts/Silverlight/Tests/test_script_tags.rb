def script_tag(index)
  yield $script_tags_run[index] if block_given?
  $script_tags_run[index]
end

def have_run
  lambda { |obj| !obj.nil? }
end

def inline
  lambda { |obj| obj.has_key?(:inline) && obj[:inline] }
end

def deferred
  lambda { |obj| obj.has_key?(:defer) && obj[:defer] }
end

def in_order
  lambda { |obj| obj == obj.sort }
end

def typed
  lambda { |obj| obj.has_key?(:typed) && obj[:typed] }
end

def get_script(id)
  System::Windows::Browser::
    HtmlPage.document.get_element_by_id(id).get_property('innerHTML').to_s
end

describe "DLR-based script tags: end-to-end" do

  it 'runs inline application/ruby tags' do
    script_tag('inline-ruby') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
      obj.should.be typed
    end
  end

  it 'runs external script-tags' do
    script_tag('ext-nodefer') do |obj|
      obj.should have_run
      obj.should.not.be inline
      obj.should.not.be deferred
      obj.should.be typed
    end
  end

  it 'runs external script-tags without type' do
    script_tag('ext-notype') do |obj|
      obj.should have_run
      obj.should.not.be inline
      obj.should.not.be deferred
      obj.should.not.be typed
    end
  end

  it 'runs inline application/x-ruby tags' do
    script_tag('inline-xruby') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
      obj.should.be typed
    end
  end

  it 'runs inline text/ruby tags' do
    script_tag('inline-text-ruby') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
      obj.should.be typed
    end
  end

  it 'can require external script files with defer=true' do
    script_tag('ext-defer') do |obj|
      obj.should have_run
      obj.should.not.be inline
      obj.should.be deferred
    end
  end

  it 'run inline script-tags with defer=true on-demand, not at startup' do
    script_tag('inline-ruby-deferred').should.not have_run
    eval get_script('deferredInline')
    script_tag('inline-ruby-deferred') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.be deferred
    end
  end

  it 'can call methods defined in other script-tags' do
    script_tag('method-call-across-tags').should have_run
  end

  it 'will not run script tags with a random prefix' do
    script_tag('random-prefix').should.not have_run
  end

  it 'runs scripts in order' do
    script_tag('in-order-execution').should.be in_order
  end

  it 'runs python tags' do
    script_tag('python') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
      obj.should.be typed
    end
  end
end

dst = Microsoft::Scripting::Silverlight::DynamicScriptTags

describe 'DynamicScriptTags.RemoveMargin' do

  it 'should not remove any spaces' do
    test = "a = 1\nb = 2\n"
    dst.RemoveMargin(test).should == test
  end

  it 'does not remove beginning blank lines' do
    test =   "\n\n\n\n\n\na = 1\nb = 2\n"
    result = "\n\n\n\n\n\na = 1\nb = 2\n"
    dst.RemoveMargin(test).should == result
  end

  it 'should always use \'\\n\' to separate lines' do
    test =   "\na = 1\r\nb = 1\r\n"
    result = "\na = 1\nb = 1\n"
    dst.RemoveMargin(test).should == result
  end
  
  it 'should remove the margin equally' do
    test =   "\n    a = 1\n    def foo()\n        print 'hi'\n    foo()\n"
    result = "\na = 1\ndef foo()\n    print 'hi'\nfoo()\n"
    dst.RemoveMargin(test).should == result
  end

  it 'should bail on indenting lines which have less margin than the first line' do
    test =   "\n        a = 1\n        b = 1\ndef foo\n  puts 'hi'\n"
    result = "\n        a = 1\n        b = 1\ndef foo\n  puts 'hi'\n"
    dst.RemoveMargin(test).should == result
  end

  it 'should indent lines at a minimum indent' do
    test =   "\n        a = 1\n        b = 1\n   def foo\n     puts 'hi'\n"
    result = "\n     a = 1\n     b = 1\ndef foo\n  puts 'hi'\n"
    dst.RemoveMargin(test).should == result
  end

  it 'should remove any spaces from a last-spaces-only line' do
    test =   "\n    a = 1\n    b = 1\n    def foo\n      puts 'hi'\n  "
    result = "\na = 1\nb = 1\ndef foo\n  puts 'hi'\n"
    dst.RemoveMargin(test).should == result
  end

  it 'should not take blank lines into account in margin' do
    test =   "\n    a = 1\n\n    b = 1\n"
    result = "\na = 1\n\nb = 1\n"
    dst.RemoveMargin(test).should == result
  end

end
