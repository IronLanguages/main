using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using IronRuby.Builtins;
using IronRuby.StandardLibrary.StringIO;
using System.Diagnostics;

namespace IronRuby.Rack.Handler {
    public class IIS {

        private readonly Stopwatch _watch = new Stopwatch();

        public readonly Application App;
        
        private static IIS _current;
        public  static IIS  Current { get { return _current; } }

        private IIS(Application app) {
            App = app;
        }

        public static void Run(Application app) {
            Run(app, new Dictionary<string, string>());
        }

        public static void Run(Application app, 
            IDictionary<string, string> options) {
            // TODO: make sure IIS is running

            if (_current == null) {
                 _current = new IIS(app);
            }
        }

        public void Handle(Request request, Response response) {
            Utils.Log("");
            Utils.Log("=== Request started at " + DateTime.Now.ToString());
            _watch.Reset();
            _watch.Start();

            var handle_scope = RubyEngine.Engine.CreateScope(); 
            handle_scope.SetVariable("__request", request);
            handle_scope.SetVariable("__response", response);

            // The environment must be an true instance of Hash (no subclassing
            // allowed) that includes CGI-like headers. The application is free 
            // to modify the environment. The environment is required to 
            // include these variables (adopted from PEP333), except when 
            // they’d be empty, but see below.
            var env = new Hash(RubyEngine.Context);

            // REQUEST_METHOD:	The HTTP request method, such as “GET” or 
            //   “POST”. This cannot ever be an empty string, and so is always
            //   required.
            // The REQUEST_METHOD must be a valid token.

            env["REQUEST_METHOD"] = request.OrigionalRequest.RequestType;

            // SCRIPT_NAME:	The initial portion of the request URL’s “path” 
            //   that corresponds to the application object, so that the 
            //   application knows its virtual “location”. This may be an empty
            //   string, if the application corresponds to the “root” of the 
            //   server.
            // The SCRIPT_NAME, if non-empty, must start with /
            
            env["SCRIPT_NAME"] = request.OrigionalRequest.Path;
            if(env["SCRIPT_NAME"].ToString() != string.Empty && !((string)env["SCRIPT_NAME"]).StartsWith("/")) {
                env["SCRIPT_NAME"] = "/" + env["SCRIPT_NAME"];
            }
            
            // SCRIPT_NAME never should be /, but instead be empty.

            if(env["SCRIPT_NAME"].ToString() == "/") 
                env["SCRIPT_NAME"] = "";
            
            // PATH_INFO:	The remainder of the request URL’s “path”, 
            //   designating the virtual “location” of the request’s target 
            //   within the application. This may be an empty string, if the 
            //   request URL targets the application root and does not have a 
            //   trailing slash. This value may be percent-encoded when I 
            //   originating from a URL.
            // The PATH_INFO, if non-empty, must start with /
           
            env["PATH_INFO"] = request.OrigionalRequest.PathInfo;
            if(env["PATH_INFO"].ToString() != string.Empty && !((string)env["PATH_INFO"]).StartsWith("/")) {
                env["PATH_INFO"] = "/" + env["PATH_INFO"];
            }
            
            // PATH_INFO should be / if SCRIPT_NAME is empty. 
            
            if(env["SCRIPT_NAME"].ToString() == string.Empty) 
                env["PATH_INFO"] = "/";

            // One of SCRIPT_NAME or PATH_INFO must be set.

            // TODO

            // QUERY_STRING:	The portion of the request URL that follows the
            //   ?, if any. May be empty, but is always required!
            
            env["QUERY_STRING"] = request.QueryString ?? "";

            // SERVER_NAME, SERVER_PORT:	When combined with SCRIPT_NAME and
            //   PATH_INFO, these variables can be used to complete the URL. 
            //   Note, however, that HTTP_HOST, if present, should be used in
            //   preference to SERVER_NAME for reconstructing the request URL. 
            //   SERVER_NAME and SERVER_PORT can never be empty strings, and so
            //   are always required.
            
            env["SERVER_NAME"] = request.OrigionalRequest.Url.Host;
            env["SERVER_PORT"] = request.OrigionalRequest.Url.Port;
            
            // HTTP_ Variables:	Variables corresponding to the client-supplied 
            //   HTTP request headers (i.e., variables whose names begin with 
            //   HTTP_). The presence or absence of these variables should 
            //   correspond with the presence or absence of the appropriate 
            //   HTTP header in the request.
            // The CONTENT_LENGTH, if given, must consist of digits only.
            
            foreach (var pair in request.Headers) {
                var prepend = "HTTP_";
                if (pair.Key.ToString() == "Content-Length" || pair.Key.ToString() == "Content-Type")
                    prepend = "";
                env[prepend + pair.Key.ToString().ToUpper().Replace("-", "_")] = pair.Value;
            }

            // In addition to this, the Rack environment must include these 
            // Rack-specific variables:
            //
            // rack.version:	The Array [1,0], representing this version of 
            //   Rack.
            
            env["rack.version"] = new int[]{1,0};
            
            // rack.url_scheme:	http or https, depending on the request URL.
            
            env["rack.url_scheme"] = request.OrigionalRequest.IsSecureConnection ? "https" : "http";

            // rack.input: The input stream is an IO-like object which contains
            // the raw HTTP POST data. If it is a file then it must be opened 
            // in binary mode. The input stream must respond to gets, each, 
            // read and rewind.
            //
            // gets must be called without arguments and return a string, or 
            //   nil on EOF.
            // read behaves like IO#read. Its signature is 
            //   read([length, [buffer]]). If given, length must be an 
            //   non-negative Integer (>= 0) or nil, and buffer must be a String
            //   and may not be nil. If length is given and not nil, then this 
            //   method reads at most length bytes from the input stream. If 
            //   length is not given or nil, then this method reads all data 
            //   until EOF. When EOF is reached, this method returns nil if 
            //   length is given and not nil, or “” if length is not given or 
            //   is nil. If buffer is given, then the read data will be placed 
            //   into buffer instead of a newly created String object.
            // each must be called without arguments and only yield Strings.
            // rewind must be called without arguments. It rewinds the input
            //   stream back to the beginning. It must not raise Errno::ESPIPE:
            //   that is, it may not be a pipe or a socket. Therefore, handler
            //   developers must buffer the input data into some rewindable
            //   object if the underlying input stream is not rewindable.
            // close must never be called on the input stream.
            
            env["rack.input"] = RubyEngine.Execute("StringIO.new(__request.body || '')", handle_scope);

            // rack.errors:	The error stream must respond to puts, write and flush.
            //
            // puts must be called with a single argument that responds to to_s.
            // write must be called with a single argument that is a String.
            // flush must be called without arguments and must be called in 
            //   order to make the error appear for sure.
            // close must never be called on the error stream.
            
            env["rack.errors"] = RubyEngine.Execute("$stderr");

            // rack.multithread:	true if the application object may be simultaneously
            //   invoked by another thread in the same process, false otherwise.
            
            env["rack.multithread"] = true;

            // rack.multiprocess:	true if an equivalent application object may be
            //   simultaneously invoked by another process, false otherwise.
            
            env["rack.multiprocess"] = false;
            
            // rack.run_once:	true if the server expects (but does not guarantee!)
            //   that the application will only be invoked this one time during the 
            //   life of its containing process. Normally, this will only be true for
            //   a server based on CGI (or something similar).
            
            env["rack.run_once"] = false;
            
            // Additional environment specifications have approved to standardized
            // middleware APIs. None of these are required to be implemented by the server.
            //
            // rack.session:	A hash like interface for storing request session data. 
            //   The store must implement: 
            //     store(key, value) (aliased as []=); 
            //     fetch(key, default = nil) (aliased as []); 
            //     delete(key); 
            //     clear;

            // noop

            // The server or the application can store their own data in the
            // environment, too. The keys must contain at least one dot, and 
            // should be prefixed uniquely. The prefix rack. is reserved for 
            // use with the Rack core distribution and other accepted 
            // specifications and must not be used otherwise. The environment 
            // must not contain the keys HTTP_CONTENT_TYPE or HTTP_CONTENT_LENGTH 
            // (use the versions without HTTP_). The CGI keys (named without a period)
            // must have String values.

            env.Remove("HTTP_CONTENT_LENGTH");
            env.Remove("HTTP_CONTENT_TYPE");

            // A Rack application is an Ruby object (not a class) that responds
            // to call. It takes exactly one argument, the environment and 
            // returns an Array of exactly three values: The status, the headers, 
            // and the body.

            RubyArray ruby_response = App.Call(env);

            try {
                handle_scope.SetVariable("__body", ruby_response[2]);

                // The Response
                // ============
                
                // The Status
                // ----------
                // This is an HTTP status. When parsed as integer (to_i), it
                // must be greater than or equal to 100.
                
                response.Status = (int)ruby_response[0];

                // The Headers
                // -----------
                // The header must respond to each, and yield values of key and
                // value. The header keys must be Strings. The header must not
                // contain a Status key, contain keys with : or newlines in 
                // their name, contain keys names that end in - or _, but only 
                // contain keys that consist of letters, digits, _ or - and 
                // start with a letter. The values of the header must be 
                // Strings, consisting of lines (for multiple header values, 
                // e.g. multiple Set-Cookie values) seperated by “n“. The lines
                // must not contain characters below 037.
                
                foreach (var header in ((Hash)ruby_response[1])) {
                    foreach (var value in header.Value.ToString().Split('\n')) {
                        response.AppendHeader(header.Key.ToString(), value.ToString());
                    }
                }
                
                // The Content-Type
                // ----------------
                // There must be a Content-Type, except when the Status is 1xx,
                // 204 or 304, in which case there must be none given.
                
                // TODO

                // The Content-Length
                // ------------------
                // There must not be a Content-Length header when the Status is
                // 1xx, 204 or 304.
            
                // TODO
    
                // The Body
                // --------
                // The Body must respond to each and must only yield String 
                // values. The Body itself should not be an instance of String,
                // as this will break in Ruby 1.9. If the Body responds to
                // close, it will be called after iteration. If the Body
                // responds to to_path, it must return a String identifying the
                // location of a file whose contents are identical to that
                // produced by calling each; this may be used by the server as
                // an alternative, possibly more efficient way to transport the 
                // response. The Body commonly is an Array of Strings, the
                // application instance itself, or a File-like object.

                RubyEngine.Execute("__body.each { |part| __response.write part }", handle_scope);

            } finally {
                RubyEngine.Execute("__body.close if __body.respond_to? :close", handle_scope);
                _watch.Stop();
                Utils.Log(">>> Request finished (" + _watch.ElapsedMilliseconds.ToString() + "ms)");
            }
        }

    }
}
