# Remedy_2016

Archiving an old tool project from 2016 in case I decide to revisit it in the future.

None of these mini projects are really that useful anymore because I have better personal practices and these probably have bugs anyway. If I ever feel especially inspired maybe I can pick them up again and make major improvements.

These files were picked straight out of a Unity project so we should be able to drop them right in again when duty calls.

Remedy comes in three parts:
1. RemedyTools (2015)
 - A custom class called Remedy.cs features its own Log() functions (as opposed to implementing ILogHandler) and supports displaying the log's calling class name with a unique color.
 - It also comes with an editor window + ScriptableObject that stores links between class names and their colors. 
 - The editor window also tracks Input.GetKeyDown()'s and displays them in the window so that 2016 me wouldn't forget where I was throwing them around for debugging. 2022 me would probably just do the occasional search all files from the ide instead. Kinda cool nonetheless.

2. RemedyConsole (2016)
 - A fully featured custom editor Console for Unity that supports new features including filtering by class names, appending a color-coded .cs class of origin for every log, and extra toggles for customization. 
 - Because parts of the console was directly taken from the original editor console through some ..stolen functions with reflection... this console is broken in the more recent versions of Unity where it looks like those very functions no longer exist. Sad :(. I'm sure I'll get around to fixing this eventually.........

3. Standalone LogHandler (2016)
- A very similar yet unrelated class to RemedyTools also named Remedy that implements Unity's ILogHandler and supports new features to the basic log including color coded class names and filtering logs by class.
- This single class was originally built out into a .dll file so that the overridden Log() functions did not appear in the stack traces of the log. We can easily build this class into a .dll again to restore this functionality.