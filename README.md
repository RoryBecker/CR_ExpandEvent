### 'Expand Event' and 'Compress Event' for CodeRush

The **'Expand Event'** refactoring allows you to transform this code...

    public event EventHandler MyEvent; 

   ... into this code....

    private EventHandler _myEvent;
	public event EventHandler MyEvent
	{
		add
		{
			_myEvent += value;
		}
		remove
		{
			_myEvent -= value;
		}
	}

The **'Compress Event'** performs the opposite operation transforming this code ... 


    private EventHandler _myEvent;
	public event EventHandler MyEvent
	{
		add
		{
			_myEvent += value;
		}
		remove
		{
			_myEvent -= value;
		}
	}


  ... into this code...

    public event EventHandler MyEvent; 

### Usage

Place the caret on the **EventHandler** keyword and choose '**Expand Event**' or '**Compress Event**' as appropriate, from the CodeRush SmartTag menu. 
