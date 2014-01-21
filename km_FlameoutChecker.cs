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
	public class KM_Flameout_Checker : PartModule
	{
		
        [KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Group")]
        public String groupName = "Stage";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Select") , UI_FloatRange(minValue = 0f, maxValue = 15f, stepIncrement = 1f)]
        public float group = 0;
        private float lastGroup = 0;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Flameout:")]
		private bool flameout = false;

		[KSPField(isPersistant = false, guiActive = true, guiName = "Ignition:")]
		private bool ignition = false;

		[KSPField(isPersistant = false, guiActive = true, guiName = "Has fired:")]
		private bool hasFired = false;

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor)
            {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                OnEditorAttach();
            }
        }

		public override void OnUpdate(){
            if (group != lastGroup) {
                groupName = Utility.KM_dictAGNames [(int)group];
                lastGroup = group;
            }

			foreach (Part child in this.part.children)
			{
				ModuleEngines[] engines = child.GetComponents<ModuleEngines>();
                if(engines != null && engines.Count() > 0) {
					ignition = engines[0].getFlameoutState;
					flameout = engines[0].getIgnitionState;
					if (!hasFired && engines[0].getFlameoutState && engines[0].getIgnitionState) {
						hasFired = true;
                        Utility.fireEvent (this.part, (int)group);


					} else if (!engines[0].getFlameoutState && engines[0].getIgnitionState) {
						// the engine is running again. Reactivate the action group.
						hasFired = false;
					}
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

        }

	} 
}

