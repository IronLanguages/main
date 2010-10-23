include System::Windows::Browser
include Microsoft::Scripting::Silverlight

module Helpers
  def tag(tag_name, options = {}, style_options = {})
    div = HtmlPage.document.create_element tag_name
    options.each do |key, value|
      div.set_property key.to_s.to_clr_string, value.to_s.to_clr_string
    end
    style_options.each do |key,value|
      div.set_style_attribute key.to_s.to_clr_string, value.to_s.to_clr_string
    end
    div
  end
end
