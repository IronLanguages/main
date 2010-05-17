require 'def'

def probe_methods(obj, name)
    di = []
    
    di << obj.i_Sx rescue 1
    di << obj.i_Sx1 rescue 1
    
    di << obj.i_C rescue 1
    di << obj.i_S1 rescue 1
    di << obj.i_S2 rescue 1
    
    di << obj.i_D rescue 1
    di << obj.i_T1 rescue 1
    di << obj.i_T2 rescue 1
    di << obj.i_T3 rescue 1
    
    di << obj.i_Object rescue 1
    di << obj.i_Object1 rescue 1
    
    di << obj.i_Module rescue 1
    di << obj.i_Module1 rescue 1

    di << obj.i_Class rescue 1
    di << obj.i_Class1 rescue 1

    di << obj.i_SMx rescue 1
    di << obj.i_SM1 rescue 1
    di << obj.i_SM1_1 rescue 1
    
    da = []
    
    da << obj.ai_Sx rescue 1
    da << obj.ai_Sx1 rescue 1
    
    da << obj.ai_C rescue 1
    da << obj.ai_S1 rescue 1
    da << obj.ai_S2 rescue 1
    
    da << obj.ai_D rescue 1
    da << obj.ai_T1 rescue 1
    da << obj.ai_T2 rescue 1
    da << obj.ai_T3 rescue 1
    
    da << obj.ai_Object rescue 1
    da << obj.ai_Object1 rescue 1
    
    da << obj.ai_Module rescue 1
    da << obj.ai_Module1 rescue 1

    da << obj.ai_Class rescue 1
    da << obj.ai_Class1 rescue 1

    da << obj.ai_SMx rescue 1
    da << obj.ai_SM1 rescue 1
    da << obj.ai_SM1_1 rescue 1
    
    dc = []
    
    dc << obj.c_Sx rescue 1
    dc << obj.c_Sx1 rescue 1
    
    dc << obj.c_C rescue 1
    dc << obj.c_S1 rescue 1
    dc << obj.c_S2 rescue 1
    
    dc << obj.c_D rescue 1
    dc << obj.c_T1 rescue 1
    dc << obj.c_T2 rescue 1
    dc << obj.c_T3 rescue 1
    
    dc << obj.c_Object rescue 1
    dc << obj.c_Object1 rescue 1
    
    dc << obj.c_Module rescue 1
    dc << obj.c_Module1 rescue 1

    dc << obj.c_Class rescue 1
    dc << obj.c_Class1 rescue 1

    dc << obj.c_SMx rescue 1
    dc << obj.c_SM1 rescue 1
    dc << obj.c_SM1_1 rescue 1

    ms = [
:i_C,
:i_Sx,
:i_Sx1,
:i_S1,
:i_S2,
:i_D,
:i_T1,
:i_T2,
:i_SM1,
:i_Object,
:i_Object1,
:i_Module,
:i_Module1,
:i_Class,
:i_Class1,
:allocate,
:superclass,
:new,
:constants,
:nesting,
    ]   
    
    
    dm = []
    ms.each { |x| 
        begin
            obj.instance_method(x)
        rescue
        else
            dm << x
        end    
    }
    
    begin
        im = obj.instance_methods() 
        im = im.find_all { |x| x[0..1] == "i_" || x[0..1] == "c_"}
    rescue
        im = "N/A"
    end    
    
    puts "#{name} instance: #{di.inspect}"
    puts "#{name} instance aliases: #{(da == di) ? "matches" : da.inspect}"
    puts "#{name} class: #{dc.inspect}"
    puts "#{name} instance_method: #{dm.inspect}"
    puts "#{name} instance_methods: #{im.inspect}"
    
end

def probe_consts(obj, name)
    c = [
:CONST_Sx,
:CONST_Sx1,
:CONST_C,
:CONST_S1, 
:CONST_S2,
:CONST_S3,
:CONST_D,
:CONST_T1, 
:CONST_T2, 
:CONST_T3,
:CONST_Object,
:CONST_Object1,
:CONST_Module,
:CONST_Module1,
:CONST_Class,
:CONST_Class1,
:CONST_SMx,
:CONST_SM1,
:CONST_SM1_1,
    ]
    d = []
    
    c.each { |x| 
        begin 
            obj.const_get(x) 
        rescue 
        else 
            d << x 
        end 
    }
    
    puts "#{name} constants: #{d.inspect}"
    puts
end

probe_methods($X, "X");
puts

probe_methods($Sx, "Sx");
probe_consts($Sx, "Sx");

probe_methods($Sx1, "Sx1");
probe_consts($Sx1, "Sx1");

probe_methods($S1, "S1");
probe_consts($S1, "S1");

probe_methods($S2, "S2");
probe_consts($S2, "S2");

probe_methods($S3, "S3");
probe_consts($S3, "S3");

probe_methods($SM1, "SM1");
probe_consts($SM1, "SM1");

probe_methods($Class, "Class");
probe_consts($Class, "Class");

probe_methods($Module, "Module");
probe_consts($Module, "Module");

probe_methods($Object, "Object");
probe_consts($Object, "Object");

probe_methods($Module, "Class1");
probe_consts($Module, "Class1");

probe_methods($Module, "Module1");
probe_consts($Module, "Module1");

probe_methods($Object, "Object1");
probe_consts($Object, "Object1");

