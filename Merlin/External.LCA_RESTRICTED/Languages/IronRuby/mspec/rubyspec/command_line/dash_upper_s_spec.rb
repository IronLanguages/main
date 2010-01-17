require File.dirname(__FILE__) + '/../spec_helper'

describe "The -S command line option" do
  it 'finds the file on the path' do
    this_dir = File.expand_path(File.dirname(__FILE__))
    fixture_dir = File.expand_path(this_dir + '/../fixtures')

    ENV['PATH'] = "#{fixture_dir};#{ENV['PATH']}"
    dash_s = ruby_exe(nil, :options => '-S file.rb')

    ENV['PATH'] = ENV['PATH'].split(';')[1..-1].join(';')
    expected = ruby_exe("#{fixture_dir}/file.rb")
    failed_dash_s = ruby_exe(nil, :args => "-S #{this_dir}/file.rb 2>&1")

    dash_s.should == expected
    dash_s.should_not == failed_dash_s
  end

  it 'does not find the file on the path' do
    dash_s = ruby_exe(nil, :args => '-S does/not/exist.rb 2>&1')
    normal = ruby_exe(nil, :args => 'does/not/exist.rb 2>&1')
    dash_s.should == normal
  end
end
