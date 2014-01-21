/*
 * Author: dtobi
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

namespace KM_Lib
{
    public class KM_Altimeter : PartModule
	{

        [KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Fire ALT", guiUnits = "m")]
        public int fireHeight = 0;

		[KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Current ALT")]
		public float currentAlt = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Condition"),
            UI_Toggle(disabledText = "Lower than", enabledText = "Higher than")]
        public bool useHigher = false;

		[KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Group")]
		public String groupName = "Stage";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Select") , UI_FloatRange(minValue = 0f, maxValue = 15f, stepIncrement = 1f)]
		public float group = 0;
		private float lastGroup = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Height", guiUnits="m") , UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 4f)]
		public float height = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Scale"),
            UI_Toggle(disabledText = "Meter", enabledText = "Kilometer")]
        public bool useKilometer = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Detection"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool isActive = true;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Enable"),
            UI_Toggle(disabledText = "At liftoff", enabledText = "After liftoff")]
        public bool offForStart = true;

        [KSPAction("Activate Detection")]
        public void doActivateAG(KSPActionParam param)
        {
            isActive = true;
        }

        [KSPAction("Deactivate Detection")]
        public void doDeActivateAG(KSPActionParam param)
        {
            isActive = false;
        }

		public override void OnStart(StartState state)
		{
			if (state == StartState.Editor)
			{
				this.part.OnEditorAttach += OnEditorAttach;
				this.part.OnEditorDetach += OnEditorDetach;
				this.part.OnEditorDestroy += OnEditorDestroy;
				OnEditorAttach();
			}

			print("KM Altimeter Detector Started");
		}

		public override void OnUpdate()
		{
            currentAlt = FlightGlobals.getAltitudeAtPos(this.vessel.findWorldCenterOfMass());
            //make height more precise if below 10km
            currentAlt = (currentAlt < 10000 ? this.vessel.heightFromTerrain : currentAlt);


            if (group != lastGroup) {
				groupName = Utility.KM_dictAGNames [(int)group];
				lastGroup = group;
			}

            fireHeight = (useKilometer?1000:1) * (int) height;

			if(!isActive){

				Utility.switchLight (this.part, "light-go", false);
				Utility.playAnimationSetToPosition (this.part, "glow", 0);
				return;
            } else {

                if (offForStart && currentAlt > fireHeight) {
                    offForStart = false;
                }

                if (!offForStart && isActive &&
                    (useHigher && currentAlt > fireHeight ||
                        !useHigher && currentAlt < fireHeight)){
					Utility.switchLight (this.part, "light-go", true);             
					Utility.playAnimationSetToPosition (this.part, "glow", 1);
					Utility.fireEvent (this.part, (int)group);
					print ("ALT reached. Fire event");
					isActive = false;
				}
			} 

		}


		private void OnEditorAttach()
		{
			RenderingManager.AddToPostDrawQueue(99, updateEditor);
		}

		private void OnEditorDetach()
		{

			RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
		}

		private void OnEditorDestroy()
		{
			RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);

		}

		private void updateEditor(){
			if (group != lastGroup) {
				groupName = Utility.KM_dictAGNames [(int)group];
				lastGroup = group;

			}
            fireHeight = (useKilometer?1000:1) * (int) height;
		}
	}
}

