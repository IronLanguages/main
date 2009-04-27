class TestRack 
  def call env
    request  = Rack::Request.new env
    response = Rack::Response.new

    response.header['Content-Type'] = 'text/html'
    response.write "Hello, <b>World</b> at #{Time.now}"

    response.finish
  end
end