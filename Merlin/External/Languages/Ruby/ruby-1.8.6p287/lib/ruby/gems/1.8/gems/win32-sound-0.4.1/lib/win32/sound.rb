require 'Win32API'
module Win32
   class Sound
      class Error < StandardError; end

      VERSION        = '0.4.1'
      LOW_FREQUENCY  = 37
      HIGH_FREQUENCY = 32767
      MAX_VOLUME     = 0xFFFF
      
      SYNC           = 0x0000  # play synchronously (default)
      ASYNC          = 0x0001  # play asynchronously
      NODEFAULT      = 0x0002  # silence (!default) if sound not found
      MEMORY         = 0x0004  # pszSound points to a memory file
      LOOP           = 0x0008  # loop the sound until next sndPlaySound
      NOSTOP         = 0x0010  # don't stop any currently playing sound 
      NOWAIT         = 8192    # don't wait if the driver is busy
      ALIAS          = 65536   # name is a registry alias
      ALIAS_ID       = 1114112 # alias is a predefined ID
      FILENAME       = 131072  # name is file name
      RESOURCE       = 262148  # name is resource name or atom
      PURGE          = 0x0040  # purge non-static events for task
      APPLICATION    = 0x0080  # look for application specific association
      
      @@Beep              = Win32API.new('kernel32', 'Beep', 'LL', 'I')
      @@PlaySound         = Win32API.new('winmm', 'PlaySound', 'PPL', 'I')
      @@waveOutSetVolume  = Win32API.new('winmm', 'waveOutSetVolume', 'PL', 'I')
      @@waveOutGetVolume  = Win32API.new('winmm', 'waveOutGetVolume', 'IP', 'I')
      @@waveOutGetNumDevs = Win32API.new('winmm', 'waveOutGetNumDevs', 'V', 'I')
      @@waveInGetNumDevs  = Win32API.new('winmm', 'waveInGetNumDevs', 'V', 'I')
      @@midiOutGetNumDevs = Win32API.new('winmm', 'midiOutGetNumDevs', 'V', 'I')
      @@midiInGetNumDevs  = Win32API.new('winmm', 'midiInGetNumDevs', 'V', 'I')
      @@auxGetNumDevs     = Win32API.new('winmm', 'auxGetNumDevs', 'V', 'I')
      @@mixerGetNumDevs   = Win32API.new('winmm', 'mixerGetNumDevs', 'V', 'I')
           
      @@GetLastError  = Win32API.new('kernel32', 'GetLastError', '', 'L')
      @@FormatMessage = Win32API.new('kernel32', 'FormatMessage', 'LPLLPLP', 'L')
      
      # Returns an array of all the available sound devices; their names contain
      # the type of the device and a zero-based ID number.  Possible return values
      # are WAVEOUT, WAVEIN, MIDIOUT, MIDIIN, AUX or MIXER.
      def self.devices
         devs = []
         
         begin
            0.upto(@@waveOutGetNumDevs.call){ |i| devs << "WAVEOUT#{i}" }
            0.upto(@@waveInGetNumDevs.call){ |i| devs << "WAVEIN#{i}" }
            0.upto(@@midiOutGetNumDevs.call){ |i| devs << "MIDIOUT#{i}" }
            0.upto(@@midiInGetNumDevs.call){ |i| devs << "MIDIIN#{i}" }
            0.upto(@@auxGetNumDevs.call){ |i| devs << "AUX#{i}" }
            0.upto(@@mixerGetNumDevs.call){ |i| devs << "MIXER#{i}" }
         rescue Exception => err
            raise Error, get_last_error
         end
         
         devs
      end

      # Generates simple tones on the speaker. The function is synchronous; it
      # does not return control to its caller until the sound finishes.
	  #
	  # The frequency (in Hertz) must be between 37 and 32767.
      # The duration is in milliseconds.
      def self.beep(frequency, duration)
         if frequency > HIGH_FREQUENCY || frequency < LOW_FREQUENCY
            raise Error, 'invalid frequency'
         end
         
         if 0 == @@Beep.call(frequency, duration)
            raise Error, get_last_error
         end        
         self
      end
      
      # Stops any currently playing waveform sound.  If +purge+ is set to
      # true, then *all* sounds are stopped.  The default is false.
      def self.stop(purge = false)
         if purge && purge != 0
            flags = PURGE
         else
            flags = 0
         end
         
         if 0 == @@PlaySound.call(0, 0, flags)
            raise Error, get_last_error
         end
         self
      end
      
      # Plays the specified sound.  The sound can be a wave file or a system
      # sound, when used in conjunction with the ALIAS flag.
      #
      # Valid flags:
      #
      # Sound::ALIAS
	  #   The sound parameter is a system-event alias in the registry or the
	  #   WIN.INI file. If the registry contains no such name, it plays the
	  #   system default sound unless the NODEFAULT value is also specified.
	  #   Do not use with FILENAME.
	  #
      # Sound::APPLICATION
	  #   The sound is played using an application-specific association.
	  #
	  # Sound::ASYNC
	  #   The sound is played asynchronously and the function returns
	  #   immediately after beginning the sound.
	  #
	  # Sound::FILENAME
	  #   The sound parameter is the name of a WAV file.  Do not use with 
	  #   ALIAS.
	  #
	  # Sound::LOOP
	  #   The sound plays repeatedly until Sound.stop() is called. You must
	  #   also specify the ASYNC flag to loop sounds.
 	  #
 	  # Sound::MEMORY
	  #   The sound points to an image of a waveform sound in memory.
      #	
      # Sound::NODEFAULT
	  #   If the sound cannot be found, the function returns silently without
	  #   playing the default sound.
	  #
	  # Sound::NOSTOP
	  #   If a sound is currently playing, the function immediately returns
	  #   false without playing the requested sound.
	  #
      # Sound::NOWAIT
	  #   If the driver is busy, return immediately without playing the sound.
	  #
      # Sound::PURGE
	  #   Stop playing all instances of the specified sound.
      #	
      # Sound::SYNC
	  #   The sound is played synchronously and the function does not return
	  #   until the sound ends.
      def self.play(sound, flags = 0)
         if 0 == @@PlaySound.call(sound, 0, flags)
            raise Error, get_last_error
         end
         self
      end
      
      # Sets the volume for the left and right channel.  If the +right_channel+
      # is omitted, the volume is set for *both* channels.
	  #
      # You may optionally pass a single Integer rather than an Array, in which
      # case it is assumed you are setting both channels to the same value.
      def self.set_wave_volume(left_channel, right_channel = nil)
         right_channel ||= left_channel
         
         lvolume = left_channel > MAX_VOLUME ? MAX_VOLUME : left_channel
         rvolume = right_channel > MAX_VOLUME ? MAX_VOLUME : right_channel
         
         volume = lvolume | rvolume << 16
         
         if @@waveOutSetVolume.call(-1, volume) != 0
            raise Error, get_last_error
         end
         self
      end
      
      # Returns a 2-element array that contains the volume for the left channel
      # and right channel, respectively.
      def self.wave_volume
         volume = [0].pack('L')
         if @@waveOutGetVolume.call(-1, volume) != 0
            raise Error, get_last_error
         end
         volume = volume.unpack('L').first
         [low_word(volume), high_word(volume)]
      end
      
      # Alias for Sound.wave_volume
      def self.get_wave_volume
         self.wave_volume
      end
      
      private
      
      def self.low_word(num)
         num & 0xFFFF
      end
      
      def self.high_word(num)
         num >> 16
      end
      
      # Convenience method that wraps FormatMessage with some sane defaults.
      def self.get_last_error(err_num = @@GetLastError.call)
         buf = 0.chr * 260
         @@FormatMessage.call(
            12288,
            0,
            err_num,
            0,
            buf,
            buf.length,
            0
         )
         buf.split(0.chr).first.chomp
      end
   end
end
