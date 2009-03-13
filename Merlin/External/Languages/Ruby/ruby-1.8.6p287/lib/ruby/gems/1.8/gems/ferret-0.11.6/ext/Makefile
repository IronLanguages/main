
SHELL = /bin/sh

#### Start of system configuration section. ####

srcdir = .
topdir = c:/vsl/m2/Merlin/External/Languages/Ruby/Ruby-1.8.6p287/lib/ruby/1.8/i386-mswin32
hdrdir = $(topdir)
VPATH = $(srcdir);$(topdir);$(hdrdir)

DESTDIR = c:
prefix = $(DESTDIR)/vsl/m2/Merlin/External/Languages/Ruby/Ruby-1.8.6p287
exec_prefix = $(prefix)
sitedir = $(prefix)/lib/ruby/site_ruby
rubylibdir = $(libdir)/ruby/$(ruby_version)
archdir = $(rubylibdir)/$(arch)
sbindir = $(exec_prefix)/sbin
datadir = $(prefix)/share
includedir = $(prefix)/include
infodir = $(prefix)/info
sysconfdir = $(prefix)/etc
mandir = $(prefix)/man
libdir = $(exec_prefix)/lib
sharedstatedir = $(DESTDIR)/etc
oldincludedir = $(DESTDIR)/usr/include
sitearchdir = $(sitelibdir)/$(sitearch)
localstatedir = $(DESTDIR)/var
bindir = $(exec_prefix)/bin
sitelibdir = $(sitedir)/$(ruby_version)
libexecdir = $(exec_prefix)/libexec

CC = cl -nologo
LIBRUBY = $(RUBY_SO_NAME).lib
LIBRUBY_A = $(RUBY_SO_NAME)-static.lib
LIBRUBYARG_SHARED = $(LIBRUBY)
LIBRUBYARG_STATIC = $(LIBRUBY_A)

RUBY_EXTCONF_H = 
CFLAGS   =  -MD -Zi -O2b2xg- -G6 
INCFLAGS = -I. -I$(topdir) -I$(hdrdir) -I$(srcdir)
DEFS     = 
CPPFLAGS =  
CXXFLAGS = $(CFLAGS) 
DLDFLAGS =  -link -incremental:no -debug -opt:ref -opt:icf -dll $(LIBPATH) -def:$(DEFFILE) -implib:$(*F:.so=)-$(arch).lib -pdb:$(*F:.so=)-$(arch).pdb 
LDSHARED = cl -nologo -LD
AR = lib -nologo
EXEEXT = .exe

RUBY_INSTALL_NAME = ruby
RUBY_SO_NAME = msvcrt-ruby18
arch = i386-mswin32
sitearch = i386-msvcrt
ruby_version = 1.8
ruby = c:/vsl/m2/Merlin/External/Languages/Ruby/Ruby-1.8.6p287/bin/ruby
RUBY = $(ruby:/=\)
RM = $(RUBY) -run -e rm -- -f
MAKEDIRS = @$(RUBY) -run -e mkdir -- -p
INSTALL = @$(RUBY) -run -e install -- -vp
INSTALL_PROG = $(INSTALL) -m 0755
INSTALL_DATA = $(INSTALL) -m 0644
COPY = copy > nul

#### End of system configuration section. ####

preload = 

libpath = . $(libdir)
LIBPATH =  -libpath:"." -libpath:"$(libdir)"
DEFFILE = $(TARGET)-$(arch).def

CLEANFILES = mkmf.log
DISTCLEANFILES = vc*.pdb $(DEFFILE)

extout = 
extout_prefix = 
target_prefix = 
LOCAL_LIBS = 
LIBS = $(LIBRUBYARG_SHARED)  oldnames.lib user32.lib advapi32.lib shell32.lib ws2_32.lib   msvcprt.lib
SRCS = analysis.c api.c array.c bitvector.c compound_io.c document.c except.c ferret.c filter.c fs_store.c global.c hash.c hashset.c helper.c index.c libstemmer.c mempool.c multimapper.c posh.c priorityqueue.c q_boolean.c q_const_score.c q_filtered_query.c q_fuzzy.c q_match_all.c q_multi_term.c q_parser.c q_phrase.c q_prefix.c q_range.c q_span.c q_term.c q_wildcard.c ram_store.c r_analysis.c r_index.c r_qparser.c r_search.c r_store.c r_utils.c search.c similarity.c sort.c stem_ISO_8859_1_danish.c stem_ISO_8859_1_dutch.c stem_ISO_8859_1_english.c stem_ISO_8859_1_finnish.c stem_ISO_8859_1_french.c stem_ISO_8859_1_german.c stem_ISO_8859_1_italian.c stem_ISO_8859_1_norwegian.c stem_ISO_8859_1_porter.c stem_ISO_8859_1_portuguese.c stem_ISO_8859_1_spanish.c stem_ISO_8859_1_swedish.c stem_KOI8_R_russian.c stem_UTF_8_danish.c stem_UTF_8_dutch.c stem_UTF_8_english.c stem_UTF_8_finnish.c stem_UTF_8_french.c stem_UTF_8_german.c stem_UTF_8_italian.c stem_UTF_8_norwegian.c stem_UTF_8_porter.c stem_UTF_8_portuguese.c stem_UTF_8_russian.c stem_UTF_8_spanish.c stem_UTF_8_swedish.c stopwords.c store.c term_vectors.c utilities.c
OBJS = analysis.obj api.obj array.obj bitvector.obj compound_io.obj document.obj except.obj ferret.obj filter.obj fs_store.obj global.obj hash.obj hashset.obj helper.obj index.obj libstemmer.obj mempool.obj multimapper.obj posh.obj priorityqueue.obj q_boolean.obj q_const_score.obj q_filtered_query.obj q_fuzzy.obj q_match_all.obj q_multi_term.obj q_parser.obj q_phrase.obj q_prefix.obj q_range.obj q_span.obj q_term.obj q_wildcard.obj ram_store.obj r_analysis.obj r_index.obj r_qparser.obj r_search.obj r_store.obj r_utils.obj search.obj similarity.obj sort.obj stem_ISO_8859_1_danish.obj stem_ISO_8859_1_dutch.obj stem_ISO_8859_1_english.obj stem_ISO_8859_1_finnish.obj stem_ISO_8859_1_french.obj stem_ISO_8859_1_german.obj stem_ISO_8859_1_italian.obj stem_ISO_8859_1_norwegian.obj stem_ISO_8859_1_porter.obj stem_ISO_8859_1_portuguese.obj stem_ISO_8859_1_spanish.obj stem_ISO_8859_1_swedish.obj stem_KOI8_R_russian.obj stem_UTF_8_danish.obj stem_UTF_8_dutch.obj stem_UTF_8_english.obj stem_UTF_8_finnish.obj stem_UTF_8_french.obj stem_UTF_8_german.obj stem_UTF_8_italian.obj stem_UTF_8_norwegian.obj stem_UTF_8_porter.obj stem_UTF_8_portuguese.obj stem_UTF_8_russian.obj stem_UTF_8_spanish.obj stem_UTF_8_swedish.obj stopwords.obj store.obj term_vectors.obj utilities.obj
TARGET = ferret_ext
DLLIB = $(TARGET).so
EXTSTATIC = 
STATIC_LIB = 

RUBYCOMMONDIR = $(sitedir)$(target_prefix)
RUBYLIBDIR = c:/vsl/m2/Merlin/External/Languages/Ruby/Ruby-1.8.6p287/lib/ruby/gems/1.8/gems/ferret-0.11.6/lib$(target_prefix)
RUBYARCHDIR = c:/vsl/m2/Merlin/External/Languages/Ruby/Ruby-1.8.6p287/lib/ruby/gems/1.8/gems/ferret-0.11.6/lib$(target_prefix)

TARGET_SO     = $(DLLIB)
CLEANLIBS     = $(TARGET).so $(TARGET).il? $(TARGET).tds $(TARGET).map
CLEANOBJS     = *.obj *.lib *.s[ol] *.pdb *.exp *.bak

all:		$(DLLIB)
static:		$(STATIC_LIB)

clean:
		@-$(RM) $(CLEANLIBS:/=\) $(CLEANOBJS:/=\) $(CLEANFILES:/=\)

distclean:	clean
		@-$(RM) Makefile $(RUBY_EXTCONF_H) conftest.* mkmf.log
		@-$(RM) core ruby$(EXEEXT) *~ $(DISTCLEANFILES:/=\)

realclean:	distclean
install: install-so install-rb

install-so: $(RUBYARCHDIR)
install-so: $(RUBYARCHDIR)/$(DLLIB)
$(RUBYARCHDIR)/$(DLLIB): $(DLLIB)
	$(INSTALL_PROG) $(DLLIB:/=\) $(RUBYARCHDIR:/=\)
install-rb: pre-install-rb install-rb-default
install-rb-default: pre-install-rb-default
pre-install-rb: Makefile
pre-install-rb-default: Makefile
$(RUBYARCHDIR):
	$(MAKEDIRS) $@

site-install: site-install-so site-install-rb
site-install-so: install-so
site-install-rb: install-rb

.SUFFIXES: .c .m .cc .cxx .cpp .obj

{$(srcdir)}.cc{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(topdir)}.cc{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(hdrdir)}.cc{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

.cc.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(srcdir)}.cxx{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(topdir)}.cxx{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(hdrdir)}.cxx{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

.cxx.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(srcdir)}.cpp{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(topdir)}.cpp{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(hdrdir)}.cpp{}.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

.cpp.obj:
	$(CXX) $(INCFLAGS) $(CXXFLAGS) $(CPPFLAGS) -c -Tp$(<:\=/)

{$(srcdir)}.c{}.obj:
	$(CC) $(INCFLAGS) $(CFLAGS) $(CPPFLAGS) -c -Tc$(<:\=/)

{$(topdir)}.c{}.obj:
	$(CC) $(INCFLAGS) $(CFLAGS) $(CPPFLAGS) -c -Tc$(<:\=/)

{$(hdrdir)}.c{}.obj:
	$(CC) $(INCFLAGS) $(CFLAGS) $(CPPFLAGS) -c -Tc$(<:\=/)

.c.obj:
	$(CC) $(INCFLAGS) $(CFLAGS) $(CPPFLAGS) -c -Tc$(<:\=/)

$(DLLIB): $(DEFFILE) $(OBJS)
	@-$(RM) $@
	$(LDSHARED) -Fe$(@) $(OBJS) $(LIBS) $(LOCAL_LIBS) $(DLDFLAGS)



$(DEFFILE): 
	$(RUBY) -e "puts 'EXPORTS', 'Init_$(TARGET)'"  > $@

$(OBJS): {.;$(srcdir);$(topdir);$(hdrdir)}ruby.h {.;$(srcdir);$(topdir);$(hdrdir)}defines.h
