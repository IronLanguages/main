module Merb
  module Parse
  
    # ==== Parameters
    # query_string<String>:: The query string.
    # delimiter<String>:: The query string divider. Defaults to "&".
    # preserve_order<Boolean>:: Preserve order of args. Defaults to false.
    #
    # ==== Returns
    # Mash:: The parsed query string (Dictionary if preserve_order is set).
    #
    # ==== Examples
    #   Merb::Parse.query("bar=nik&post[body]=heya")
    #     # => { :bar => "nik", :post => { :body => "heya" } }
    #
    # :api: plugin
    def self.query(query_string, delimiter = '&;', preserve_order = false)
      query = preserve_order ? Dictionary.new : {}
      for pair in (query_string || '').split(/[#{delimiter}] */n)
        key, value = unescape(pair).split('=',2)
        next if key.nil?
        if key.include?('[')
          normalize_params(query, key, value)
        else
          query[key] = value
        end
      end
      preserve_order ? query : query.to_mash
    end

    NAME_REGEX         = /Content-Disposition:.* name="?([^\";]*)"?/ni.freeze
    CONTENT_TYPE_REGEX = /Content-Type: (.*)\r\n/ni.freeze
    FILENAME_REGEX     = /Content-Disposition:.* filename="?([^\";]*)"?/ni.freeze
    CRLF               = "\r\n".freeze
    EOL                = CRLF
  
    # ==== Parameters
    # request<IO>:: The raw request.
    # boundary<String>:: The boundary string.
    # content_length<Fixnum>:: The length of the content.
    #
    # ==== Raises
    # ControllerExceptions::MultiPartParseError:: Failed to parse request.
    #
    # ==== Returns
    # Hash:: The parsed request.
    #
    # :api: plugin
    def self.multipart(request, boundary, content_length)
      boundary = "--#{boundary}"
      paramhsh = {}
      buf      = ""
      input    = request
      input.binmode if defined? input.binmode
      boundary_size = boundary.size + EOL.size
      bufsize       = 16384
      content_length -= boundary_size
      # status is boundary delimiter line
      status = input.read(boundary_size)
      return {} if status == nil || status.empty?
      raise ControllerExceptions::MultiPartParseError, "bad content body:\n'#{status}' should == '#{boundary + EOL}'"  unless status == boundary + EOL
      # second argument to Regexp.quote is for KCODE
      rx = /(?:#{EOL})?#{Regexp.quote(boundary,'n')}(#{EOL}|--)/
      loop {
        head      = nil
        body      = ''
        filename  = content_type = name = nil
        read_size = 0
        until head && buf =~ rx
          i = buf.index("\r\n\r\n")
          if( i == nil && read_size == 0 && content_length == 0 )
            content_length = -1
            break
          end
          if !head && i
            head = buf.slice!(0, i+2) # First \r\n
            buf.slice!(0, 2)          # Second \r\n

            # String#[] with 2nd arg here is returning
            # a group from match data
            filename     = head[FILENAME_REGEX, 1]
            content_type = head[CONTENT_TYPE_REGEX, 1]
            name         = head[NAME_REGEX, 1]

            if filename && !filename.empty?
              body = Tempfile.new(:Merb)
              body.binmode if defined? body.binmode
            end
            next
          end

          # Save the read body part.
          if head && (boundary_size+4 < buf.size)
            body << buf.slice!(0, buf.size - (boundary_size+4))
          end

          read_size = bufsize < content_length ? bufsize : content_length
          if( read_size > 0 )
            c = input.read(read_size)
            raise ControllerExceptions::MultiPartParseError, "bad content body"  if c.nil? || c.empty?
            buf << c
            content_length -= c.size
          end
        end

        # Save the rest.
        if i = buf.index(rx)
          # correct value of i for some edge cases
          if (i > 2) && (j = buf.index(rx, i-2)) && (j < i)
             i = j
           end
          body << buf.slice!(0, i)
          buf.slice!(0, boundary_size+2)

          content_length = -1  if $1 == "--"
        end

        if filename && !filename.empty?
          body.rewind
          data = {
            :filename => File.basename(filename),
            :content_type => content_type,
            :tempfile => body,
            :size => File.size(body.path)
          }
        else
          data = body
        end
        paramhsh = normalize_params(paramhsh,name,data)
        break  if buf.empty? || content_length == -1
      }
      paramhsh
    end

    # ==== Parameters
    # value<Array, Hash, Dictionary ~to_s>:: The value for the query string.
    # prefix<~to_s>:: The prefix to add to the query string keys.
    #
    # ==== Returns
    # String:: The query string.
    #
    # ==== Alternatives
    # If the value is a string, the prefix will be used as the key.
    #
    # ==== Examples
    #   params_to_query_string(10, "page")
    #     # => "page=10"
    #   params_to_query_string({ :page => 10, :word => "ruby" })
    #     # => "page=10&word=ruby"
    #   params_to_query_string({ :page => 10, :word => "ruby" }, "search")
    #     # => "search[page]=10&search[word]=ruby"
    #   params_to_query_string([ "ice-cream", "cake" ], "shopping_list")
    #     # => "shopping_list[]=ice-cream&shopping_list[]=cake"
    #
    # :api: plugin
    def self.params_to_query_string(value, prefix = nil)
      case value
      when Array
        value.map { |v|
          params_to_query_string(v, "#{prefix}[]")
        } * "&"
      when Hash, Dictionary
        value.map { |k, v|
          params_to_query_string(v, prefix ? "#{prefix}[#{escape(k)}]" : escape(k))
        } * "&"
      else
        "#{prefix}=#{escape(value)}"
      end
    end

    # ==== Parameters
    # s<String>:: String to URL escape.
    #
    # ==== returns
    # String:: The escaped string.
    #
    # :api: public
    def self.escape(s)
      s.to_s.gsub(/([^ a-zA-Z0-9_.-]+)/n) {
        '%'+$1.unpack('H2'*$1.size).join('%').upcase
      }.tr(' ', '+')
    end

    # ==== Parameter
    # s<String>:: String to URL unescape.
    #
    # ==== returns
    # String:: The unescaped string.
    #
    # :api: public
    def self.unescape(s)
      s.tr('+', ' ').gsub(/((?:%[0-9a-fA-F]{2})+)/n){
        [$1.delete('%')].pack('H*')
      }
    end

    # ==== Parameters
    # s<String>:: String to XML escape.
    #
    # ==== returns
    # String:: The escaped string.
    #
    # :api: public
    def self.escape_xml(s)
      Erubis::XmlHelper.escape_xml(s)
    end

    private

    # Converts a query string snippet to a hash and adds it to existing
    # parameters.
    #
    # ==== Parameters
    # parms<Hash>:: Parameters to add the normalized parameters to.
    # name<String>:: The key of the parameter to normalize.
    # val<String>:: The value of the parameter.
    #
    # ==== Returns
    # Hash:: Normalized parameters
    #
    # :api: private
    def self.normalize_params(parms, name, val=nil)
      name =~ %r([\[\]]*([^\[\]]+)\]*)
      key = $1 || ''
      after = $' || ''

      if after == ""
        parms[key] = val
      elsif after == "[]"
        (parms[key] ||= []) << val
      elsif after =~ %r(^\[\]\[([^\[\]]+)\]$)
        child_key = $1
        parms[key] ||= []
        if parms[key].last.is_a?(Hash) && !parms[key].last.key?(child_key)
          parms[key].last.update(child_key => val)
        else
          parms[key] << { child_key => val }
        end
      else
        parms[key] ||= {}
        parms[key] = normalize_params(parms[key], after, val)
      end
      parms
    end  
  
  end
end
