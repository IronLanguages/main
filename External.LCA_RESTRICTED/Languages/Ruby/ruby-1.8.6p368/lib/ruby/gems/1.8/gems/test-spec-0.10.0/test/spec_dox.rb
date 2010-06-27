require 'test/spec'

require 'test/spec/dox'

context "SpecDox" do
  setup do
    r = Test::Unit::UI::SpecDox::TestRunner.new(nil)
    @unmangler = r.method(:unmangle)
  end
  
  specify "can unmangle Test::Unit names correctly" do
    @unmangler["test_foo_bar(TestFoo)"].should.equal ["Foo", "foo bar"]
    @unmangler["test_foo_bar(FooTest)"].should.equal ["Foo", "foo bar"]
    @unmangler["test_he_he(Foo)"].should.equal ["Foo", "he he"]
    @unmangler["test_heh(Foo)"].should.equal ["Foo", "heh"]

    @unmangler["test_heh(Test::Unit::TC_Assertions)"].
      should.equal ["Test::Unit::TC_Assertions", "heh"]

    @unmangler["test_heh(Foo::Bar::Test)"].
      should.equal ["Foo::Bar::Test", "heh"]
  end

  specify "can unmangle Test::Spec names correctly" do
    @unmangler["test_spec {context} 007 [whee]()"].
      should.equal ["context", "whee"]
    @unmangler["test_spec {a bit longish context} 069 [and more text]()"].
      should.equal ["a bit longish context", "and more text"]
    @unmangler["test_spec {special chars !\"/&%$} 2 [special chars !\"/&%$]()"].
      should.equal ["special chars !\"/&%$", "special chars !\"/&%$"]
    @unmangler["test_spec {[]} 666666 [{}]()"].
      should.equal ["[]", "{}"]
  end

  specify "has sensible fallbacks" do
    @unmangler["weird"].should.equal [nil, nil]
  end
end
  
