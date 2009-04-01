= dm-is-viewable

Adds simple SQL View-like functionality to DataMapper.  It helps keep your queries dry.

= Viewables
=== Before
@users = User.all(:active => true, :age.gt => 18, :gender => :female)

=== After
class User
  include DataMapper::Resource

 is :viewable

 # After the name symbol you can treat supply anything that would work in Resource#all
 create_view :legal_women, :active => true, :age.gt => 18, :gender => :female
 create_view :legal_people, :active => true, :age.gt => 18

 #... Your code here ...
end

@users = User.view :legal_women # :)

=== Further restricting results
@users = User.view :legal_women, :city => 'Los Angeles'
# => Only finds women in LA

@users = User.view :legal_people, :gender => :male
# => Finds dudes if thats what you are into.

# Remember, views restrict results so the following would return nothing
@users = User.view :legal_women, :gender => :male

# => returns nil, the query essentially became:
#    Select * from users where gender = 'female' and gender = 'male';

#Example of JOIN TODO

=== Complaints
<nasally_nerd_voice>
But sirs, a SQL View is a separate table like structure, not something definable in an object!??!1eleven.
</nasally_nerd_voice>

This is for an ORM, so views are mapped into the objects, besides you are probably accessing your true MySQL views with another DataMapper::Resource.
