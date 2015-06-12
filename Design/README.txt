Artefacts
---------
Digital asset & information management software suite
An experimental/learning project\n  

Joel E Bowman
Started 10/2014
---------

This README.txt created 14/11/2014
Also on this day, I have decided to try out MonoDevelop's integrated version control functionality for pushing
incremental versions to my github repo - https://github.com/jbowwww/

Design solution folder will (surprisingly) hold any and all design documentation.
It will not be perfectly maintained but a reasonable level of time, effort and thought put into this area will
no doubt be of great benefit, on many levels.

At a minimum, keep a running document of your current tasks/goals to aid in focus and efficiency.
Once again this will not be perfect either but you should document tasks/goals/features to be implemented with:
	- Task number (Recorded according to subtasks i.e. Task x.x.x.x)
	- Task Name/Title
		- ~1-5 words
		- To provide ability to easily refer to task
	- BRIEF description (one sentence)
	- Date the task was added to design
	- Summary describing overall function/purpose
		- <= Paragraph
		- Optional for complex/compound tasks with sub tasks, which may instead replace this with
		  bullet points of sub task names & descriptions
	- Initial priority
	- For simple tasks (ie leaf nodes) ONLY -
		- Estimated time required
			- If you cannot easily estimate the required time, you probably need to divide into sub-tasks
		- Work log containing date/times and notes on work done
			- This is where I foresee myself becoming inconsistent and maybe a little "lazy"
				- More important not to waste time over-documenting. Would be more useful for tasks that are:
					- Larger / expected to take more time
					- Less predictable / less confident/certain in their time estimate
					- Tasks not well defined yet, due to lack of knowledge, required decisions pending, ...
		- The estimated time required and work logs can then be aggregated to the parent task
			- Measurement of estimated/actual time taken in a heirarchial fashion (useful I think)
			- Automate this in a spreadsheet at some point, but don't waste time perfecting it pointlessly	
	- Date task started, and therefore estimated completion date (assuming all sub-tasks are defined)
	- More difficult or complex tasks can have their own document for notes, whatever
		- TODO: Define that later, at least as loosely as it needs to be
			- Maybe try to keep a document per "simple"/leaf task? Well maybe not that would prob get messy..
			
The other necessity is a "release" or more accurately a build log
	- Records each non-trivial build
		- TODO: Define a basic build numbering system?
		- Features and functionality added or removed relative to previous build
		- Does not matter how many errors or bugs the build has, record these too
		- Git commit number
		- Source Tarball
		- Binaries for some if not all builds should also be archived
		- One build per ~1-4 hour work session seems about right
			- Log a build whenever necessary however
			- Any new feature introduced or removed
				- Whether it works or it broke the build
			- Increntally at milestones, or even just steps towards a signifigant milestone
			- Too many builds is pointless and messy
			- Too few builds makes reverting costly when it is necessary (and might be necessary when you don't always expect it)
		- Input data or notes about execution environment where necessary
		- Snapshots of server VMs or databases would be handy
			- Only when necessary
				- Critical data or changes
				- Builds may depend on specific snapshots
		- Screenshots
		- Performance data (eventually)