namespace IronPython {{
    public static class CurrentVersion {{
        public const int Major = {0};
        public const int Minor = {1};
        public const int Micro = {2};
        public const string ReleaseLevel = "{3}";
        public const int ReleaseSerial = {4};

        public const string ShortReleaseLevel = "{5}";

        public const string Series = "{0}.{1}";
        public const string DisplayVersion = "{6}";
        public const string DisplayName = "IronPython {6}";

#if !SILVERLIGHT
        public const string AssemblyVersion = "{0}.{1}.0.{7}";
#else
        public const string AssemblyVersion = "{0}.{1}.1300.{7}";
#endif
        
        public const string AssemblyFileVersion = "{0}.{1}.{2}.{8}";
        public const string AssemblyInformationalVersion = "IronPython {0}.{1}.{2} {3} {4}";
    }}
}}