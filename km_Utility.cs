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


    public static class Utility
    {    


        /*
        None = 0,
        Stage = 1,
        Gear = 2,
        Light = 4,
        RCS = 8,
        SAS = 16,
        Brakes = 32,
        Abort = 64,
        Custom01 = 128,
        Custom02 = 256,
        Custom03 = 512,
        Custom04 = 1024,
        Custom05 = 2048,
        Custom06 = 4096,
        Custom07 = 8192,
        Custom08 = 16384,
        Custom09 = 32768,
        Custom10 = 65536,
        */

        public static Dictionary<int, KSPActionGroup> KM_dictAG = new Dictionary<int, KSPActionGroup>
        {
            {0,  KSPActionGroup.None},
            {1,  KSPActionGroup.Custom01},
            {2,  KSPActionGroup.Custom02},
            {3,  KSPActionGroup.Custom03},
            {4,  KSPActionGroup.Custom04},
            {5,  KSPActionGroup.Custom05},
            {6,  KSPActionGroup.Custom06},
            {7,  KSPActionGroup.Custom07},
            {8,  KSPActionGroup.Custom08},
            {9,  KSPActionGroup.Custom09},
            {10, KSPActionGroup.Custom10},
            {11, KSPActionGroup.Light},
            {12, KSPActionGroup.RCS},
            {13, KSPActionGroup.SAS},
            {14, KSPActionGroup.Brakes},
            {15, KSPActionGroup.Abort}
        };
        public static Dictionary<int, String> KM_dictAGNames = new Dictionary<int, String>
        {
            {0,  "Stage"},
            {1,  "Custom01"},
            {2,  "Custom02"},
            {3,  "Custom03"},
            {4,  "Custom04"},
            {5,  "Custom05"},
            {6,  "Custom06"},
            {7,  "Custom07"},
            {8,  "Custom08"},
            {9,  "Custom09"},
            {10, "Custom10"},
            {11, "Light"},
            {12, "RCS"},
            {13, "SAS"},
            {14, "Brakes"},
            {15, "Abort"},
            {16, "Beep"},
        };
        public static int maxEvent = 17;



        public static void switchLight(Part p, String lightName, bool state){
            Transform lightTransform = p.FindModelTransform (lightName);
            if (lightTransform != null) {
                Light light = lightTransform.GetComponent<Light> ();
                if (light != null)
                    light.intensity = (state?1:0);
            }
        
        }

        private static void traverseChildren(Part p, int nextStage, ref List<Part> resultList)
        {
            MonoBehaviour.print ("Check:" + p);
            if (p.inverseStage >= nextStage) {
                resultList.Add (p);
            }
            foreach (Part child in p.children)
            {

                traverseChildren (child, nextStage, ref resultList);
            }
        }



        /*                 Getting the next stage can be messy because empty stages may remain
         * on jetissoned parts. The timer is supposed to work on these as well.
         * Hence, it will pick the highest stage with an inactive part for staging.
         */
        private static int getHighestNextStage(Part p, int currentStage)
        {

            int highestChildStage = 0;

            // if this is the root part and its a decoupler: ignore it. It was probably fired before.
            // This is dirty guesswork but everything else seems not to work. KSP staging is too messy.
            if(p.vessel.rootPart == p && 
                (p.name.IndexOf("ecoupl") != -1 || p.name.IndexOf("eparat") != -1)){
            } else if (p.inverseStage < currentStage) {
                highestChildStage = p.inverseStage;
            }


            // Check all children. If this part has no children, inversestage or current Stage will be returned
            int childStage = 0;
            foreach (Part child in p.children) {
                childStage = getHighestNextStage (child, currentStage);
                if (childStage > highestChildStage && childStage < currentStage) {
                    highestChildStage = childStage;
                }
            }
            MonoBehaviour. print ("Highest Child Stage:" + highestChildStage);
            return highestChildStage;
        }

        public static void fireNextNonEmptyStage(Vessel v)
        {

            // the parts to be fired
            List<Part> resultList = new List<Part>();

            int highestNextStage = getHighestNextStage (v.rootPart, v.currentStage);
            traverseChildren (v.rootPart, highestNextStage, ref resultList);

            foreach (Part stageItem in resultList)
            {
                MonoBehaviour.print ("Activate:" + stageItem);
                stageItem.activate (highestNextStage, stageItem.vessel);
            }
            v.currentStage = highestNextStage;

        }




		public static void playAnimation(Part p, string animationName, bool forward, bool play, float speed)
		{
			Animation anim;
			anim = p.FindModelAnimators (animationName).FirstOrDefault ();
			if (anim != null) {


				if (forward) {
					anim [animationName].speed = 1f*speed;
					PartModule.print ("NTime forward: " + anim [animationName].normalizedTime);
					if(!play || !anim.isPlaying) anim [animationName].normalizedTime = (play?0f:1f);
					anim.Blend (animationName, 2f);
				} else {
					anim[animationName].speed = -1f*speed;
					PartModule.print ("NTime backward: " + anim [animationName].normalizedTime);
					if(!play || !anim.isPlaying) anim [animationName].normalizedTime = (play?1f:0f);
					anim.Blend (animationName, 2f);
				}
			}
		}

        public static void playAnimationSetToPosition(Part p, string animationName, float position)
        {
            Animation anim;
            anim = p.FindModelAnimators (animationName).FirstOrDefault ();
            if (anim != null) {
                anim [animationName].normalizedTime = position;
                anim [animationName].speed = 0f;
                anim.Play (animationName);
                // anim [animationName].speed = 0;
            }
        }


		public static void playAudio (Part p, string clipName)
		{
			if (clipName != "") {
				AudioSource sound;
				sound = p.gameObject.AddComponent<AudioSource> ();
				sound.clip = GameDatabase.Instance.GetAudioClip (clipName);
                if (sound.clip != null) {
					sound.volume = 1;
					sound.Stop ();
					sound.Play ();
				} 
			} 	
		}

        public static void fireEvent(Part p, int eventID){
            if (p == null)
                return;
            if (eventID == 0) {
                MonoBehaviour.print ("Fire Stage from part " + p);
                fireNextNonEmptyStage (p.vessel);
                return;
            } else if (eventID == 16) {
                playAudio (p, "Klockheed_Martian/Sounds/beep");
                return;
            } else if (eventID > 0 && eventID <=maxEvent){
                MonoBehaviour.print ("Fire Event "+ KM_dictAGNames[eventID]+" from part " + p);
                p.vessel.ActionGroups.ToggleGroup (KM_dictAG[eventID]);   
            }
        }
	}
}










