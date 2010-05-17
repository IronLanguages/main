class Module
  def focus *wanteds
    wanteds.map! { |m| m.to_s }
    unwanteds = public_instance_methods(false).grep(/test_/) - wanteds
    unwanteds.each do |unwanted|
      remove_method unwanted
    end
  end

  def blur
    parent = self.superclass

    ObjectSpace.each_object Class do |klass|
      next unless parent > klass
      next if klass == self

      klass.send :focus
      klass.send :undef_method, :default_test
    end
  end
end
