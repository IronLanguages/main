require 'erb'

class App 
  def call env
    request  = Rack::Request.new env
    response = Rack::Response.new

    response.header['Content-Type'] = 'text/html'

    @msg1 = "Hello"
    @msg2 = "World"
    msg = ERB.new('IronRuby running Rack says "<%= @msg1 %>, <b><%= @msg2 %></b>" at <%= Time.now %>').result(binding)

    response.write msg

    response.finish
  end
end
