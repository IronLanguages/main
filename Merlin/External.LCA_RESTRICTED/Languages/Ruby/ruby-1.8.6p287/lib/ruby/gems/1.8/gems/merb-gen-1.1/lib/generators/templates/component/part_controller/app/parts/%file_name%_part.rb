<% with_modules(modules) do -%>
class <%= class_name %>Part < Merb::PartController

  def index
    render
  end

end
<% end -%>