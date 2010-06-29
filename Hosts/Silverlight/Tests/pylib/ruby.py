from Microsoft.Scripting.Silverlight import DynamicApplication

engine = DynamicApplication.Current.Runtime.GetEngine('IronRuby')
scope = engine.CreateScope()

def execute(str, type = 'file'):
  global engine, scope
  return engine.Execute(str, scope)

