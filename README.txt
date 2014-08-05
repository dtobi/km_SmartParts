Klockheed Martian Engine Manufacture - Smart Parts Pack
--------------------------------------------------------------

Visit the official forum thread for more information: http://forum.kerbalspaceprogram.com/threads/64227


Source: https://github.com/dtobi/km_SmartParts

Parts:

* Auto-staging device that detects empty tanks, SRBs, batteries, etc.
* Timer that triggers stages and action groups after a countdown
* Remote controller that triggers actions on nearby vessels
* Altimeter device that triggers actions and stages once altitude hits configured target
* Radial fuel controllers that control fuel lines' flow
* In-line fuel controller that controls fuel flow
* Flameout detectors for jet engines
* A fuel valve



------------------------------

About the pack:

Smart Parts control action groups and some other features (light, SAS). Gear control is too unreliable to be added. Most Smart Parts offer a "Select" tweakable that controls the desired action; the field above shows the selected action.

Installation Instructions
-------------------------
Before updating remove old Klockheed Martian Parts and, if using Space Shuttle Engines, update them.

Staging Controllers (also check example crafts Firefly and Explorer)
----------------------------------------------------------------------
The Staging Controllers must be attached to a fuel tank or SRB. The device detects when the attached fuel tank has drained and then, depending on the device, can fire an action group or stage.
Staging indicators can be made by assigning a light to an action group and automatically triggering that action group from via proper stager.
How to use the Staging Controllers (also check example crafts Firefly and Explorer)

Fuel Breakers and Controllers
-------------------------------
Controls fuel drain via action groups and a context menu. Breakers break one large fuel tank into segments that fuel lines can individually drain, far less shifting overall center of mass.

Radio Controller
-------------------
Sends events to other radio controllers on a channel. All radio controllers in range (typically < 2000 m) will execute the command if the channel matches. You can also set the throttle and heading of remote vessels.

Timer
------
Triggers a delayed staging or action group event. The staging event is executed on the vessel to which the timer is attached, therefore able to trigger a timed action even on an inactive vessel (e.g., a separated booster).

Valve
------
Drains excess fuel. It can drain LF, LFO and RCS. Attach it to the tank of a resource that you would like to drain. Provides nominal amount of thrust, useful to push external tank away from shuttle on seperation.

Changelog:

V0.1 Initial release
V0.2 Trigger improvements
	* Heading control over radio
	* Improved altimeter device (thanks to Firov)
	* Bugfix in the timer code
	* Fixed overly long part names
	* Added example craft
	* Corrected size descriptions of fuel breakers
V0.3 Smart Parts Update and New Example Craft
	* Added example craft for smart parts
	* Minor fixes for smart parts
	* Optimized altimeter scale
	* Made fuel detector more robust against remaining fuel in empty tanks
	* New KM_Lib.dll
V0.3 Fuel Valve and Detail Improvements
	* Activation and deactivation action group commands for altimeter and fuel detector
	* Beep command to beep when a condition occurs
	* Fuel valve to drain excess fuel
	* Staging for the timer
	* Improved fuel drain detection (should not fire prematurely)
V0.4 Improved fuel sensor, valve and altimeter
	* The stager now works with any resource (including electric charge and custom resources)
	* The valve now works with any resource (excluding electric charge). Other resources (e.g., real fuels should work, too.)
	* Removed double word in descriptions
	* Tweakable added to allow removal of timer staging icon
	* Altimeter altitude detection logic tweaked. Now works over water and buildings!
	* Altimeter no longer requires specifiying if it fires "above" or "below"
	* Altimeter now supports automatic reset as a result of above change
V0.5 Improved parts and bugfixes
	* Fixed bug that resulted in a lack of physics on parts ejected by altimeter
	* Fixed bug that created an empty stage whenever staging was activated by a SmartPart
	* Fuel valve now generates minimal amount of thrust and can be staged
	* Timer timescales adjusted. Added 0-30 seconds in .2 second increments and .5 to 60 minutes in .5 minute increments
v0.6 Upgraded GUI, Improved parts, and KSP .24 compatibility
	* Compatibility with KSP .24
	* Implemented new GUI to allow more control of Smart Parts
	* Improved Stager
		* Works with any resources, including electricity or custom resources
		* Allows user to select resource to monitor in VAB/SPH
		* Allows user to select target percentage
	* Improved Fuel Controller
		* Crossfeed tweakable now accessible in VAB/SPH
		* Removed redundant fuel controller ("ON")
v0.6.1 KSP .24.1 compatibility
	* KSP .24.1 compatibility
v0.6.2 KSP .24.2 compatibility
	* KSP .24.2 compatibility
v2.0.2 Updated KSPAPIExtension and Real Fuels compatbility
	* Updated KSPAPIExtension to 1.7.0
	* Auto stager now fully supports Real Fuels and will auto update on tank resource change
	* No longer dependant on km_lib.dll

	 
	 
Credits and Acknowledgments
------------------------------
Thanks to Duxwing for copy-editing SSE's and SmartParts front page and README.
	 
License	 
-----------------------------
This work is shared under Creative Commons CC BY-NC-ND 3.0 license.
You may use it for non commercial as long as you do not change or extend it (share as is) and as long as you give credit to the author in the download and on the download page.
Please contact me if you wish to modify or extend the code.
Author: dtobi and Firov
------------------------------
