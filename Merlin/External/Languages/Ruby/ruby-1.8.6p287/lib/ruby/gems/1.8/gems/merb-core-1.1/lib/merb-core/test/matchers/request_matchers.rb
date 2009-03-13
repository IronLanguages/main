Spec::Matchers.create(:be_successful, :respond_successfully) do
  matches do |rack|
    @status = rack.respond_to?(:status) ? rack.status : rack
    @inspect = describe_input(rack)

    (200..207).include?(@status)
  end
  
  message do |not_string, rack|
    if @inspect.is_a?(Numeric)
      "Expected status code#{not_string} to be successful, " \
      "but it was #{@inspect}"
    else
      "Expected #{@inspect}#{not_string} " \
      "to be successful, but it returned a #{@status}"
    end
  end
end

Spec::Matchers.create(:be_missing, :be_client_error) do
  matches do |rack|
    @status = rack.respond_to?(:status) ? rack.status : rack
    @inspect = describe_input(rack)

    (400..417).include?(@status)
  end
  
  message do |not_string, rack|
    unless @inspect.is_a?(Numeric)
      "Expected #{@inspect}#{not_string} " \
      "to be missing, but it returned a #{@status}"
    else
      "Expected #{not_string ? "not to get " : ""}a missing error code, " \
      "but got #{@inspect}"
    end
  end
end

Spec::Matchers.create(:have_body) do
  matches do |rack, body|
    @actual = if rack.respond_to?(:body)
      rack.body.to_s
    else
      rack.to_s
    end
    
    @actual == body
  end
  
  negative_failure_message do |rack, body|
    "Expected the response not to match:\n    #{body}\nActual response was:\n    #{@actual}" 
  end
  
  failure_message do |rack, body|
    "Expected the response to match:\n    #{body}\nActual response was:\n    #{@actual}" 
  end
end

Spec::Matchers.create(:have_content_type) do
  matches do |rack, mime_symbol|
    content_type = rack.headers["Content-Type"].split("; ").first
    if registered_mime = Merb.available_mime_types[mime_symbol]
      registered_mime[:accepts].include?(content_type)
    else
      @error = "Mime #{mime_symbol.inspect} was not registered"
      false
    end
  end
  
  failure_message do |rack, mime_symbol|
    if @error
      @error
    else
      ret = "Expected your response to be of the #{mime_symbol} type, "
      if mime = Merb.available_accepts[rack.headers["Content-Type"]]
        ret << "but it was #{mime}"
      else
        ret << "but it was #{rack.headers["Content-Type"]}, which was " \
               "not a registered Merb content type."
      end
    end
  end
end

Spec::Matchers.create(:redirect) do
  matches do |rack|
    @inspect = describe_input(rack)
    @status_code = status_code(rack)
    (300..399).include?(@status_code)
  end
  
  message do |not_string, rack|
    "Expected #{@inspect}#{not_string} to be a redirect, but the " \
    "status code was #{@status_code}"
  end
end

Spec::Matchers.create(:redirect_to) do
  expected_value do |location|
    url(location) if location.is_a?(Symbol)
  end
  
  matches do |rack, location|
    @inspect = describe_input(rack)
    
    return false unless rack.headers["Location"]
    @location, @query = rack.headers["Location"].split("?")
    @status_code = status_code(rack)
    @status_code.in?(300..399) && @location == location
  end
  
  negative_failure_message do |rack, location|
    "Expected #{@inspect} not to redirect to " \
    "<#{location}> but it did."
  end
  
  failure_message do |rack, location|
    if !rack.status.in?(300..399)
      "Expected #{@inspect} to be a redirect, but " \
      "it returned status code #{rack.status}."
    elsif rack.headers["Location"] != location
      "Expected #{@inspect} to redirect to " \
      "<#{location}>, but it redirected to <#{rack.headers["Location"]}>"
    end
  end
end
