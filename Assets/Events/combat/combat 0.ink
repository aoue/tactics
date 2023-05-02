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
//Then we jump to whatever label corresponds. (index is round number.)

//switch statement:
{
- label == 0: -> mission_begin
- label == 1: -> reinforce1
- label == -2: -> mission_end_win
- label == -3: -> mission_end_loss
}

=== mission_begin ===
~play_music(0)
~jump(3, 5)

//~cg(0)
~n("Voice")
~p(100)
Testy test.

~n("")
~p(-1)
Time to fight!
-> END

=== reinforce1 ===
~jump(5,5)
reinforcements 1!
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
