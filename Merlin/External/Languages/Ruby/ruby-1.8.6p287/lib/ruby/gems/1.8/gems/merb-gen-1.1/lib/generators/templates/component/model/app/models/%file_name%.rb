<% with_modules(modules) do -%>
class <%= class_name %>
<% if attributes? -%>
  attr_accessor <%= attributes_for_accessor %>
<% end -%>
end
<% end -%>