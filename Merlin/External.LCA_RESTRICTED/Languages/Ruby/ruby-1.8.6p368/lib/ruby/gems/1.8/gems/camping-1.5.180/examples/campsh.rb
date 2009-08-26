#!/usr/bin/env ruby

$:.unshift File.dirname(__FILE__) + "/../../lib"
%w(rubygems redcloth camping acts_as_versioned).each { |lib| require lib }
  
Camping.goes :CampSh

module CampSh
    NAME = 'CampSh'
    DESCRIPTION = %{
        Script your own URL commands, then run these commands through 
        the proxy with "cmd/CommandName".  All scripts are versioned 
        and attributed.
    }
    VERSION = '0.1'
    ANON = 'AnonymousCoward'

    begin
        require 'syntax/convertors/html'
        SYNTAX = ::Syntax::Convertors::HTML.for_syntax "ruby"
    rescue LoadError
    end

    def self.create
        Models.create_schema :assume => (Models::Command.table_exists? ? 1.0 : 0.0)
    end
end

module CampSh::Models
    class Command < Base
        validates_uniqueness_of :name
        validates_presence_of :author
        acts_as_versioned
    end
    class CreateBasics < V 1.0
        def self.up
            create_table :campsh_commands, :force => true do |t|
                t.column :id,         :integer, :null => false
                t.column :author,     :string,  :limit => 40
                t.column :name,       :string,  :limit => 255
                t.column :created_at, :datetime
                t.column :doc,        :text
                t.column :code,       :text
            end
            Command.create_versioned_table
            Command.reset_column_information
        end
        def self.down
            drop_table :campsh_commands
            Command.drop_versioned_table
        end
    end
end

module CampSh::Controllers
    class Index < R '/'
        def get
            redirect List
        end
    end

    class List
        def get
            @cmds = Command.find :all, :order => 'name'
            @title = "Command List"
            render :list
        end
    end

    class Run < R '/run/(\w+)', '/run/(\w+)/(.+)', '/run/(\w+) (.+)'
        def get(cmd, args=nil)
            @cmd = Command.find_by_name(cmd)
            unless @cmd
                redirect New, name
                return
            end

            args = args.to_s.strip.split(/[\/\s]+/)
            eval(@cmd.code)
        end
    end

    class Authors
        def get
            @authors = 
                Command.find(:all, :order => "author, name").inject({}) do |hsh, cmd|
                    (hsh[cmd.author] ||= []) << cmd
                    hsh
                end
            @title = "Author"
            render :authors
        end
    end

    class Recent
        def get
            @days = 
                Command.find(:all, :order => "created_at DESC").inject({}) do |hsh, cmd|
                    (hsh[cmd.created_at.strftime("%B %d, %Y")] ||= []) << cmd
                    hsh
                end
            @title = "Recently Revised"
            render :recent
        end
    end

    class Show < R '/show/(\w+)', '/show/(\w+)/(\d+)', '/cancel_edit/(\w+)'
        def get(name, version = nil)
            unless @cmd = Command.find_by_name(name)
                redirect(Edit, name)
                return
            end
            @version = (version.nil? or version == @cmd.version.to_s) ? @cmd : @cmd.versions.find_by_version(version)
            render :show
        end
    end

    class New < R '/new', '/new/(\w+)'
        def get(name)
            @cmd = Command.new(:name => name)
            @title = "Creating #{name}"
            render :edit
        end
        def post
            @cmd = Command.new(:name => input.cmd)
            @title = "Creating #{input.cmd}"
            render :edit
        end
    end

    class Edit < R '/edit/(\w+)', '/edit/(\w+)/(\d+)'
        def get(name, version = nil)
            @cmd = Command.find_by_name(name)
            @cmd = @cmd.versions.find_by_version(version) unless version.nil? or version == @cmd.version.to_s
            @title = "Editing #{name}"
            @author = @cookies.cmd_author || CampSh::ANON
            render :edit
        end
        def post(name)
            @cookies.cmd_author = input.command.author
            Command.find_or_create_by_name(name).update_attributes(input.command)
            redirect Show, name
        end
    end

    class HowTo 
        def get
            @title = "How To"
            render :howto
        end
    end

    class Style < R '/styles.css'
        def get
            @headers['Content-Type'] = 'text/css'
            %Q[
                h1#pageName, .newWikiWord a, a.existingWikiWord, .newWikiWord a:hover, 
                #TextileHelp h3 { color: #003B76; }

                #container { width: 720px; }

                #container {
                    float: none;
                    margin: 0 auto;
                    text-align: center;
                    padding: 2px;
                    border: solid 2px #999;
                }

                #content {
                    margin: 0;
                    padding: 9px;
                    text-align: left;
                    border-top: none;
                    border: solid 1px #999;
                    background-color: #eee;
                }

                body, p, ol, ul, td {
                    font-family: verdana, arial, helvetica, sans-serif;
                    font-size:   15px;
                    line-height: 110%;
                }

                a { color: #000; }

                .newWikiWord { background-color: #eee; }
                .newWikiWord a:hover { background-color: white; }

                a:visited { color: #666; }
                a:hover { color: #fff; background-color:#000; }

                /* a.edit:link, a.edit:visited { color: #DA0006; } */


                h1, h2, h3 { color: #333; font-family: georgia, verdana; text-align: center; line-height: 70%; margin-bottom: 0; }
                h1 { font-size: 28px }
                h2 { font-size: 22px }
                h3 { font-size: 19px }

                h1#pageName {
                    margin: 5px 0px 0px 0px;
                    padding: 0px 0px 0px 0px;
                    line-height: 28px;
                }

                h1#pageName small {
                    color: grey;
                    line-height: 10px;
                    font-size: 10px;
                    padding: 0px;
                }

                a.nav, a.nav:link, a.nav:visited { color: #000; }
                a.nav:hover { color: #fff; background-color:#000; }

                li { margin-bottom: 7px }

                .navigation {
                    margin-top: 5px;
                    font-size : 12px;
                    color: #999;
                    text-align: center;
                }

                .navigation a:hover { color: #fff; background-color:#000; }

                .navigation a {
                    font-size: 11px;
                    color: black;
                    font-weight: bold;
                }

                .navigation small a {
                    font-weight: normal;
                    font-size: 11px;
                }

                .navOn{
                    font-size: 11px;
                    color: grey;
                    font-weight: bold;
                    text-decoration: none;
                }

                .help {
                    font-family: verdana, arial, helvetica, sans-serif;
                    font-size: 11px;
                }

                .inputBox {
                    font-family: verdana, arial, helvetica, sans-serif;
                    font-size: 11px;
                    background-color: #eee;
                    padding: 5px;
                    margin-bottom: 20px;
                }

                blockquote {
                    display: block;
                    margin: 0px 0px 20px 0px;
                    padding: 0px 30px;
                    font-size:11px;
                    line-height:17px;
                    font-style: italic;  
                }

                pre {
                    background-color: #eee;
                    padding: 10px;
                    font-size: 11px;
                }

                ol.setup {
                    font-size: 19px;
                    font-family: georgia, verdana;
                    padding-left: 25px;
                }

                ol.setup li {
                    margin-bottom: 20px
                }

                .byline {
                    font-size: 10px;
                    font-style: italic;  
                    margin-bottom: 10px;
                    color: #999;
                }

                .references {
                    font-size: 10px;
                }

                .diffdel {
                    background: pink;
                }

                .diffins {
                    background: lightgreen;
                }

                #allCommands ul {
                    list-style: none;
                }

                #allCommands li {
                    padding: 1px 4px;
                }

                #allCommands li:hover {
                    background-color: white;
                }

                #allCommands a,
                #allCommands a:hover,
                #allCommands a:link,
                #allCommands a:visited,
                #allCommands h2 {
                    color: #369;
                    background-color: transparent;
                    font-weight: normal;
                    font-size: 24px;
                    text-align: left;
                }

                #TextileHelp table {
                    margin-bottom: 0;
                }

                #TextileHelp table+h3 {
                    margin-top: 11px;
                }

                #TextileHelp table td {
                    font-size: 11px;
                    padding: 3px;
                    vertical-align: top;
                    border-top: 1px dotted #ccc;
                }

                #TextileHelp table td.arrow {
                    padding-right: 5px;
                    padding-left: 10px;
                    color: #999;
                }

                #TextileHelp table td.label {
                    font-weight: bold;
                    white-space: nowrap;
                    font-size: 10px;
                    padding-right: 15px;
                    color: #000;
                }

                #TextileHelp h3 {
                    font-size: 11px;
                    font-weight: bold;
                    font-weight: normal;
                    margin: 0 0 5px 0;
                    padding: 5px 0 0 0;
                }

                #TextileHelp p {
                    font-size: 10px;
                }

                .rightHandSide {
                    float: right;
                    width: 147px;
                    margin-left: 10px;
                    padding-left: 20px;
                    border-left: 1px dotted #ccc;
                }

                .rightHandSide p {
                    font-size: 10px;
                }

                .newsList {
                margin-top: 20px;
                }

                .newsList p {
                margin-bottom:30px
                }

                .leftHandSide
                {
                    float: right;
                    width: 147px;
                    margin-left: 10px;
                    padding-left: 20px;
                    border-left: 1px dotted #ccc;
                }

                .leftHandSide p
                {
                    font-size: 10px;
                    margin: 0;
                    padding: 0;
                }

                .leftHandSide h2
                {
                    font-size: 12px;
                    margin-bottom: 0;
                    padding-bottom: 0;
                }

                .property
                {
                    color: grey;
                    font-size: 9px;
                    text-align: right;
                }

                body
                {
                    background-color: #ccc;
                    padding: 0;
                    margin: 20px;
                    color: #333;
                    line-height: 1.5;
                    font-size: 85%;
                    /*  hacky hack */
                    voice-family: "\"}\"";
                    voice-family: inherit;
                    font-size: 80%;
                }

                /* be nice to opera */
                html>body { font-size: 80%; }

                /* syntax highlighting */
                .keyword {
                    font-weight: bold;
                }
                .comment {
                    color: #555;
                }
                .string, .number {
                    color: #396;
                }
                .regex {
                    color: #435;
                }
                .ident {
                    color: #369;
                }
                .symbol {
                    color: #000;
                }
                .constant, .class {
                    color: #630;
                    font-weight: bold;
                }
            ]
        end
    end

end

module CampSh::Views
    def red( str )
        require 'redcloth'
        self << RedCloth.new( str ).to_html
    end

    def layout 
        html do
            head do
                title "CampShell -- #{ @title }"
                style "@import '#{ self / R(Style) }';", :type => 'text/css'
            end
            body do
                div.container! do
                    div.content! do
                        h2 "CampShell"
                        h1 @title
                        _navigation
                        self << yield
                    end
                end
            end
        end
    end

    def _navigation
        form :id => "navigationForm", :class => "navigation", :style => "font-size: 10px" do  
            [["Command List", R(List), "Alphabetical list of commands", "A"],
             ["Recently Revised", R(Recent), "Pages sorted by when they were last changed", "U"],
             ["Authors", R(Authors), "Who wrote what", "W"],
             ["How To", R(HowTo), "How to use CampShell", "H"]
            ].map do |txt, link, title, key|
                a txt, :href => link, :title => title, :accesskey => key
            end.join(" | ")
        end
    end

    def list
        div.allCommands! :style => "width: 500px; margin: 0 100px" do
            form.editForm! :action => R(New), :method => "post" do
                ul do
                    if @cmds.each do |cmd|
                        li do 
                            h2 { a cmd.name, :href => R(Show, cmd.name) }
                            red cmd.doc
                        end
                    end.empty?
                        h2 "No commands created yet."
                    end

                    li do
                        text "Create command: "
                        input :type => "text", :name => "cmd", :id=> "newCmd", :value => "shortcut",
                              :onClick => "this.value == 'shortcut' ? this.value = '' : true"
                    end
                end
            end
        end
    end

    def authors
        ul.authorList! do
            @authors.each do |author, cmds|
                li do
                    strong(author) + " worked on: " +
                        cmds.map { |cmd| a cmd.name, :href => R(Show, cmd.name) }.join(", ")
                end
            end
        end
    end

    def recent
        @days.each do |day, cmds|
            strong day
            ul do
                cmds.each do |cmd|
                    li do
                        a cmd.name, :href => R(Show, cmd.name)
                        div.byline "by #{ cmd.author } at #{ cmd.created_at.strftime("%H:%M") }"
                    end
                end
            end
        end
    end

    def show
        h2 "Instructions"
        red @version.doc

        h2 "Code"
        if defined? SYNTAX
            SYNTAX.convert(@version.code)
        else
            pre @version.code
        end

        div.byline "Revised on #{ @version.created_at } by #{ @version.author }"

        div.navigation do
            a "Edit Page", :href => R(Edit, @version.name, @version.version), 
                           :class => "navlink", :accesskey => "E"
            unless @version.version == 1
                text " | "
                a 'Back in time', :href => R(Show, @version.name, @version.version-1)
            end
            unless @version.version == @cmd.version
                text " | "
                a 'Next',    :href => R(Show, @version.name, @version.version+1)
                text " | "
                a 'Current', :href => R(Show, @version.name)
            end
            if @cmd.versions.size > 1
                small "(#{ @cmd.versions.size } revisions)"
            end
        end
    end

    def edit
        form :id => "editForm", :action => R(Edit, @cmd.name), :method => "post", :onSubmit => "cleanAuthorName();" do
            div :style => "margin: 0 100px;" do
                h2 "Instructions"
                textarea @cmd.doc, :name => "command[doc]", :style => "width: 450px; height: 120px"

                h2 "Code"
                textarea @cmd.code, :name => "command[code]", :style => "width: 450px; height: 380px"

                p do
                    input :type => "submit", :value => "Save"; text " as "
                    input :type => "text", :name => "command[author]", :id => "authorName", :value => @author,
                          :onClick => "this.value == '#{ CampSh::ANON }' ? this.value = '' : true"; text " | "
                    a "Cancel", :href => "/cancel_edit/#{ @cmd.name }"
                end
            end

            script <<-END, :language => "JavaScript1.2"
                function cleanAuthorName() {
                    if (document.getElementById('authorName').value == "") {
                        document.getElementById('authorName').value = '#{ @anon }';
                    }
                }
            END
        end
    end

    def howto
        red %[
            Here's the rundown on CampShell.  It's wiki-like storage for simple Ruby scripts.  You can then run these scripts from
            the URL.  So, your job is to fill up CampShell with these commands, some of which can be found
            "here":http://mousehole.rubyforge.org/wiki/wiki.pl?CampShells.

            h2. Using with Firefox

            For best effect, go to @about:config@ and set @keyword.URL@ to @http://#{env['HTTP_HOST']}#{self./ "/run/"}@. Restart Firefox,
            you will then be able to run commands directly from the address bar.

            For example:

            * @new google@ will open the page for creating a new @google@ command.
            * @show google@ will display the documentation for that command once it is saved.
            * @edit google@ will open the editor for the @google@ command.
            * @google mousehole commands@ will run the @google@ command with the arguments @mousehole@ and @commands@.
            * @list all@ or @keyword:list@ displays all commands.
            * @howto ?@ or @keyword:howto@ shoots you over to this page.
        ].gsub(/^ +/, '')
    end
end

