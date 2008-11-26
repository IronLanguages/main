require 'Builtins.rb'
dump(Kernel)
dump(Object)
dump(Module)
dump(Class)
dump(Array)
dump(Numeric)
dump(Integer)
dump(Fixnum)
dump(Bignum)
dump(Enumerable)
dump(String)
dump(Math)
dump(Range)
dump(Regexp)
dump(Hash)

dump(NilClass)
dump(TrueClass)
dump(FalseClass)
dump(Symbol)

dump(Exception)

dump(NoMemoryError);
dump(ScriptError);
dump(LoadError);
dump(NotImplementedError);
dump(SyntaxError);
dump(SignalException);
dump(Interrupt);
dump(StandardError);
dump(ArgumentError);
dump(IOError);
dump(EOFError);
dump(IndexError);
dump(LocalJumpError);
dump(NameError);
dump(NoMethodError);
dump(RangeError);
dump(FloatDomainError);
dump(RegexpError);
dump(RuntimeError);
dump(SecurityError);
dump(SystemCallError);
dump(ThreadError);
dump(TypeError);
dump(ZeroDivisionError);
dump(SystemExit);
dump(SystemStackError);

dump(Proc);
dump(Method);
dump(UnboundMethod);
dump(Thread);
dump(IO);
dump(File);

class C
  def instC()
  end
  
  def C.clsC()
  end
end

class D < C
  def instD()
  end
  
  def C.clsCD()
  end
  
  def D.clsD()
  end
end

module M
  private
  
  def M.private_c
  end
  
  def private_i
  end
  
  public
  
  def M.public_c
  end
  
  def public_i
  end
  
  protected
  
  def M.protected_c
  end
  
  def protected_i
  end
end

dump(C)
dump(D)
dump(M)
