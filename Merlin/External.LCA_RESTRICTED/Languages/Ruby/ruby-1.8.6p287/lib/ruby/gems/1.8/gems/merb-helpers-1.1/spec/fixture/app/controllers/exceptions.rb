class Exceptions < Merb::Controller

  attr_reader :handler 
  
  # handle NotFound exceptions (404)
  
   def not_found
     @handler = :not_found
     render :format => :html
   end
 
   # handle NotAcceptable exceptions (406)
   
   def not_acceptable
     @handler = :not_acceptable
     render "Handled by: not_acceptable"
   end
 
  # # Any client error (400 series)
  def client_error
    @handler = :client_error
    render "Handled by: client_error"
  end

end
