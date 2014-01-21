// /*
//  * Author: dtobi
//  * This work is shared under CC BY-NC-ND 3.0 license.
//  * Non commercial, no derivatives, attribution if shared unmodified.
//  * You may distribute this code and the compiled .dll as is.
//  * 
//  * Exception from the no-deriviates clause in case of KSP updates:
//  * In case of an update of KSP that breaks the code, you may change
//  * this code to make it work again and redistribute it under a different
//  * class name until the author publishes an updated version. After a
//  * release by the author, the right to redistribute the changed code
//  * vanishes.
//  * 
//  * You must keep this boilerplate in the file and give credit to the author
//  * in the download file as well as on the webiste that links to the file.
//  * 
//  * Should you wish to change things in the code, contact me via the KSP forum.
//  * Patches are welcome.
//  *
//  */
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KM_Lib
{
    public class KM_Valve : PartModule
    {
        static float maxSpeedY = -1.0f;
        KSPParticleEmitter valveEffect = null;

        [KSPField(isPersistant = true, guiName = "Opened")] // remember if the part is inflated
        public bool isOpen = false;

        public override void OnStart(StartState state)
        {
            valveEffect = (KSPParticleEmitter )this.part.GetComponentInChildren <KSPParticleEmitter > ();
            if (valveEffect  != null) {
                valveEffect.emit = isOpen;
                valveEffect.localVelocity.y = maxSpeedY * force / 100;    
            } else {
                print ("Launch effect not found");
            }
        }

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Outlet") , UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 5f)]
        public float force = 10;




        [KSPEvent(guiName = "Toggle", guiActive = true, guiActiveEditor = false)]
        public void toggleValve ()
        {   
            setValve (!isOpen); 
        }

        [KSPAction("Toggle")]
        public void toggleValveAG (KSPActionParam param)
        {   
            setValve (!isOpen); 
        }

        [KSPEvent(guiName = "Open", guiActive = true, guiActiveEditor = false)]
        public void openValve ()
        {   
            setValve (true); 
        }

        [KSPAction("Open")]
        public void openValveAG (KSPActionParam param)
        {   
            setValve (true); 
        }

        [KSPEvent(guiName = "Close", guiActive = true, guiActiveEditor = false)]
        public void closeValve ()
        {   
            setValve (false); 
        }

        [KSPAction("Close")]
        public void closeValveAG (KSPActionParam param)
        {   
            setValve (false); 
        }

          public void setValve (bool nextIsOpen)
        {   
            if (valveEffect  != null) {
                isOpen = nextIsOpen;
                valveEffect.emit = nextIsOpen;
            } 
        }


        public override void OnUpdate()
        {
            if (isOpen) {
                valveEffect.localVelocity.y = maxSpeedY * force / 100;
                float receivedRessource = 0;
                if (part.parent.Resources ["LiquidFuel"] != null)     receivedRessource += this.part.RequestResource("LiquidFuel", force*TimeWarp.fixedDeltaTime);
                if (part.parent.Resources ["Oxidizer"] != null)       receivedRessource += this.part.RequestResource("Oxidizer", force*TimeWarp.fixedDeltaTime);
                if (part.parent.Resources ["MonoPropellant"] != null) receivedRessource += this.part.RequestResource("MonoPropellant", force*TimeWarp.fixedDeltaTime);
                if (receivedRessource == 0)
                    setValve (false);
            }
        }
    }
}

