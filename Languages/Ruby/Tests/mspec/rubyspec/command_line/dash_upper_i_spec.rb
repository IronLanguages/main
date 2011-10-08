require File.expand_path('../../spec_helper', __FILE__)

describe "The -I command line option" do
  before :each do
    @script = fixture __FILE__, "loadpath.rb"
  end

  it "adds the path to the load path ($:)" do
    ruby_exe(@script, :options => "-I fixtures").should include("fixtures")
  end

  it "allows different formats" do
    [['-I fixtures',  'fixtures'],
     ['-Ifixtures',   'fixtures'],
     ['-I"fixtures"', 'fixtures'],
     ['-I./fixtures', './fixtures'],
     ['-I.\fixtures', '.\fixtures']].each do |command_line, resulting_path|
      ruby_exe(@script, :options => command_line, :dir => File.dirname(__FILE__)).should =~ /^#{Regexp.escape(resulting_path)}$/
    end
  end

  it "allows quoted paths with spaces" do
    ruby_exe(@script, :options => '-I"foo bar"', :dir => File.dirname(__FILE__)).should =~ /^foo bar$/
  end

  it "concatenates adjacent quoted strings" do
    ruby_exe(@script, :options => '-Ifoo"bar"baz', :dir => File.dirname(__FILE__)).should =~ /^foobarbaz$/
  end

  it "allows multiple paths separated with ;" do
    ruby_exe(@script, :options => '-Ifoo;bar', :dir => File.dirname(__FILE__)).should =~ /^bar$/
  end

  it "treats ; as a separator even within a quoted string" do
    ruby_exe(@script, :options => '-I"foo ; bar"', :dir => File.dirname(__FILE__)).should =~ /^foo $/
  end

  it "concatenates adjacent quoted strings, but separates at ;" do
    ruby_exe(@script, :options => '-Ifoo"bar;baz"', :dir => File.dirname(__FILE__)).should =~ /^foobar$/
  end

  it "allows non-existent paths" do
    ruby_exe(@script, :options => '-Inon-existent', :dir => File.dirname(__FILE__)).should =~ /^non-existent$/
  end

  it "can be set from RUBYOPT environment variable" do
    [['-I fixtures',  'fixtures'],
     ['-Ifixtures',   'fixtures'],
     ['-I"fixtures"', 'fixtures'],
     ['-I./fixtures', './fixtures'],
     ['-I.\fixtures', '.\fixtures']].each do |command_line, resulting_path|
      begin
        ENV['RUBYOPT'] = command_line
        ruby_exe(@script, :options => "", :dir => File.dirname(__FILE__)).should =~ /^#{Regexp.escape(resulting_path)}$/
      ensure
        ENV['RUBYOPT'] = nil
      end    
    end
  end

end
