/*
 * Author: dtobi, Firov
 * This work is shared under CC BY-NC-ND 3.0 license.
 * Non commercial, no derivatives, attribution if shared unmodified.
 * You may distribute this code and the compiled .dll as is.
 * 
 * Exception from the no-deriviates clause in case of KSP updates:
 * In case of an update of KSP that breaks the code, you may change
 * this code to make it work again and redistribute it under a different
 * class name until the author publishes an updated version. After a
 * release by the author, the right to redistribute the changed code
 * vanishes.
 * 
 * You must keep this boilerplate in the file and give credit to the author
 * in the download file as well as on the webiste that links to the file.
 * 
 * Should you wish to change things in the code, contact me via the KSP forum.
 * Patches are welcome.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace km_Lib
{
	public class KM_FuelController : PartModule
	{
		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Flow enabled")]
		bool flowEnabled = false;

		public override void OnStart(PartModule.StartState state)
		{
			this.part.fuelCrossFeed = flowEnabled;
		}

		[KSPEvent(guiName = "Toggle Crossfeed", guiActiveEditor = true ,guiActive = true)]
		public void toggleCrossfeed()
		{
			flowEnabled = !flowEnabled;
			this.part.fuelCrossFeed = flowEnabled;
		}

		[KSPAction("Toggle Crossfeed")]
		public void toggleCrossfeedAction(KSPActionParam param)
		{
			toggleCrossfeed ();
		}

		[KSPAction("Activate Crossfeed")]
		public void activateCrossfeedAction(KSPActionParam param)
		{
            flowEnabled = true;
            this.part.fuelCrossFeed = flowEnabled;
		}

		[KSPAction("Deactivate Crossfeed")]
		public void deactivateCrossfeedAction(KSPActionParam param)
		{
            flowEnabled = false;
            this.part.fuelCrossFeed = flowEnabled;
		}
	}
}

