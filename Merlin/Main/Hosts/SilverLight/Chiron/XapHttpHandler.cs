using System;
using System.Web;
using System.IO;
using System.Xml;

namespace Chiron {

    public class BaseXapHttpHandler : IHttpHandler {
        const string XAP_CONTENT_TYPE = "application/x-zip-compressed";
        public bool IsReusable { get { return false; } }

        internal void InternalProcessRequest(HttpContext context, Action<string, string> createXap) {
            // if there's a XAP file on disk already, simply write it to the 
            // response stream directly
            if (File.Exists(context.Request.PhysicalPath)) {
                context.Response.ContentType = XAP_CONTENT_TYPE;
                context.Response.WriteFile(context.Request.PhysicalPath);
            } else {
                var path = context.Request.Path;
                var vfilename = Path.GetFileNameWithoutExtension(VirtualPathUtility.GetFileName(path));
                var vdirpath = VirtualPathUtility.Combine(VirtualPathUtility.GetDirectory(path), vfilename);
                var pdirpath = context.Server.MapPath(vdirpath);

                // create in memory Xap archive
                createXap.Invoke(pdirpath, path);
            }
        }

        public virtual void ProcessRequest(HttpContext context) {
            throw new NotImplementedException("ProcessRequest needs to be implemented");
        }
    }

    public class XapHttpHandler : BaseXapHttpHandler {
        public override void ProcessRequest(HttpContext context) {
            InternalProcessRequest(context, delegate(string pdirpath, string path) {
                // create in memory XAP archive
                if (Directory.Exists(pdirpath)) {
                    MemoryStream ms = new MemoryStream();
                    ZipArchive xap = new ZipArchive(ms, FileAccess.Write);
                    xap.CopyFromDirectory(pdirpath, "");
                    xap.Close();
                    var xapBuffer = ms.ToArray();
                    context.Response.OutputStream.Write(xapBuffer, 0, xapBuffer.Length);
                } else {
                    throw new HttpException(404, "Missing " + path);
                }
            });
        }
    }
    
    public class XapDlrHttpHandler : BaseXapHttpHandler {
        public override void ProcessRequest(HttpContext context) {
            InternalProcessRequest(context, delegate(string pdirpath, string path) {
                // create in memory XAP archive
                if (Directory.Exists(pdirpath)) {
                    var xapBuffer = XapBuilder.XapToMemory(pdirpath);
                    context.Response.OutputStream.Write(xapBuffer, 0, xapBuffer.Length);
                } else {
                    throw new HttpException(404, "Missing " + path);
                }
            });
        }
    }
}
