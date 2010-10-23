require System::Reflection::Assembly.get_assembly(IronRuby.create_engine.class.to_clr_type).to_s
engine = IronRuby.create_engine(System::Action.of(Microsoft::Scripting::Hosting::LanguageSetup).new {|a| })
puts engine.execute("1+1")
