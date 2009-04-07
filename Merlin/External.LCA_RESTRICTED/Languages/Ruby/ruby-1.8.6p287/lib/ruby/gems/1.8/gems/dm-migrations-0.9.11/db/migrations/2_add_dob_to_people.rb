migration 2, :add_dob_to_people do
  up do
    modify_table :people do
      add_column :dob, DateTime, :nullable? => true
    end
  end

  down do
    modify_table :people do
      drop_column :dob
    end
  end
end
