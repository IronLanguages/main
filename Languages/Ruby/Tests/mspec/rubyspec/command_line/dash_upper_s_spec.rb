require File.dirname(__FILE__) + '/../spec_helper'

describe "The -S command line option" do
  def push_path(dir)
    ENV['PATH'] = "#{dir};#{ENV['PATH']}"
  end

  def pop_path
    ENV['PATH'] = ENV['PATH'].split(';')[1..-1].join(';')
  end

  def with_path(*dirs)
    dirs.each{|dir| push_path dir}
    yield
    dirs.size.times{ pop_path }
  end

  it 'finds the file on the path' do
    this_dir = File.expand_path(File.dirname(__FILE__))
    fixture_dir = File.expand_path(this_dir + '/../fixtures')
    
    dash_s = nil
    with_path(fixture_dir) do
      dash_s = ruby_exe(nil, :options => '-S file.rb')
    end
  end

  it 'finds the first file on the path' do
    this_dir = File.expand_path(File.dirname(__FILE__))
    expected = [
      File.expand_path(this_dir + '/../fixtures'),
      File.expand_path(this_dir + '/fixtures')
    ]
   
    output = []
    with_path(*expected.reverse) do
      output << ruby_exe(nil, :args => "-S file.rb 2>&1")
    end
    with_path(*expected) do
      output << ruby_exe(nil, :args => "-S file.rb 2>&1")
    end

    output.size.times do |i|
      actual = output[i].split("\n")[0]
      actual.should == File.expand_path(expected[i] + '/file.rb')
    end
  end
end
