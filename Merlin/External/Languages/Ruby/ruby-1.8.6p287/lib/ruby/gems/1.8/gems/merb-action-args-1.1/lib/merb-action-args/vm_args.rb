begin
  require "methopara"
rescue
  puts "make sure you have methora http://github.com/genki/methopara installed if you want to use action args on Ruby 1.9"
end

module GetArgs
  def get_args
    unless respond_to?(:parameters)
      raise NotImplementedError, "Ruby #{RUBY_VERSION} doesn't support #{self.class}#parameters"
    end

    required = []
    optional = []

    parameters.each do |(type, name)|
      if type == :opt
        required << [name, nil]
        optional << name
      else
        required << [name]
      end
    end

    return [required, optional]
  end
end
