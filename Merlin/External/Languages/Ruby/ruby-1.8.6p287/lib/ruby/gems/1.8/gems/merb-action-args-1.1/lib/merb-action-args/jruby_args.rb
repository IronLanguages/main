require 'java'
require 'jruby'

module GetArgs
  Methods = org.jruby.internal.runtime.methods

  def get_args
    real_method = JRuby.reference(self)

    # hack to expose a protected field; could be improved in 1.1.5
    method_field = org.jruby.RubyMethod.java_class.declared_field(:method)

    method_field.accessible = true
  
    dyn_method = method_field.value(real_method)

    case dyn_method
    when Methods.MethodArgs
      return build_args(dyn_method.args_node)
    else
      raise "Can't get args from method: #{self}"
    end
  end

  def build_args(args_node)
    args = []
    required = []
    optional = []

    # required args
    if (args_node.args && args_node.args.size > 0)
      required << args_node.args.child_nodes.map { |arg| [arg.name.to_s.intern] }
    end
  
    # optional args
    if (args_node.opt_args && args_node.opt_args.size > 0)
      optional << args_node.opt_args.child_nodes.map do |arg|
        name = arg.name.to_s.intern
        value_node = arg.value_node
        case value_node
        when org.jruby.ast::FixnumNode
          value = value_node.value
        when org.jruby.ast::SymbolNode
          value = value_node.get_symbol(JRuby.runtime)
        when org.jruby.ast::StrNode
          value = value_node.value
        else
          value = nil
        end
        [name, value]
      end
    end

    first_args = required.first
    optional.first.each {|arg| first_args << arg} if optional.first
        
    args = [first_args]
    
    rest = args_node.rest_arg_node
    args << (rest ? rest.name.to_s.intern : nil)
  
    block = args_node.block_arg_node
    args << (block ? block.name.to_s.intern : nil)

    args
  end
end