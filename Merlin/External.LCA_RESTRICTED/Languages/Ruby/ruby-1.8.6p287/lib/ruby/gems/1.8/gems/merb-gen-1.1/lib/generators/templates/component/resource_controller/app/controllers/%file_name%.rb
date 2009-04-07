<% with_modules(modules) do -%>
class <%= class_name %> < Application
  
  # GET /<%= resource_path %>
  def index
    render
  end

  # GET /<%= resource_path %>/:id
  def show
    render
  end

  # GET /<%= resource_path %>/new
  def new
    render
  end

  # GET /<%= resource_path %>/:id/edit
  def edit
    render
  end

  # GET /<%= resource_path %>/:id/delete
  def delete
    render
  end

  # POST /<%= resource_path %>
  def create
    render
  end

  # PUT /<%= resource_path %>/:id
  def update
    render
  end

  # DELETE /<%= resource_path %>/:id
  def destroy
    render
  end
end
<% end -%>