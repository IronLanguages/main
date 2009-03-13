module Merb::Test::MultipartRequestHelper
  
  def multipart_request(path, params = {}, env = {})
    multipart = Merb::Test::MultipartRequestHelper::Post.new(params)
    body, head = multipart.to_multipart
    env["CONTENT_TYPE"] = head
    env["CONTENT_LENGTH"] = body.size
    env[:input] = StringIO.new(body)
    request(path, env)
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
  def multipart_post(path, params = {}, env = {})
    env[:method] = "POST"
    multipart_request(path, params, env)
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
    env[:method] = "PUT"
    multipart_request(path, params, env)
  end
  
end