module BlockSpecs
  def self.take_block
    x = yield 
    ScratchPad << :after_yield
    x
  end 

  def self.take_arg_and_block(arg)
    ScratchPad << arg
    x = yield arg
    ScratchPad << :after_yield
    x
  end 

  def self.call_method_which_take_block(&procobj)
    x = take_block(&procobj)
    ScratchPad << :after_call
    x
  end

  def self.call_method_which_take_arg_and_block(arg, &procobj)
    x = take_arg_and_block(arg, &procobj)
    ScratchPad << :after_call
    x
  end

  def self.take_block_in_loop
    x = nil
    3.times do 
      ScratchPad << :before_yield
      x = yield
      ScratchPad << :after_yield
    end
    x
  end 

  def self.take_block_return_block &procobj
    procobj
  end 

  def self.method
    block_given?
  end

  def self.method_with_1_arg arg
    block_given?
  end 

  def self.method_with_explicit_block &procobj
    l = []
    l << :p if procobj
    l << :block if block_given?
    l
  end 
  
  def self.meth_with_one_arg(arg)
    lambda {1+arg}
  end
  
  def self.meth_with_two_args(arg1,arg2)
    l1 = arg1
    l2 = 7
    lambda {$global_var + l1 + l2 + arg2}
  end
end
