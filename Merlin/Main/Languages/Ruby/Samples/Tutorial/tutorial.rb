class Task
    attr :name
    attr :description
    attr :hint

    def initialize name, description, hint, &success_condition
        @name = name
                
        @description = description.gsub(/\n\s*\w/, "\n\s")
        
        if hint.kind_of? Array
            @hint = hint.join("\n")
        else
            @hint = hint
        end
        @success_condition = success_condition
    end

    def to_s
        @name
    end
    
    def success? bind, output, result
        begin
            return @success_condition.call(bind, output, result)
        rescue
            false
        end
    end
end

class Chapter
    attr :name, true ############### true is needed only to workaround data-binding bug
    attr :description
    attr :tasks

    def initialize name, description, tasks
        @name = name
        @description = description
        @tasks = tasks
    end

    def to_s
        @name
    end
end

class Section
    attr :name, true ############### true is needed only to workaround data-binding bug
    attr :description
    attr :chapters, true ############### true is needed only to workaround data-binding bug

    def initialize name, description, chapters=nil
        @name = name
        @description = description
        if chapters
            @chapters = chapters
        else
            chapter_names = self.class.instance_methods.select { |name| name =~ /_chapter$/ }
            chapter_factories = chapter_names.collect { |name| self.method(name) }
            @chapters = chapter_factories.collect { |factory| factory.call }
        end
    end

    def to_s
        @name
    end    
end

class Tutorial
    attr :name
    attr :sections

    def initialize name, sections
        @name = name
        @sections = sections
    end
    
    def to_s
        @name
    end
end

