# Menace Amiga Format

 The 1988 Amiga game 'Menace' that was published in Amiga Format in 1990, this repo contains source code and changes that were made for the YouTube video series about it on http://www.youtube.com/@DavePoo2
 The plan is to upgrade this source to assemble and work for a more modern Amiga (source was written during the A1000/500 era of the Amiga)

https://www.youtube.com/playlist?list=PLr783JgI3IBd9PZuc9WMmwwxoG2ic-NzB - Video series playlist

The aim of this is to...

* Nostalgia - I read and learned from these articles back in the day, and I'd like to re-vist them
* Take a look at some old Amiga source code, find out how it works and learn from it
* Try to develop (as much as possible) like it was done in the good ol' days to see what we had to put up with (developing on the Amiga for the Amiga)
* Upgrade the source to compile for Workbench 3.2 and work on other Amigas than just the A500. Possibly just upgrade to work on AGA machines (as we alreday have the non-AGA version in the release title)
* Upgrade the source/game to work on the newer Amigas, not downgrade or cripple the Amiga to make the game work (WHDLoad can do that)
* Preserve the source code, articles & coverdisks for the future (should they dissapear from other online sources)

# Amiga Format - Dave Jones / Menace Articles - 'The Whole Truth About Games Programming'

The source was published across the following editons of Amiga Format in 1990. Articles on how the source code worked were written up by Dave Jones to go with the source.

* Amiga Format Issue 7 - February 1990 - Pages 63 to 68 - 'The Whole Truth About Games Programming'
* Amiga Format Issue 8 - March 1990 - Pages 63 - 67 - 'The Whole Truth About Games Programming: 2'
* Amiga Format Issue 9 - April 1990 - XXXXXXXX -'The Whole Truth About Games Programming: 3'
* Amiga Format Issue 10 - May 1990 - Pages 85 to 89 -'The Whole Truth About Games Programming Part 4 aliens'
* Amiga Format Issue 11 - June 1990 - XXXXXXXXX -'The Whole Truth About Games Programming Part 5 aliens 2'
* Amiga Format Issue 12 - July 1990 - XXXXXXXXX to 158 - 'The Whole Truth About Games Programming Part 5 [sic] Collision Detection'
* Amiga Format Issue 13 - August 1990 - Pages 127 to - 130 - 'The Whole Truth About Games Programming Part 7 The Guardian'

* https://amr.abime.net/issue_163
* https://amr.abime.net/issue_164
* https://amr.abime.net/issue_165
* https://amr.abime.net/issue_166
* https://amr.abime.net/issue_167
* https://amr.abime.net/issue_168
* https://amr.abime.net/issue_169

Note that July 1990 edition says "Part 5" but is actually "Part 6" but does say "Games Programming 6" in the header of the page.

## Cover Disk Code

The code was built up each edition via the files on cover disks.
The last coverdisk does contain all the source that was ever published, but a few of the files that were required are only available by pulling them from a previous cover disk.
I have pulled all the files together in one place to create the newer source code.

# Assembing on and Amiga

The code has been going through changes to upgrade to Workbench 3.2, but I am still assembing in DevPac.

* DevPac 3 (I am using DevPac 3.01 but later versions should probably work ) https://archive.org/download/CommodoreAmigaApplicationsADF
* Workbench NDK 3.2 (I am using NDK3.2 Release 1) https://www.hyperion-entertainment.com/index.php/downloads

## DevPac 3 Settings

These are the settings I am using to assemble correctly in DevPac 3 on the Amiga.

* Assembler Control Include folder is set to the :NDK3.2/Include_I (go to Settings -> Assember -> Control), and the 'Headers' is left blank (DevPac will have some WB 2 includes .gs file in there by default )
* Assembler Options Processor is set to 68020 (go to Settings -> Assember -> Options)
* 'Settings -> Assemble To Disk' is checked to out the file after assembly.

# Other Useful Resources

* https://www.amazon.co.uk/Bare-Metal-Amiga-Programming-OCS-ECS/dp/B09GJQ3SF6 - Edwin Th van den Oosterkamp (Author) - Bare-Metal Amiga Programming: For OCS, ECS and AGA


