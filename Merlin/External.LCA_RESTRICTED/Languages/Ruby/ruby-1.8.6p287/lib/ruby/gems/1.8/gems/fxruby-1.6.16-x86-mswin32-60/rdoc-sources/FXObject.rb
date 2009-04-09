module Fox
  #
  # FXObject is the base class for all objects in FOX; in order to receive
  # messages from the user interface, your class must derive from FXObject.
  # The FXObject class also provides serialization facilities, with which
  # you can save and restore the object's state.  If you've subclassed
  # from FXObject, you can save your subclasses' state by overloading the
  # save() and load() functions and use the stream API to serialize its
  # member data.
  #
  class FXObject
    #
    # Handle a message sent from _sender_, with given _selector_
    # and message _data_.
    #
    def handle(sender, selector, data); end

    #
    # Save object to stream.
    #
    def save(stream) ; end

    #
    # Load object from _stream_.
    #
    def load(stream) ; end
  end
end
