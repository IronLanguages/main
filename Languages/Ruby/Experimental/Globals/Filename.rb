p $<.filename
p $FILENAME

class << $<
  alias old_filename filename
  
  def filename
    "foo"
  end 
end

p $<.filename
p $FILENAME             # not foo -> doesn't call the method via dynamic dispatch

class << $<
  remove_method :filename
end

p $<.filename rescue p $!	
p $FILENAME            

class << $<
  alias filename old_filename
end

p $<.filename
p $FILENAME           

($FILENAME = "goo") rescue p $!