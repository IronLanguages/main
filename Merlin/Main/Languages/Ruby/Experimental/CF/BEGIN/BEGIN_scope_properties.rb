def f1
end

BEGIN {
  def f2
  end
}

class M
  private
  while true
    eval('
    BEGIN {
      def f3
      end
      
      private
      
      break
      puts "unreachable"
    }    
    ')
  end
  
  def f4
  end
end

p M.public_instance_methods(false)
p M.private_instance_methods(false)

p self.private_methods(false)
p self.public_methods(false)


