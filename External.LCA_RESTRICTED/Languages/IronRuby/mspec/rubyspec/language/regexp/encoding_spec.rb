require File.dirname(__FILE__) + '/../../spec_helper'
require File.expand_path(File.dirname(__FILE__) + '/../fixtures/classes')

describe "Regexps with encoding modifiers" do

  # Note: The encoding implied by a given modifier is specified in
  # core/regexp/encoding_spec.rb for 1.9

  ruby_version_is ""..."1.9" do
    not_compliant_on :macruby do
      it 'supports /e (EUC encoding)' do
        match = /./e.match("\303\251")
        match.to_a.should == ["\303\251"]
      end
      
      it 'supports /e (EUC encoding) with interpolation' do
        match = /#{/./}/e.match("\303\251")
        match.to_a.should == ["\303\251"]
      end
      
      it 'supports /e (EUC encoding) with interpolation and /o' do
        match = /#{/./}/e.match("\303\251")
        match.to_a.should == ["\303\251"]
      end
      
      it 'supports /n (Normal encoding)' do
        /./n.match("\303\251").to_a.should == ["\303"]
      end
      
      it 'supports /n (Normal encoding) with interpolation' do
        /#{/./}/n.match("\303\251").to_a.should == ["\303"]
      end
      
      it 'supports /n (Normal encoding) with interpolation and /o' do
        /#{/./}/no.match("\303\251").to_a.should == ["\303"]
      end
      
      it 'supports /s (SJIS encoding)' do
        /./s.match("\303\251").to_a.should == ["\303"]
      end
      
      it 'supports /s (SJIS encoding) with interpolation' do
        /#{/./}/s.match("\303\251").to_a.should == ["\303"]
      end
      
      it 'supports /s (SJIS encoding) with interpolation and /o' do
        /#{/./}/so.match("\303\251").to_a.should == ["\303"]
      end
      
      it 'supports /u (UTF8 encoding)' do
        /./u.match("\303\251").to_a.should == ["\303\251"]
      end

      it 'supports /u (UTF8 encoding) with interpolation' do
        /#{/./}/u.match("\303\251").to_a.should == ["\303\251"]
      end

      it 'supports /u (UTF8 encoding) with interpolation and /o' do
        /#{/./}/uo.match("\303\251").to_a.should == ["\303\251"]
      end
      
      it 'selects last of multiple encoding specifiers' do
        /foo/ensuensuens.should == /foo/s
      end
      
      it 'overrides the current value of $KCODE' do
        old_kcode, $KCODE = $KCODE, "UTF-8"
        begin
          s = "\xe2\x85\x9c"
          
          # KCODE is used: dot matches 3 bytes (one character):
          /(.)/ =~ s
          $1.should == s

          # KCODE is ignored: dot matches a single byte
          /(.)/n =~ s
          $1.should == "\xe2"
          
        ensure
          $KCODE = old_kcode
        end
      end
      
      it 'matches invalid/incomplete characters' do
        c = "\xff"
        /(#{c})/u =~ c
        $1.should == c

        c = "\xff"
        /(#{c})/n =~ c
        $1.should == c
      end
      
    end
  end

  ruby_version_is "1.9" do
    it 'supports /e (EUC encoding)' do
      match = /./e.match("\303\251".force_encoding(Encoding::EUC_JP))
      match.to_a.should == ["\303\251".force_encoding(Encoding::EUC_JP)]
    end
    
    it 'supports /e (EUC encoding) with interpolation' do
      match = /#{/./}/e.match("\303\251".force_encoding(Encoding::EUC_JP))
      match.to_a.should == ["\303\251".force_encoding(Encoding::EUC_JP)]
    end
    
    it 'supports /e (EUC encoding) with interpolation /o' do
      match = /#{/./}/e.match("\303\251".force_encoding(Encoding::EUC_JP))
      match.to_a.should == ["\303\251".force_encoding(Encoding::EUC_JP)]
    end
    
    it 'supports /n (No encoding)' do
      /./n.match("\303\251").to_a.should == ["\303"]
    end
    
    it 'supports /n (No encoding) with interpolation' do
      /#{/./}/n.match("\303\251").to_a.should == ["\303"]
    end
    
    it 'supports /n (No encoding) with interpolation /o' do
      /#{/./}/n.match("\303\251").to_a.should == ["\303"]
    end
    
    it 'supports /s (Windows_31J encoding)' do
      match = /./s.match("\303\251".force_encoding(Encoding::Windows_31J))
      match.to_a.should == ["\303".force_encoding(Encoding::Windows_31J)]
    end
    
    it 'supports /s (Windows_31J encoding) with interpolation' do
      match = /#{/./}/s.match("\303\251".force_encoding(Encoding::Windows_31J))
      match.to_a.should == ["\303".force_encoding(Encoding::Windows_31J)]
    end
    
    it 'supports /s (Windows_31J encoding) with interpolation and /o' do
      match = /#{/./}/s.match("\303\251".force_encoding(Encoding::Windows_31J))
      match.to_a.should == ["\303".force_encoding(Encoding::Windows_31J)]
    end
    
    it 'supports /u (UTF8 encoding)' do
      /./u.match("\303\251".force_encoding('utf-8')).to_a.should == ["\u{e9}"]
    end
    
    it 'supports /u (UTF8 encoding) with interpolation' do
      /#{/./}/u.match("\303\251".force_encoding('utf-8')).to_a.should == ["\u{e9}"]
    end
    
    it 'supports /u (UTF8 encoding) with interpolation and /o' do
      /#{/./}/u.match("\303\251".force_encoding('utf-8')).to_a.should == ["\u{e9}"]
    end
    
    # Fails on 1.9; reported as bug #2052
    it 'selects last of multiple encoding specifiers' do
      /foo/ensuensuens.should == /foo/s
    end
  end
end
