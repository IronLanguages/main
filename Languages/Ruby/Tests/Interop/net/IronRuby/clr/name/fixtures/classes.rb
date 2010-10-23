module IronRubySpec
  def two_letter_words
    %w{at as by do id it if in is go me my of ok on to up}
  end

  def not_unmangled
    %w{class
    clone
    display
    dup
    extend
    freeze
    hash
    initialize
    inspect
    instance_eval
    instance_exec
    instance_variable_get
    instance_variable_set
    instance_variables
    method
    methods
    object_id
    private_methods
    send
    singleton_methods
    taint
    untaint
    }
  end

  def not_mangled
    %w{Class
    Clone
    Display
    Dup
    Extend
    Freeze
    Hash
    Initialize
    Inspect
    InstanceEval
    InstanceExec
    InstanceVariableGet
    InstanceVariableSet
    InstanceVariables
    Method
    Methods
    ObjectId
    PrivateMethods
    ProtectedMethods
    PublicMethods
    Send
    SingletonMethods
    Taint
    Untaint}
  end
end
