require File.dirname(__FILE__) + '/../spec_helper'

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
  
describe "The -S command line option" do
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
