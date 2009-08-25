#!/usr/bin/env ruby

argv = ARGV + %w[-a fcgi]
Merb.start(argv)