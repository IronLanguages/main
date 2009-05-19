# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

module Tutorial

	# If the first line contains "heredoc:", the first line is removed. This allows
	# use of simple multi-line strings which are easier to use than heredoc strings. Heredoc strings
	# cannot be used easily as arguments to functions if the arguments are written on separate lines.
	def self.check_pseudo_heredoc text
		if text =~ /\Aheredoc:\n(.*)/m
			text = $1
		end
		
		text
	end
	
	class Task
		attr :description
		attr :code

		def initialize description, code, &success_evaluator	                
			@description = ::Tutorial.check_pseudo_heredoc description
			@code = code
			@success_evaluator = success_evaluator
		end

		def success? interaction
			begin
				return @success_evaluator.call(interaction)
			rescue
				false
			end
		end
	end

	class Chapter
		attr :name, true # TODO - true is needed only to workaround data-binding bug
		attr :introduction, true
		attr :summary, true
		attr :tasks, true
		attr :next_item, true

		def initialize name, introduction = nil, summary = nil, tasks = [], next_item = nil
			@name = name
			@introduction = introduction
			@summary = summary
			@tasks = tasks
			@next_item = next_item
		end

		def to_s
			@name
		end
	end

	class Section
		attr :name, true # TODO - true is needed only to workaround data-binding bug
		attr :introduction, true
		attr :chapters, true

		def initialize name, introduction = nil, chapters = []
			@name = name
			@introduction = introduction
			@chapters = chapters
		end

		def to_s
			@name
		end    
	end

	class Tutorial
		attr :name
		attr :introduction, true
		attr :summary, true
		attr :sections, true

		def initialize name, introduction = nil, summary = nil, sections = []
			@name = name
			@introduction = introduction
			@summary = summary
			@sections = sections
		end
	    
		def to_s
			@name
		end
	end
	
	@@tutorials = {}
	
	@@ironruby_tutorial_path = File.expand_path("Tutorials/ironruby_tutorial", File.dirname(__FILE__))
	
	def self.get_tutorial path = @@ironruby_tutorial_path
		if not @@tutorials.has_key? path		
			require path
			raise "path does not contains a tutorial definition" if not Thread.current[:tutorial]
			@@tutorials[path] = Thread.current[:tutorial]
			Thread.current[:tutorial] = nil
		end
		
		return @@tutorials[path]
	end
	
	class InteractionResult
		attr :bind
		attr :output
		attr :result
		attr :exception
		
		def initialize(bind, output, result, exception = nil)
			@bind = bind
			@output = output
			@result = result
			@exception = exception
			
			raise "result should be nil if an exception was raised" if result and exception
		end
		
		def result
			raise "Interaction resulted in an exception" if exception
			@result
		end
	end
end

class Object
	def tutorial name
		Thread.current[:tutorial] = Tutorial::Tutorial.new name
		Thread.current[:prev_chapter] = nil
		yield
	end

	def introduction intro
		intro = Tutorial.check_pseudo_heredoc intro
		if Thread.current[:chapter]
			Thread.current[:chapter].introduction = intro
		elsif Thread.current[:section]
			Thread.current[:section].introduction = intro
		elsif Thread.current[:tutorial]
			Thread.current[:tutorial].introduction = intro
		else
			raise "introduction should only be used within a tutorial definition"
		end
	end
	
	def summary s
		if Thread.current[:tutorial]
			Thread.current[:tutorial].summary = Tutorial.check_pseudo_heredoc s
		else
			raise "summary should only be used within a tutorial definition"
		end
	end
		
	def section name
		section = Tutorial::Section.new name
		Thread.current[:section] = section
		if Thread.current[:prev_chapter]
			Thread.current[:prev_chapter].next_item = section
		end

		yield

		Thread.current[:tutorial].sections << section
		Thread.current[:section] = nil
	end

	def chapter name
		chapter = Tutorial::Chapter.new name
		Thread.current[:chapter] = chapter
		if Thread.current[:prev_chapter]
			Thread.current[:prev_chapter].next_item = chapter
		end

		yield

		Thread.current[:section].chapters << chapter
		Thread.current[:prev_chapter] = chapter
		Thread.current[:chapter] = nil
	end

	def task(name, code, &success_evaluator)
		Thread.current[:chapter].tasks << Tutorial::Task.new(name, code, &success_evaluator)
	end

	# This represents an operation that consists of several closely realated task. It is useful to have
	# direct support for this so that the UI can better support it (by showing all the descriptions on the same
	# page)
	def multi_step_task(name, *tasks, &success_evaluator)
		raise NotImplementedError
	end
end