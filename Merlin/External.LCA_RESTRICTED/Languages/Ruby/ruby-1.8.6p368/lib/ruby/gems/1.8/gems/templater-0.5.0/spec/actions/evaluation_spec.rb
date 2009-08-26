require File.dirname(__FILE__) + '/../spec_helper'

describe Templater::Actions::Evaluation do
  before do
    @generator = mock('a generator')
    @generator.stub!(:source_root).and_return('/tmp/source')
    @generator.stub!(:destination_root).and_return('/tmp/destination')
  end

  describe '#render' do
    it "returns result of block evaluation" do
      evaluation = Templater::Actions::Evaluation.new(@generator, :monkey) do
        "noop"
      end
      evaluation.render.should == "noop"
    end

    it "returns empty string when block returned nil" do
      evaluation = Templater::Actions::Evaluation.new(@generator, :monkey) do
        nil
      end
      evaluation.render.should == ""
    end
  end

  describe "#identical?" do
    it "always returns false" do
      noop_evaluation = Templater::Actions::Evaluation.new(@generator, :monkey) do
        "noop"
      end

      another_evaluation = Templater::Actions::Evaluation.new(@generator, :monkey) do
        "noop"
      end      

      noop_evaluation.should_not be_identical
      another_evaluation.should_not be_identical
    end
  end
end
