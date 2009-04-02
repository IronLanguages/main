migration 1, :create_people_table do
  up do
    create_table :people do
      column :id,     Integer, :serial => true
      column :name,   String, :size => 50
      column :age,    Integer
    end
  end
  down do
    drop_table :people
  end
end
