using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.BWF.Demo.Util
{
    /// <summary>Changes the sensitivity of ScrollRects under various platforms.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_demo_1_1_util_1_1_scroll_rect_handler.html")]
    public class ScrollRectHandler : MonoBehaviour
    {

        #region Variables

        public ScrollRect Scroll;
        private float WindowsSensitivity = 35f;
        private float MacSensitivity = 25f;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            if (BWF.Util.Helper.isWindowsPlatform)
            {
                Scroll.scrollSensitivity = WindowsSensitivity;
            }
            else if (BWF.Util.Helper.isMacOSPlatform)
            {
                Scroll.scrollSensitivity = MacSensitivity;
            }
        }

        #endregion
    }
}
// © 2016-2017 crosstales LLC (https://www.crosstales.com)