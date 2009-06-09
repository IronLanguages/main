class Class
  # Allows the definition of methods on a class that will be available via
  # super.
  # 
  # ==== Examples
  #     class Foo
  #       chainable do
  #         def hello
  #           "hello"
  #         end
  #       end
  #     end
  #
  #     class Foo
  #       def hello
  #         super + " Merb!"
  #       end
  #     end
  #
  # Foo.new.hello #=> "hello Merb!"
  # 
  # ==== Parameters
  # &blk:: 
  #   a block containing method definitions that should be
  #   marked as chainable
  #
  # ==== Returns
  # Module:: The anonymous module that was created
  def chainable(&blk)
    mod = Module.new(&blk)
    include mod
    mod
  end
end