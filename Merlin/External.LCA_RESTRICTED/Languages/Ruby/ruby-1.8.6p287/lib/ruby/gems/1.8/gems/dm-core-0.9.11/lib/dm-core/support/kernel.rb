module Kernel
  # Delegates to DataMapper::repository.
  # Will not overwrite if a method of the same name is pre-defined.
  def repository(*args)
    if block_given?
      DataMapper.repository(*args) { |*block_args| yield(*block_args) }
    else
      DataMapper.repository(*args)
    end
  end
end # module Kernel
