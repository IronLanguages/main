module KernelSpecs
  class TestException < StandardError
    def initialize record_exception_method = false, record_set_backtrace = false
      @record_exception_method = record_exception_method
      @record_set_backtrace = record_set_backtrace
    end
    
    def exception arg="default value"
      if @record_exception_method then
        ScratchPad << self
        ScratchPad << :exception_method
        ScratchPad << arg
      end
      
      TestException.new(@record_exception_method, @record_set_backtrace)
    end
    
    def set_backtrace b
      if @record_set_backtrace then
        ScratchPad << self
        ScratchPad << :set_backtrace_method
        ScratchPad << b.class
        ScratchPad << b[0].class
      end
      super(b)
    end
  end
  
end
