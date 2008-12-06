def l &p; p; end

class A
  def defp
    $p = l do
      p self
    end
  end
end

A.new.defp

class C
  define_method :foo, &$p
end

C.new.foo