require 'test/spec'

context "Empty context" do
  # should.not.raise
end

context "Outer context" do
  context "Inner context" do
    specify "is nested" do
    end
    specify "has multiple empty specifications" do
    end
  end
  context "Second Inner context" do
    context "Inmost context" do
      specify "works too!" do
      end
      specify "whoo!" do
      end
    end
    specify "is indented properly" do
    end
    specify "still runs in order of definition" do
    end
  end
end
