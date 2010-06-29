begin
  require 'Microsoft.Scripting, Version=2.0.5.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
rescue LoadError
  require 'Microsoft.Scripting, Version=2.0.5.0, Culture=neutral, PublicKeyToken=null'
end
include Microsoft::Scripting
include Microsoft::Scripting::Hosting
include Microsoft::Scripting::Silverlight

def python(str, type = :file)
  @python_engine ||= DynamicApplication.Current.Runtime.
    GetEngine("IronPython")
  @python_scope ||= @python_engine.CreateScope()
  @python_engine.
    CreateScriptSourceFromString(str.strip, SourceCodeKind.send(type)).
    Execute(@python_scope)
end
