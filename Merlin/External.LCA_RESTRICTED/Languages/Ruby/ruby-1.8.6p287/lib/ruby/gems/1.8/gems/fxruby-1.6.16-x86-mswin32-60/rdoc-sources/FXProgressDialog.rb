module Fox
  #
  # A progress dialog is a simple dialog which is used to
  # keep a user informed of the progress of a lengthy operation
  # in a program and that the program is in fact still working.
  #
  # === Options
  #
  # +PROGRESSDIALOG_NOCANCEL+::		Default is no cancel button
  # +PROGRESSDIALOG_CANCEL+::		Enable the cancel button
  # +PROGRESSDIALOG_NORMAL+::		same as <tt>DECOR_TITLE|DECOR_BORDER</tt>
  #
  class FXProgressDialog < FXDialogBox
    # Progress message [String]
    attr_accessor :message

    # Amount of progress [Integer]
    attr_accessor :progress
    
    # Maximum value for progress [Integer]
    attr_accessor :total

    #
    # Construct progress dialog box with given caption and message string.
    #
    def initialize(owner, caption, label, opts=PROGRESSDIALOG_NORMAL, x=0, y=0, width=0, height=0) # :yields: theProgressDialog
    end
  
    # Increment progress by given _amount_.
    def increment(amount); end
  
    # Return true if the operation was cancelled.
    def cancelled?; end
  end
end

