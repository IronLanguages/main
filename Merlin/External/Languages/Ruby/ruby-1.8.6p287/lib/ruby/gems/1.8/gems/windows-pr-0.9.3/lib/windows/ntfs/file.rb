require 'windows/api'

module Windows
   module NTFS
      module File
         API.auto_namespace = 'Windows::NTFS::File'
         API.auto_constant  = true
         API.auto_method    = true
         API.auto_unicode   = false

         FileAccessInformation            = 8
         FileAlignmentInformation         = 17
         FileAllInformation               = 18
         FileAllocationInformation        = 19
         FileAlternateNameInformation     = 21
         FileAttributeTagInformation      = 35
         FileBasicInformation             = 4
         FileBothDirectoryInformation     = 3
         FileCompletionInformation        = 30
         FileCompressionInformation       = 28
         FileDirectoryInformation         = 1
         FileDispositionInformation       = 13
         FileEaInformation                = 7
         FileEndOfFileInformation         = 20
         FileFullDirectoryInformation     = 2
         FileFullEaInformation            = 15
         FileHardLinkInformation          = 46
         FileIdBothDirectoryInformation   = 37
         FileIdFullDirectoryInformation   = 38
         FileInternalInformation          = 6
         FileLinkInformation              = 11
         FileMailslotQueryInformation     = 26
         FileMailslotSetInformation       = 27
         FileModeInformation              = 16
         FileMoveClusterInformation       = 31
         FileNameInformation              = 9
         FileNamesInformation             = 12
         FileNetworkOpenInformation       = 34
         FileObjectIdInformation          = 29
         FilePipeInformation              = 23
         FilePipeLocalInformation         = 24
         FilePipeRemoteInformation        = 25
         FilePositionInformation          = 14
         FileQuotaInformation             = 32
         FileRenameInformation            = 10
         FileReparsePointInformation      = 33
         FileShortNameInformation         = 40
         FileStandardInformation          = 5
         FileStreamInformation            = 22
         FileTrackingInformation          = 36
         FileValidDataLengthInformation   = 39

         STATUS_SUCCESS = 0

         API.new('NtQueryInformationFile', 'LPPLL', 'L', 'ntdll')
      end
   end
end
