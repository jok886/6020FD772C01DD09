using UnityEngine;
using UnityEngine.UI;
using Crosstales.BWF.Model;

namespace Crosstales.BWF.Demo
{
    /// <summary>Wrapper for sources.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_demo_1_1_source_entry.html")]
    public class SourceEntry : MonoBehaviour
    {

        #region Variables

        public Text Text;
        public Image Icon;
        public Image Main;

        public Source Source;

        public GUIMain GuiMain;

        public Color32 EnabledColor = new Color32(0, 255, 0, 192);
        private Color32 disabledColor;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            disabledColor = Main.color;
        }

        public void Update()
        {
            Text.text = Source.Name;
            Icon.sprite = Source.Icon;

            if (GuiMain.Sources.Contains(Source.Name))
            {
                Main.color = EnabledColor;
            }
            else
            {
                Main.color = disabledColor;
            }
        }

        #endregion


        #region Public methods

        public void Click()
        {
            if (GuiMain.Sources.Contains(Source.Name))
            {
                GuiMain.Sources.Remove(Source.Name);
            }
            else
            {
                GuiMain.Sources.Add(Source.Name);
            }
        }

        #endregion
    }
}
// © 2015-2017 crosstales LLC (https://www.crosstales.com)