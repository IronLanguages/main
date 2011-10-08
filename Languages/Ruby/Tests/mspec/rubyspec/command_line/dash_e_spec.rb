require File.expand_path('../../spec_helper', __FILE__)

describe "The -e command line option" do
  it "evaluates the given string" do
    [%Q{-e "puts 'hello'"},
     %Q{-e"puts 'hello'"}].each do |cmd|
      ruby_exe(nil, :args => cmd).chomp.should == "hello"
    end
  end

  it "joins multiple strings with newlines" do
    ruby_exe(nil, :args => %Q{-e "puts 'hello" -e "world'" 2>&1}).chomp.should == "hello\nworld"
  end

  it "uses 'main' as self" do
    ruby_exe("puts self").chomp.should == "main"
  end

  it "uses '-e' as file" do
    ruby_exe("puts __FILE__").chomp.should == "-e"
  end

  it "uses '-e' as $0" do
    ruby_exe("puts $0").chomp.should == "-e"
  end

  it "preserves ARGV" do
    ruby_exe(nil, :args => %Q{-eputs(ARGV);puts($0) 1 2 3 4}).chomp.should == "1\n2\n3\n4\n-e"
  end

  it "throws LocalJumpError when return is called" do
    ruby_exe("begin;return;rescue LocalJumpError;puts 'pass';end").chomp.should == "pass"
  end
  
  #needs to test return => LocalJumpError

  ruby_version_is "1.8.7.248" do
    describe "with -n and a Fixnum range" do
      before :each do
        @script = "-ne 'print if %s' #{fixture(__FILE__, "conditional_range.txt")}"
      end

      it "mimics an awk conditional by comparing an inclusive-end range with $." do
        ruby_exe(nil, :args => (@script % "2..3")).should == "2\n3\n"
        ruby_exe(nil, :args => (@script % "2..2")).should == "2\n"
      end

      it "mimics a sed conditional by comparing an exclusive-end range with $." do
        ruby_exe(nil, :args => (@script % "2...3")).should == "2\n3\n"
        ruby_exe(nil, :args => (@script % "2...2")).should == "2\n3\n4\n5\n"
      end
    end
  end
end
