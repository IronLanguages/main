class Object
  def iobject
  end
  
  class << self
    def cobject
    end

    class << self
      def ccobject
      end
    end

  end
end

class Module
  def imodule
  end

  class << self
    def cmodule
    end

    class << self
      def ccmodule
      end
    end
  end
end

class Class
  def iclass
  end

  class << self
    def cclass
    end

    class << self
      def ccclass
      end
    end
  end
end

class C
  def ifoo
  end
  
  class << self
    def cfoo
    end

    class << self
      def ccfoo
      end
    end
  end
end

class D < C
  def ibar
  end

  class << self
    def cbar
    end

    class << self
      def ccbar
      end
    end
  end
end

x = D.new
class << x
  $Sx = self
  class << self
    $SSx = self
    class << self
      $SSSx = self
    end
  end
end

p $Sx.instance_methods(false).sort
p $SSx.instance_methods(false).sort
p $SSSx.instance_methods(false).sort

p C.instance_methods(false).sort
p D.instance_methods(false).sort
