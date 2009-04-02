module Merb
  # Module that is mixed in to all implemented controllers.
  module ControllerMixin
    
    # Enqueu a block to run in a background thread outside of the request
    # response dispatch
    # 
    # ==== Parameters
    # &blk:: proc to run later
    # 
    # ==== Example
    # run_later do
    #   SomeBackgroundTask.run
    # end
    # 
    # :api: public
    def run_later(&blk)
      Merb.run_later(&blk)
    end
    
    # Renders the block given as a parameter using chunked encoding.
    # 
    # ==== Parameters
    # &blk::
    #   A block that, when called, will use send_chunks to send chunks of data
    #   down to the server. The chunking will terminate once the block returns.
    # 
    # ==== Examples
    #   def stream
    #     prefix = '<p>'
    #     suffix = "</p>\r\n"
    #     render_chunked do
    #       IO.popen("cat /tmp/test.log") do |io|
    #         done = false
    #         until done
    #           sleep 0.3
    #           line = io.gets.chomp
    #           
    #           if line == 'EOF'
    #             done = true
    #           else
    #             send_chunk(prefix + line + suffix)
    #           end
    #         end
    #       end
    #     end
    #   end
    # 
    # :api: public
    def render_chunked(&blk)
      must_support_streaming!
      headers['Transfer-Encoding'] = 'chunked'
      Proc.new { |response|
        @response = response
        response.send_status_no_connection_close('')
        response.send_header
        blk.call
        response.write("0\r\n\r\n")
      }
    end
    
    # Writes a chunk from +render_chunked+ to the response that is sent back to
    # the client. This should only be called within a +render_chunked+ block.
    #
    # ==== Parameters
    # data<String>:: a chunk of data to return.
    # 
    # :api: public
    def send_chunk(data)
      only_runs_on_mongrel!
      @response.write('%x' % data.size + "\r\n")
      @response.write(data + "\r\n")
    end
    
    # ==== Parameters
    # &blk::
    #   A proc that should get called outside the mutex, and which will return
    #   the value to render.
    # 
    # ==== Returns
    # Proc::
    #   A block that the server can call later, allowing Merb to release the
    #   thread lock and render another request.
    # 
    # :api: public
    def render_deferred(&blk)
      Proc.new do |response|
        response.write(blk.call)
      end
    end
    
    # Renders the passed in string, then calls the block outside the mutex and
    # after the string has been returned to the client.
    # 
    # ==== Parameters
    # str<String>:: A +String+ to return to the client.
    # &blk:: A block that should get called once the string has been returned.
    # 
    # ==== Returns
    # Proc::
    #   A block that Mongrel can call after returning the string to the user.
    # 
    # :api: public
    def render_then_call(str, &blk)
      Proc.new do |response|
        response.write(str)
        blk.call
      end
    end
    
    # ==== Parameters
    # url<String>::
    #   URL to redirect to. It can be either a relative or fully-qualified URL.
    # opts<Hash>:: An options hash (see below)
    # 
    # ==== Options (opts)
    # :message<Hash>::
    #   Messages to pass in url query string as value for "_message"
    # :permanent<Boolean>::
    #   When true, return status 301 Moved Permanently
    # 
    # ==== Returns
    # String:: Explanation of redirect.
    # 
    # ==== Examples
    #   redirect("/posts/34")
    #   redirect("/posts/34", :message => { :notice => 'Post updated successfully!' })
    #   redirect("http://www.merbivore.com/")
    #   redirect("http://www.merbivore.com/", :permanent => true)
    # 
    # :api: public
    def redirect(url, opts = {})
      default_redirect_options = { :message => nil, :permanent => false }
      opts = default_redirect_options.merge(opts)
      if opts[:message]
        notice = Merb::Parse.escape([Marshal.dump(opts[:message])].pack("m"))
        url = url =~ /\?/ ? "#{url}&_message=#{notice}" : "#{url}?_message=#{notice}"
      end
      self.status = opts[:permanent] ? 301 : 302
      Merb.logger.info("Redirecting to: #{url} (#{self.status})")
      headers['Location'] = url
      "<html><body>You are being <a href=\"#{url}\">redirected</a>.</body></html>"
    end
    
    # Retreives the redirect message either locally or from the request.
    # 
    # :api: public
    def message
      @_message = defined?(@_message) ? @_message : request.message
    end
    
    # Sends a file over HTTP.  When given a path to a file, it will set the
    # right headers so that the static file is served directly.
    # 
    # ==== Parameters
    # file<String>:: Path to file to send to the client.
    # opts<Hash>:: Options for sending the file (see below).
    # 
    # ==== Options (opts)
    # :disposition<String>::
    #   The disposition of the file send. Defaults to "attachment".
    # :filename<String>::
    #   The name to use for the file. Defaults to the filename of file.
    # :type<String>:: The content type.
    #
    # ==== Returns
    # IO:: An I/O stream for the file.
    # 
    # :api: public
    def send_file(file, opts={})
      opts.update(Merb::Const::DEFAULT_SEND_FILE_OPTIONS.merge(opts))
      disposition = opts[:disposition].dup || 'attachment'
      disposition << %(; filename="#{opts[:filename] ? opts[:filename] : File.basename(file)}")
      headers.update(
        'Content-Type'              => opts[:type].strip,  # fixes a problem with extra '\r' with some browsers
        'Content-Disposition'       => disposition,
        'Content-Transfer-Encoding' => 'binary'
      )
      Proc.new do |response|
        file = File.open(file, 'rb')
        while chunk = file.read(16384)
          response.write chunk
        end
        file.close
      end
    end
    
    # Send binary data over HTTP to the user as a file download. May set content type,
    # apparent file name, and specify whether to show data inline or download as an attachment.
    # 
    # ==== Parameters
    # data<String>:: Path to file to send to the client.
    # opts<Hash>:: Options for sending the data (see below).
    # 
    # ==== Options (opts)
    # :disposition<String>::
    #   The disposition of the file send. Defaults to "attachment".
    # :filename<String>::
    #   The name to use for the file. Defaults to the filename of file.
    # :type<String>:: The content type.
    # 
    # :api: public
    def send_data(data, opts={})
      opts.update(Merb::Const::DEFAULT_SEND_FILE_OPTIONS.merge(opts))
      disposition = opts[:disposition].dup || 'attachment'
      disposition << %(; filename="#{opts[:filename]}") if opts[:filename]
      headers.update(
        'Content-Type'              => opts[:type].strip,  # fixes a problem with extra '\r' with some browsers
        'Content-Disposition'       => disposition,
        'Content-Transfer-Encoding' => 'binary'
      )
      data
    end
    
    # Streams a file over HTTP.
    # 
    # ==== Parameters
    # opts<Hash>:: Options for the file streaming (see below).
    # &stream::
    #   A block that, when called, will return an object that responds to
    #   +get_lines+ for streaming.
    # 
    # ==== Options
    # :disposition<String>::
    #   The disposition of the file send. Defaults to "attachment".
    # :type<String>:: The content type.
    # :content_length<Numeric>:: The length of the content to send.
    # :filename<String>:: The name to use for the streamed file.
    #
    # ==== Examples
    #   stream_file({ :filename => file_name, :type => content_type,
    #     :content_length => content_length }) do |response|
    #     AWS::S3::S3Object.stream(user.folder_name + "-" + user_file.unique_id, bucket_name) do |chunk|
    #       response.write chunk
    #     end
    #   end
    # 
    # :api: public
    def stream_file(opts={}, &stream)
      opts.update(Merb::Const::DEFAULT_SEND_FILE_OPTIONS.merge(opts))
      disposition = opts[:disposition].dup || 'attachment'
      disposition << %(; filename="#{opts[:filename]}")
      headers.update(
        'Content-Type'              => opts[:type].strip,  # fixes a problem with extra '\r' with some browsers
        'Content-Disposition'       => disposition,
        'Content-Transfer-Encoding' => 'binary',
        # Rack specification requires header values to respond to :each
        'CONTENT-LENGTH'            => opts[:content_length].to_s
      )
      Proc.new do |response|
        stream.call(response)
      end
    end
    
    # Uses the nginx specific +X-Accel-Redirect+ header to send a file directly
    # from nginx.
    # 
    # ==== Notes
    # Unless Content-Disposition is set before calling this method,
    # it is set to attachment with streamed file name.
    # 
    # For more information, see the nginx wiki:
    # http://wiki.codemongers.com/NginxXSendfile
    # 
    # and the following sample gist:
    # http://gist.github.com/11225
    # 
    # there's also example application up on GitHub:
    # 
    # http://github.com/michaelklishin/nginx-x-accel-redirect-example-application/tree/master
    # 
    # ==== Parameters
    # path<String>:: Path to file to send to the client.
    # content_type<String>:: content type header value. By default is set to empty string to let
    #                        Nginx detect it.
    # 
    # ==== Return
    # String:: precisely a single space.
    # 
    # :api: public
    def nginx_send_file(path, content_type = "")
      # Let Nginx detect content type unless it is explicitly set
      headers['Content-Type']        = content_type
      headers["Content-Disposition"] ||= "attachment; filename=#{path.split('/').last}"
      
      headers['X-Accel-Redirect']    = path
      
      return ' '
    end  
    
    # Sets a cookie to be included in the response.
    # 
    # If you need to set a cookie, then use the +cookies+ hash.
    # 
    # ==== Parameters
    # name<~to_s>:: A name for the cookie.
    # value<~to_s>:: A value for the cookie.
    # expires<~gmtime:~strftime, Hash>:: An expiration time for the cookie, or a hash of cookie options.
    # 
    # :api: public
    def set_cookie(name, value, expires)
      options = expires.is_a?(Hash) ? expires : {:expires => expires}
      cookies.set_cookie(name, value, options)
    end
    
    # Marks a cookie as deleted and gives it an expires stamp in the past. This
    # method is used primarily internally in Merb.
    # 
    # Use the +cookies+ hash to manipulate cookies instead.
    # 
    # ==== Parameters
    # name<~to_s>:: A name for the cookie to delete.
    # 
    # :api: public
    def delete_cookie(name)
      set_cookie(name, nil, Merb::Const::COOKIE_EXPIRED_TIME)
    end
    
    # Escapes the string representation of +obj+ and escapes it for use in XML.
    #
    # ==== Parameter
    # obj<~to_s>:: The object to escape for use in XML.
    #
    # ==== Returns
    # String:: The escaped object.
    # 
    # :api: public
    def escape_xml(obj)
      Merb::Parse.escape_xml(obj.to_s)
    end
    alias h escape_xml
    alias escape_html escape_xml
    
    private
    
    # Marks an output method that only runs on the Mongrel webserver.
    # 
    # ==== Raises
    # NotImplemented:: The Rack adapter is not mongrel.
    # 
    # :api: private
    def only_runs_on_mongrel!
      unless Merb::Config[:log_stream] == 'mongrel'
        raise(Merb::ControllerExceptions::NotImplemented, "Current Rack adapter is not mongrel. cannot support this feature")
      end
    end
  end
end
