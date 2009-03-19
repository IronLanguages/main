require 'windows/api'

module Windows
   module MSVCRT
      module String
         API.auto_constant  = false # We want multiple versions
         API.auto_method    = false # We need to handle 0 & nil explicitly
         API.auto_unicode   = false

         Strchr   = API.new('strchr', 'PI', 'P', MSVCRT_DLL)
         Strcmp   = API.new('strcmp', 'PP', 'I', MSVCRT_DLL)
         Strcpy   = API.new('strcpy', 'PL', 'L', MSVCRT_DLL)
         Strcspn  = API.new('strcspn', 'PP', 'L', MSVCRT_DLL)
         Strlen   = API.new('strlen', 'P', 'L', MSVCRT_DLL)
         Strncpy  = API.new('strncpy', 'PPL', 'P', MSVCRT_DLL)
         Strpbrk  = API.new('strpbrk',  'PP', 'P', MSVCRT_DLL)
         Strrchr  = API.new('strrchr', 'PI', 'P', MSVCRT_DLL)
         Strrev   = API.new('_strrev', 'P', 'P', MSVCRT_DLL)
         Strspn   = API.new('strspn', 'PP', 'L', MSVCRT_DLL)
         Strstr   = API.new('strstr', 'PP', 'P', MSVCRT_DLL)
         Strtok   = API.new('strtok', 'PP', 'P', MSVCRT_DLL)
        
         Mbscmp   = API.new('_mbscmp', 'PP', 'I', 'msvcrt')
         Mbscpy   = API.new('_mbscpy', 'PL', 'L', 'msvcrt')
         Mbslen   = API.new('_mbslen', 'P', 'L', 'msvcrt')
         Mbsrev   = API.new('_mbsrev', 'P', 'P', 'msvcrt')
         
         Wcscmp   = API.new('wcscmp', 'PP', 'I', MSVCRT_DLL)
         Wcscpy   = API.new('wcscpy', 'PL', 'L', MSVCRT_DLL)
         Wcslen   = API.new('wcslen', 'P', 'L', MSVCRT_DLL)
         Wcsncpy  = API.new('wcsncpy', 'PPL', 'P', MSVCRT_DLL)
         Wcsrev   = API.new('_wcsrev', 'P', 'P', MSVCRT_DLL)
         
         begin
            Strtok_s = API.new('strtok_s', 'PPI', 'P', MSVCRT_DLL)
         rescue Windows::API::Error
            # Do nothing. Not supported on your system.
         end
         
         def strchr(string, char)
            return nil if string == 0 || char == 0
            Strchr.call(string, char)
         end

         def strcmp(str1, str2)
            if str1 == 0 || str2 == 0
               return nil
            end
            Strcmp.call(str1, str2)
         end
         
         def strcpy(dest, src)
            return nil if src == 0
            Strcpy.call(dest, src)
         end
         
         def strlen(string)
            return nil if string == 0
            Strlen.call(string)
         end
         
         def strcspn(string, charset)
            return nil if string == 0
            Strcspn.call(string, charset)
         end
         
         def strncpy(dest, source, count)
            return nil if source == 0
            Strncpy.call(dest, source, count)
         end

         def strpbrk(string, charset)
            return nil if string == 0 || charset == 0
            Strpbrk.call(string, charset)
         end
         
         def strrchr(string, int)
            return nil if string == 0
            Strrchr.call(string, int)
         end
         
         def strrev(str)
            return nil if str == 0
            Strrev.call(str)
         end

         def strspn(string, charset)
            return nil if string == 0 || charset == 0
            Strspn.call(string, charset)
         end

         def strstr(string, search)
            return nil if string == 0 || search == 0
            Strstr.call(string, search)
         end
         
         def strtok(token, delimeter)
            return nil if token == 0 || delimeter == 0
            Strtok.call(token, delimeter)
         end

         if defined? Strtok_s
            def strtok_s(token, delimeter, context)
               return nil if [token, delimter, context].include?(0)
	            Strtok_s.call(token, delimeter, context)
	         end
         end         
         
         def mbscmp(str1, str2)
            if str1 == 0 || str2 == 0
               return nil
            end
            Mbscmp.call(str1, str2)
         end
         
         def mbscpy(dest, src)
            return nil if src == 0
            Mbscpy.call(dest, src)
         end
         
         def mbslen(string)
            return nil if string == 0
            Mbslen.call(string)
         end
         
         def mbsrev(str)
            return nil if str == 0
            Mbsrev.call(str)
         end
         
         def wcscmp(str1, str2)
            if str1 == 0 || str2 == 0
               return nil
            end
            Wcscmp.call(str1, str2)
         end

         def wcscpy(dest, src)
            return nil if src == 0
            Wcscpy.call(dest, src)
         end
         
         def wcslen(string)
            return nil if string == 0
            Wcslen.call(string)
         end
         
         def wcsncpy(dest, source, count)
            return nil if source == 0
            Wcsncpy.call(dest, source, count)
         end          
         
         def wcsrev(str)
            return nil if str == 0
            Wcsrev.call(str)
         end  
      end
   end
end
