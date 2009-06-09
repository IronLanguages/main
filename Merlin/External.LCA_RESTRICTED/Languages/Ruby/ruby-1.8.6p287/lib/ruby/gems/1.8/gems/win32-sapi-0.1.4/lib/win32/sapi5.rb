require "win32ole"

# The Win32 module serves as a namespace only.
module Win32

   #--
   # The values provided in the various initializers come from the Windows
   # registry. Look in HKLM\Software\Classes\CLSID.
   #++

   # An abstract base class for the SAPI automation classes
   class SAPI5 < WIN32OLE
      VERSION = '0.1.4'
   end
   
   class SpAudioFormat < SAPI5
      def initialize
         super("{9EF96870-E160-4792-820D-48CF0649E4EC}")
      end
   end
   
   class SpCustomStream < SAPI5
      def initialize
         super("{8DBEF13F-1948-4aa8-8CF0-048EEBED95D8}")
      end
   end

   class SpFileStream < SAPI5
      def initialize
         super("{947812B3-2AE1-4644-BA86-9E90DED7EC91}")
      end
   end
   
   class SpInProcRecoContext < SAPI5
      def initialize
         super("{73AD6842-ACE0-45E8-A4DD-8795881A2C2A}")
      end
   end
   
   class SpInProcRecognizer < SAPI5
      def initialize
         super("{41B89B6B-9399-11D2-9623-00C04F8EE628}")
      end
   end
   
   class SpLexicon < SAPI5
      def initialize
         super("{0655E396-25D0-11D3-9C26-00C04F8EF87C}")
      end
   end
   
   class SpMemoryStream < SAPI5
      def initialize
         super("{5FB7EF7D-DFF4-468a-B6B7-2FCBD188F994}")
      end
   end
   
   class SpMMAudioIn < SAPI5
      def initialize
         super("{CF3D2E50-53F2-11D2-960C-00C04F8EE628}")
      end
   end
   
   class SpMMAudioOut < SAPI5
      def initialize
         super("{A8C680EB-3D32-11D2-9EE7-00C04F797396}")
      end
   end
   
   class SpObjectToken < SAPI5
      def initialize
         super("{EF411752-3736-4CB4-9C8C-8EF4CCB58EFE}")
      end
   end
   
   class SpObjectTokenCategory < SAPI5
      def initialize
         super("{A910187F-0C7A-45AC-92CC-59EDAFB77B53}")
      end
   end
   
   class SpPhoneConverter < SAPI5
      def initialize
         super("{9185F743-1143-4C28-86B5-BFF14F20E5C8}")
      end
   end
   
   class SpPhraseInfoBuilder < SAPI5
      def initialize
         super("{C23FC28D-C55F-4720-8B32-91F73C2BD5D1}")
      end
   end
   
   class SpSharedRecoContext < SAPI5
      def initialize
         super("{47206204-5ECA-11D2-960F-00C04F8EE628}")
      end
   end
   
   class SpSharedRecognizer < SAPI5
      def initialize
         super("{3BEE4890-4FE9-4A37-8C1E-5E7E12791C1F}")
      end
   end
   
   class SpTextSelectionInformation < SAPI5
      def initialize
         super("{0F92030A-CBFD-4AB8-A164-FF5985547FF6}")
      end
   end
   
   class SpUnCompressedLexicon < SAPI5
      def initialize
         super("{C9E37C15-DF92-4727-85D6-72E5EEB6995A}")
      end
   end
      
   class SpVoice < SAPI5
      def initialize
         super("{96749377-3391-11D2-9EE3-00C04F797396}")
      end
   end
   
   class SpWaveFormatEx < SAPI5
      def initialize
         super("{C79A574C-63BE-44b9-801F-283F87F898BE}")
      end
   end
end