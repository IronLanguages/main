/***************************************************
 * open3.c
 *
 * Source for the win32-open3 extension.
 ***************************************************/
#include "ruby.h"
#include "rubysig.h"
#include "rubyio.h"
#include "open3.h"

#include <malloc.h>
#include <io.h>
#include <string.h>
#include <fcntl.h>
#include <sys/stat.h>

/* Necessary to work with Ruby 1.8.3 or later */
#ifdef HAVE_TYPE_RB_PID_T
#define pid_t rb_pid_t
#endif

static VALUE win32_last_status = Qnil;
static HANDLE pid_handle = NULL;

static VALUE ruby_popen(char *, int, VALUE);

static int rb_io_mode_flags2(int mode){
   int flags;

   switch (mode & (O_RDONLY|O_WRONLY|O_RDWR)) {
      case O_RDONLY:
         flags = FMODE_READABLE;
         break;
      case O_WRONLY:
         flags = FMODE_WRITABLE;
         break;
      case O_RDWR:
         flags = FMODE_WRITABLE|FMODE_READABLE;
         break;
   }

#ifdef O_BINARY
   if(mode & O_BINARY)
      flags |= FMODE_BINMODE;
#endif

   return flags;
}

static char* rb_io_modenum_mode(int flags, char* mode){
    char *p = mode;

    switch(flags & (O_RDONLY|O_WRONLY|O_RDWR)){
      case O_RDONLY:
         *p++ = 'r';
         break;
      case O_WRONLY:
         *p++ = 'w';
         break;
      case O_RDWR:
         *p++ = 'r';
         *p++ = '+';
         break;
   }

   *p++ = '\0';

#ifdef O_BINARY
   if(flags & O_BINARY){
      if(mode[1] == '+'){
         mode[1] = 'b';
         mode[2] = '+';
         mode[3] = '\0';
      }
      else{
         mode[1] = 'b'; mode[2] = '\0';
      }
   }
#endif
   return mode;
}

/* Used to close io handle */
static VALUE io_close(VALUE val) {
   int i;

   for(i = 0; i < 3; i++){
      if(rb_funcall(RARRAY(val)->ptr[i], rb_intern("closed?"), 0) == Qfalse)
         rb_funcall(RARRAY(val)->ptr[i], rb_intern("close"), 0);
   }

   return Qnil;
}

/*
 * call-seq:
 *    Open3.popen3(cmd, mode='t', show=false)
 *    Open3.popen3(cmd, mode='t', show=false){ |io_in, io_out, io_err| ... }
 *
 * Executes 'command', returning an array of three IO handles representing
 * STDIN, STDOUT and STDERR, respectively.  In block form these IO handles
 * are yielded back to the block and automatically closed at the end of the
 * block.
 *
 * You may optionally pass a mode flag of 't' (text, the default) or 'b'
 * (binary) to this method.
 *
 * If the 'show' variable is set to true, then a console window is shown.
 */
static VALUE win32_popen3(int argc, VALUE *argv, VALUE klass)
{
   VALUE v_name, v_mode, v_port;
   VALUE v_show_window = Qfalse;
   char mbuf[4];
   int tm = 0;
   char *mode = "t";

   rb_scan_args(argc, argv, "12", &v_name, &v_mode, &v_show_window);

   // Mode can be either a string or a number
   if(!NIL_P(v_mode)){
      if(FIXNUM_P(v_mode))
         mode = rb_io_modenum_mode(FIX2INT(v_mode), mbuf);
      else
         mode = StringValuePtr(v_mode);
   }

   if(*mode == 't')
      tm = _O_TEXT;
   else if(*mode != 'b')
      rb_raise(rb_eArgError, "popen3() arg 2 must be 't' or 'b'");
   else
      tm = _O_BINARY;

   v_port = ruby_popen(StringValuePtr(v_name), tm, v_show_window);

   // Ensure handles are closed in block form
   if(rb_block_given_p()){
      rb_ensure(rb_yield_splat, v_port, io_close, v_port);
      return win32_last_status;
   }

   return v_port;
}

static BOOL RubyCreateProcess(char *cmdstring, HANDLE hStdin, HANDLE hStdout,
   HANDLE hStderr, HANDLE *hProcess, pid_t *pid, VALUE v_show_window)
{
   PROCESS_INFORMATION piProcInfo;
   STARTUPINFO siStartInfo;
   char *s1,*s2, *s3 = " /c ";
   int i, x;

   if(i = GetEnvironmentVariable("COMSPEC", NULL, 0)){
      char *comshell;
      s1 = (char *)_alloca(i);

      if(!(x = GetEnvironmentVariable("COMSPEC", s1, i)))
         return x;

      /* Explicitly check if we are using COMMAND.COM. If we are
       * then use the w9xpopen hack.
       */
      comshell = s1 + x;

      while(comshell >= s1 && *comshell != '\\')
         --comshell;

      ++comshell;

      // Windows 95, 98 and ME are not supported
      if(GetVersion() < 0x80000000 && _stricmp(comshell, "command.com") != 0){
         x = i + strlen(s3) + strlen(cmdstring) + 1;
         s2 = ALLOCA_N(char, x);
         sprintf(s2, "%s%s%s", s1, s3, cmdstring);
      }
      else{
         rb_raise(rb_eRuntimeError,"not supported on this platform");
      }
   }
   else{
      // Could be an else here to try cmd.exe / command.com in the path.
      // Now we'll just error out..
      rb_raise(rb_eRuntimeError,
         "Cannot locate a COMSPEC environment variable to use as the shell"
      );
      return FALSE;
   }

   ZeroMemory(&siStartInfo, sizeof(STARTUPINFO));

   siStartInfo.cb = sizeof(STARTUPINFO);
   siStartInfo.dwFlags = STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW;
   siStartInfo.hStdInput = hStdin;
   siStartInfo.hStdOutput = hStdout;
   siStartInfo.hStdError = hStderr;
   siStartInfo.wShowWindow = SW_HIDE;

   if(v_show_window == Qtrue)
      siStartInfo.wShowWindow = SW_SHOW;

   // Try the command first without COMSPEC
   if(CreateProcess(
      NULL,
      cmdstring,
      NULL,
      NULL,
      TRUE,
      0,
      NULL,
      NULL,
      &siStartInfo,
      &piProcInfo)
    ){
      // Close the handles now so anyone waiting is woken.
      CloseHandle(piProcInfo.hThread);

      // Return process handle
      *hProcess = piProcInfo.hProcess;
      *pid = (pid_t)piProcInfo.dwProcessId;
      return TRUE;
   }

   // If that failed, try again with COMSPEC
   if(CreateProcess(
      NULL,
      s2,
      NULL,
      NULL,
      TRUE,
      CREATE_NEW_CONSOLE,
      NULL,
      NULL,
      &siStartInfo,
      &piProcInfo)
    ){
      // Close the handles now so anyone waiting is woken.
      CloseHandle(piProcInfo.hThread);

      // Return process handle
      *hProcess = piProcInfo.hProcess;
      *pid = (pid_t)piProcInfo.dwProcessId;
      return TRUE;
   }

   rb_raise(rb_eRuntimeError, "CreateProcess() failed: %s",
      ErrorDescription(GetLastError())
   );

   return FALSE;
}

/* Set the Process::Status. This code is based on patches by Samuel Tesla and
 * John-Mason Shackelford.
 */
static void win32_set_last_status(const int status, const int pid)
{
  // rb_last_status is defined in process.c in the main ruby.exe
  __declspec (dllimport) extern VALUE rb_last_status;
  VALUE klass = rb_path2class("Process::Status");
  VALUE process_status = rb_obj_alloc(klass);
  rb_iv_set(process_status, "status", INT2FIX(status << 8));
  rb_iv_set(process_status, "pid", INT2FIX(pid));
  rb_last_status = process_status;
  win32_last_status = process_status;
}

static void win32_pipe_finalize(OpenFile *file, int noraise)
{
   int status;

   if(file->f){
      fclose(file->f);
      file->f = NULL;
   }

   if(file->f2){
      fclose(file->f2);
      file->f2 = NULL;
   }

   if(pid_handle != NULL){
      GetExitCodeProcess(pid_handle, &status);

      if(status != STILL_ACTIVE){
    	 CloseHandle(pid_handle);
         pid_handle = NULL;
         win32_set_last_status(status, file->pid);
      }
   }
}

// The following code is based off of KB: Q190351
static VALUE ruby_popen(char *cmdstring, int mode, VALUE v_show_window)
{
   HANDLE hChildStdinRd, hChildStdinWr, hChildStdoutRd, hChildStdoutWr,
     hChildStderrRd, hChildStderrWr, hChildStdinWrDup, hChildStdoutRdDup,
     hChildStderrRdDup, hProcess;

   pid_t pid;

   SECURITY_ATTRIBUTES saAttr;
   BOOL fSuccess;
   int fd1, fd2, fd3;
   FILE *f1, *f2, *f3;
   long file_count;
   VALUE port;
   VALUE p1,p2,p3;
   int modef;
   OpenFile *fptr;
   char *m1, *m2;

   saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
   saAttr.bInheritHandle = TRUE;
   saAttr.lpSecurityDescriptor = NULL;

   if(!CreatePipe(&hChildStdinRd, &hChildStdinWr, &saAttr, 0)) {
      rb_raise(rb_eRuntimeError, "CreatePipe() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   /* Create new output read handle and the input write handle. Set
    * the inheritance properties to FALSE. Otherwise, the child inherits
    * the these handles; resulting in non-closeable handles to the pipes
    * being created.
    */
   fSuccess = DuplicateHandle(
      GetCurrentProcess(),
      hChildStdinWr,
      GetCurrentProcess(),
      &hChildStdinWrDup,
      0,
      FALSE,
      DUPLICATE_SAME_ACCESS
   );

   if(!fSuccess){
      rb_raise(rb_eRuntimeError, "DuplicateHandle() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   // Close the inheritable version of ChildStdin that we're using
   CloseHandle(hChildStdinWr);

   if(!CreatePipe(&hChildStdoutRd, &hChildStdoutWr, &saAttr, 0)) {
      rb_raise(rb_eRuntimeError, "CreatePipe() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   fSuccess = DuplicateHandle(
      GetCurrentProcess(),
      hChildStdoutRd,
      GetCurrentProcess(),
      &hChildStdoutRdDup,
      0,
      FALSE,
      DUPLICATE_SAME_ACCESS
   );

   if(!fSuccess){
      rb_raise(rb_eRuntimeError, "DuplicateHandle() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   // Close the inheritable version of ChildStdout that we're using.
   CloseHandle(hChildStdoutRd);

   if(!CreatePipe(&hChildStderrRd, &hChildStderrWr, &saAttr, 0)) {
      rb_raise(rb_eRuntimeError, "CreatePipe() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   fSuccess = DuplicateHandle(
      GetCurrentProcess(),
      hChildStderrRd,
      GetCurrentProcess(),
      &hChildStderrRdDup,
      0,
      FALSE,
      DUPLICATE_SAME_ACCESS
   );

   if (!fSuccess) {
      rb_raise(rb_eRuntimeError, "DuplicateHandle() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   // Close the inheritable version of ChildStdErr that we're using.
   CloseHandle(hChildStderrRd);

   if(mode & _O_TEXT){
      m1 = "r";
      m2 = "w";
   }
   else{
      m1 = "rb";
      m2 = "wb";
   }

   // Convert HANDLE's into file descriptors and, ultimately, Ruby IO objects.
   fd1 = _open_osfhandle((long)hChildStdinWrDup, mode);
   f1 = _fdopen(fd1, m2);

   fd2 = _open_osfhandle((long)hChildStdoutRdDup, mode);
   f2 = _fdopen(fd2, m1);

   fd3 = _open_osfhandle((long)hChildStderrRdDup, mode);
   f3 = _fdopen(fd3, m1);

   file_count = 3;

   if(!RubyCreateProcess(cmdstring,hChildStdinRd,hChildStdoutWr,
      hChildStderrWr, &hProcess, &pid, v_show_window))
   {
      return Qnil;
   }

   // I think this is only possible on Win9x, but we'll leave it here
   if(pid < 0){
      pid = -pid;
   }

   modef = rb_io_mode_flags(m2);
   p1 =  io_alloc(rb_cIO);
   MakeOpenFile(p1, fptr);

   fptr->finalize = win32_pipe_finalize;
   fptr->mode = modef;
   fptr->pid = pid;

   if(modef & FMODE_READABLE){
      fptr->f  = f1;
   }

   if(modef & FMODE_WRITABLE){
      if(fptr->f){
         fptr->f2 = f1;
      }
      else{
         fptr->f = f1;
      }
      fptr->mode |= FMODE_SYNC;
   }

   modef = rb_io_mode_flags(m1);
   p2 = io_alloc(rb_cIO);
   MakeOpenFile(p2, fptr);

   pid_handle = hProcess;
   fptr->finalize = win32_pipe_finalize;
   fptr->mode = modef;
   fptr->pid = pid;

   if(modef & FMODE_READABLE){
      fptr->f  = f2;
   }
   if(modef & FMODE_WRITABLE){
      if(fptr->f){
         fptr->f2 = f2;
      }
      else{
         fptr->f = f2;
      }
      fptr->mode |= FMODE_SYNC;
   }

   modef = rb_io_mode_flags(m1);
   p3 =  io_alloc(rb_cIO);
   MakeOpenFile(p3, fptr);

   fptr->finalize = win32_pipe_finalize;
   fptr->mode = modef;
   fptr->pid = pid;

   if(modef & FMODE_READABLE){
      fptr->f  = f3;
   }

   if(modef & FMODE_WRITABLE){
      if(fptr->f){
         fptr->f2 = f3;
      }
      else{
         fptr->f = f3;
      }
      fptr->mode |= FMODE_SYNC;
   }

   port = rb_ary_new2(4);
   rb_ary_push(port,(VALUE)p1);
   rb_ary_push(port,(VALUE)p2);
   rb_ary_push(port,(VALUE)p3);
   rb_ary_push(port,UINT2NUM((DWORD)pid));

  /* Child is launched. Close the parents copy of those pipe
   * handles that only the child should have open.  You need to
   * make sure that no handles to the write end of the output pipe
   * are maintained in this process or else the pipe will not close
   * when the child process exits and the ReadFile() will hang.
   */

   if(!CloseHandle(hChildStdinRd)){
      rb_raise(rb_eRuntimeError, "CloseHandle() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   if(!CloseHandle(hChildStdoutWr)){
      rb_raise(rb_eRuntimeError, "CloseHandle() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   if(!CloseHandle(hChildStderrWr)){
      rb_raise(rb_eRuntimeError, "CloseHandle() failed: %s",
         ErrorDescription(GetLastError())
      );
   }

   return port;
}

/*
 * Spawn an external program and retrieve the stdin, stdout and stderr IO
 * handles, as well as the process ID. Use in block or non-block form.
 */
void Init_open3()
{
   VALUE mOpen3 = rb_define_module("Open3");

   VALUE mOpen4 = rb_define_module("Open4");

   rb_define_module_function(mOpen3, "popen3", win32_popen3, -1);
   rb_define_module_function(mOpen4, "popen4", win32_popen3, -1);

   /* 0.2.9: The version of this library */
   rb_define_const(mOpen3, "WIN32_OPEN3_VERSION", rb_str_new2(WIN32_OPEN3_VERSION));

   /* 0.2.9: The version of this library */
   rb_define_const(mOpen4, "WIN32_OPEN3_VERSION", rb_str_new2(WIN32_OPEN3_VERSION));
}
