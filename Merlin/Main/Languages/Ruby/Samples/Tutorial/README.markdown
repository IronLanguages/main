IronRuby Tutorial
=================

Description
-----------

The application is an interactive tutorial, allowing users to use a REPL window to
follow along the teaching material

Topics covered
--------------

- Creating WPF UI using XAML
  - Using Blend for UI design
  - Creating WPF FlowDocument from RDoc SimpleMarkup text
- Creating domain-specific-languages (DSLs) in Ruby
- Using the Test::Unit testing framework
- Creating an application that can be developed incrementally from an
  interactive session with ability to reload modified source files.

Running the app
---------------

  ir.exe wpf_tutorial.rb
  
  ir.exe wpf_tutorial.rb <tutorial_file_path>

Running the app interactively
-----------------------------

  ir.exe
  >>> load "wpf_tutorial.rb"
  => true
  >>> # Edit wpf_tutorial.rb. For example, change the settings on the window in
  >>> # the XAML
  >>> reload # This should show the new window now...
  => true


Running the tests
-----------------

  ir.exe test\test_console.rb

