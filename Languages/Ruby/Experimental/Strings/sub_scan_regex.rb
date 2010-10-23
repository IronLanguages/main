def m
	$p = lambda do
	  p $1
	end
	
	"ab".sub(/(.)/) do
	  $p[]
	  p $1
	  "x".match(/(.)/)  
	  p $1
	end
	
	p $1
	
	"12".scan(/(.)/) do
	  "y".match(/(.)/)  	  
	end
	p $1
	
	"12".scan(/(.)/) do
	  "y".match(/(.)/)  	  
	  break
	end
	p $1
end

m