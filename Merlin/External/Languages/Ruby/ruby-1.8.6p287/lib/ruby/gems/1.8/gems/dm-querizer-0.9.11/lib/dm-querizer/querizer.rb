module DataMapper
  class Querizer
    self.instance_methods.each { |m| send(:undef_method, m) unless m =~ /^(__|instance_eval)/ }

    { :eql  => '==',
      :like => '=~',
      :gte  => '>=',
      :lte  => '<=',
      :gt   => '>',
      :lt   => '<',
      :not  => '~'
    }.each do |dm, real|
      class_eval <<-EOS, __FILE__, __LINE__
        def #{real}(val)
          @conditions << condition(val, :#{dm})
        end
      EOS
    end

    def condition(value,opr)
      condition = @stack.length > 1 ? eval(@stack * '.') : @stack.pop
      condition = condition.send(opr) if condition.respond_to?(opr) && opr != :eql
      @stack.clear
      [condition,value]
    end

    def self.translate(&block)
      (@instance||=self.new).translate(&block)
    end

    def translate(&block)
      @query, @stack, @conditions = {}, [], []
      self.instance_eval(&block)
      @conditions.each {|c| @query[c[0]] = c[1]}
      return @query
    end

    def method_missing(method,value=nil)
      @stack << method
      self
    end
  end
end
