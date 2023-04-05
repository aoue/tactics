//Link external functions here.
EXTERNAL bg(id)
EXTERNAL n(name) //set to empty string to hide namebox.
EXTERNAL talk(mode) //1: true, use quotes. 0: false, use parantheses.
EXTERNAL center(mode) 
EXTERNAL toggle_font()
EXTERNAL show(whichSlot, portraitID)
EXTERNAL hide(whichSlot)
EXTERNAL stop_music()
EXTERNAL play_music(whichTrack)
EXTERNAL play_sound(whichTrack)
EXTERNAL shake(intensity, duration) //camera shake
//end functions

//variable controllers here. set by EventManager at scene start.
VAR ic = 0
VAR player = "playerCharName"
//end variables

//SCENE OUTLINE
//Welcome to part X event X. It ...
//=============

base event. don't you worry about the contents, it's just here as a reference.

/*
~show(0, 0)
~n("Friday")
~talk(1)
testing talk and think speech modes. This should be talk mode.
~talk(0)
I think I did a good job...
~n("")
*/


/*
[player] is how you display the valuable of the player variable.
an if statement is like this:
{ ic > 0:
    ic is greater than 0.

- else:
    ic less not greater than 0.


}
*/

/*
~toggle_font()
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
~toggle_font()
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
~toggle_font()
Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
~toggle_font()
Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
*/
-> END
﻿
