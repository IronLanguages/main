class WIN32OLE
  def initialize progIdOrClsId
    if progIdOrClsId.is_a? System::Guid
      type = System::Type.GetTypeFromCLSID(progIdOrClsId)
      raise "Unknown CLSID #{progIdOrClsId}" if type.nil?
    elsif WIN32OLE.is_guid progIdOrClsId
      type = System::Type.GetTypeFromCLSID(System::Guid.new(progIdOrClsId))
      raise "Unknown CLSID #{progIdOrClsId}" if type.nil?
    else
      type = System::Type.GetTypeFromProgID(progIdOrClsId)
      raise "Unknown PROGID '#{progIdOrClsId}'" if type.nil?
    end
    @com_object = System::Activator.CreateInstance(type)
  end
  
  def self.is_guid(str)
    !!(/[{]?[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}[}]?|[0-9A-F]{32}/ =~ str)
  end
  
  def method_missing name, *args
    @com_object.send(name, *args)
  end
  
  attr_reader :com_object
end

ex = WIN32OLE.new("Excel.Application")
#ex = WIN32OLE.new("{00024500-0000-0000-C000-000000000046}")

ex.Visible = true
nb = ex.Workbooks.Add
ws = nb.Worksheets[1]
p ws.Name

10.times do |i| 
  10.times do |j| 
    ws.Cells[i + 1, j + 1] = (i + 1) * (j + 1)
  end
end
