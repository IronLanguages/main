using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HostingTest {
    /// <summary>
    ///      
    /// Idea for testing paths :
    /// 1) Absolute Paths
    ///_TmpPath + "\\" + _TmpFile,
    /// 2) relative paths
    ///"..\\..\\"
    /// 3) Mapped Drive - how do we create a test - Mock Obj?
    /// 4) UNC - again how do we test this every time in env
    ///       we don't know about.
    /// 5) IP address - loopback (127.0.0.1) or mock?
    /// 6) Other languages
    /// 7) Long paths
    /// 8) Paths with different '.' placement
    /// 9) Paths that ...?
    /// 10) Paths with spaces
    /// 11) Paths with back slash and forward slash paths (i.e, '/' and '\')
    ///      were are the tests running? need find parent path to script
    /// 12) Nonexistent
    /// 
    ///  Other possible test cases things to check.
    ///  
    ///   'file only
    ///   "foo.txt"
    ///  
    ///  'absolute paths
    ///    "c:\"
    ///    "c:\foo.txt"
    ///    "c:\cstools\"
    ///    "c:\cstools\foo.txt"
    ///   
    ///  'relative paths
    ///    "..\..\"
    ///    "..\..\foo.txt"
    ///    "..\..\cstools\"
    ///    "..\..\cstools\foo.txt"
    ///   
    ///  'mapped drive
    ///    "m:\"
    ///    "m:\foo.txt"
    ///    "m:\cstools\"
    ///    "m:\cstools\foo.txt"
    ///   
    ///  'UNC
    ///    "\\some_machine_on_network"
    ///    "\\some_machine_on_network\foo.txt"
    ///    "\\some_machine_on_network\cstools"
    ///    "\\some_machine_on_network\cstools\foo.txt"
    ///   
    ///  'IP address
    ///    "\\"
    ///    "\\\foo.txt"
    ///    "\\\d$"
    ///    "\\\d$\cstools\"
    ///    "\\\d$\cstools\foo.txt"
    ///   
    ///  'URL
    ///    "ms.htm"
    ///    "www.microsoft.com"
    ///    "http://www.microsoft.com"
    ///    "http://www.microsoft.com/ms.htm"
    ///
    /// 
    ///  Refactor Todo :
    ///  
    ///  1) Might want to redesign this to use delegates or OOP so 
    ///     we can add 'Setup' and 'TearDown' features for individual tests.
    ///  2) Verify that the path does exist in order to avoid *false positives* etc
    ///  3) Try to make this a 'static' class.
    ///  4) Change GetAllPaths from function to a static property that
    ///     only runs once and check a flag to see if a the data has been populated
    ///     in order to run or not run.
    /// </summary>




    /// <summary>
    /// Container to hold each string path.
    /// 
    /// </summary>
    public class TestPathType {

        string _path = "<empty_path>";
        bool _isTmp = false;
        public bool IsTmp {

            set { _isTmp = value; }
            get { return _isTmp; }
        }



        public string Path {
            get { return _path; }
            set { _path = value; }
        }
        /// <summary>
        /// Create a test container with path defined 
        /// by user
        /// </summary>
        /// <param name="p">String path to test</param>
        public TestPathType(string p) {
            _path = p;
        }

        /// <summary>
        /// Constructor for path that is tmp
        /// </summary>
        /// <param name="p">Path to tmp file</param>
        /// <param name="isTmp">bool flag to indicate tmp file</param>
        public TestPathType(string p, bool isTmp) {
            _path = p;
            _isTmp = isTmp;
        }

        /// <summary>
        /// Default Empty Path
        /// </summary>
        public TestPathType() {

        }

        /// <summary>
        /// Destructor deletes any tmp files stored if this
        /// class nows about it.
        /// </summary>
        ~TestPathType() {
            if (_isTmp) {
                File.Delete(_path);
            }
        }

    }

    /// <summary>
    /// Positive Tests:
    ///     ABS_PATH, RELATIVE_PATH, DIR_SEP_FORWARD, DIR_SEP_BACKWARD
    /// Negative Tests:
    ///     Illegal chars in path
    ///     
    /// Add abilty to get name of Path Test if their is a failure.
    /// </summary>
    public class StandardTestPaths {
        // If these vars were actually defined the
        // would not be included in the 
        // AllPaths property because of the way
        // this class works.
        //public static int One, Two, Three;

        static string[] _paths; // Should this be StringBuilder?


        // Add the tests here
        public static TestPathType SpaceInPath = new TestPathType(
                                        "c:\\tmp\\lsdfja dfa  aslsl"
                                         );
        public static TestPathType MiscPath = new TestPathType(
                                        Path.GetTempFileName(),
                                        true);


        // How do I create a temp valid path with Spaces and 
        // then delete it safely after done using.
        // **** May need to do some redesign ****
        //public static TestPathType ValidSpaceInPath = new TestPathType(
          //                                      Path.GetTempPath(),


        public static TestPathType IllegalCharsInPath = new TestPathType(
                                         "c:\\sldks----as#%&~!!!+");




        /// <summary>
        /// Property 
        /// </summary>
        public static string[] AllPaths {
            get {
                if (null == _paths) {
                    //Populate if necessary
                    GetAllPaths();
                }
                //return (string[])_paths.Clone();
                return _paths;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static string[] CreatePathCombinations(string aPath)
        {
            // TODO : 
            // Check for depth of tmpPath Depending on how deep the
            // path is return a set of alternative but valid path
            // types - Maybe Create 3 functions.  one that calculates
            // the different paths and one for that automatically
            // picks a file and one for a passed in path. both will
            // call the calculated paths alternatives
            string pRoot = Path.GetFullPath(aPath);
            List<string> newPath = new List<string>();
            newPath.Add("<DUMMY_REPLACE_ME>");
            
            return newPath.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        public static string[] CreateBadPathCombinations(string aPath)
        {
            string pRoot = Path.GetFullPath(aPath);
            List<string> newPath = new List<string>();

            
            newPath.Add(pRoot + "\\..\\..\\~\\");
            newPath.Add("__88sdf>");
            newPath.Add(pRoot + "\\..\\blah_blah space\\");
            newPath.Add(pRoot + "/../.");
            newPath.Add(pRoot + "___ __ sllslss\\");

            string[] newStrs = newPath.ToArray();
            return newStrs;

        }

        /// <summary>
        /// Iterate over all the members that are of the StringPathContainer
        /// and return an array of path strings
        /// 
        /// TODO: What if there are no string in the array?
        /// </summary>
        /// <returns>Array of path strings</returns>
        static void GetAllPaths() {
            //PathStringCollection p = new PathStringCollection();

            Type t = typeof(StandardTestPaths);

            FieldInfo[] _attrib = t.GetFields();
            //object[] _attrib = theObjInf.GetCustomAttributes();
            TestPathType tt = new TestPathType();

            // List<string> StringLstType;
            // StringLstType strLst;
            List<string> strLst = new List<string>();

            foreach (FieldInfo n in _attrib) {
                try {
                    if (n.FieldType == typeof(TestPathType)) {
                        // Console.WriteLine("The method name is {0} of type {1}", n.Name, n.FieldType);

                        // Boxing...
                        object obj; // = newT;
                        obj = n.GetValue((object)(new StandardTestPaths()));
                        TestPathType newT = (TestPathType)obj;

                        // Console.WriteLine("The path {0}", newT.path);
                        strLst.Add(newT.Path);

                    }

                } catch (Exception e) {
                    Console.WriteLine("The error was {0}", e.ToString());
                }
            }
            _paths = strLst.ToArray();

        }

        /// <summary>
        ///  Test the StringPathContainer class
        /// </summary>
        /// <param name="args"></param>
        static void _Main(string[] args) {
            // Quick example using all the string path contents
            StandardTestPaths TestPaths = new StandardTestPaths();
            string[] paths = StandardTestPaths.AllPaths;

            foreach (string testp in paths) {
                Console.WriteLine("The path {0}", testp);
            }



        }
    }
}
