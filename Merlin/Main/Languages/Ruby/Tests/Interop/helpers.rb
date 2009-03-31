class Object
  def metaclass
    class << self; self; end
  end

  def metaclass_eval(&block)
    metaclass.class_eval(&block)
  end

  def metaclass_alias(new,old)
    metaclass_eval { alias_method new, old}
  end

  def metaclass_temp_alias(new, old)
    metaclass_alias new, old
    res = nil
    if block_given?
      res = yield
      metaclass_alias old, new
    end
    res
  end

  def metaclass_def(meth_name)
    metaclass_eval do 
      define_method(meth_name) { yield }
    end
  end
end
