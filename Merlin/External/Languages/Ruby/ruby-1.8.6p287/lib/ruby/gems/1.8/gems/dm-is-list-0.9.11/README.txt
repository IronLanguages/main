= dm-is-list

DataMapper plugin for creating and organizing lists.

== Installation

Download dm-more and install dm-is-list. Remember to require it in your app.

== Getting started

Lets say we have a user-class, and we want to give users the possibility of
having their own todo-lists

class Todo
  include DataMapper::Resource

  property :id, Serial
  property :title, String
  property :done, DateTime

  belongs_to :user

  # here we define that this should be a list, scoped on :user_id
  is :list, :scope => [:user_id]
end

You can now move objects around like this:

 item = Todo.get(1)
 other = Todo.get(2)

 item.move(:highest)        # moves to top of list
 item.move(:lowest)         # moves to bottom of list
 item.move(:up)             # moves one up (:higher and :up is the same)
 item.move(:down)           # moves one up (:lower and :down is the same)
 item.move(:to => position) # moves item to a specific position
 item.move(:above => other) # moves item above the other item.*
 item.move(:below => other) # moves item above the other item.*

 * won't move if the other item is in another scope. (should this be allowed?)

The list will try to act as intelligently as possible. If you set the position
manually, and then save, the list will reorganize itself to correctly:

 item.position = 3 # setting position manually
 item.save # the list will now move the item correctly, and updating others

If you move items between scopes, the list will also try to do what you most
likely want to do:

 item.user_id # => 1
 item.user_id = 2 # giving this item to another user
 item.save # the list will now have detached item from old list, and inserted
           # at the bottom of the new (user 2) list.

If something is not behaving intuitively, it is a bug, and should be reported.
Report it here: http://wm.lighthouseapp.com/projects/4819-datamapper/overview
