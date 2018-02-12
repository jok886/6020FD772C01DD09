#if !UNITY_WSA
using UnityEngine;
using System.Collections;

namespace Crosstales.BWF.Test
{
    /// <summary>Test for multi-threading of the BWF.</summary>
    [HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_test_1_1_multi_thread_test.html")]
    public class MultiThreadTest : MonoBehaviour
    {
        #region Variables

        public string DirtyText;

        //private bool containsUnwantedWords;
        private System.Collections.Generic.List<string> unwantedWords = new System.Collections.Generic.List<string>();
        //private string cleanText;

        #endregion


        #region MonoBehaviour methods

        void Start()
        {
            Debug.Log("Test the text: " + DirtyText);
            Debug.Log("Text length: " + DirtyText.Length);

            StartCoroutine(multiTreaded());
        }

        #endregion


        #region Private methods

        private IEnumerator multiTreaded()
        {
            Debug.Log("Starting...");

            while (!Manager.BadWordManager.isReady)
            {
                yield return null;
            }

            //System.Threading.Thread worker = new System.Threading.Thread(() => BWFManager.ContainsMT(out containsUnwantedWords, DirtyText));
            System.Threading.Thread worker = new System.Threading.Thread(() => BWFManager.GetAllMT(out unwantedWords, DirtyText));
            //System.Threading.Thread worker = new System.Threading.Thread(() => BWFManager.ReplaceAllMT(out cleanText, DirtyText));
            //System.Threading.Thread worker = new System.Threading.Thread(() => BadWordManager.ReplaceAllMT(out CleanText, DirtyText));

            worker.Start();

            Debug.Log("Checking...");

            do
            {
                yield return null;
            } while (worker.IsAlive);

            Debug.Log("Finished: " + System.Environment.NewLine + unwantedWords.CTDump());
        }

        #endregion
    }
}
#endif
// © 2016-2017 crosstales LLC (https://www.crosstales.com)
