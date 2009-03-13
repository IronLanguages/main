dm-aggregates
=============

DataMapper plugin providing support for aggregates, functions on collections and datasets.

It provides the following functions:

== count

Count results (given the conditions)

 Friend.count # returns count of all friends
 Friend.count(:age.gt => 18) # returns count of all friends older then 18
 Friend.count(:conditions => [ 'gender = ?', 'female' ]) # returns count of all your female friends
 Friend.count(:address) # returns count of all friends with an address (NULL values are not included)
 Friend.count(:address, :age.gt => 18) # returns count of all friends with an address that are older then 18
 Friend.count(:address, :conditions => [ 'gender = ?', 'female' ]) # returns count of all your female friends with an address

== min

Get the lowest value of a property

 Friend.min(:age) # returns the age of the youngest friend
 Friend.min(:age, :conditions => [ 'gender = ?', 'female' ]) # returns the age of the youngest female friends

== max

Get the highest value of a property

 Friend.max(:age) # returns the age of the oldest friend
 Friend.max(:age, :conditions => [ 'gender = ?', 'female' ]) # returns the age of the oldest female friends

== avg

Get the average value of a property

 Friend.avg(:age) # returns the average age of friends
 Friend.avg(:age, :conditions => [ 'gender = ?', 'female' ]) # returns the average age of the female friends

== sum

Get the total value of a property

 Friend.sum(:age) # returns total age of all friends
 Friend.max(:age, :conditions => [ 'gender = ?', 'female' ]) # returns the total age of all female friends
