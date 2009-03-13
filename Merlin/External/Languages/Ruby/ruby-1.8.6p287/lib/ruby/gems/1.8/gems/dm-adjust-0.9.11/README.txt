dm-adjust
=============

DataMapper plugin providing methods to increment and decrement properties

It provides the following function (for collections and resources):

== adjust!

 Person.adjust!(:salary => 1000) # increases the salary of all people with 1000.
 Person.all(:age.gte => 40).adjust!(:salary => 2000) # increase salary of them oldies.
 @child.adjust!(:allowance => -100) # less money for candy and concerts
