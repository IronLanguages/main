module Fox
  FXWindow.subclasses.each do |klass|
    klass.send(:alias_method, :create_without_parent_created_check, :create)
    klass.send(:define_method, :create) do
      unless parent.created?
        raise RuntimeError, "trying to create window before creating parent window"
      end
      if owner && !owner.created?
        raise RuntimeError, "trying to create window before creating owner window"
      end
      if visual.nil?
        raise RuntimeError, "trying to create window without a visual"
      end
      create_without_parent_created_check
    end
  end
end
