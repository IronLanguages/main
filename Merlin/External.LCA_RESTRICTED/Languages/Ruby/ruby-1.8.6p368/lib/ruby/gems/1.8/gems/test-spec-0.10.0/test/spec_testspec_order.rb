require 'test/spec'

$foo = 0

context "Context First" do
  specify "runs before Second" do
    $foo.should.equal 0
    $foo += 1
  end
end

context "Context Second" do
  specify "runs before Last" do
    $foo.should.equal 1
    $foo += 1
  end
end

context "Context Last" do
  specify "runs last" do
    $foo.should.equal 2
    $foo += 1
  end
end

  
