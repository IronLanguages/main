$: << File.dirname(__FILE__) + '/../lib/'
require 'test/spec'
require 'test/spec/should-output'

context "should.output" do
  specify "works for print" do
    lambda { print "foo" }.should.output "foo"
    lambda { print "foo" }.should.output(/oo/)
  end

  specify "works for puts" do
    lambda { puts "foo" }.should.output "foo\n"
    lambda { puts "foo" }.should.output(/foo/)
  end

  specify "works with readline" do
    lambda { require 'readline' }.should.not.raise(LoadError)
    lambda { puts "foo" }.should.output "foo\n"
    lambda { puts "foo" }.should.output(/foo/)

    File.should.not.exist(File.join(Dir.tmpdir, "should_output_#{$$}"))
  end
end

  
  
