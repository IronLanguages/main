describe_shared "A new-style shared description" do
  it "should work as well with shared descriptions" do
    true.should.be true
  end
end

describe "A new-style description" do
  before do
    @before = true
    @a = 2
  end

  before(:each) do
    @before_each = true
  end

  before(:all) do
    $before_all = true
  end

  it "should run before-clauses" do
    $before_all.should.be true
    @before.should.be true
    @before_each.should.be true
  end

  it "should behave like context/specify" do
    (1+1).should.equal 2
  end

  xit "this is disabled" do
    bla
  end

  after do
    @a.should.equal 2
    @a = 3
  end

  after(:each) do
    @a.should.equal 3
  end

  after(:all) do
    @b = 1
  end

  after(:all) do
    @b.should.equal 1
  end

  $describescope = self
  it "should raise on unimplement{ed,able} before/after" do
    lambda {
      $describescope.before(:foo) {}
    }.should.raise(ArgumentError)
    lambda {
      $describescope.after(:foo) {}
    }.should.raise(ArgumentError)

    lambda {
      context "foo" do
      end
    }.should.raise(Test::Spec::DefinitionError)
  end

  describe "when nested" do
    it "should work" do
    end
  end

  behaves_like "A new-style shared description"
end

describe "An empty description" do
end

xdescribe "An disabled description" do
  it "should not be run"
end
