class Method
  def with_class_and_method_name
    if self.inspect =~ /<Method: (.*)\#(.*)>/ then
      klass = eval $1
      method  = $2.intern
      raise "Couldn't determine class from #{self.inspect}" if klass.nil?
      return yield(klass, method)
    else
      raise "Can't parse signature: #{self.inspect}"
    end
  end

  def to_sexp
    require 'parse_tree'
    require 'unified_ruby'
    parser = ParseTree.new(false)
    unifier = Unifier.new
    with_class_and_method_name do |klass, method|
      old_sexp = parser.parse_tree_for_method(klass, method)
      unifier.process(old_sexp)
    end
  end

  def to_ruby
    sexp = self.to_sexp
    Ruby2Ruby.new.process sexp
  end
end

class ProcStoreTmp
  @@n = 0
  def self.new_name
    @@n += 1
    return :"myproc#{@@n}"
  end
end

class UnboundMethod
  def to_ruby
    name = ProcStoreTmp.new_name
    ProcStoreTmp.send(:define_method, name, self)
    m = ProcStoreTmp.new.method(name)
    result = m.to_ruby.sub(/def #{name}(?:\(([^\)]*)\))?/,
                           'proc { |\1|').sub(/end\Z/, '}')
    return result
  end
end

class Proc
  def to_sexp
    pt = ParseTree.new(false)
    sexp = pt.parse_tree_for_proc(self)
    Unifier.new.process(sexp)
  end

  def to_ruby
    Ruby2Ruby.new.process(self.to_sexp).sub(/^\Aproc do/, 'proc {').sub(/end\Z/, '}')
  end
end
