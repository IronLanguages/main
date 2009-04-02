= dm-is-nested_set

DataMapper plugin allowing the creation of nested sets from data models.
Provides all the same functionality as dm-is-tree, plus tons more! Read on.

== What is a nested set?

Nested set is a clever model for storing hierarchical data in a flat table.
Instead of (only) storing the id of the parent on each node, a nested set puts
all nodes in a clever structure (see Example below). That is what makes it
possible to get the all of the descendants (not only immediate children),
ancestors, or siblings, in one single query to the database.

The only downside to nested sets (compared to trees] is that the queries it
takes to know these things, and to move nodes around in the tree are rather
complex. That is what this plugin takes care of (+ lots of other neat stuff)!

Nested sets are a good choice for most kinds of ordered trees with more than
two levels of nesting. Very good for menus, categories, and threaded posts.

== Installation

Download and install the latest dm-more-gem. Remember to require it!

== Getting started

Coming

== Traversing the tree

Coming

== Moving nodes

Coming


== Example of a nested set

We have a nested menu of categories. The categories are as follows:

-Electronics
 - Televisions
   - Tube
   - LCD
   - Plasma
 - Portable Electronics
   - MP3 Players
   - CD Players

In a nested set, each of these categories would have 'left' and 'right' fields,
informing about where in the set they are positioned. This can be illustrated:
 _____________________________________________________________________________
|   _________________________________    __________________________________   |
|  |   ______    _____    ________   |  |   _______________    _________   |  |
|  |  |      |  |     |  |        |  |  |  |               |  |         |  |  |
1  2  3      4  5     6  7        8  9  10 11             12  13  CD-  14 15 16
|  |  | Tube |  | LCD |  | Plasma |  |  |  |  MP3 Players  |  | Players |  |  |
|  |  |______|  |_____|  |________|  |  |  |_______________|  |_________|  |  |
|  |                                 |  |                                  |  |
|  |          Televisions            |  |       Portable Electronics       |  |
|  |_________________________________|  |__________________________________|  |
|                                                                             |
|                                 Electronics                                 |
|_____________________________________________________________________________|

All sets has a left / right value, that just says 'here do I start', and 'here
do I end'. The category 'Televisions' starts at 2, and ends at 9. We then know
that _all_ descendants of 'Televisions' reside between 2 and 9. Whats more, we
can see all categories that does not have any subcategory, by checking if their
left and right value has a gap between them. Clever huh?

Now, if we want to insert the category 'Flash' into 'MP3 Players', the new set
and left/right values would now be:
 _____________________________________________________________________________
|                                        __________________________________   |
|   _________________________________   |   _______________                |  |
|  |   ______    _____    ________   |  |  |   _________   |   _________   |  |
|  |  |      |  |     |  |        |  |  |  |  |         |  |  |         |  |  |
1  2  3      4  5     6  7        8  9  10 11 12 Flash 13 14  15  CD-  16 17 18
|  |  | Tube |  | LCD |  | Plasma |  |  |  |  |_________|  |  | Players |  |  |
|  |  |______|  |_____|  |________|  |  |  |               |  |_________|  |  |
|  |                                 |  |  |  MP3 Players  |               |  |
|  |          Televisions            |  |  |_______________|  Portable El. |  |
|  |_________________________________|  |__________________________________|  |
|                                                                             |
|                                 Electronics                                 |
|_____________________________________________________________________________|

== More about nested sets

* http://www.developersdex.com/gurus/articles/112.asp
* http://dev.mysql.com/tech-resources/articles/hierarchical-data.html
* http://www.codeproject.com/KB/database/nestedsets.aspx (nice illustrations)
