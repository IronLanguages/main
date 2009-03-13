# Copyright (c) 2005 Martin Ankerl
class Search_Engine
  def initialize(gui, data)
    @gui = gui
    @data = data
    @search_thread = nil
  end

  # Executed whenever a search criteria changes to update the packet list.
  def on_search
    # restart current search
    @end_time = Time.now + $cfg.search_delay
    @restart_search = true
    @gui.search_label.enabled = false
    return if @search_thread && @search_thread.status

    @search_thread = Thread.new(@search_thread) do
      begin
        @gui.search_label.enabled = false
        # wait untill deadline
        while (t = (@end_time - Time.now)) > 0
          sleep(t)
        end

        @data.gui_mutex.synchronize do
          # the thread has to use the gui mutex inside
          @restart_search = false

          match_data = get_match_data

          # remove all items
          @gui.packet_list.dirty_clear

          # add all items that match the search criteria
          status_text_deadline = Time.now + $cfg.status_line_update_interval
          @data.items.each do |item|
            #item.parent = @gui.packet_list if match?(item, match_data)
            if match?(item, match_data)
              item.show
              now = Time.now
              if now > status_text_deadline
                update_search_status_text
                status_text_deadline = now + $cfg.status_line_update_interval
              end
            end
            break if @restart_search
          end
          update_search_status_text

          if (@gui.packet_list.numItems > 0)
            @gui.packet_list.setCurrentItem(0)
            @gui.packet_list.selectItem(0)
            @gui.main.show_info(@gui.packet_list.getItem(0).packet_item.data)
          end
          @gui.search_label.enabled = true

        end # synchronize
      end while @restart_search# || match_data != @gui.search_field.text.downcase.split
    end #thread.new
  end

  def get_match_data
    str_to_match_data(@gui.search_field.text)
  end

  # Converts a string into a match_data representation.
  def str_to_match_data(str, index=0)
    words = [ ]
    exclude = [ ]
    is_exclude = false

    while str[index]
      case str[index]
      when ?", ?'
        word, index = get_word(str, index+1, str[index])
        unless word.empty?
          if is_exclude
            exclude.push word
            is_exclude = false
          else
            words.push word
          end
        end

      when 32 # space
        is_exclude = false

=begin
      when ?>
        min, index = get_word(str, index+1)
        min = @gui.logic.size_to_nr(min)

      when ?<
        max, index = get_word(str, index+1)
        max = @gui.logic.size_to_nr(max)
=end
      when ?-
        is_exclude = true

      else
        word, index = get_word(str, index)
        if is_exclude
          exclude.push word
          is_exclude = false
        else
          words.push word
        end
      end

      index += 1
    end

    # check if word has upcase letters
    words.collect! do |w|
      [w, /[A-Z]/.match(w)!=nil]
    end
    exclude.collect! do |w|
      [w, /[A-Z]/.match(w)!=nil]
    end
    [words, exclude]
  end

  def get_word(str, index, delim=32) # 32==space
    word = ""
    c = str[index]
    while (c && c != delim)
      word += c.chr
      index += 1
      c = str[index]
    end
    [word, index]
  end

  # Update the text for the number of displayed packs.
  def update_search_status_text
    @gui.search_label.text = sprintf($cfg.text.search, @gui.packet_list.numItems, @data.items.size)
  end

  # Find out if item is matched by current search criteria.
  def match?(item, match_data)
    words, exclude, min, max = match_data

    # check text that has to be there
    searchable, sortable_downcase, sortable_normal = item.sortable(0)
    words.each do |match_str, is_sensitive|
      if is_sensitive
        return false unless sortable_normal.include?(match_str)
      else
        return false unless sortable_downcase.include?(match_str)
      end
    end

    # check text not allowed to be there
    exclude.each do |match_str, is_sensitive|
      if is_sensitive
        return false if sortable_normal.include?(match_str)
      else
        return false if sortable_downcase.include?(match_str)
      end
    end

    # each check ok
    true
  end
end
