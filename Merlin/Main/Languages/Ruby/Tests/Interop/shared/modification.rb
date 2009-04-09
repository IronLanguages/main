describe :adding_a_method, :shared => true do
  it "is allowed directly" do
    begin
      string = <<-EOL
        #{@klass.class.to_s.downcase} #{@klass.name}
          def test_direct_method
            1
          end
        end
      EOL
      eval string
      @obj.test_direct_method.should == 1
    rescue Exception => e
      flunk e.message
    ensure
      @klass.module_eval { undef :test_direct_method } rescue nil
    end
  end
  
  it "is allowed via module/class_eval" do
    begin
      @klass.module_eval do
        def test_mod_eval_method
          2
        end
      end
      @obj.test_mod_eval_method.should == 2  
    rescue Exception => e
      flunk e.message
    ensure 
      @klass.module_eval do
        undef :test_mod_eval_method
      end rescue nil
    end
  end
end 

describe :adding_class_methods, :shared => true do
  it "is allowed directly on the class" do
    begin
      string = <<-EOL
        #{@klass.class.to_s.downcase} #{@klass.name}
          def self.test_direct_class_method
            1
          end
        end
      EOL
      eval string
      @klass.test_direct_class_method.should == 1
    rescue Exception => e
      flunk e.message
    ensure
      @klass.metaclass_eval { undef :test_direct_class_method } rescue nil
    end
  end
  
  it "is allowed via class_eval on the metaclass" do
    begin
      @klass.metaclass_def(:test_meta_class_method) { 2 }
      @klass.test_meta_class_method.should == 2
    rescue Exception => e
      flunk e.message
    ensure
      @klass.metaclass_eval { undef :test_meta_class_method } rescue nil
    end
  end
end
  
describe :adding_metaclass_methods, :shared => true do
  it "is allowed on metaclass" do
    begin
      @obj.metaclass_def(:test_meta_method) { 1 }
      @obj.test_meta_method.should == 1
    rescue Exception => e
      flunk e.message
    ensure
      @obj.metaclass_eval { undef :test_meta_method } rescue nil
    end
  end
end