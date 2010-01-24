module NoMethodErrorSpecs
  class NoMethodErrorA; end

  class NoMethodErrorB; end

  class NoMethodErrorC;
    protected
    def a_protected_method;end
    private
    def a_private_method; end
  end

  class NoMethodErrorD; end
end

module ExceptionSpecs
  class Exceptional < Exception; end
  
  class Backtrace
    def self.backtrace
      begin
        raise
      rescue RuntimeError => e
        e.backtrace
      end
    end
  end

  class UnExceptional < Exception
    def backtrace
      nil
    end
    def message
      nil
    end
  end

  class ConstructorException < Exception

    def initialize
    end

  end
  
  class InitializedException < Exception
    def initialize
      ScratchPad.record :initialized_exception
    end
  end
  
  def self.exception_from_ensure_block_with_rescue_clauses
    begin
      raise "some message"
    ensure
      ScratchPad.record $!.backtrace
    end
  end
  
  def self.record_exception_from_ensure_block_with_rescue_clauses
    begin
      exception_from_ensure_block_with_rescue_clauses
    rescue
    end
  end
end
