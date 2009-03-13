require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

describe "DataMapper::DependencyQueue" do
  before :each do
    @q = DataMapper::DependencyQueue.new
    @dependencies = @q.instance_variable_get("@dependencies")
  end

  describe "#add" do
    it "should store the supplied callback in @dependencies" do
      @q.add('MissingConstant') { true }
      @dependencies['MissingConstant'].first.call.should == true
    end
  end

  describe "#resolve!" do
    describe "(when dependency is not defined)" do
      it "should not alter @dependencies" do
        @q.add('MissingConstant') { true }
        old_dependencies = @dependencies.dup
        @q.resolve!
        old_dependencies.should == @dependencies
      end
    end

    describe "(when dependency is defined)" do
      before :each do
        @q.add('MissingConstant') { |klass| klass.instance_variable_set("@resolved", true) } # add before MissingConstant is loaded

        class ::MissingConstant
        end
      end

      it "should execute stored callbacks" do
        @q.resolve!
        MissingConstant.instance_variable_get("@resolved").should == true
      end

      it "should clear @dependencies" do
        @q.resolve!
        @dependencies['MissingConstant'].should be_empty
      end
    end
  end

end
