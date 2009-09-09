def script_tag(index)
  yield $script_tags_run[index]
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

def get_script(id)
  System::Windows::Browser::HtmlPage.document.get_element_by_id(id).get_property('innerHTML').to_s
end

describe "DLR-based script tags" do

  it 'runs inline application/ruby tags' do
    script_tag('inline-ruby') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
    end
  end

  it 'runs external script-tags with defer=false' do
    script_tag('ext-nodefer') do |obj|
      obj.should have_run
      obj.should.not.be inline
      obj.should.not.be deferred
    end
  end

  it 'runs inline application/x-ruby tags' do
    script_tag('inline-xruby') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
    end
  end

  it 'runs inline text/ruby tags' do
    script_tag('inline-text-ruby') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.not.be deferred
    end
  end
  
  it 'can require external script files' do
    script_tag('ext-defer') do |obj|
      obj.should have_run
      obj.should.not.be inline
      obj.should.be deferred
    end
  end

  it 'run inline script-tags with defer=true on-demand, not at startup' do
    script_tag('inline-ruby-deferred') do |obj|
      obj.should.not have_run
    end
    eval get_script('deferredInline')
    script_tag('inline-ruby-deferred') do |obj|
      obj.should have_run
      obj.should.be inline
      obj.should.be deferred
    end
  end
end
