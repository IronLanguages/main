module Templater
  module Actions
    class Directory < File
      def identical?
        sanity = ::File.directory?(destination) && exists?
        if sanity
          src  = Dir[::File.join(source, "**", "*")]
          src.map! {|f| f.gsub(/#{source}/, "")}
          dest = Dir[::File.join(destination, "**", "*")]
          dest.map!   {|f| f.gsub(/#{destination}/, "")}
          src == dest
        else
          false
        end
      end
    end
  end
end

module Templater
  module Actions
    class File < Action

      # Renders the template and copies it to the destination.
      def invoke!
        callback(:before)
        ::FileUtils.mkdir_p(::File.dirname(destination))
        ::FileUtils.rm_rf(destination)
        ::FileUtils.cp_r(source, destination)
        callback(:after)
      end
    
      # removes the destination file
      def revoke!
        ::FileUtils.rm_r(destination, :force => true)
      end

    end
  end
end
