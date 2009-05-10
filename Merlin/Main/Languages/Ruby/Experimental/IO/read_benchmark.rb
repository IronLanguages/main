require 'benchmark'

File.delete("big.txt") rescue 0
File.delete("big_lines.txt") rescue 0

Benchmark.bm do |x| 
  puts "write big.txt"    
  x.report {     
    File.open("big.txt", "wb") { |f|
      10_000_000.times { f.write('x') }
    }
  } 

  puts "write big_lines.txt"
  x.report { 
    File.open("big_lines.txt", "wb") { |f|
      2.times {
        1_000.times { f.write('x' * 1000 + "\r\n") }
        1_000.times { f.write('x' * 1000 + "\r") }
        1_000.times { f.write('x' * 1000 + "\r\n") }
        1_000.times { f.write('x' * 1000 + "\n") }
        1_000.times { f.write('x' * 1000 + "\r\n") }
      }
    }
  } 

  ["big.txt", "big_lines.txt"].each do |file|
      puts "read #{file}"
            
	  [    
		[100,          1, 10_000_000],
		[100,         10,  1_000_000],
		[100,      1_000,     10_000],
		[100,     10_000,      1_000],
		[ 10,    100_000,        100],
		[ 10,  1_000_000,         10],
		[  1, 10_000_000,          1],
	  ].each { |n, m, s|	     
		  x.report("#{n} x #{m} x #{s}") { 
			File.open(file, "r") { |f|
			  n.times {
				f.seek(0); 
				m.times { 
				  f.read(s) 
				}
			  }
			}
		  }
	  }
  
  end
end


