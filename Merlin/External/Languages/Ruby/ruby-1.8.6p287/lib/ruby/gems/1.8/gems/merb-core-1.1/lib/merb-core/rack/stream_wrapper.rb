module Merb
  module Rack

    class StreamWrapper
      # :api: private
      def initialize(body)
         @body = body
      end
      
      # :api: private
      def each(&callback)
        if Proc === @body
          @writer = lambda { |x| callback.call(x) }
          @body.call(self)
        elsif @body.is_a?(String)
          @body.each_line(&callback)
        else
          @body.each(&callback)
        end
      end
      
      # :api: private
      def write(str)
        @writer.call str.to_s
        str
      end
      
      # :api: private
      def to_s
        @body.to_s
      end
            
      # :api: private
      def ==(other)
        @body == other
      end

      # :api: private
      def method_missing(sym, *args, &blk)
        @body.send(sym, *args, &blk)
      end
       
    end   
  
  end
end  
