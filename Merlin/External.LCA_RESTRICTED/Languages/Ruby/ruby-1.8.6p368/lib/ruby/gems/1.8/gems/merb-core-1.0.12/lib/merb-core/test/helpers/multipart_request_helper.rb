module Merb::Test::MultipartRequestHelper
  require 'rubygems'
  gem "mime-types"
  require 'mime/types'

  class Param
    attr_accessor :key, :value

    # ==== Parameters
    # key<~to_s>:: The parameter key.
    # value<~to_s>:: The parameter value.
    def initialize(key, value)
      @key   = key
      @value = value
    end

    # ==== Returns
    # String:: The parameter in a form suitable for a multipart request.
    def to_multipart
      return %(Content-Disposition: form-data; name="#{key}"\r\n\r\n#{value}\r\n)
    end
  end

  class FileParam
    attr_accessor :key, :filename, :content

    # ==== Parameters
    # key<~to_s>:: The parameter key.
    # filename<~to_s>:: Name of the file for this parameter.
    # content<~to_s>:: Content of the file for this parameter.
    def initialize(key, filename, content)
      @key      = key
      @filename = filename
      @content  = content
    end

    # ==== Returns
    # String::
    #   The file parameter in a form suitable for a multipart request.
    def to_multipart
      return %(Content-Disposition: form-data; name="#{key}"; filename="#{filename}"\r\n) + "Content-Type: #{MIME::Types.type_for(@filename)}\r\n\r\n" + content + "\r\n"
    end
  end

  class Post
    BOUNDARY = '----------0xKhTmLbOuNdArY'
    CONTENT_TYPE = "multipart/form-data, boundary=" + BOUNDARY

    # ==== Parameters
    # params<Hash>:: Optional params for the controller.
    def initialize(params = {})
      @multipart_params = []
      push_params(params)
    end

    # Saves the params in an array of multipart params as Param and
    # FileParam objects.
    #
    # ==== Parameters
    # params<Hash>:: The params to add to the multipart params.
    # prefix<~to_s>:: An optional prefix for the request string keys.
    def push_params(params, prefix = nil)
      params.sort_by {|k| k.to_s}.each do |key, value|
        param_key = prefix.nil? ? key : "#{prefix}[#{key}]"
        if value.respond_to?(:read)
          @multipart_params << FileParam.new(param_key, value.path, value.read)
        else
          if value.is_a?(Hash) || value.is_a?(Mash)
            value.keys.each do |k|
              push_params(value, param_key)
            end
          else
            @multipart_params << Param.new(param_key, value)
          end
        end
      end
    end

    # ==== Returns
    # Array[String, String]:: The query and the content type.
    def to_multipart
      query = @multipart_params.collect { |param| "--" + BOUNDARY + "\r\n" + param.to_multipart }.join("") + "--" + BOUNDARY + "--"
      return query, CONTENT_TYPE
    end
  end 

  # Similar to dispatch_to but allows for sending files inside params.  
  #
  # ==== Paramters 
  # controller_klass<Controller>::
  #   The controller class object that the action should be dispatched to.
  # action<Symbol>:: The action name, as a symbol.
  # params<Hash>::
  #   An optional hash that will end up as params in the controller instance.
  # env<Hash>::
  #   An optional hash that is passed to the fake request. Any request options
  #   should go here (see +fake_request+).
  # &blk:: The block is executed in the context of the controller.
  #  
  # ==== Example
  #   dispatch_multipart_to(MyController, :create, :my_file => @a_file ) do |controller|
  #     controller.stub!(:current_user).and_return(@user)
  #   end
  #
  # ==== Notes
  # Set your option to contain a file object to simulate file uploads.
  #   
  # Does not use routes.
  #---
  # @public
  def dispatch_multipart_to(controller_klass, action, params = {}, env = {}, &blk)
    request = multipart_fake_request(env, params)
    dispatch_request(request, controller_klass, action, &blk)
  end

  # An HTTP POST request that operates through the router and uses multipart
  # parameters.
  #
  # ==== Parameters
  # path<String>:: The path that should go to the router as the request uri.
  # params<Hash>::
  #   An optional hash that will end up as params in the controller instance.
  # env<Hash>::
  #   An optional hash that is passed to the fake request. Any request options
  #   should go here (see +fake_request+).
  # block<Proc>:: The block is executed in the context of the controller.
  #
  # ==== Notes
  # To include an uploaded file, put a file object as a value in params.
  def multipart_post(path, params = {}, env = {}, &block)
    env[:request_method] = "POST"
    env[:test_with_multipart] = true
    mock_request(path, params, env, &block)
  end

  # An HTTP PUT request that operates through the router and uses multipart
  # parameters.
  #
  # ==== Parameters
  # path<String>:: The path that should go to the router as the request uri.
  # params<Hash>::
  #   An optional hash that will end up as params in the controller instance.
  # env<Hash>::
  #   An optional hash that is passed to the fake request. Any request options
  #   should go here (see +fake_request+).
  # block<Proc>:: The block is executed in the context of the controller.
  #
  # ==== Notes
  # To include an uplaoded file, put a file object as a value in params.
  def multipart_put(path, params = {}, env = {}, &block)
    env[:request_method] = "PUT"
    env[:test_with_multipart] = true
    mock_request(path, params, env, &block)
  end
  
  # ==== Parameters
  # env<Hash>::
  #   An optional hash that is passed to the fake request. Any request options
  #   should go here (see +fake_request+).
  # params<Hash>::
  #   An optional hash that will end up as params in the controller instance.
  # 
  # ==== Returns
  # FakeRequest::
  #   A multipart Request object that is built based on the parameters.
  def multipart_fake_request(env = {}, params = {})
    if params.empty?
      fake_request(env)
    else
      m = Post.new(params)
      body, head = m.to_multipart
      fake_request(env.merge( :content_type => head, 
                              :content_length => body.length), :post_body => body)
    end
  end
end
