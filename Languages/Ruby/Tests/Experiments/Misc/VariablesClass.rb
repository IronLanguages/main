class C
  def run() 
    puts
    (0..255).each do |x|
      begin
        z = eval("@#{x.chr} = 1")
        puts "@#{x.chr}: OK";
      rescue SyntaxError:
      end
    end

    puts
    (0..255).each do |x|
      begin
        z = eval("@@#{x.chr} = 1")
        puts "@@#{x.chr}: OK";
      rescue SyntaxError:
      end
    end
  end
end

C.new.run()

