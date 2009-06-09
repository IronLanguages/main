Merb::Config[:framework] = {
  :application => Merb.root / "application.rb",
  :config => [Merb.root / "config", nil],
  :public => [Merb.root / "public", nil],
  :view   => Merb.root / "views"
}

