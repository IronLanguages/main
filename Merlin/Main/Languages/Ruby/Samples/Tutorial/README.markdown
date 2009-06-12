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

TODO
----

- Start tutorial button
- scratch REPL surface
- Collapsable tutorial section/chapter navigation
- UI to show all available tutorials
- Multi-line Ruby constructs (class/def/if/etc)
- Ruby syntax colorization
- Integration with RubyGems to download tutorial gems
- Multi-step tasks with hinting for next step
- UI surface for WPF tutorials, embedding external content, etc
- Use Task.legal_notice (needs some UI to show the copyright notices)
- Use Task.source_files (need to show a button which can open the source file in notepad)
- Silverlight support
- Voting feedback
- Callback to task before activating it so that the task can check for pre-reqs (from previous chapters,
  which might have been skipped), ask to skip to a subsequent task if the task does not make sense (in 
  the case where the Chapter is being executed a second-time or out of order), etc.
