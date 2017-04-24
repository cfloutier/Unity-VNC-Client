using UnityEngine;
using Valve.VR;

namespace Wacki {

    public class ViveUILaserPointer : IUILaserPointer {

        public EVRButtonId button = EVRButtonId.k_EButton_SteamVR_Trigger;

        private bool _connected = false;
        private int _index;

        protected override void Initialize()
        {
            base.Initialize();
            Debug.Log("Initialize");

            var trackedObject = GetComponent<SteamVR_TrackedObject>();

            if(trackedObject != null) {
                _index = (int)trackedObject.index;
                _connected = true;
            }
        }

        public override bool ButtonDown()
        {
            if(!_connected)
                return false;

            var device = SteamVR_Controller.Input(_index);
            if(device != null) {
                var result = device.GetPressDown(button);
                return result;
            }

            return false;
        }

        public override bool ButtonUp()
        {
            if(!_connected)
                return false;

            var device = SteamVR_Controller.Input(_index);
            if(device != null)
                return device.GetPressUp(button);

            return false;
        }

        public bool overControl = false;
        
        public override void OnEnterControl(GameObject control)
        {
            tick();
            overControl = true;
        }

        public void tick()
        {
            var device = SteamVR_Controller.Input(_index);
          
            device.TriggerHapticPulse(500);
        }

        public override void OnExitControl(GameObject control)
        {
            
            overControl = false;
            
        }
    }

}