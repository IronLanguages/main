module N
  def i_n
    puts 'i_m'
  end

  def self.c_n
    puts 'c_n'
  end

  puts '- 0 -'
    
  (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
  (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
  (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
  (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
  puts
end

module M
  def i_m
    puts 'i_m'
  end

  def self.c_m
    puts 'c_m'
  end
  
  puts '- 1 -'
    
  (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
  (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
  (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
  (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
    
  puts 


  N.class_eval {
    puts '- 2 -'
    
    (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
    (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
    (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
    (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
    puts 
  }

  def N.test3
    puts '- 3 -'
        
    (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
    (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
    (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
    (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
    puts
  end
  
  N.class_eval {
    def N.test4
	
	  puts '- 4 -'
		
	  (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
	  (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
	  (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
	  (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
	  puts 	
    end 
  }

  def N.test5  
    N.class_eval {
	
	  puts '- 5 -'
		
	  (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
	  (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
	  (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
	  (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
	  puts 	
    }
  end 

  def N.test6
    $p = proc {
	
	  puts '- 6 -'
		
	  (alias_method :xxx, :i_m; puts 'i_m OK') rescue p $!
	  (alias_method :xxx, :c_m; puts 'c_m OK') rescue p $!
	  (alias_method :xxx, :i_n; puts 'i_n OK') rescue p $!
	  (alias_method :xxx, :c_n; puts 'c_n OK') rescue p $!
	  puts 	
    }
    
    1.times &$p
  end 
end

class D
  def i_d
    puts 'i_d'
  end

  def self.c_d
    puts 'c_d'
  end
end

class C
  def i_c
    puts 'i_c'
  end

  def self.c_c
    puts 'c_c'
  end
 
  D.class_eval {
	  puts '- !7 -'
	  
	  (alias_method :xxx, :i_c; puts 'i_c OK') rescue p $!
	  (alias_method :xxx, :c_c; puts 'c_c OK') rescue p $!
	  (alias_method :xxx, :i_d; puts 'i_d OK') rescue p $!
	  (alias_method :xxx, :c_d; puts 'c_d OK') rescue p $!
	  puts 	
  } 
  
  D.send(:define_method, :test7) {
	  puts '- 8 -'
	  
	  (alias_method :xxx, :i_c; puts 'i_c OK') rescue p $!
	  (alias_method :xxx, :c_c; puts 'c_c OK') rescue p $!
	  (alias_method :xxx, :i_d; puts 'i_d OK') rescue p $!
	  (alias_method :xxx, :c_d; puts 'c_d OK') rescue p $!
	  puts 	
  } 
  
end

N.test3
N.test4
N.test5
N.test6
D.new.test7





