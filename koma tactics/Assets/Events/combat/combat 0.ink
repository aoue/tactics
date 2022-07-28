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
EXTERNAL play_music(which)
EXTERNAL n(name)
EXTERNAL show(which)
EXTERNAL jump(x, y)


//Navigation to the right label
VAR label = 0

//before the script begins, labelIndex will be set to start.
//Then we jump to whatever label corresponds.

//switch statement:
{
- label == 0: -> mission_begin
- label == 1: -> round_one
- label == -2: -> mission_end_win
- label == -3: -> mission_end_loss
}


=== mission_begin ===
~n("Voice")
//also, setup music here too.
~play_music(0)
(music playing right?) This is dummy text for the start of the mission.
Okay, we're changing tracks.
~play_music(1)
Time to fight!
-> END

=== round_one ===
~n("Man's voice")

hghggggggggggggnn!
Oh my shoulder!
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
