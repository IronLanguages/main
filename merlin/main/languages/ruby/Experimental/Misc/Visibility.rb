class C
 def instance_test()
   puts 'instance test:'
   
   pub  # pub
   pri  # pri
   pro  # pro

   puts 'instance self test:'
   
   self.pub    # pub
   # self.pri  # error: private called
   self.pro    # pro
 end

 def C.class_test()
   puts 'class test:'
   
   C.new.pub
   # C.new.pri  # error: private called
   # C.new.pro  # error: protected called
 end

 public
  
    def pub()
      puts 'pub'
    end
  
  protected
  
    def pro()
      puts 'pro'
    end
  
  private

    def pri()
      puts 'pri'
    end

end

c = C.new

c.pub    # pub
# c.pri  # error: private called
# c.pro  # error: protected called

c.instance_test
C.class_test