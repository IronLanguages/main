require File.dirname(__FILE__) + "/../spec_helper"
require File.dirname(__FILE__) + "/fixtures/blocks"
describe "Blocks" do
  before :each do
    ScratchPad.clear
    ScratchPad.record []
    @empty = lambda {}
  end

  describe "block_given behavior" do
    describe '(method)' do
      it 'is false with no passed in block' do
        BlockSpecs.method.should be_false
      end

      it "is true with a bare proc" do
        (BlockSpecs.method {}).should be_true
      end

      it "is true with a to_proc" do
        (BlockSpecs.method(&@empty)).should be_true
      end

      it "raises an error with a proc object instead of a proc" do
        lambda { BlockSpecs.method(@empty)}.should raise_error(ArgumentError)
      end
    end

    describe 'method(arg)' do
      it 'is false with no passed in block' do
        BlockSpecs.method_with_1_arg(1).should be_false
      end

      it "is false with a proc as the arg" do
        BlockSpecs.method_with_1_arg(@empty).should be_false
      end

      it "is true with a bare proc" do
        (BlockSpecs.method_with_1_arg(1) {}).should be_true
      end

      it "is true with a to_proc" do
        (BlockSpecs.method_with_1_arg(1, &@empty)).should be_true
      end

      it "raises an error with a proc object instead of a proc" do
        lambda { BlockSpecs.method_with_1_arg(1, @empty)}.should raise_error(ArgumentError)
      end
    end

    describe "method(&arg)" do
      it "is false when argument is empty" do
        BlockSpecs.method_with_explicit_block.should == []
      end

      it "raises an error when the arg is not a proc object" do
        lambda { BlockSpecs.method_with_explicit_block(1) }.should raise_error(ArgumentError)
      end

      it "is true when passed a block directly" do
        (BlockSpecs.method_with_explicit_block {}).should == [:p,:block]
      end

      it "is true when passed a block via to_proc" do
        (BlockSpecs.method_with_explicit_block(&@empty)).should == [:p,:block]
      end

      it "raises an error when passed a proc object instead of a proc" do
        lambda { BlockSpecs.method_with_explicit_block(@empty) }.should raise_error(ArgumentError)
      end
    end
  end

  describe "break" do
    it "stops flow and returns nil" do
      BlockSpecs.should break_from(:take_block)
    end

    it "stops flow and returns its argument" do
      BlockSpecs.should break_from(:take_block).with(1)
    end

    it "stops flow in nested block" do
      BlockSpecs.should break_from(:call_method_which_take_block).with(2)
    end

    it "stops flow in a nested loop" do
      BlockSpecs.should break_from(:take_block_in_loop).with(1).prepending(:before_yield)
    end
    
    it "should raise error from lambda" do
      pr = lambda { break 8; flunk "Should not have gotten here"}
      lambda {BlockSpecs.take_block &pr}.should raise_error(LocalJumpError)
      ScratchPad.recorded.should == []
    end
    
    it "should not raise error from lambda when calling" do
      lambda { break 8; flunk "Should not have gotten here"}.call.should == 8
    end
    
    it "should raise error from Proc.new" do
      pr = Proc.new { break 8; flunk "Should not have gotten here"}
      lambda {BlockSpecs.take_block &pr}.should raise_error(LocalJumpError)
      ScratchPad.recorded.should == []
    end
    it "should raise error from Proc.new when calling" do
      pr = Proc.new { break 8; flunk "Should not have gotten here"}
      lambda {pr.call}.should raise_error(LocalJumpError)
    end
  end
  describe "closures" do
    it "uses local variables and doesn't cache" do
      local_var = 10
      pr = lambda {1 + local_var}
      BlockSpecs.take_block(&pr).should == 11
      BlockSpecs.take_block {2+local_var}.should == 12
      local_var = 100
      BlockSpecs.take_block(&pr).should == 101
      BlockSpecs.take_block {2+local_var}.should == 102
    end
    
    it "uses global variables and doesn't cache" do
      $global_var = 10
      pr = lambda {1 + $global_var}
      BlockSpecs.take_block(&pr).should == 11
      BlockSpecs.take_block {2+$global_var}.should == 12
      $global_var = 100
      BlockSpecs.take_block(&pr).should == 101
      BlockSpecs.take_block {2+$global_var}.should == 102
    end

    it "uses arg from method returning block" do
      BlockSpecs.take_block(&BlockSpecs.meth_with_one_arg(10)).should == 11
      BlockSpecs.take_block(&BlockSpecs.meth_with_one_arg(100)).should == 101
    end

    it "uses multiple closed over args" do
      $global_var = 1
      BlockSpecs.meth_with_two_args(10, 100).call.should == 118
      $global_var = 2
      BlockSpecs.take_block(&BlockSpecs.meth_with_two_args(20,200)).should == 229
    end

    it "can modify local variables" do
      local_var = 1
      p = lambda {|x| local_var += x }
      p.call(10)
      local_var.should == 11
      p.call(100)
      local_var.should == 111
    end

    it "can modify global variables" do
      $global_var = 1
      p = lambda {|x| $global_var += x }
      p.call(10)
      $global_var.should == 11
      p.call(100)
      $global_var.should == 111
    end
  end

  describe "invoking" do
    it "works with 0 args" do
      b = lambda {1}
      b.call.should == 1
      b.call(1,2,4).should == 1
      b = lambda { raise "foo"  }
      lambda { b.call  }.should raise_error(RuntimeError)
    end

    it "works with 1 arg" do
      b = lambda {|x| x}
      b.call(9).should == 9
      lambda {b.call(3,4).should == [3,4]}.should complain
    end
  end

  describe "next" do
    it "works with no args" do
      BlockSpecs.should handle_next_for(:take_block).appending(:after_yield)
    end
    
    it "works with 1 args" do
      BlockSpecs.should handle_next_for(:take_block).with(1).appending(:after_yield).resulting_in(1)
    end

    it "works with nested calls" do
      BlockSpecs.should handle_next_for(:call_method_which_take_block).
        with(2).
        appending(:after_yield).
        appending(:after_call).
        resulting_in(2)
    end

    it "works with loops" do
      BlockSpecs.should handle_next_for(:take_block_in_loop).
        with(1).
        prepending(:before_yield).
        appending(:after_yield).
        appending(:before_yield).
        appending(:before_next).
        appending(:after_yield).
        appending(:before_yield).
        appending(:before_next).
        appending(:after_yield).
        resulting_in(1)
    end
  end

  describe "redo" do
    it "redoes the block in a simple case" do
      BlockSpecs.should handle_redo_for(:take_block).
        with(5).
        appending(:after_yield)
    end

    it "redoes the block in a loop" do
      BlockSpecs.should handle_redo_for(:take_block_in_loop).
        with(5).
        prepending(:before_yield).
        appending(:after_yield).
        appending(:before_yield).
        appending(:before_redo).
        appending(:after_redo).
        appending(:after_yield).
        appending(:before_yield).
        appending(:before_redo).
        appending(:after_redo).
        appending(:after_yield).
        resulting_in(7)
    end
    
    it "works with nested calls" do
      BlockSpecs.should handle_redo_for(:call_method_which_take_block).
        with(2).
        appending(:after_yield).
        appending(:after_call)
    end

    it "doesn't cause re-evaluation of method args" do
      BlockSpecs.should handle_redo_for(:take_arg_and_block).
        arg(:arg).
        with(5).
        prepending(:arg).
        appending(:after_yield)
    end
    
    it "doesn't cause re-evaluation of method args" do
      BlockSpecs.should handle_redo_for(:call_method_which_take_arg_and_block).
        arg(:arg).
        with(5).
        prepending(:arg).
        appending(:after_yield).
        appending(:after_call)
    end
  end
  describe "retry" do
    it "redoes the block in a simple case" do
      BlockSpecs.should handle_retry_for(:take_block).
        with(5).
        appending(:after_yield)
    end

    it "redoes the block in a loop" do
      BlockSpecs.should handle_retry_for(:take_block_in_loop).
        with(5).
        prepending(:before_yield).
        appending(:after_yield).
        appending(:before_yield).
        appending(:before_redo).
        appending(:after_redo).
        appending(:after_yield).
        appending(:before_yield).
        appending(:before_redo).
        appending(:after_redo).
        appending(:after_yield).
        resulting_in(7)
    end
    
    it "works with nested calls" do
      BlockSpecs.should handle_retry_for(:call_method_which_take_block).
        with(2).
        appending(:after_yield).
        appending(:after_call)
    end

    it "doesn't cause re-evaluation of method args" do
      BlockSpecs.should handle_retry_for(:take_arg_and_block).
        arg(:arg).
        with(5).
        prepending(:arg).
        appending(:after_yield)
    end
    
    it "doesn't cause re-evaluation of method args" do
      BlockSpecs.should handle_retry_for(:call_method_which_take_arg_and_block).
        arg(:arg).
        with(5).
        prepending(:arg).
        appending(:after_yield).
        appending(:after_call)
    end
  end
end
