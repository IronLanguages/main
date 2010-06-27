# == About camping/webrick.rb
#
# For many who have Ruby installed, Camping and WEBrick is a great option.
# It's definitely the easiest configuration, however some performance is sacrificed.
# For better speed, check out Mongrel at http://mongrel.rubyforge.org/, which comes
# with Camping hooks and is supported by the Camping Tool.
require 'camping'
require 'webrick/httpservlet/abstract.rb'

module WEBrick
# WEBrick::CampingHandler is a very simple handle for hosting Camping apps in
# a WEBrick server.  It's used much like any other WEBrick handler.
# 
# == Mounting a Camping App
#
# Assuming Camping.goes(:Blog), the Blog application can be mounted alongside
# other WEBrick mounts.
#
#   s = WEBrick::HTTPServer.new(:BindAddress => host, :Port => port)
#   s.mount "/blog", WEBrick::CampingHandler, Blog
#   s.mount_proc("/") { ... }
#
# == How Does it Compare?
#
# Compared to other handlers, WEBrick is well-equipped in terms of features.
# 
# * The <tt>X-Sendfile</tt> header is supported, along with etags and 
#   modification time headers for the file served.  Since this handler
#   is a subclass of WEBrick::HTTPServlet::DefaultFileHandler, all of its
#   logic is used.
# * IO is streaming up and down.  When you upload a file, it is streamed to
#   the server's filesystem.  When you download a file, it is streamed to
#   your browser.
#
# While WEBrick is a bit slower than Mongrel and FastCGI options, it's 
# a decent choice, for sure!
class CampingHandler < WEBrick::HTTPServlet::DefaultFileHandler
    # Creates a CampingHandler, which answers for the application within +klass+.
    def initialize(server, klass)
        super(server, klass)
        @klass = klass
    end
    # Handler for WEBrick requests (also aliased as do_POST).
    def service(req, resp)
        controller = @klass.run((req.body and StringIO.new(req.body)), req.meta_vars)
        resp.status = controller.status
        @local_path = nil
        controller.headers.each do |k, v|
            if k =~ /^X-SENDFILE$/i
                @local_path = v
            else
                [*v].each do |vi|
                    resp[k] = vi
                end
            end
        end

        if @local_path
            do_GET(req, resp)
        else
            resp.body = controller.body.to_s
        end
    end
end
end
