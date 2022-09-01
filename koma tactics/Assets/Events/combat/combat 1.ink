//The base combat mission event.
//This one script file will hold all the dialogue scenes for this mission, and we navigate by label.
//A mission dialogue can be played at:
//(mandatory)
//  -start of mission
//  -end of mission (victory)
//  -end of mission (loss)
//(optional)
//  -start of any round after

//Externals here, can:
EXTERNAL play_music(which) //play song using int->sound manager
EXTERNAL n(name) //set name to string
EXTERNAL p(which) //set box portrait using int->portrait library
EXTERNAL jump(x, y) //jump to x, y coord on game map
EXTERNAL slide(x, y) //slide to x, y coord on game map


//Navigation to the right label
VAR label = 0

//before the script begins, labelIndex will be set to start.
//Then we jump to whatever label corresponds.
//(the label index is equal to the round the event plays on.)
//switch statement:

{
- label == 0: -> mission_begin
- label == 3: -> reinforce1
- label == 6: -> reinforce2
- label == 9: -> reinforce3
- label == 12: -> reinforce3
- label == -2: -> mission_end_win
- label == -3: -> mission_end_loss
}

=== mission_begin ===
~jump(4, 7)
~n("Voice")
~p(-1)
~play_music(0)
mission 1 dummy.
Time to fight!
-> END

=== reinforce1 ===
~jump(11,5)
reinforcements 1!
-> END

=== reinforce2 ===
~jump(11,5)
reinforcements 2!
-> END

=== reinforce3 ===
~jump(11,5)
reinforcements 3!
-> END

=== reinforce4 ===
~jump(11,5)
reinforcements 4!
-> END

=== mission_end_win ===
This is dummy text for triumph at the end of the mission.
blah blah blah
-> END

=== mission_end_loss ===
This is dummy text for loss at the end of the mission.
blah blah blah
-> END








//eof
﻿
